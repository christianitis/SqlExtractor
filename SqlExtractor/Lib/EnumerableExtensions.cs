using SqlExtractor.Lib.TreeItem;

namespace SqlExtractor.Lib;

public static class EnumerableExtensions
{
	public static IEnumerable<AbstractSqlTreeItem> RecursiveSelect(this IEnumerable<AbstractSqlTreeItem> rootItems, Func<AbstractSqlTreeItem, bool> predicate)
	{
		if (rootItems == null)
			yield break;
        
		foreach (var item in rootItems)
		{
			// If the current item matches the predicate, include it in the results
			if (predicate(item))
				yield return item;
            
			// Recursively process children
			foreach (var childItem in RecursiveSelect(item.Children, predicate))
			{
				yield return childItem;
			}
		}
	}

	/**
	 * Selects all elements that match predicate and removes them from rootItems. Returns the matching (removed) items.
	 */
	public static IEnumerable<AbstractSqlTreeItem> TakeWhere(this IList<AbstractSqlTreeItem> rootItems, Func<AbstractSqlTreeItem, bool> predicate)
	{
		foreach (var item in new List<AbstractSqlTreeItem>(rootItems).Where(predicate))
		{
			yield return item;
			rootItems.Remove(item);
		}
	}
}