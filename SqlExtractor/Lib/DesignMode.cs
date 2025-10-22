using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace SqlExtractor.Lib;

public static class DesignMode
{
	public static Lazy<ServerConnection> DesignModeConnection = new(() =>
	{
		var connection = new ServerConnection() { ServerVersion = new ServerVersion(15, 0), TrueName="designMode" };
		
		// TODO: Is this part truly necessary?
		var server = new Server(connection);
		(server as ISfcHasConnection).ConnectionContext.Mode = SfcConnectionContextMode.Offline;

		return connection;
	});
}