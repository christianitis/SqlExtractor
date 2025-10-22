using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace SqlExtractor.Lib.TreeItem;

/// <summary>
/// Classes which implement this interface should show up in a properly formatted SMO URN.
/// </summary>
public interface IUrnPart
{
	/// <summary>
	/// This is what SMO calls this type of object in a URN.
	/// </summary>
	/// <example>For a tree item representing a stored procedure, this would be <c>StoredProcedure</c>.</example>
	public string UrnTypeName { get; }
	
	public string Name { get; }

	public record struct UrnAttribute(string Name, string Value, UrnAttribute.Relation? RelationToNext) 
	{
		public enum Relation
		{
			and, or
		}
	}
	/// <summary>
	/// Returns a linked list containing the URN query attributes required to uniquely identify this object in the tree.
	/// </summary>
	/// <param name="path">The path to this object in the tree. May or may not include <c>this</c> as the last item.</param>
	/// <example>For a tree item representing a stored procedure, this would be <c>Name</c> and <c>Schema</c>.</example>
	/// <returns>A linked list of URN query attributes. Attribute names will not contain a leading @.</returns>
	public LinkedList<UrnAttribute> GetAttributes(IEnumerable<AbstractSqlTreeItem> path) => GetAttributes();
	
	/// <summary>
	/// Returns a linked list containing the URN query attributes required to uniquely identify this object in the tree.
	/// </summary>
	/// <param name="path">The path to this object in the tree. May or may not include <c>this</c> as the last item.</param>
	/// <remarks>This method is used when the path is not known. A best effort attempt is made to get as many attributes
	/// as possible, but without context; some important attributes might be omitted.</remarks>
	/// <example>For a tree item representing a stored procedure, this would be <c>Name</c>.</example>
	public LinkedList<UrnAttribute> GetAttributes() => new([new UrnAttribute("Name", Name, null)]);

	/// <summary>
	/// Returns a string containing the URN fragment for this object.
	/// </summary>
	/// <remarks>Does not contain delimiting forward slash.</remarks>
	/// <example>For a tree item representing a stored procedure, this would be <c>Name[@Name='MyStoredProc' and ]</c>.</example>
	public string GetUrnFragment(LinkedList<UrnAttribute> attributes)
	{
		var builder = new StringBuilder(UrnTypeName);
		builder.Append('[');
		var attribute = attributes.First;
		while (attribute?.Next is not null)
		{
			builder.Append($"@{attribute.Value.Name}='{attribute.Value.Value}'");
			if (attribute.Next is not null)
			{
				var relationToNext = attribute.Value.RelationToNext!;
				builder.Append($" {relationToNext} ");
			}
		}
		return builder.Append(']').ToString();
	}
}