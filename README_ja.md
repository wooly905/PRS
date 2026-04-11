# PRS (Portable Relational Schema)

[English](README.md) | [台灣中文](README_tw.md) | [日本語](README_ja.md)

PRS は、SQL Server データベースと開発者（または AI エージェント）の間のギャップを埋めるために設計された強力なツールです。データベーススキーマをローカルの人間が読める形式の Markdown ファイルにダンプすることができます。これらのスキーマは、豊富なコマンドラインインターフェース（CLI）を介して照会したり、Model Context Protocol (MCP) サーバーを介して AI コーディングアシスタント（Cursor、Claude Desktop、Windsurf など）に公開したりできます。

## 特徴

- **ローカルファースト**: データベーススキーマは、ローカルの `%APPDATA%\.prs\schemas\` に Markdown ファイルとして保存されます。
- **AI 強化**: 本番データベースを公開することなく、LLM に構造化されたデータベースコンテキストを提供します。
- **マルチフォーマット出力**: クエリ結果を `table`、`ddl`、`json`、`text` 形式で出力可能。人間と LLM の両方に最適化されています。
- **豊富な CLI**: スキーマを検索および探索するための包括的なコマンドセット。
- **MCP サーバー**: Model Context Protocol をサポートするあらゆる AI ツールとのシームレスな統合。
- **高速かつ軽量**: 最小限の依存関係で .NET 10.0 上に構築されています。

## インストール

### 1. 前提条件

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Microsoft SQL Server へのアクセス

### 2. グローバルツールとしてインストール

PRS を .NET グローバルツールとしてインストールできます：

```bash
dotnet tool install wooly905.prs -g
```

または、ソースから直接実行します：

```bash
dotnet run --project src/PRS.csproj -- [command]
```

## クイックスタート

### 1. 接続の設定

SQL Server の接続文字列を設定します（これはローカルマシンに安全に保存されます）：

```bash
prs wcs "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;TrustServerCertificate=True"
```

### 2. データベーススキーマのダンプ

データベーススキーマをローカルの Markdown ファイルにエクスポートします：

```bash
prs dds my_db_name
```

### 3. CLI による探索

```
使用方法: prs <command> [arguments] [-f <format>]

セットアップ:
  scs                           保存済み接続文字列を表示
  wcs <connection-string>       接続文字列を保存
  dds <schema-name>             データベーススキーマをローカルファイルにダンプ

スキーマ管理:
  ls                            保存済みスキーマ一覧（* = アクティブ）
  use <schema-name>             アクティブスキーマを切り替え
  rm  <schema-name>             保存済みスキーマを削除

クエリ（部分一致、大文字小文字区別なし）:
  ft  <keyword>                 名前でテーブルを検索
  fc  <keyword>                 全テーブルから名前でカラムを検索
  ftc <table> <column>          一致するテーブル内のカラムを検索
  sc  <table-name>              テーブルの全カラムを表示（完全一致）
  fsp <keyword>                 名前でストアドプロシージャを検索
