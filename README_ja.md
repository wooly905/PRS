# PRS (Portable Relational Schema)

[English](README.md) | [台灣中文](README_tw.md) | [日本語](README_ja.md)

PRS は、SQL Server データベースと開発者（または AI エージェント）の間のギャップを埋めるために設計された強力なツールです。データベーススキーマをローカルの人間が読める形式の Markdown ファイルにダンプすることができます。これらのスキーマは、豊富なコマンドラインインターフェース（CLI）を介して照会したり、Model Context Protocol (MCP) サーバーを介して AI コーディングアシスタント（Cursor、Claude Desktop、Windsurf など）に公開したりできます。

## 特徴

- **ローカルファースト**: データベーススキーマは、ローカルの `%APPDATA%\.prs\schemas\` に Markdown ファイルとして保存されます。
- **AI 強化**: 本番データベースを公開することなく、LLM に構造化されたデータベースコンテキストを提供します。
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

- **テーブルを検索**: `prs ft Order`
- **カラムを検索**: `prs fc Email`
- **テーブルの詳細を表示**: `prs sc Users`
- **ストアドプロシージャを検索**: `prs fsp Search`

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

## プロジェクト構造

- `src/`: コアロジックと CLI ツール。
- `src-mcp/`: MCP サーバーの実装。
- `tests/`: すべてのコンポーネントに対する広範なテストスイート。

## 開発とテスト

包括的なテストにより、高いコード品質を維持しています。

```bash
dotnet test
```

貢献の詳細は、[CONTRIBUTING.md](CONTRIBUTING.md) を参照してください。

## ライセンス

MIT ライセンスの下で配布されています。詳細は `LICENSE` を参照してください。
