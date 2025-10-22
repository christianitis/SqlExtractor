using System.Collections.ObjectModel;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using UrnAttribute = SqlExtractor.Lib.TreeItem.IUrnPart.UrnAttribute;

namespace SqlExtractor.Lib.TreeItem.Impl;

/// <summary>
/// Represents a generic base SQL object, like a table or view.
/// </summary>
/// <typeparam name="T">The <see cref="SqlSmoObject"/> class which represents this object in the database.</typeparam>
/// <remarks>More derived classes used to represent specific SQL object types should inherit from this class.</remarks>
public class GenericBaseSqlTreeItem<T> : AbstractSqlTreeItem, IInitializeFromDatabase<T>, IUrnPart
	where T : SqlSmoObject
{
	public override SortedObservableCollection<AbstractSqlTreeItem> Children => [];
	public virtual string UrnTypeName => SqlSmoObject.GetUrnSuffix(typeof(T));

	public LinkedList<UrnAttribute> GetAttributes(IEnumerable<AbstractSqlTreeItem> path) => new([
		new UrnAttribute("Name", Name, UrnAttribute.Relation.and),
		new UrnAttribute("Schema", path.OfType<SchemaSqlTreeItem>().Single().Name, null)
	]);

	public LinkedList<UrnAttribute> GetAttributes() => new([
		new UrnAttribute("Name", Name, null),
	]);

	public Urn OriginalUrn { get; private set; }

	public GenericBaseSqlTreeItem(string name)
	{
		Name = name;
	}

	public override object Clone()
	{
		throw new NotImplementedException();
	}


	public virtual void InitializeFromDatabase(T smoObject)
	{
		OriginalUrn = smoObject.Urn;
		Name = OriginalUrn.GetAttribute("Name");
	}

	public virtual void InitializeFromDatabaseWithChildren(T smoObject)
	{
		InitializeFromDatabase(smoObject);
	}
}