```

### 出力フォーマット

クエリコマンドは、出力形式を制御するオプションの `-f` フラグをサポートしています：

| フォーマット | フラグ | 対応コマンド | 説明 |
|---|---|---|---|
| `table` | `-f table` | 全クエリコマンド（デフォルト） | 罫線付きフォーマットテーブル |
| `json` | `-f json` | 全クエリコマンド | JSON 構造化形式 |
| `text` | `-f text` | 全クエリコマンド | プレーンテキスト形式 |
| `ddl` | `-f ddl` | `sc` のみ | SQL DDL（`CREATE TABLE`）文 |

### 使用例

- **テーブルを検索** (`ft`): 名前でテーブルまたはビューを検索します。
  ```bash
  prs ft Orders
  ```
  **実行結果:**
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

- **カラムを検索** (`fc`): データベース全体から特定のカラム名を検索します。
  ```bash
  prs fc OrderID
  ```
  **実行結果:**
  ```text
  ┌───────────────────────┬──────────────┬─────┬─────────┬──────────┬──────────┬─────┬────────┬────────────┬─────┬─────────────┬──────────────┐
  │ Table                 │ Column       │ Pos │ Default │ Nullable │ DataType │ PK  │ Unique │ Identity   │ FK  │ FK.Table    │ FK.Column    │
  ├───────────────────────┼──────────────┼─────┼─────────┼──────────┼──────────┼─────┼────────┼────────────┼─────┼─────────────┼──────────────┤
  │ OrderAddresses        │ OrderId      │ 2   │         │ NO       │ bigint   │ NO  │ NO     │ NO         │ YES │ Orders      │ OrderId      │
  │ OrderEmails           │ OrderId      │ 2   │         │ NO       │ int      │ NO  │ NO     │ NO         │ YES │ Orders      │ OrderId      │
  │ Orders                │ OrderId      │ 1   │         │ NO       │ bigint   │ YES │ YES    │ YES (1, 1) │ NO  │             │              │
  └───────────────────────┴──────────────┴─────┴─────────┴──────────┴──────────┴─────┴────────┴────────────┴─────┴─────────────┴──────────────┘
  ```

- **テーブル内のカラムを検索** (`ftc`): 特定のテーブル内でカラムを検索します。
  ```bash
  prs ftc Orders Total
  ```
  **実行結果:**
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

- **テーブルの詳細を表示** (`sc`): テーブルのすべてのカラムとその属性を表示します。
  ```bash
  prs sc Orders
  ```
  **実行結果 (一部):**
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

- **DDL 出力フォーマット**: SQL DDL 形式で結果を取得します。LLM に最適です。
  ```bash
  prs sc Orders -f ddl
  ```
  **実行結果:**
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

- **ストアドプロシージャを検索** (`fsp`): 名前でストアドプロシージャを検索します。
  ```bash
  prs fsp Search
  ```

## AI 統合 (MCP)

PRS には、AI エージェントがデータベーススキーマを「見る」ことを可能にする MCP サーバーが含まれています。

### Cursor / Claude Desktop の設定

`mcpServers` 設定に以下の構成を追加します：

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

設定が完了したら、AI に次のように尋ねるだけです：
- 「`Users` テーブルと `Orders` テーブルの関係を説明してください。」
- 「`Products` テーブルにはどのようなカラムがありますか？」
- 「ユーザー登録に関連するストアドプロシージャを見つけてください。」

MCP サーバーはデフォルトで **DDL 出力フォーマット** を使用します。これは LLM にとって最もトークン効率が高く、自然な表現方式です。MCP ツールの完全なドキュメントは [src-mcp/README.md](src-mcp/README.md) を参照してください。

### Claude Code スキル（スラッシュコマンド）

PRS には、Claude にデータベーススキーマを効果的にクエリする方法を教える 2 つの Claude Code スキルが付属しています。環境に合わせて選択してください：

| スキル | ファイル | Claude のスキーマクエリ方法 |
|---|---|---|
| **CLI** | [`query-schema-cli.md`](skills/claude-code/query-schema-cli.md) | ターミナルで `prs` コマンドを実行（`prs ft`、`prs sc -f ddl` など） |
| **MCP** | [`query-schema-mcp.md`](skills/claude-code/query-schema-mcp.md) | PRS MCP ツールを直接呼び出し（`find_table`、`get_table_schema` など） |

`prs` を dotnet グローバルツールとしてインストール済みの場合は **CLI 版** を使用してください。AI ツールに PRS MCP サーバーを設定済みの場合は **MCP 版** を使用してください。

インストール方法：

```bash
# CLI 版 — prs がインストールされている環境で動作
cp skills/claude-code/query-schema-cli.md /path/to/your-project/.claude/commands/

# MCP 版 — PRS MCP サーバーの設定が必要
cp skills/claude-code/query-schema-mcp.md /path/to/your-project/.claude/commands/
```

Claude Code で使用：

```
/query-schema-cli Show me all columns in the Orders table
/query-schema-mcp How are Users and Orders related?
```

両方のスキルは、Claude がアクティブスキーマの確認、DDL フォーマットによるスキーマ理解、外部キー関係の追跡、実際のスキーマ定義に基づいた SQL クエリの生成を行うよう導きます。

## プロジェクト構造

- `src/`: コアロジックと CLI ツール。
- `src-mcp/`: MCP サーバーの実装。
- `tests/`: すべてのコンポーネントに対する広範なテストスイート。

## 開発とテスト

包括的なテストにより、高いコード品質を維持しています。

```bash
dotnet test
```
