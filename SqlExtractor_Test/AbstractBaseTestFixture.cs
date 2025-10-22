using Microsoft.SqlServer.Management.Common;

namespace SqlExtractor_Test;

/// <summary>
/// Contains some helper methods for tests.
/// </summary>
public abstract class AbstractBaseTestFixture
{
	public string Timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

	public ServerConnection ServerA = new()
		{ ConnectionString = TestContext.Parameters["ServerA"] };

	public ServerConnection ServerB = new()
		{ ConnectionString = TestContext.Parameters["ServerB"] };
}