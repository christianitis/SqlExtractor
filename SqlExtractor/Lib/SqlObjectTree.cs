using System.IO;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using SqlExtractor.Lib.TreeItem;
using SqlExtractor.Lib.TreeItem.Impl;

namespace SqlExtractor.Lib;

/**
 * <summary>Represents a tree of <see cref="SqlTreeItem"/>s.</summary>
 * <remarks>
 * Do not directly modify <see cref="Items"/> directly.
 * Use <see cref="AddItem"/> and <see cref="RemoveItem"/> instead.
 * </remarks>
 */
public class SqlObjectTree
{
	public SortedObservableCollection<ServerSqlTreeItem> Items { get; } = [];

	public IEnumerable<AbstractSqlTreeItem> AsFlatList() => Items.RecursiveSelect(item => item is not GroupSqlTreeItem);

	public IEnumerable<ServerSqlTreeItem> Servers => Items.Cast<ServerSqlTreeItem>();

	public IEnumerable<DatabaseSqlTreeItem> Databases => Servers.SelectMany(server => server.Databases);

	public IEnumerable<AbstractSqlTreeItem> Schemas => Databases.SelectMany(database => database.Schemas);

	public IEnumerable<AbstractSqlTreeItem> Jobs =>
		Servers.SelectMany(server => server.Children.Where(it => it.Name == "Jobs"));

	public IEnumerable<AbstractSqlTreeItem> Groups =>
		Schemas.SelectMany(schema => schema.Children.OfType<GroupSqlTreeItem>());

	public IEnumerable<AbstractSqlTreeItem> Objects =>
		Groups.SelectMany(group => group.Children);


	/// <summary>
	/// Gets the path from one or more objects referred to by <paramref name="filter"/>to the root of the tree.
	/// </summary>
	public IEnumerable<IEnumerable<AbstractSqlTreeItem>>? GetPathToRoot(Func<AbstractSqlTreeItem, bool> filter)
	{
		// TODO: There is a bug in List, the indexer is not allowing me to set values. Report to Microsoft. Workaround: use .Add.
		var objectsByLevel = new List<List<AbstractSqlTreeItem>>();
		objectsByLevel.Add(Items.OfType<AbstractSqlTreeItem>().ToList()); // Add the roots at level 0
		var objectsInPathToRoot = new List<AbstractSqlTreeItem>();
		var levelOfTarget = -1;
		var level = 0;
		do
		{
			var childrenOfLevel = objectsByLevel[level].SelectMany(sqlObject => sqlObject.Children).ToList();

			if (childrenOfLevel.Any(filter))
				levelOfTarget = level + 1;

			if (childrenOfLevel.IsNullOrEmpty())
				break;
			else
				objectsByLevel.Add(childrenOfLevel);

			level++;
		} while (levelOfTarget == -1);

		// If we have multiple targets according to the filter, we need to make sure we don't return the same path twice.
		// Our rules dictate that each level is a unique set. Since we determine parenthood by reference, we keep a
		// reference of all parents at targetLevel - 1 that have already been used in a path. If we find a parent
		// which is already in the list, we know we've already returned this path, so we skip to the next possible path.
		var usedParents = new HashSet<AbstractSqlTreeItem>();

		// We have now flattened the tree to a list of all objects in the tree by level.
		// Now we need to work backwards from the target to the root for each target object we find.
		foreach (var foundLeaf in objectsByLevel[levelOfTarget].Where(filter))
		{
			var path = new List<AbstractSqlTreeItem>();
			path.Add(foundLeaf);
			level = levelOfTarget - 1;
			do
			{
				var targetParent = objectsByLevel[level]
						.Except(usedParents)
						.FirstOrDefault(parent => parent.Children.Contains(path.Last()));
				if (targetParent is not null)
				{
					if (usedParents.Contains(targetParent))
					{
						continue;
					}

					if (level == levelOfTarget - 1)
					{
						usedParents.Add(targetParent);
					}

					level--;

					path.Add(targetParent);
				}
				else
				{
					break;
				}
			} while (level >= 0);

			if (path.Count > 1)
			{
				yield return path;
			}
		}
	}

