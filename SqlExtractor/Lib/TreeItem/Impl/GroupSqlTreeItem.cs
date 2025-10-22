using Microsoft.SqlServer.Management.Sdk.Sfc;

namespace SqlExtractor.Lib.TreeItem.Impl;

public class GroupSqlTreeItem : AbstractSqlTreeItem
{
	public GroupSqlTreeItem(string name, IEnumerable<AbstractSqlTreeItem>? children = null) : base(name, children)
	{
	}
	
	public override object Clone()
	{
		throw new NotImplementedException();
	}

}