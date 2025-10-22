using SqlExtractor.Lib.TreeItem;

namespace SqlExtractor.Lib;

public class SqlTreeItemComparer:IComparer<AbstractSqlTreeItem>
{
	public int Compare(AbstractSqlTreeItem? x, AbstractSqlTreeItem? y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (y is null) return 1;
		if (x is null) return -1;
		return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
	}
}