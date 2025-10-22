using System.IO;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlExtractor.Lib.TreeItem;

/// <summary>
/// Objects which implement this interface can have its properties initialized from a SQL statement.
/// </summary>
public interface IInitializeFromSql
{
	public void InitializeFromSql(TSqlStatement sql);
}