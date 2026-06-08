# PRS (Portable Relational Schema)

[English](README.md) | [台灣中文](README_tw.md) | [日本語](README_ja.md)

PRS is a powerful dotnet CLI tool designed to bridge the gap between SQL Server databases and developers (or AI agents). It allows you to dump database schemas into local, human-readable Markdown files. These schemas can then be queried through a rich Command-Line Interface (CLI).

## Features

- **Local First**: Your database schema is stored locally in `%APPDATA%\.prs\schemas\` as Markdown files.
- **AI-Enhanced**: Provide structured database context to LLMs without exposing your production database.
- **Multi-Format Output**: Query results in `table`, `ddl`, `json`, or `text` format — optimized for both humans and LLMs.
- **Rich CLI**: A comprehensive set of commands to search and explore your schemas.
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

```
Usage: prs <command> [arguments] [-f <format>]

Setup:
  scs                           Show saved connection string
  wcs <connection-string>       Save connection string
  dds <schema-name>             Dump database schema to local file

Schema Management:
  ls                            List all saved schemas (* = active)
  use <schema-name>             Switch active schema
  rm  <schema-name>             Remove a saved schema

Query (partial match, case-insensitive):
  ft  <keyword>                 Find tables by name
  fc  <keyword>                 Find columns by name across all tables
  ftc <table> <column>          Find columns in matching tables
  sc  <table-name>              Show all columns in a table (exact match)
  fsp <keyword>                 Find stored procedures by name
