using Microsoft.SqlServer.Management.Smo;
using SqlExtractor.Lib;
using SqlExtractor.Lib.TreeItem.Impl;

namespace SqlExtractor_Test;

[TestFixture]
public class AbstractSqlTreeItem_Test : AbstractBaseTestFixture
{
	[Test]
	public void Test_GetPathToRoot()
	{
		var tree = new SqlObjectTree();

		var targetObject = new GenericBaseSqlTreeItem<Table>("TARGET");

		tree.Items.Add(
			new ServerSqlTreeItem("ROOT1", [
				new DatabaseSqlTreeItem("DB1", [
					new SchemaSqlTreeItem("SCHEMA1", [
						new GroupSqlTreeItem("GROUP1", [
							targetObject
						])
					])
				]),
				new DatabaseSqlTreeItem("DB2", [
					new SchemaSqlTreeItem("SCHEMA2", [
						new GroupSqlTreeItem("GROUP2", [
							new GenericBaseSqlTreeItem<Table>("WrongObject1"),
							targetObject,
						])
					])
				]),
				new DatabaseSqlTreeItem("DB3", [
					new SchemaSqlTreeItem("SCHEMA3", [
						new GroupSqlTreeItem("GROUP3", [
							new GenericBaseSqlTreeItem<Table>("WrongObject2")
						])
					])
				]),
				new DatabaseSqlTreeItem("DB4", [
					new SchemaSqlTreeItem("SCHEMA4A", [
						new GroupSqlTreeItem("GROUP4A", [
							targetObject,
						])
					]),
					new SchemaSqlTreeItem("SCHEMA4B", [
						new GroupSqlTreeItem("GROUP4B", [
							targetObject,
						])
					])
				])
			])
		);

		tree.Items.Add(
			new ServerSqlTreeItem("ROOT2", [
				new DatabaseSqlTreeItem("DB1", [
					new SchemaSqlTreeItem("SCHEMA1", [
						new GroupSqlTreeItem("GROUP1", [
							targetObject
						])
					])
				])
			])
		);

		// Example of a valid URN: Server[@Name='DEV-2k19-ChristianH']/Database[@Name='A2']/Table[@Name='Table1' and @Schema='dbo']
		var path = tree.GetPathToRoot(obj => ReferenceEquals(obj, targetObject))!.ToArray();
		Assert.That(path, Is.Not.Null);
		Assert.That(path, Has.Length.EqualTo(5));
		Assert.Multiple(() =>
		{
			Assert.That(path[0].Select(node => node.Name),
				Is.EquivalentTo(["ROOT1", "DB1", "SCHEMA1", "GROUP1", "TARGET"]));
			Assert.That(path[1].Select(node => node.Name),
				Is.EquivalentTo(["ROOT1", "DB2", "SCHEMA2", "GROUP2", "TARGET"]));
			Assert.That(path[2].Select(node => node.Name),
				Is.EquivalentTo(["ROOT1", "DB4", "SCHEMA4A", "GROUP4A", "TARGET"]));
			Assert.That(path[3].Select(node => node.Name),
				Is.EquivalentTo(["ROOT1", "DB4", "SCHEMA4B", "GROUP4B", "TARGET"]));
			Assert.That(path[4].Select(node => node.Name),
				Is.EquivalentTo(["ROOT2", "DB1", "SCHEMA1", "GROUP1", "TARGET"]));
		});
	}
}