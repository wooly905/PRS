namespace PRS;

/// <summary>
/// Defines the output format for schema query results.
/// </summary>
public enum OutputFormat
{
    /// <summary>Spectre.Console table format (CLI default)</summary>
    Table,

    /// <summary>SQL DDL (CREATE TABLE) format (MCP default, best for LLMs)</summary>
    Ddl,

    /// <summary>JSON format</summary>
    Json,

    /// <summary>Human-readable plain text format</summary>
    Text
}