```

### Output Format

Query commands support an optional `-f` flag to control output format:

| Format | Flag | Available for | Description |
|---|---|---|---|
| `table` | `-f table` | All query commands (default) | Formatted table with borders |
| `json` | `-f json` | All query commands | JSON structured format |
| `text` | `-f text` | All query commands | Plain text format |
| `ddl` | `-f ddl` | `sc` only | SQL DDL (`CREATE TABLE`) statement |

### Examples

- **Find Tables** (`ft`): Search for tables or views by name.
  ```bash
  prs ft Orders
  ```
  **Output:**
  ```text
  ┌────────────────────┬────────────┐
  │ TableName          │ TableType  │
  ├────────────────────┼────────────┤
  │ Orders             │ BASE TABLE │
  │ OrdersTransactions │ BASE TABLE │
  │ PrintOrders        │ BASE TABLE │
  │ VDPOrders          │ BASE TABLE │
  └────────────────────┴────────────┘
  ```

- **Find Columns** (`fc`): Search for a specific column across the entire database.
  ```bash
  prs fc OrderID
  ```
  **Output:**
  ```text
  ┌───────────────────────┬──────────────┬─────┬─────────┬──────────┬──────────┬─────┬────────┬────────────┬─────┬─────────────┬──────────────┐
  │ Table                 │ Column       │ Pos │ Default │ Nullable │ DataType │ PK  │ Unique │ Identity   │ FK  │ FK.Table    │ FK.Column    │
  ├───────────────────────┼──────────────┼─────┼─────────┼──────────┼──────────┼─────┼────────┼────────────┼─────┼─────────────┼──────────────┤
  │ OrderAddresses        │ OrderId      │ 2   │         │ NO       │ bigint   │ NO  │ NO     │ NO         │ YES │ Orders      │ OrderId      │
  │ OrderEmails           │ OrderId      │ 2   │         │ NO       │ int      │ NO  │ NO     │ NO         │ YES │ Orders      │ OrderId      │
  │ Orders                │ OrderId      │ 1   │         │ NO       │ bigint   │ YES │ YES    │ YES (1, 1) │ NO  │             │              │
  └───────────────────────┴──────────────┴─────┴─────────┴──────────┴──────────┴─────┴────────┴────────────┴─────┴─────────────┴──────────────┘
  ```

- **Find Columns in Table** (`ftc`): Search for specific columns within a single table.
  ```bash
  prs ftc Orders Total
  ```
  **Output:**
  ```text
  ┌────────┬─────────────────┬─────┬─────────┬──────────┬──────────┬────┬────────┬──────────┬────┬──────────┬───────────┐
  │ Table  │ Column          │ Pos │ Default │ Nullable │ DataType │ PK │ Unique │ Identity │ FK │ FK.Table │ FK.Column │
  ├────────┼─────────────────┼─────┼─────────┼──────────┼──────────┼────┼────────┼──────────┼────┼──────────┼───────────┤
  │ Orders │ TotalItemsPrice │ 8   │         │ NO       │ money    │ NO │ NO     │ NO       │ NO │          │           │
  │ Orders │ TotalShipping   │ 9   │         │ NO       │ money    │ NO │ NO     │ NO       │ NO │          │           │
  │ Orders │ TotalTaxAmount  │ 23  │         │ NO       │ money    │ NO │ NO     │ NO       │ NO │          │           │
  │ Orders │ OrderTotal      │ 25  │         │ NO       │ money    │ NO │ NO     │ NO       │ NO │          │           │
  └────────┴─────────────────┴─────┴─────────┴──────────┴──────────┴────┴────────┴──────────┴────┴──────────┴───────────┘
  ```

- **Show Table Details** (`sc`): List all columns and their properties for a specific table.
  ```bash
  prs sc Orders
  ```
  **Output (Partial):**
  ```text
  ┌────────┬──────────────────┬─────┬─────────┬──────────┬─────────────┬─────┬────────┬────────────┬─────┬────────────────┬────────────────┐
  │ Table  │ Column           │ Pos │ Default │ Nullable │ DataType    │ PK  │ Unique │ Identity   │ FK  │ FK.Table       │ FK.Column      │
  ├────────┼──────────────────┼─────┼─────────┼──────────┼─────────────┼─────┼────────┼────────────┼─────┼────────────────┼────────────────┤
  │ Orders │ OrderId          │ 1   │         │ NO       │ bigint      │ YES │ YES    │ YES (1, 1) │ NO  │                │                │
  │ Orders │ Created          │ 6   │         │ NO       │ datetime    │ NO  │ NO     │ NO         │ NO  │                │                │
  │ Orders │ TotalTaxAmount   │ 23  │         │ NO       │ money       │ NO  │ NO     │ NO         │ NO  │                │                │
  │ Orders │ BillingAddressId │ 11  │         │ YES      │ bigint      │ NO  │ NO     │ NO         │ YES │ OrderAddresses │ OrderAddressId │
  └────────┴──────────────────┴─────┴─────────┴──────────┴─────────────┴─────┴────────┴────────────┴─────┴────────────────┴────────────────┘
  ```

- **DDL Output Format**: Get results in SQL DDL format, ideal for LLMs.
  ```bash
  prs sc Orders -f ddl
  ```
  **Output:**
  ```sql
  CREATE TABLE Orders (
      OrderId bigint NOT NULL IDENTITY(1,1),
      Created datetime NOT NULL,
      TotalTaxAmount money NOT NULL,
      BillingAddressId bigint NULL,
      CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
      CONSTRAINT FK_Orders_BillingAddressId FOREIGN KEY (BillingAddressId) REFERENCES OrderAddresses(OrderAddressId)
  );
  ```

- **Find Stored Procedures** (`fsp`): Search for stored procedures by name.
  ```bash
  prs fsp Search
  ```

### Claude Code Skills (Slash Commands)

PRS ships with a ready-made Claude Code skill that teaches Claude how to query your database schema effectively using the CLI:

Install:

```bash
cp skills/claude-code/query-schema-cli.md /path/to/your-project/.claude/commands/
```

Then use in Claude Code:

```
/query-schema-cli Show me all columns in the Orders table
```

This skill guides Claude to verify the active schema, use DDL format for understanding, trace foreign key relationships, and ground SQL queries in actual schema definitions.

## Project Structure

- `src/`: Core logic and CLI tool.
- `tests/`: Extensive test suite for all components.

## Development & Testing

We maintain high code quality with comprehensive tests.

```bash
dotnet test
```
