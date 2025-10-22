using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using UrnAttribute = SqlExtractor.Lib.TreeItem.IUrnPart.UrnAttribute;


namespace SqlExtractor.Lib.TreeItem.Impl;

/// <summary>
/// Represents a Server in the tree.
/// </summary>
public class ServerSqlTreeItem : AbstractSqlTreeItem, IInitializeFromDatabase<Server>, IUrnPart
{
	public ServerConnection ServerConnection { get; }
	public Urn? OriginalUrn { get; private set; } 
	
	public string UrnTypeName => "Server";

	public LinkedList<UrnAttribute> GetAttributes() => new([
		new UrnAttribute("Name", Name, null)
	]);

	public IEnumerable<DatabaseSqlTreeItem> Databases => Children.OfType<DatabaseSqlTreeItem>();

	public ServerSqlTreeItem(ServerConnection serverConnection, IEnumerable<DatabaseSqlTreeItem>? children = null)
		: base(serverConnection.ServerInstance, children)
	{
		ServerConnection = serverConnection;
		OriginalUrn = new Urn($"Server[@Name='{serverConnection.ServerInstance}']");
	}

	public ServerSqlTreeItem(string name, IEnumerable<DatabaseSqlTreeItem>? children = null)
		: base(name, children)
	{
		OriginalUrn = new Urn($"Server[@Name='{name}']");
	}

	public void InitializeFromDatabase(Server smoObject)
	{
		Name = smoObject.InstanceName;
		OriginalUrn = smoObject.Urn;
	}

	public void InitializeFromDatabaseWithChildren(Server smoObject)
	{
		InitializeFromDatabase(smoObject);
		foreach (var database in smoObject.Databases.OfType<Database>())
		{
			Children.Add(new DatabaseSqlTreeItem());
		}
	}
	
	public override object Clone()
	{
		throw new NotImplementedException();
	}
	

}