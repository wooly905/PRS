# PRS (Portable Relational Schema)

[English](README.md) | [台灣中文](README_tw.md) | [日本語](README_ja.md)

PRS is a powerful tool designed to bridge the gap between SQL Server databases and developers (or AI agents). It allows you to dump database schemas into local, human-readable Markdown files. These schemas can then be queried through a rich Command-Line Interface (CLI) or exposed to AI coding assistants (like Cursor, Claude Desktop, or Windsurf) via a Model Context Protocol (MCP) server.

## Features

- **Local First**: Your database schema is stored locally in `%APPDATA%\.prs\schemas\` as Markdown files.
- **AI-Enhanced**: Provide structured database context to LLMs without exposing your production database.
- **Rich CLI**: A comprehensive set of commands to search and explore your schemas.
- **MCP Server**: Seamless integration with any AI tool that supports the Model Context Protocol.
- **Fast & Lightweight**: Built on .NET 10.0 with minimal dependencies.

## Installation

### 1. Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Access to a Microsoft SQL Server

### 2. Install as a Global Tool

You can install PRS as a .NET global tool:

```bash
dotnet tool install wooly905.prs -g
```

Or run it directly from the source:

```bash
dotnet run --project src/PRS.csproj -- [command]
```

## Quick Start

### 1. Configure Connection

Set your SQL Server connection string (this is stored securely on your local machine):

```bash
prs wcs "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;TrustServerCertificate=True"
```

### 2. Dump Database Schema

Export your database schema to local Markdown files:

```bash
prs dds my_db_name
```

### 3. Explore via CLI

- **Find a table**: `prs ft Order`
- **Find a column**: `prs fc Email`
- **Show table details**: `prs sc Users`
- **Find stored procedures**: `prs fsp Search`

## AI Integration (MCP)

PRS includes an MCP server that allows AI agents to "see" your database schema.

### Configuring Cursor / Claude Desktop

Add this configuration to your `mcpServers` settings:

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

Once configured, you can simply ask your AI:
- "Explain the relationship between the `Users` and `Orders` tables."
- "What columns are in the `Products` table?"
- "Find any stored procedures related to user registration."

## Project Structure

- `src/`: Core logic and CLI tool.
- `src-mcp/`: MCP Server implementation.
- `tests/`: Extensive test suite for all components.

## Development & Testing

We maintain high code quality with comprehensive tests.

```bash
dotnet test
```

For more details on contributing, see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Distributed under the MIT License. See `LICENSE` for more information.
