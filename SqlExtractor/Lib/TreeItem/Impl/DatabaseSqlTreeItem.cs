using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using UrnAttribute = SqlExtractor.Lib.TreeItem.IUrnPart.UrnAttribute;

namespace SqlExtractor.Lib.TreeItem.Impl;

public class DatabaseSqlTreeItem : AbstractSqlTreeItem, IInitializeFromDatabase<Database>, IUrnPart
{
	public IEnumerable<SchemaSqlTreeItem> Schemas => Children.OfType<SchemaSqlTreeItem>();

	public string UrnTypeName => "Database";

	public LinkedList<UrnAttribute> GetAttributes() =>
		new([
			new UrnAttribute("Name", Name, null)
		]);

	public DatabaseSqlTreeItem(string name, IEnumerable<SchemaSqlTreeItem>? children = null) : base(name, children)
	{
	}

	public DatabaseSqlTreeItem()
	{
	}

	public Urn OriginalUrn { get; private set; }

	public void InitializeFromDatabase(Database smoObject)
	{
		Name = smoObject.Name;
	}

	public void InitializeFromDatabaseWithChildren(Database smoObject)
	{
		InitializeFromDatabase(smoObject);
	}

	public override object Clone()
	{
		throw new NotImplementedException();
	}
}