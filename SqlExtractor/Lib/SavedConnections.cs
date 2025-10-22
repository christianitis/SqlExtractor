using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SqlExtractor.Lib;

// TODO this is really slow
public static class SavedConnections
{
	private const string ConfigFileName = "connections.json";
	private const string ConnectionsSection = "RecentConnections";
	private const int MaxConnectionsToStore = 10;

	private static IConfiguration GetConfiguration()
	{
		return new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile(ConfigFileName, optional: true, reloadOnChange: true)
			.Build();
	}

	public static HashSet<string?> GetAllConnectionStrings()
	{
		var config = GetConfiguration();
		return config.GetSection(ConnectionsSection).AsEnumerable()
			.Where(it => !it.Value.IsNullOrEmpty())
			.Select(it => it.Value)
			.ToHashSet();
	}

	public static IEnumerable<SqlConnectionStringBuilder> AsBuilders(this HashSet<string> connectionStrings)
	{
		return connectionStrings.Select(it => new SqlConnectionStringBuilder(it));
	}

	public static string? GetLastUsedConnectionString()
	{
		return GetAllConnectionStrings().FirstOrDefault();
	}

	public static void SaveConnectionString(string connectionString)
	{
		if (connectionString.IsNullOrEmpty()) return;

		// Get existing connections
		var connections = GetAllConnectionStrings().ToList();

		// Remove this connection if it already exists to avoid duplicates
		connections.Remove(connectionString);

		// Add the new connection at the beginning (most recent)
		connections.Insert(0, connectionString);

		// Limit the number of stored connections
		if (connections.Count > MaxConnectionsToStore)
		{
			connections = connections.Take(MaxConnectionsToStore).ToList();
		}

		// Create configuration structure
		var configJson = new
		{
			RecentConnections = connections
		};

		// Save to JSON file
		var json = System.Text.Json.JsonSerializer.Serialize(configJson,
			new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(ConfigFileName, json);
	}
}