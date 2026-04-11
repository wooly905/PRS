# PRS MCP Server

PRS MCP Server 是一個 Model Context Protocol (MCP) 伺服器，將 PRS 的核心功能公開給 AI 工具（如 Cursor、Claude Desktop），讓這些工具可以透過標準化協議查詢 SQL Server 資料庫 schema 資訊。

## 功能

PRS MCP Server 提供以下工具：

### 搜尋工具

- **find_table**: 根據關鍵字搜尋表格（部分匹配，不區分大小寫）
- **find_column**: 根據關鍵字搜尋欄位（部分匹配，不區分大小寫）
- **find_stored_procedure**: 根據關鍵字搜尋預存程序（部分匹配，不區分大小寫）

### Schema 查詢工具

- **get_table_schema**: 取得指定表格的完整結構資訊，包含所有欄位、資料型別、可為空性和外鍵關係
- **list_schemas**: 列出所有可用的 schema 並顯示當前活動的 schema
- **use_schema**: 切換當前活動的 schema

### 輸出格式

工具支援 `output_format` 參數來控制回應格式：

| 格式 | 適用工具 | 說明 |
|---|---|---|
| `json` | 所有搜尋工具 **（預設）** | JSON 結構化格式 |
| `text` | 所有搜尋工具 | 人類可讀的純文字格式 |
| `ddl` | 僅 `get_table_schema` **（預設）** | SQL DDL（`CREATE TABLE`）語句 |

`get_table_schema` 預設為 `ddl`，因為 CREATE TABLE 語句是 LLM 理解單一表格 schema 最省 token 且最自然的表示方式。搜尋工具（`find_table`、`find_column`、`find_stored_procedure`）預設為 `json`，因為其結果橫跨多個物件，DDL 不適用。

### 資源

- **Schema Resources**: 將 schema 檔案作為資源公開，允許直接讀取 schema 內容
  - URI 格式: `prs://schema/{schemaName}`

## 安裝

### 前置需求

- .NET 10.0 SDK 或更新版本
- 已安裝並設定 PRS CLI 工具（用於下載 schema）

### 建置

```bash
cd src-mcp
dotnet build
```

### 執行

```bash
dotnet run --project src-mcp/PRS.McpServer.csproj
```

## 設定

### Cursor 設定

在 Cursor 的設定檔中（通常位於 `%APPDATA%\Cursor\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json` 或類似位置），加入以下設定：

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

或者，如果您已經建置了可執行檔：

```json
{
  "mcpServers": {
    "prs": {
      "command": "C:\\path\\to\\PRS\\src-mcp\\bin\\Debug\\net10.0\\prs-mcp.exe"
    }
  }
}
```

### Claude Desktop 設定