	/// <summary>
	/// Get a URN for the path to the root of the tree.
	/// </summary>
	public Urn? PathToUrn(IEnumerable<AbstractSqlTreeItem> path)
	{
		var builder = new StringBuilder();
		
		foreach (var item in path.OfType<IUrnPart>())
		{
			builder.Append(item.GetUrnFragment(item.GetAttributes(path)));
			if (path.Last() != item)
			{
				builder.Append('/');
			}
		}
		
		return new Urn(builder.ToString());
	}
	
	/**
	 * This method returns the objects in the order they should be scripted to prevent dependency issues.
	 */
	public IEnumerable<AbstractSqlTreeItem> InScriptingOrder()
	{
		var outputList = new List<AbstractSqlTreeItem>();
		/*var sourceList = new List<SqlTreeItem>(AsFlatList().Where(it => it.Urn.Type != "Server"));
		var outputList = new List<SqlTreeItem>();

		// Databases can be added first
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "Database"));

		var systemSchemas = new[]
		{
			"db_accessadmin", "db_backupoperator", "db_datareader", "db_datawriter", "db_owner", "db_securityadmin",
			"dbo", "INFORMATION_SCHEMA", "sys"
		};
		sourceList = sourceList.Where(it => !systemSchemas.Contains(it.Name)).ToList();
		// Then the schemas
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "Schema"));

		// Next we'll add the types
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "UserDefinedDataType"));
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "UserDefinedTableType"));
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "UserDefinedType"));

		// Then the tables
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "Table"));

		// Then the queues
		outputList.AddRange(sourceList.TakeWhere(it => it.Urn.Type == "ServiceQueue"));

		string[] typesToSkip =
		[
			"Database", "Schema", "UserDefinedDataType",
			"UserDefinedTableType", "UserDefinedType", "Table", "ServiceQueue"
		];

		var dependencyWalker = new DependencyWalker();

		// Then everything else
		foreach (var sqlObject in sourceList.TakeWhere(it => it.Urn.Type != "Job"))
		{
			void AddWithDependencies(SqlTreeItem _sqlObject)
			{
				var server = new Server(_sqlObject.OriginalServer);
				dependencyWalker.Server = server;
				var dependencyTree =
					dependencyWalker.DiscoverDependencies(new[] { _sqlObject.Urn }, DependencyType.Parents);
				var dependencies = dependencyWalker.WalkDependencies(dependencyTree)
					.Where(dependency =>
							!dependency
								.IsRootNode && // SMO includes the root node (_sqlObject), which is stupid; we don't want that.
							!typesToSkip.Contains(dependency.Urn
								.Type) && // We already added all this stuff, so we can just skip it.
							sourceList.Any(it =>
								it.Urn == dependency.Urn) && // Is this a dependency we intend to add at some point?
							!outputList.Any(it =>
								it.Urn == dependency.Urn) // Is this NOT a dependency we've already added?
					).ToList();
				if (dependencies.Count > 0)
				{
					Debug.WriteLine($"Processing dependencies for {_sqlObject.Urn}:");
				}

				foreach (var dependency in dependencies)
				{
					// Does the list NOT already contain this dependency? If so, add it.
					if (!outputList.Any(it => it.Urn == dependency.Urn))
					{
						// Is this a circular dependency? I.E., does dependency depend on _sqlObject? If so, add it to the list without adding the dependency. We'll just have to "eat" the warning when this script is run.
						if (dependencyWalker
						    .WalkDependencies(dependencyWalker.DiscoverDependencies(new[] { dependency.Urn },
							    DependencyType.Parents)).Any(it => it.Urn == _sqlObject.Urn))
						{
							Debug.WriteLine($"Circular dependency for {_sqlObject.Urn}: {dependency.Urn}");
							continue;
						}

						Debug.WriteLine($"Adding dependency for {_sqlObject.Urn}: {dependency.Urn}");
						AddWithDependencies(sourceList.First(it => it.Urn == dependency.Urn));
					}
				}

				Debug.WriteLine("Adding: " + _sqlObject.Urn);
				outputList.Add(_sqlObject);
			}

			AddWithDependencies(sqlObject);
		}
*/
		return outputList;
	}
}