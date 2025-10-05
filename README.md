[![prs](https://github.com/wooly905/PRS/actions/workflows/build.yml/badge.svg)](https://github.com/wooly905/PRS/actions/workflows/build.yml)
[![NuGet latest version](https://badgen.net/nuget/v/wooly905.prs/latest)](https://nuget.org/packages/wooly905.prs)
[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://lbesson.mit-license.org/)

# PRS
A simple command-line tool to query database schema in Microsoft SQL Server

## Introduction:
This is a tool to help you quickly search the names of objects (columns, tables, stored procedure) in Microsoft SQL server.
This tool has been released as a dotnet global tool. You can install it by the following dotnet command,
>dotnet tool install wooly905.prs -g

The followings are the examples to show you how to use this tool.

The following screenshots show you the tables and columns created by Hangfire.

![pic1](https://user-images.githubusercontent.com/18693681/129477044-0f98c657-97c3-4d98-8540-1f3ee8f4fe2c.JPG)
![pic2](https://user-images.githubusercontent.com/18693681/129477114-e508e069-b4d7-4694-a07f-3eb24f10ad78.JPG)
![pic3](https://user-images.githubusercontent.com/18693681/129477117-53aeb080-3579-4664-8fce-cddeb5f9979c.JPG)

### How to use this tool:

```
Usage: prs [options] [argument]

Options:
scs     Show MS SQL Server connection string.
wcs     Write MS SQL Server connection string.
dds     Dump db schema to local machine.
ls      List saved schemas and show active one.
use     Switch active schema. prs use [schema name]
rm      Remove a schema. prs rm [schema name]
ft      Find table(s) (view).
fc      Find column(s).
ftc     Find column(s) in some table (view).
fsp     Find stored procedure.
sc      Show all columns in a table.
erd     Show ERD around a table. prs erd [table name]
ai      Generate T-SQL from natural language: prs ai "question..."
lu      Set LLM endpoint URL. prs lu "https://.../openai/v1"
lk      Set LLM API key. prs lk your_api_key
slu     Show current LLM endpoint URL.
slk     Show current LLM API key.
```

### Setup connection string (prs wcs)

This is the first thing to do for this tool. Make sure your connection string is good with correct host URL, database name, username, and password.

![pic5](https://user-images.githubusercontent.com/18693681/129477397-7de49c08-5844-4938-a73c-bf93377af9c4.JPG)

### Show connection string (prs scs)

![pic6](https://user-images.githubusercontent.com/18693681/129477454-07529fde-7f86-49bc-83fd-573251a509d4.JPG)

### Dump schema to local machine (prs dds [schema name])

This command dumps the database schema into your local user folder.
Schema name is required and determines the saved file name.

Examples:

> prs dds database1

Notes:
- Schemas are saved under your user profile: %APPDATA%\.prs\schemas
- The saved file is <schema name>.schema.txt and becomes the active schema after dump.
- All query commands use the active schema.

![pic7](https://user-images.githubusercontent.com/18693681/129477524-83185aa4-871d-47c6-beed-0cf169d168bc.JPG)

### Find table (prs ft) - table name can be *partial*

When running any query command (ft, fc, ftc, fsp, sc), PRS prints the active schema in use, e.g., “Using schema: database1.schema.txt”.

<img width="482" height="185" alt="image" src="https://github.com/user-attachments/assets/fc952485-121c-4bc7-a539-a87894315d74" />

### Find column (prs fc) - find matched column in all tables

<img width="2206" height="191" alt="image" src="https://github.com/user-attachments/assets/9c410a77-3444-4156-b745-6cc2cffd5ad0" />

### Find column in table(s) (prs ftc)

<img width="1702" height="173" alt="image" src="https://github.com/user-attachments/assets/68c28620-32ef-4d6a-98f1-3d3cc39dd0f3" />

### Display all columns of a table (shows FK info)

<img width="1944" height="322" alt="image" src="https://github.com/user-attachments/assets/5883af1c-943e-4e0d-99a8-d5eafd9a275c" />

### Find stored procedure

> prs fsp [full or partial name of a storec procedure to find]

## Manage multiple schemas

PRS supports storing multiple schemas locally and switching between them.

- List saved schemas and show the active one

> prs ls

- Switch active schema (by schema name, not file name)

> prs use [schema name]

- Remove a saved schema (by schema name)

> prs rm [schema name]

Notes:
- Query commands always read from the active schema.
- The active schema pointer is stored under %APPDATA%\.prs\active.txt.

## Display ERD around a table

> prs erd [table name]
- Shows you the tables which have relations to the table that you enter.

<img width="716" height="812" alt="image" src="https://github.com/user-attachments/assets/f7e1c94f-8b43-4dde-8a65-b3ac61d96bfb" />

## AI-assisted SQL generation

PRS can help you generate T-SQL from natural language, restricted to your active schema.

### Configure LLM

- Set endpoint URL

> prs lu "https://your-endpoint/openai/v1"

- Set API key

> prs lk your_api_key

Note:
- They are shared across schemas and unaffected by switching the active schema.

### Show current LLM settings

> prs slu

> prs slk

### Generate SQL from a question

> prs ai "show me the top 10 orders in 2024 with total amount"

Behavior:
- Extracts entities/keywords from your question
- Searches your active schema context to constrain allowed tables/columns
- Generates a single T-SQL statement using only the active schema
- Performs a basic validation pass and attempts one correction if needed