在 Claude Desktop 的設定檔中（macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`, Windows: `%APPDATA%\Claude\claude_desktop_config.json`），加入：

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

## 使用方式

### 1. 準備 Schema

首先，使用 PRS CLI 工具下載資料庫 schema：

```bash
prs wcs "Server=your-server;Database=your-db;User Id=user;Password=pass;"
prs dds mydatabase
```

### 2. 在 AI 工具中使用

設定完成後，您可以在 Cursor 或 Claude Desktop 中直接使用這些工具。例如：

- "幫我找包含 'user' 的表格"
- "列出所有可用的 schemas"
- "切換到 mydatabase schema"
- "顯示 Users 表格的完整結構"
- "找出所有包含 'email' 的欄位"

## 工具詳細說明

### find_table

搜尋表格名稱中包含指定關鍵字的表格。

**參數**:
- `keyword` (string, 必填): 要搜尋的關鍵字
- `output_format` (string, 選填): 輸出格式 — `json`（預設）或 `text`

**傳回**: 匹配的表格列表，包含 schema、名稱和類型

### find_column

搜尋欄位名稱中包含指定關鍵字的欄位，會搜尋所有表格。

**參數**:
- `keyword` (string, 必填): 要搜尋的關鍵字
- `output_format` (string, 選填): 輸出格式 — `json`（預設）或 `text`

**傳回**: 匹配的欄位列表，包含表格、欄位名稱、資料型別和外鍵資訊

### find_stored_procedure

搜尋預存程序名稱中包含指定關鍵字的預存程序。

**參數**:
- `keyword` (string, 必填): 要搜尋的關鍵字
- `output_format` (string, 選填): 輸出格式 — `json`（預設）或 `text`

**傳回**: 匹配的預存程序名稱列表

### get_table_schema

取得指定表格的完整結構資訊。

**參數**:
- `tableName` (string, 必填): 表格名稱
- `schema` (string, 選填): Schema 名稱（例如 'dbo'）。如果不提供，會在所有 schemas 中搜尋
- `output_format` (string, 選填): 輸出格式 — `ddl`（預設）、`json` 或 `text`

**傳回**: 表格的完整結構，包含所有欄位的詳細資訊

**範例（DDL，預設）**:
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

**範例（JSON）**:
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

列出所有可用的 schema 並顯示當前活動的 schema。

**參數**: 無

**傳回**: Schema 列表和當前活動的 schema

### use_schema

切換當前活動的 schema。後續的查詢會使用這個 schema。

**參數**:
- `schemaName` (string, 必填): Schema 名稱（不含 .schema.md 副檔名）

**傳回**: 切換結果和新的活動 schema 名稱

## Claude Code Skill

PRS 附帶兩個現成的 Claude Code skill。MCP 伺服器使用者請使用 MCP 版本，位於 [`skills/claude-code/query-schema-mcp.md`](../skills/claude-code/query-schema-mcp.md)：

```bash
cp skills/claude-code/query-schema-mcp.md /path/to/your-project/.claude/commands/
```

另有 CLI 版本（[`query-schema-cli.md`](../skills/claude-code/query-schema-cli.md)），適合已安裝 `prs` 為 dotnet 全域工具但未使用 MCP 伺服器的使用者。

兩個 skill 都會教導 Claude 先驗證活動 schema、使用 DDL 格式理解架構、追蹤外鍵關係，並基於實際 schema 定義生成 SQL 查詢。

```
/query-schema-mcp 顯示 Orders 表格的所有欄位
/query-schema-cli Users 和 Orders 之間有什麼關係？
```

## 注意事項

1. **Schema 檔案位置**: Schema 檔案儲存在 `%APPDATA%\.prs\schemas\` 目錄下
2. **活動 Schema**: 所有查詢工具都會使用當前活動的 schema。使用 `use_schema` 工具可以切換活動的 schema
3. **輸出格式**: 預設的 `ddl` 格式針對 LLM 使用進行最佳化。當您需要結構化資料進行程式化處理時，請使用 `json`
4. **錯誤處理**: 如果 schema 檔案不存在或查詢失敗，工具會傳回適當的錯誤訊息

## 疑難排解

### MCP Server 無法啟動

- 確認已安裝 .NET 10.0 SDK
- 確認 PRS 專案已正確建置
- 檢查路徑是否正確

### 找不到 Schema

- 確認已使用 `prs dds` 命令下載 schema
- 檢查 `%APPDATA%\.prs\schemas\` 目錄是否存在且包含 schema 檔案
- 使用 `list_schemas` 工具確認可用的 schemas

### 工具執行錯誤

- 確認當前活動的 schema 存在
- 檢查 schema 檔案是否損壞
- 查看錯誤訊息中的詳細資訊

## 開發

### 專案結構

```
src-mcp/
├── PRS.McpServer.csproj    # 專案檔
├── Program.cs              # 主程式
├── Mcp/
│   ├── McpServer.cs        # MCP 協議核心
│   ├── IMcpTool.cs         # Tool 介面
│   ├── Tools/              # MCP Tools
│   │   └── OutputFormatter.cs  # 格式路由（DDL/JSON/Text）
│   └── Resources/          # MCP Resources
└── Services/
    └── SchemaService.cs    # Schema 服務
```

### 新增工具

1. 建立新的 Tool 類別，實作 `IMcpTool` 介面
2. 透過 `OutputFormatter.ParseMcpFormat()` 加入 `output_format` 參數支援
3. 在 `Program.cs` 中註冊新工具
4. 更新此 README 文件
