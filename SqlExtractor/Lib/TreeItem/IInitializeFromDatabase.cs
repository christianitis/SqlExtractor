using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace SqlExtractor.Lib.TreeItem;

/// <summary>
/// Classes which implement this interface can have their properties initialized from a database.
/// </summary>
public interface IInitializeFromDatabase<in T> where T : SqlSmoObject
{
	public void InitializeFromDatabase(T smoObject);
	public void InitializeFromDatabaseWithChildren(T smoObject);

	/// <summary>
	/// The URN of the object as it was when it was first retrieved from the database.
	/// </summary>
	/// <remarks>
	/// If you were expecting a value but got null, you probably haven't called <see cref="InitializeFromDatabase"/> yet.
	/// If you move this object around in a tree, this URN might not reflect its actual position in the tree.
	/// </remarks>
	public Urn OriginalUrn { get; }
}