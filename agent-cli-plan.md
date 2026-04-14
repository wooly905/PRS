# PRS 改寫計畫：同時支援人類與 LLM

## 現況盤點

PRS 目前的狀態，對照文章的 7 個面向：

| 面向 | 現況 | 狀態 |
|---|---|---|
| JSON 輸入 | 只接受位置參數（positional args） | ❌ 缺少 |
| Schema 內省 | 無描述命令，LLM 無法在執行時查詢可用命令 | ❌ 缺少 |
| Context Window 管理 | 輸出全部欄位，無法篩選 | ❌ 缺少 |
| 輸入驗證/防幻覺 | 只有基本的參數數量檢查 | ⚠️ 不足 |
| Skill Files | 已有 2 個 Claude Code skill files | ✅ 部分完成 |
| 多介面架構 | 已有 MCP server（src-mcp/） | ✅ 部分完成 |
| 安全護欄 | 無 `--dry-run`、無回應消毒 | ❌ 缺少 |

---

## 第一階段：統一 JSON 輸出（夯實基礎）

### 目的

LLM 靠結構化資料來推理。目前 `-f json` 已存在，但預設是 `table`（Spectre.Console 格式），LLM 呼叫時必須每次手動加 `-f json`。需要讓 PRS 能自動偵測呼叫情境並切換預設。

### 做法

**1.1 新增 `--output` / `-o` 全域旗標（與既有 `-f` 並存）**

在 `src/Commands/CommandHelper.cs` 的 `ParseOutputFormat` 方法中，除了現有的 `-f <format>`，額外支援 `--output <format>` 作為同義詞。這讓 LLM 生成指令時可以用更明確的長格式旗標。

**1.2 新增環境變數 `PRS_OUTPUT_FORMAT`**

在 `src/Commands/CommandHelper.cs` 的 `ParseOutputFormat` 中加入 fallback 邏輯：

```
優先順序：命令列 -f / --output > 環境變數 PRS_OUTPUT_FORMAT > 預設值 table
```

LLM 的 agent 框架可以在啟動時設定 `PRS_OUTPUT_FORMAT=json`，不需要每個指令都加旗標。

**1.3 所有輸出走 stdout，訊息走 stderr**

目前 `ShowInfo` 和 `ShowError` 都寫入 stdout（透過 `Console.WriteLine`）。LLM 需要 stdout 裡只有純資料。改法：

- `src/Display/LcdMonitor.cs` 中的 `ShowInfo()` / `ShowError()` 改寫為輸出到 `Console.Error`
- 結果資料繼續送 `Console.Out`
- 這樣 LLM 可以安全 parse stdout 而不會被混入的提示訊息干擾

---

## 第二階段：JSON 輸入模式

### 目的

文章的核心論點：LLM 生成 JSON 比生成 flags 更精確，零轉換損耗。PRS 目前只能用位置參數（`prs ft user`），LLM 需要打散解析參數語法。加入 `--json` 旗標讓 LLM 可以用一個 JSON 物件傳入所有參數。

### 做法

**2.1 定義每個命令的 JSON 輸入 schema**

在每個 `ICommand` 實作中，新增一個方法回傳該命令接受的 JSON schema。例如 `FindTableCommand` 接受：

```json
{
  "command": "ft",
  "keyword": "user",
  "format": "json"
}
```

`ShowColumnCommand` 接受：

```json
{
  "command": "sc",
  "tableName": "Orders",
  "format": "ddl",
  "fields": ["columnName", "dataType", "isPrimaryKey"]
}
```

**2.2 在 `src/Program.cs` 加入 `--json` 解析路徑**

偵測 `args[0] == "--json"` 時，讀取 `args[1]`（或 stdin）作為 JSON payload，解析出 `command` 欄位做分發，其餘欄位傳入對應 command。流程：

```bash
prs --json '{"command":"ft","keyword":"user","format":"json"}'
```

也支援 stdin pipe：

```bash
echo '{"command":"ft","keyword":"user"}' | prs --json -
```

**2.3 在 ICommand 介面加入 `RunAsync(JsonElement input)` 重載**

現有的 `RunAsync(string[] args)` 保留給人類使用。新增 `RunAsync(JsonElement input)` 讓 JSON 輸入直接進入 command，不需要再轉成 string array。

---

## 第三階段：Schema 內省命令（`describe`）

### 目的

文章強調：LLM 不應該靠靜態文件或 `--help` 學習工具用法，而是需要在執行時查詢機器可讀的 schema。這讓 LLM 可以動態發現命令、了解參數格式，不需要把整份文件塞進 system prompt。

### 做法

**3.1 新增 `describe` 命令**

```bash
# 列出所有可用命令及其 JSON schema
prs describe

# 查看單一命令的完整 schema
prs describe ft

# 輸出格式（預設 JSON）
prs describe ft -f json
prs describe ft -f text
```

**3.2 `describe` 的輸出格式**

`prs describe` 輸出所有命令的摘要：

