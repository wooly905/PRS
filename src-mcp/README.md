# PRS MCP Server

PRS MCP Server is a Model Context Protocol (MCP) server that exposes PRS core functionality to AI tools (such as Cursor and Claude Desktop), allowing these tools to query SQL Server database schema information through a standardized protocol.

## Features

PRS MCP Server provides the following tools:

### Search Tools

- **find_table**: Search for tables by keyword (partial match, case-insensitive)
- **find_column**: Search for columns by keyword (partial match, case-insensitive)
- **find_stored_procedure**: Search for stored procedures by keyword (partial match, case-insensitive)

### Schema Query Tools

- **get_table_schema**: Get complete structure information for a specified table, including all columns, data types, nullability, and foreign key relationships
- **list_schemas**: List all available schemas and show the currently active schema
- **use_schema**: Switch the currently active schema

### Output Format

Tools support an `output_format` parameter to control the response format:

| Format | Available for | Description |
|---|---|---|
| `json` | All search tools **(default)** | JSON structured format |
| `text` | All search tools | Human-readable plain text |
| `ddl` | `get_table_schema` only **(default)** | SQL DDL (`CREATE TABLE`) statement |

`get_table_schema` defaults to `ddl` because a CREATE TABLE statement is the most token-efficient and natural representation of a single table's schema for LLMs. Search tools (`find_table`, `find_column`, `find_stored_procedure`) default to `json` since their results span multiple objects where DDL is not applicable.

### Resources

- **Schema Resources**: Expose schema files as resources, allowing direct reading of schema content
  - URI format: `prs://schema/{schemaName}`

## Installation

### Prerequisites

- .NET 10.0 SDK or later
- PRS CLI tool installed and configured (for downloading schemas)

### Build

```bash
cd src-mcp
dotnet build
```

### Run

```bash
dotnet run --project src-mcp/PRS.McpServer.csproj
```

## Configuration

### Cursor Configuration

In Cursor's configuration file (usually located at `%APPDATA%\Cursor\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json` or similar location), add the following configuration:

```json
{
  "mcpServers": {
    "prs": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\PRS\\src-mcp\\PRS.McpServer.csproj"
      ]
    }
  }
}
```

Or, if you have already built the executable:

```json
{
  "mcpServers": {
    "prs": {
      "command": "C:\\path\\to\\PRS\\src-mcp\\bin\\Debug\\net10.0\\prs-mcp.exe"
    }
  }
}
```

### Claude Desktop Configuration

In Claude Desktop's configuration file (macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`, Windows: `%APPDATA%\Claude\claude_desktop_config.json`), add:

```json
{
  "mcpServers": {
    "prs": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\PRS\\src-mcp\\PRS.McpServer.csproj"
      ]
    }
  }
}
```

## Usage

### 1. Prepare Schema

First, use the PRS CLI tool to download the database schema:

```bash
prs wcs "Server=your-server;Database=your-db;User Id=user;Password=pass;"
prs dds mydatabase
```

### 2. Use in AI Tools

After configuration, you can use these tools directly in Cursor or Claude Desktop. For example:

- "Find tables containing 'user'"
- "List all available schemas"
- "Switch to mydatabase schema"
- "Show the complete structure of the Users table"
- "Find all columns containing 'email'"

## Tool Details

### find_table

Search for tables whose names contain the specified keyword.

**Parameters**:
- `keyword` (string, required): The keyword to search for
- `output_format` (string, optional): Output format — `json` (default) or `text`

**Returns**: List of matching tables, including schema, name, and type

### find_column

Search for columns whose names contain the specified keyword across all tables.

**Parameters**:
- `keyword` (string, required): The keyword to search for
- `output_format` (string, optional): Output format — `json` (default) or `text`

**Returns**: List of matching columns, including table, column name, data type, and foreign key information

### find_stored_procedure

Search for stored procedures whose names contain the specified keyword.

