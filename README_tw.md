# PRS (Portable Relational Schema)

[English](README.md) | [台灣中文](README_tw.md) | [日本語](README_ja.md)

PRS 是一個強大的 dotnet CLI 工具，旨在彌合 SQL Server 資料庫與開發人員（或 AI 代理）之間的鴻溝。它允許您將資料庫架構（Schema）匯出到本地且易於閱讀的 Markdown 檔案。接著，您可以透過豐富的命令列介面（CLI）查詢這些架構，或者透過模型內容協定（Model Context Protocol, MCP）伺服器將其開放給 AI 編碼助理（如 Cursor、Claude Desktop 或 Windsurf）。

## 特色

- **本地優先**：您的資料庫架構會以 Markdown 檔案的形式儲存在本地的 `%APPDATA%\.prs\schemas\` 目錄下。
- **AI 增強**：在不公開生產環境資料庫的情況下，為 LLM 提供結構化的資料庫上下文。
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

使用直觀的指令探索您的資料庫架構。以下是以 `Orders` 表格為例的範例：

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
  # prs ftc [表格名稱] [欄位名稱關鍵字]
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

## 專案結構

- `src/`：核心邏輯與 CLI 工具。
- `src-mcp/`：MCP 伺服器實作。
- `tests/`：各個組件的廣泛測試套件。

## 開發與測試

我們透過全面的測試維持高程式碼品質。

```bash
dotnet test
```

有關貢獻的更多詳細資訊，請參閱 [CONTRIBUTING.md](CONTRIBUTING.md)。

## 授權

根據 MIT 授權條款分發。詳情請參閱 `LICENSE`。
