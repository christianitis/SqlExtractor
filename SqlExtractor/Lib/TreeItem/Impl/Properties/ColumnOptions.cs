using Microsoft.SqlServer.Management.Smo;

namespace SqlExtractor.Lib.TreeItem.Properties;

public record ColumnOptions(
	string Name,
	SqlDataType DataType,
	bool IsIdentity = false,
	bool Nullable = false,
	bool IsComputed = false,
	object? DefaultValue = null);