```json
{
  "tool": "prs",
  "version": "10.1.0.6",
  "commands": [
    {
      "name": "ft",
      "description": "Find tables by name (partial match, case-insensitive)",
      "parameters": {
        "keyword": { "type": "string", "required": true, "description": "Partial table name to search" },
        "format": { "type": "string", "required": false, "enum": ["table","json","text"], "default": "table" }
      },
      "examples": [
        { "args": "prs ft user", "json": "{\"command\":\"ft\",\"keyword\":\"user\"}" }
      ]
    }
  ]
}
```

`prs describe ft` 輸出單一命令的詳細 schema，包含參數說明、型別、範例。

**3.3 實作方式**

- 在 `ICommand` 介面新增 `CommandMetadata GetMetadata()` 方法
- 每個 command 實作回傳自己的元資料（名稱、描述、參數 schema、範例）
- 新增 `DescribeCommand` 彙整所有已註冊命令的元資料
- `CommandProvider` 改為可列舉所有已註冊命令（目前只能 TryGet 單一命令）

---

## 第四階段：Context Window 管理（Field Masks）

### 目的

LLM 的 context window 有 token 上限。PRS 的 `sc` 命令會回傳一張表的所有欄位資訊，若表有 100+ 欄位，JSON 輸出會非常大。Field mask 讓 LLM 只取需要的欄位，大幅減少 token 消耗。

### 做法

**4.1 新增 `--fields` 旗標**

```bash
# 只取欄位名稱和資料型別
prs sc Orders --fields columnName,dataType

# 只取有 FK 的欄位
prs sc Orders --fields columnName,foreignKey

# JSON 模式
prs --json '{"command":"sc","tableName":"Orders","fields":["columnName","dataType","isPrimaryKey"]}'
```

**4.2 在 `src/Formatting/SchemaFormatter.cs` 實作欄位過濾**

- 新增 `FieldMask` 類別，定義可選欄位列表：`columnName`, `dataType`, `isNullable`, `ordinalPosition`, `columnDefault`, `isPrimaryKey`, `isUnique`, `isIdentity`, `foreignKey`, `characterMaximumLength`
- 在 JSON 格式化時，只序列化 field mask 中指定的欄位
- DDL 格式不支援 field mask（DDL 本身就是緊湊表示），給出明確訊息
- Text 格式根據 mask 決定要顯示哪些行

**4.3 `fc` / `ftc` 搜尋結果也支援 `--fields`**

跨表搜尋的結果同樣可以用 field mask 控制回傳的資訊量。

**4.4 結果計數（`--count`）**

新增 `--count` 旗標，只回傳符合條件的筆數而非完整資料。讓 LLM 先探查結果量再決定是否展開。

```bash
prs ft user --count
# 輸出: {"count": 12}
```

---

## 第五階段：輸入驗證強化（防禦幻覺）

### 目的

文章強調「Agent 會幻覺，必須像對待不可信的使用者輸入一樣做防禦」。PRS 目前的驗證只檢查參數數量，不檢查參數內容。LLM 可能生成包含路徑穿越、控制字元、或特殊符號的參數。

### 做法

**5.1 新增 `InputValidator` 靜態類別**

在 `src/` 下新增 `InputValidator.cs`，集中處理所有輸入消毒邏輯：

```csharp
public static class InputValidator
{
    // 拒絕控制字元（ASCII < 0x20，除了常見空白）
    public static bool ContainsControlCharacters(string input);

    // 拒絕路徑穿越模式（../, ..\）
    public static bool ContainsPathTraversal(string input);

    // 拒絕 URL 特殊字元（?、#、%）在非 connection string 的參數中
    public static bool ContainsUrlSpecialCharacters(string input);

    // 統一驗證入口
    public static (bool IsValid, string ErrorMessage) ValidateKeyword(string input);
    public static (bool IsValid, string ErrorMessage) ValidateTableName(string input);
    public static (bool IsValid, string ErrorMessage) ValidateSchemaName(string input);
}
```

**5.2 在每個 Command 的 RunAsync 開頭加入驗證**

目前的各 command（`ft`, `fc`, `sc`, `ftc`, `fsp`, `use`, `rm`, `dds`）在解析出使用者輸入後、執行查詢前，呼叫 `InputValidator` 做驗證。

**5.3 Connection String 特別處理**

`wcs` 命令寫入 connection string，這是高敏感操作。加入：

- 拒絕包含控制字元的 connection string
- 驗證 connection string 的基本格式（至少包含 `Server=` 或 `Data Source=`）
- 長度上限檢查（避免 buffer 相關問題）

**5.4 Schema 檔名消毒強化**

`src/Global.cs` 的 `SafeFileName` 已經處理了非法檔名字元，但需加入：

- 拒絕 `..` 路徑穿越
- 正規化路徑後驗證仍在 `SchemasDirectory` 內
- 拒絕以 `.` 開頭的隱藏檔名

---

## 第六階段：安全護欄

### 目的

PRS 有 3 個變更型命令：`dds`（寫入 schema 檔）、`wcs`（寫入 connection string）、`rm`（刪除 schema）。LLM 在執行這些命令前應該有「先想再做」的機制。

### 做法

**6.1 新增 `--dry-run` 旗標**

