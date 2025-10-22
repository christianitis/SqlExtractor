namespace SqlExtractor.Lib.TreeItem;

public abstract class AbstractSqlTreeItem : ICloneable
{
	/// <summary>
	/// The name of the item, as displayed in the UI.
	/// </summary>
	public string Name { get; set; }

	public virtual SortedObservableCollection<AbstractSqlTreeItem> Children { get; private set; } = [];

	public AbstractSqlTreeItem()
	{
	}

	public AbstractSqlTreeItem(IEnumerable<AbstractSqlTreeItem>? children = null)
	{
		Children = new SortedObservableCollection<AbstractSqlTreeItem>(children ?? []);
	}

	public AbstractSqlTreeItem(string name, IEnumerable<AbstractSqlTreeItem>? children = null)
	{
		Name = name;
		Children = new SortedObservableCollection<AbstractSqlTreeItem>(children ?? []);
	}
	
	public abstract object Clone();

	public override string ToString() => $"{nameof(AbstractSqlTreeItem)} {Name}";
}