using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using SqlExtractor.Lib.TreeItem;
using SqlExtractor.Lib.TreeItem.Impl;

namespace SqlExtractor.Lib;

public class SortedObservableCollection<T>: ObservableCollection<T> where T : AbstractSqlTreeItem
{
	private readonly IComparer<T> _comparer;
	
	public delegate T MergeConflictResolverDelegate(Urn pathToHere, T existingItem, T newItem);

	public MergeConflictResolverDelegate MergeConflictResolver { get; set; } = DefaultResolver;

	public SortedObservableCollection(IComparer<T>? comparer = null)
	{
		_comparer = comparer ?? new SqlTreeItemComparer() ;
	}
	public SortedObservableCollection(IEnumerable<T> items, IComparer<T>? comparer = null)
	{
		_comparer = comparer ?? new SqlTreeItemComparer();
		foreach (var item in items)
		{
			Add(item);
		}
	}

	protected void InsertItem(int index, T item, MergeConflictResolverDelegate resolver)
	{
		var itemToAdd = item;
		
		if (this.SingleOrDefault(x => x.Name == item.Name) is { } existingItem) 
		{
			// Merge conflict! Any item referred to by a URN must have a unique name within this node.
			itemToAdd = resolver(existingItem, itemToAdd);
		}
		
		
		for (var i = 0; i < Count; i++)
		{
			if (_comparer.Compare(itemToAdd, this[i]) < 0)
			{
				base.InsertItem(i, itemToAdd);
				return;
			}
		}
		base.InsertItem(Count, itemToAdd);
	}

	protected override void InsertItem(int index, T item) =>
		InsertItem(index, item, MergeConflictResolver);
	
	public static MergeConflictResolverDelegate DefaultResolver =>
		(pathToHere, oldItem, newItem) =>
		{
			Debug.Assert(oldItem.Name == newItem.Name);
			if (oldItem is IUrnPart oldUrn && newItem is IUrnPart newUrn)
			{
				var dialog = new MergeConflictResolutionDialog(pathToHere, oldUrn, newUrn);
				var result = dialog.ShowDialog();
				var (objectToUse, action) = dialog.Result;
				if (result is true)
				{
					
				}
			} 
			else if (oldItem is GroupSqlTreeItem oldGroup && newItem is GroupSqlTreeItem newGroup)
			{
				
			}
			else
			{
				throw new InvalidArgumentException($"Trying to merge a non-cosmetic SQL object with a cosmetic group.");
			}
		};
}