**Parameters**:
- `keyword` (string, required): The keyword to search for
- `output_format` (string, optional): Output format — `json` (default) or `text`

**Returns**: List of matching stored procedure names

### get_table_schema

Get complete structure information for a specified table.

**Parameters**:
- `tableName` (string, required): Table name
- `schema` (string, optional): Schema name (e.g., 'dbo'). If not provided, will search in all schemas
- `output_format` (string, optional): Output format — `ddl` (default), `json`, or `text`

**Returns**: Complete table structure, including detailed information for all columns

**Example (DDL, default)**:
```sql
CREATE TABLE dbo.Users (
    UserId int NOT NULL IDENTITY(1,1),
    Email nvarchar(255) NOT NULL,
    DisplayName nvarchar(100) NULL,
    DepartmentId int NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT FK_Users_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId)
);
```

**Example (JSON)**:
```json
{
  "tableName": "Users",
  "schema": "dbo",
  "found": true,
  "columns": [
    {
      "name": "UserId",
      "dataType": "int",
      "isNullable": false,
      "ordinalPosition": 1,
      "isPrimaryKey": true,
      "isIdentity": true
    }
  ]
}
```

### list_schemas

List all available schemas and show the currently active schema.

**Parameters**: None

**Returns**: Schema list and the currently active schema

### use_schema

Switch the currently active schema. Subsequent queries will use this schema.

**Parameters**:
- `schemaName` (string, required): Schema name (without .schema.md extension)

**Returns**: Switch result and the new active schema name

## Claude Code Skill

PRS includes two ready-made Claude Code skills. For MCP server users, use the MCP variant at [`skills/claude-code/query-schema-mcp.md`](../skills/claude-code/query-schema-mcp.md):

```bash
cp skills/claude-code/query-schema-mcp.md /path/to/your-project/.claude/commands/
```

A CLI variant ([`query-schema-cli.md`](../skills/claude-code/query-schema-cli.md)) is also available for users who have `prs` installed as a dotnet global tool but don't use the MCP server.

Both skills teach Claude to verify the active schema first, use DDL format for understanding, trace FK relationships, and ground SQL queries in actual schema definitions.

```
/query-schema-mcp Show me all columns in the Orders table
/query-schema-cli How are Users and Orders related?
```

## Notes

1. **Schema File Location**: Schema files are stored in the `%APPDATA%\.prs\schemas\` directory
2. **Active Schema**: All query tools use the currently active schema. Use the `use_schema` tool to switch the active schema
3. **Output Format**: The default `ddl` format is optimized for LLM consumption. Use `json` when you need structured data for programmatic processing
4. **Error Handling**: If a schema file does not exist or a query fails, the tool will return an appropriate error message

## Troubleshooting

### MCP Server Cannot Start

- Verify that .NET 10.0 SDK is installed
- Verify that the PRS project is built correctly
- Check that paths are correct

### Schema Not Found

- Verify that you have used the `prs dds` command to download the schema
- Check that the `%APPDATA%\.prs\schemas\` directory exists and contains schema files
- Use the `list_schemas` tool to verify available schemas

### Tool Execution Errors

- Verify that the currently active schema exists
- Check if the schema file is corrupted
- Review detailed information in error messages

## Development

### Project Structure

```
src-mcp/
├── PRS.McpServer.csproj    # Project file
├── Program.cs              # Main program
├── Mcp/
│   ├── McpServer.cs        # MCP protocol core
│   ├── IMcpTool.cs         # Tool interface
│   ├── Tools/              # MCP Tools
│   │   └── OutputFormatter.cs  # Format routing (DDL/JSON/Text)
│   └── Resources/          # MCP Resources
└── Services/
    └── SchemaService.cs    # Schema service
```

### Adding New Tools

1. Create a new Tool class implementing the `IMcpTool` interface
2. Include `output_format` parameter support via `OutputFormatter.ParseMcpFormat()`
3. Register the new tool in `Program.cs`
4. Update this README file
