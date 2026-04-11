# PRS (Portable Relational Schema)

[English](README.md) | [台灣中文](README_tw.md) | [日本語](README_ja.md)

PRS 是一個強大的 dotnet CLI 工具，旨在彌合 SQL Server 資料庫與開發人員（或 AI 代理）之間的鴻溝。它允許您將資料庫架構（Schema）匯出到本地且易於閱讀的 Markdown 檔案。接著，您可以透過豐富的命令列介面（CLI）查詢這些架構，或者透過模型內容協定（Model Context Protocol, MCP）伺服器將其開放給 AI 編碼助理（如 Cursor、Claude Desktop 或 Windsurf）。

## 特色

- **本地優先**：您的資料庫架構會以 Markdown 檔案的形式儲存在本地的 `%APPDATA%\.prs\schemas\` 目錄下。
- **AI 增強**：在不公開生產環境資料庫的情況下，為 LLM 提供結構化的資料庫上下文。
- **多格式輸出**：查詢結果可以 `table`、`ddl`、`json` 或 `text` 格式輸出，同時最佳化人類與 LLM 的閱讀體驗。
- **豐富的 CLI**：提供一套完整的命令，用於搜尋和探索您的架構。
- **MCP 伺服器**：與任何支援模型內容協定（MCP）的 AI 工具無縫整合。
- **快速輕量**：基於 .NET 10.0 構建，依賴最小化。

## 安裝

### 1. 前置需求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- 可存取 Microsoft SQL Server

### 2. 安裝為全域工具

您可以直接將 PRS 安裝為 .NET 全域工具：

```bash
dotnet tool install wooly905.prs -g
```

或者直接從程式碼執行：

```bash
dotnet run --project src/PRS.csproj -- [command]
```

## 快速上手

### 1. 設定連線

設定您的 SQL Server 連線字串（這會安全地儲存在您的本地機器上）：

```bash
prs wcs "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;TrustServerCertificate=True"
```

### 2. 匯出資料庫架構

將您的資料庫架構匯出到本地 Markdown 檔案：

```bash
prs dds my_db_name
```

### 3. 透過 CLI 探索

```
使用方式: prs <command> [arguments] [-f <format>]

設定:
  scs                           顯示已儲存的連線字串
  wcs <connection-string>       儲存連線字串
  dds <schema-name>             匯出資料庫架構到本地檔案

Schema 管理:
  ls                            列出所有已儲存的 schema（* = 使用中）
  use <schema-name>             切換使用中的 schema
  rm  <schema-name>             移除已儲存的 schema

查詢（部分匹配，不區分大小寫）:
  ft  <keyword>                 依名稱搜尋表格
  fc  <keyword>                 在所有表格中依名稱搜尋欄位
  ftc <table> <column>          在符合的表格中搜尋欄位
  sc  <table-name>              顯示表格的所有欄位（完全匹配）
  fsp <keyword>                 依名稱搜尋預存程序