適用於 `dds`、`wcs`、`rm` 三個變更型命令：

```bash
# 驗證連線但不寫入檔案
prs dds mydb --dry-run
# 輸出: {"dryRun": true, "action": "dumpSchema", "target": "mydb", "wouldCreate": "localhost_mydb.schema.md", "valid": true}

# 驗證 connection string 格式但不儲存
prs wcs "Server=..." --dry-run
# 輸出: {"dryRun": true, "action": "writeConnectionString", "server": "localhost", "database": "mydb", "valid": true}

# 確認檔案存在但不刪除
prs rm mydb --dry-run
# 輸出: {"dryRun": true, "action": "removeSchema", "target": "mydb", "exists": true}
```

**6.2 回應消毒（Response Sanitization）**

PRS 的資料來源是 SQL Server 的 metadata（表名、欄位名、資料型別等）。這些值理論上是安全的，但惡意設計的資料庫名稱可能包含 prompt injection 嘗試。

在 `src/Formatting/SchemaFormatter.cs` 的輸出路徑中加入：

- 過濾 JSON 輸出中字串值的控制字元
- 如果欄位名稱/表名超過合理長度（例如 256 字元），截斷並標記
- 不直接影響 DDL 和 Table 格式（這兩者主要給人類看）

**6.3 在 MCP Tool 層同步加入 `--dry-run` 語意**

MCP tool 的 `inputSchema` 中加入 `dryRun` boolean 參數，讓 LLM 透過 MCP 呼叫時也能預覽操作結果。

---

## 第七階段：擴展 Skill Files

### 目的

文章指出 Skill files 是 LLM 學習工具使用方式的主要管道。PRS 已有 2 個 skill files，但可以更細緻地編碼 agent 專用指引。

### 做法

**7.1 擴充現有 skill files 加入防禦性指引**

在 `skills/claude-code/query-schema-cli.md` 和 `skills/claude-code/query-schema-mcp.md` 中加入：

```markdown
## Safety Rules
- Always use `--dry-run` before executing `dds`, `wcs`, or `rm` commands
- Always confirm with the user before executing write/delete operations
- Use `--fields` to limit output when querying large tables
- Use `--count` to check result size before fetching full results
- Use `-f json` (or set PRS_OUTPUT_FORMAT=json) for all queries — never parse table format
```

**7.2 新增 `describe-schema` skill**

新增 skill file 專門指導 LLM 如何使用 `prs describe` 來自我探索可用命令，而不是依賴 skill file 中的硬編碼列表：

```markdown
## Discovery Workflow
1. Run `prs describe` to get all available commands and their schemas
2. Run `prs describe <command>` to get detailed parameter info for a specific command
3. Use the JSON schema to construct correct --json input
```

**7.3 新增 CONTEXT.md**

在專案根目錄新增 `CONTEXT.md`（依照文章建議），內容為 PRS 的完整機器可讀描述：

- 工具用途
- 所有命令列表（指向 `prs describe` 取得最新版）
- 常見 workflow
- 已知限制

---

## 第八階段：強化 MCP Server

### 目的

PRS 已有 MCP server，這是目前最成熟的 LLM 介面。但需要與 CLI 側的新功能同步，並補強安全性。

### 做法

**8.1 新增 `describe` MCP tool**

讓 LLM 可以透過 MCP 呼叫 `describe` 工具，動態查詢所有可用工具的 schema，不需要依賴靜態的 tool definition。

**8.2 在所有 MCP tool 加入 `fields` 參數**

與 CLI 的 `--fields` 對齊，讓 MCP 呼叫也能控制回傳欄位。

**8.3 所有變更型 tool 加入 `dryRun` 參數**

目前 MCP 沒有變更型 tool（只有 `use_schema` 切換 active schema），但如果未來加入 dump 或 delete 功能，需要預留。

**8.4 MCP Server 版本號與 CLI 同步**

目前 MCP server 硬編碼 `Version = "1.0.0"`（`src-mcp/Mcp/McpServer.cs` 第 167 行），應改為從 assembly 版本讀取，與 CLI 保持一致。

---

## 實施順序與優先級

```
第一階段 ─ 統一 JSON 輸出          ★★★ 基礎設施，其他階段依賴它
    │
第二階段 ─ JSON 輸入模式           ★★★ LLM 可用性的最大提升
    │
第三階段 ─ Schema 內省命令         ★★☆ 讓 LLM 自我探索，減少 skill file 維護負擔
    │
第五階段 ─ 輸入驗證強化            ★★★ 安全性，越早做越好（可與第二階段並行）
    │
第四階段 ─ Field Masks             ★★☆ token 效率最佳化
    │
第六階段 ─ 安全護欄                ★★☆ dry-run + 回應消毒
    │
第七階段 ─ 擴展 Skill Files        ★☆☆ 與其他階段同步更新即可
    │
第八階段 ─ 強化 MCP Server         ★★☆ 與 CLI 新功能對齊
```

建議第一、二、五階段優先實作（這三個帶來最大的 LLM 可用性和安全性提升），第三到第八階段可以增量交付。每個階段都是獨立可部署的，不需要一次全部完成。
