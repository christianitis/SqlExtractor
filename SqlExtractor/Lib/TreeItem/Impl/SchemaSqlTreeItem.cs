using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using UrnAttribute = SqlExtractor.Lib.TreeItem.IUrnPart.UrnAttribute;

namespace SqlExtractor.Lib.TreeItem.Impl;

public class SchemaSqlTreeItem
	: AbstractSqlTreeItem, IInitializeFromDatabase<Schema>, IInitializeFromSql, IUrnPart
{
	public Urn OriginalUrn { get; private set; }
	public string UrnTypeName => "Schema";
	
	public LinkedList<UrnAttribute> GetAttributes() => new([
		new UrnAttribute("Name", Name, null)
	]);

	public SchemaSqlTreeItem(string serverName, string dbName, string schemaName,
		IEnumerable<AbstractSqlTreeItem>? children = null) : base(schemaName, children)
	{
		OriginalUrn = new Urn($"Server[@Name='{serverName}']/Database[@Name='{dbName}']/Schema[@Name='{schemaName}']");
	}

	public SchemaSqlTreeItem(string name, IEnumerable<AbstractSqlTreeItem>? children = null) : base(name, children)
	{
	}

	public void InitializeFromDatabase(Schema smoObject)
	{
		Name = smoObject.Name;
		OriginalUrn = smoObject.Urn;
	}

	public void InitializeFromDatabaseWithChildren(Schema smoObject)
	{
		InitializeFromDatabase(smoObject);
		smoObject.EnumOwnedObjects();
	}


	public void InitializeFromSql(TSqlStatement sql)
	{
		throw new NotImplementedException();
	}

	public string LevelName { get; }
	
	public override object Clone()
	{
		throw new NotImplementedException();
	}
}