```

### 輸出格式

查詢命令支援選用的 `-f` 旗標來控制輸出格式：

| 格式 | 旗標 | 適用命令 | 說明 |
|---|---|---|---|
| `table` | `-f table` | 所有查詢命令（預設） | 含邊框的格式化表格 |
| `json` | `-f json` | 所有查詢命令 | JSON 結構化格式 |
| `text` | `-f text` | 所有查詢命令 | 純文字格式 |
| `ddl` | `-f ddl` | 僅 `sc` | SQL DDL（`CREATE TABLE`）語句 |

### 範例

- **尋找表格** (`ft`)：按名稱搜尋表格或檢視表。
  ```bash
  prs ft Orders
  ```
  **輸出：**
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

- **尋找欄位** (`fc`)：在整個資料庫中搜尋特定欄位。
  ```bash
  prs fc OrderID
  ```
  **輸出：**
  ```text
  ┌───────────────────────┬──────────────┬─────┬─────────┬──────────┬──────────┬─────┬────────┬────────────┬─────┬─────────────┬──────────────┐
  │ Table                 │ Column       │ Pos │ Default │ Nullable │ DataType │ PK  │ Unique │ Identity   │ FK  │ FK.Table    │ FK.Column    │
  ├───────────────────────┼──────────────┼─────┼─────────┼──────────┼──────────┼─────┼────────┼────────────┼─────┼─────────────┼──────────────┤
  │ OrderAddresses        │ OrderId      │ 2   │         │ NO       │ bigint   │ NO  │ NO     │ NO         │ YES │ Orders      │ OrderId      │
  │ OrderEmails           │ OrderId      │ 2   │         │ NO       │ int      │ NO  │ NO     │ NO         │ YES │ Orders      │ OrderId      │
  │ Orders                │ OrderId      │ 1   │         │ NO       │ bigint   │ YES │ YES    │ YES (1, 1) │ NO  │             │              │
  └───────────────────────┴──────────────┴─────┴─────────┴──────────┴──────────┴─────┴────────┴────────────┴─────┴─────────────┴──────────────┘
  ```

- **搜尋表格內的欄位** (`ftc`)：在單一表格中搜尋特定欄位。
  ```bash
  prs ftc Orders Total
  ```
  **輸出：**
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

- **顯示表格詳情** (`sc`)：列出表格的所有欄位及其屬性。
  ```bash
  prs sc Orders
  ```
  **輸出 (部分)：**
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

- **DDL 輸出格式**：以 SQL DDL 格式取得結果，最適合 LLM 使用。
  ```bash
  prs sc Orders -f ddl
  ```
  **輸出：**
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

- **尋找預存程序** (`fsp`)：按名稱搜尋預存程序。
  ```bash
  prs fsp Search
  ```

## AI 整合 (MCP)

PRS 包含一個 MCP 伺服器，允許 AI 代理「看見」您的資料庫架構。

### 設定 Cursor / Claude Desktop

將此設定加入到您的 `mcpServers` 設定中：

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

設定完成後，您只需詢問您的 AI：
- 「解釋 `Users` 和 `Orders` 表格之間的關係。」
- 「`Products` 表格中有哪些欄位？」
- 「尋找任何與使用者註冊相關的預存程序。」

MCP 伺服器預設使用 **DDL 輸出格式**，這是對 LLM 來說最省 token 且最自然的表示方式。完整的 MCP 工具文件請參閱 [src-mcp/README.md](src-mcp/README.md)。

### Claude Code Skills（斜線指令）

PRS 附帶兩個現成的 Claude Code skill，教導 Claude 如何有效地查詢您的資料庫架構。請根據您的環境選擇：

| Skill | 檔案 | Claude 查詢 schema 的方式 |
|---|---|---|
| **CLI** | [`query-schema-cli.md`](skills/claude-code/query-schema-cli.md) | 在終端機執行 `prs` 命令（`prs ft`、`prs sc -f ddl` 等） |
| **MCP** | [`query-schema-mcp.md`](skills/claude-code/query-schema-mcp.md) | 直接呼叫 PRS MCP 工具（`find_table`、`get_table_schema` 等） |

如果您已安裝 `prs` 為 dotnet 全域工具，請使用 **CLI 版本**。如果您已在 AI 工具中設定 PRS MCP 伺服器，請使用 **MCP 版本**。

安裝方式：

```bash
# CLI 版本 — 任何安裝了 prs 的環境皆可使用
cp skills/claude-code/query-schema-cli.md /path/to/your-project/.claude/commands/

# MCP 版本 — 需要設定 PRS MCP 伺服器
cp skills/claude-code/query-schema-mcp.md /path/to/your-project/.claude/commands/
```

然後在 Claude Code 中使用：

```
/query-schema-cli 顯示 Orders 表格的所有欄位
/query-schema-mcp Users 和 Orders 之間有什麼關係？
```

兩個 skill 都會引導 Claude 驗證活動 schema、使用 DDL 格式理解架構、追蹤外鍵關係，並基於實際 schema 定義生成 SQL 查詢。

## 專案結構

- `src/`：核心邏輯與 CLI 工具。
- `src-mcp/`：MCP 伺服器實作。
- `tests/`：各個組件的廣泛測試套件。

## 開發與測試

我們透過全面的測試維持高程式碼品質。

```bash
dotnet test
```
