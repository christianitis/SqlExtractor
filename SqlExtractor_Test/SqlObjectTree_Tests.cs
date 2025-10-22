using Microsoft.SqlServer.Management.Smo;
using SqlExtractor.Lib;

namespace SqlExtractor_Test;

[TestFixture]
public class SqlObjectTreeTests: AbstractBaseTestFixture
{
	/**
	 * Set up a database with some objects.
	 * @param database The database to set up. Assumes it already exists on the server.
	 */
	private static void SetUpDatabase(Database database)
	{
		var type = new UserDefinedDataType(database, "TypeA")
		{
			SystemType = "VARCHAR",
			Length = 1
		};
		var table = new Table(database, "TableA");
		var col = new Column(table, "ColumnA", DataType.UserDefinedDataType(type.Name));
		var viewA = new View(database, "ViewA")
		{
			TextHeader = "CREATE VIEW ViewA AS",
			TextBody = "SELECT * FROM TableA"
		};
		var viewB = new View(database, "ViewB")
		{
			TextHeader = "CREATE VIEW ViewB AS",
			TextBody = "SELECT * FROM ViewA"
		};
		var spA = new StoredProcedure(database, "SpA")
		{
			TextHeader = "CREATE PROCEDURE SpA AS",
			TextBody = "EXEC SpB"
		};
		var spB = new StoredProcedure(database, "SpB") // Circular dependency
		{
			TextHeader = "CREATE PROCEDURE SpB AS",
			TextBody = "EXEC SpA"
		};
		var spC = new StoredProcedure(database, "SpC")
		{
			TextHeader = "CREATE PROCEDURE SpC AS",
			TextBody = "SELECT * FROM SynA"
		};
		var synA = new Synonym(database, "SynA")
		{
			BaseObject = "TableA"
		};
		table.Columns.Add(col);
		database.Tables.Add(table);
		database.Synonyms.Add(synA);
		database.Views.Add(viewA);
		database.Views.Add(viewB);
		database.StoredProcedures.Add(spA);
		database.StoredProcedures.Add(spB);
		database.StoredProcedures.Add(spC);
		
		type.Create();
		table.Create();
		synA.Create();
		viewA.Create();
		viewB.Create();
		spA.Create();
		spB.Create();
		spC.Create();
	}

	[Test]
	public void Test_InReverseScriptingOrder_DoesCauseWarnings()
	{
		// // Set up
		// var sourceDatabase = new Database(new Server(ServerA), $"{nameof(Test_InReverseScriptingOrder_DoesCauseWarnings)}_{Timestamp}");
		// var destinationDatabase = new Database(new Server(ServerB), $"{nameof(Test_InReverseScriptingOrder_DoesCauseWarnings)}_{Timestamp}");
		// sourceDatabase.Create();
		// destinationDatabase.Create();
		// SetUpDatabase(sourceDatabase);
		// var tree = new SqlObjectTree();
		// tree.AddDatabase(sourceDatabase);
		// var messages = new List<object>();
		// ServerB.ServerMessage += (sender, args) => messages.Add(args);
		// var scripter = new Scripter(new Server(ServerA))
		// {
		// 	Options =
		// 	{
		// 		IncludeDatabaseContext = true,
		// 		IncludeIfNotExists = true,
		// 		WithDependencies = false // We are handling dependencies ourselves
		// 	}
		// };
		// scripter.Options.GetScriptingPreferences().Behavior = ScriptBehavior.CreateOrAlter; // In the case of service queues, etc
		//
		// // Execute
		// foreach (var item in tree.InScriptingOrder().Reverse())
		// {
		// 	try
		// 	{
		// 		var script = scripter.EnumScript(new[] { item.Urn });
		// 		foreach (var statement in script)
		// 		{
		// 			if (!string.IsNullOrWhiteSpace(statement))
		// 			{
		// 				destinationDatabase.ExecuteNonQuery(statement);
		// 			}
		// 		}
		// 	}
		// 	catch (Exception e)
		// 	{
		// 		messages.Add(e);
		// 	}
		// }
		//
		// // Verify
		// Assert.That(messages, Is.Not.Empty);
	}
	
	[Test]
	public void Test_InScriptingOrder_DoesNotCauseWarnings()
	{
		// // Set up
		// var sourceServer = new Server(ServerA);
		// var destinationServer = new Server(ServerB);
		// var dbName = $"{nameof(Test_InScriptingOrder_DoesNotCauseWarnings)}_{Timestamp}";
		// var sourceDatabase = new Database(sourceServer, dbName);
		// sourceServer.Databases.Add(sourceDatabase);
		// sourceDatabase.Create();
		// SetUpDatabase(sourceDatabase);
		// var destinationDatabase = new Database(destinationServer, dbName);
		// destinationDatabase.Create();
		// var tree = new SqlObjectTree();
		// tree.AddDatabase(sourceDatabase);
		// var messages = new List<object>();
		// ServerB.ServerMessage += (sender, args) => messages.Add(args);
		// ServerB.InfoMessage += (sender, args) => messages.Add(args);
		//
		// var scripter = new Scripter(new Server(ServerA))
		// {
		// 	Options =
		// 	{
		// 		IncludeDatabaseContext = true,
		// 		IncludeIfNotExists = true,
		// 		WithDependencies = false // We are handling dependencies ourselves
		// 	}
		// };
		// scripter.Options.GetScriptingPreferences().Behavior = ScriptBehavior.CreateOrAlter; // In the case of service queues, etc
		//
		// // Execute
		// foreach (var item in tree.InScriptingOrder())
		// {
		// 	var script = scripter.EnumScript(new[] { item.Urn });
		// 	foreach (var statement in script)
		// 	{
		// 		if (!string.IsNullOrWhiteSpace(statement))
		// 		{
		// 			destinationDatabase.ExecuteNonQuery(statement);
		// 		}
		// 	}
		// }
		// destinationDatabase.Refresh();
		//
		// // Verify
		// Assert.That(messages, Is.Empty);
		//
		// Assert.That(destinationDatabase.Views.Cast<View>().Any(v => v.Name == "ViewA"), Is.True, "ViewA was not created correctly");
		// Assert.That(destinationDatabase.Views.Cast<View>().Any(v => v.Name == "ViewB"), Is.True, "ViewB was not created correctly");
		//
		// Assert.That(destinationDatabase.StoredProcedures.Cast<StoredProcedure>().Any(sp => sp.Name == "SpA"), Is.True, "SpA was not created correctly");
		// Assert.That(destinationDatabase.StoredProcedures.Cast<StoredProcedure>().Any(sp => sp.Name == "SpB"), Is.True, "SpB was not created correctly");
		// Assert.That(destinationDatabase.StoredProcedures.Cast<StoredProcedure>().Any(sp => sp.Name == "SpC"), Is.True, "SpC was not created correctly");
		//
		// Assert.That(destinationDatabase.Tables.Cast<Table>().Any(t => t.Name == "TableA"), Is.True, "TableA was not created correctly");
		//
		// Assert.That(destinationDatabase.UserDefinedDataTypes.Cast<UserDefinedDataType>().Any(t => t.Name == "TypeA"), Is.True, "TypeA was not created correctly");
	}
	
}