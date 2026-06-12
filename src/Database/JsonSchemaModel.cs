#nullable enable
using System.Text.Json.Serialization;

namespace PRS.Database;

public class JsonSchemaModel
{
    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; set; } = string.Empty;

    [JsonPropertyName("tables")]
    public List<JsonTableModel> Tables { get; set; } = new();

    [JsonPropertyName("storedProcedures")]
    public List<string> StoredProcedures { get; set; } = new();
}

public class JsonTableModel
{
    [JsonPropertyName("tableSchema")]
    public string TableSchema { get; set; } = string.Empty;

    [JsonPropertyName("tableName")]
    public string TableName { get; set; } = string.Empty;

    [JsonPropertyName("tableType")]
    public string TableType { get; set; } = string.Empty;

    [JsonPropertyName("columns")]
    public List<JsonColumnModel> Columns { get; set; } = new();
}

public class JsonColumnModel
{
    [JsonPropertyName("columnName")]
    public string ColumnName { get; set; } = string.Empty;

    [JsonPropertyName("ordinalPosition")]
    public string OrdinalPosition { get; set; } = string.Empty;

    [JsonPropertyName("columnDefault")]
    public string? ColumnDefault { get; set; }

    [JsonPropertyName("isNullable")]
    public string IsNullable { get; set; } = string.Empty;

    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    [JsonPropertyName("maxLength")]
    public string? CharacterMaximumLength { get; set; }

    [JsonPropertyName("isPrimaryKey")]
    public bool IsPrimaryKey { get; set; }

    [JsonPropertyName("isUnique")]
    public bool IsUnique { get; set; }

    // Identity info
    [JsonPropertyName("isIdentity")]
    public bool IsIdentity { get; set; }

    [JsonPropertyName("identitySeed")]
    public string? IdentitySeed { get; set; }

    [JsonPropertyName("identityIncrement")]
    public string? IdentityIncrement { get; set; }

    // Foreign key info (optional)
    [JsonPropertyName("referencedTableSchema")]
    public string? ReferencedTableSchema { get; set; }

    [JsonPropertyName("FKTable")]
    public string? ReferencedTableName { get; set; }

    [JsonPropertyName("FKColumn")]
    public string? ReferencedColumnName { get; set; }
}
