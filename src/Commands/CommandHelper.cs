using PRS.Database;
using PRS.Display;
using PRS.FileHandle;

namespace PRS.Commands;

internal static class CommandHelper
{
    public static async Task<string> GetConnectionStringAsync(IDisplay display, IFileProvider fileProvider)
    {
        if (!File.Exists(Global.ConnectionStringFilePath))
        {
            display.ShowError("Connection string doesn't exist. Please set it first.");
            return string.Empty;
        }

        IFileReader reader = fileProvider.GetFileReader(Global.ConnectionStringFilePath);
        string output = await reader.ReadLineAsync();
        reader.Dispose();

        return output;
    }

    public static async Task<string> GetSingleLineValueAsync(string filePath, IFileProvider fileProvider)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        IFileReader reader = fileProvider.GetFileReader(filePath);
        string output = await reader.ReadLineAsync();
        reader.Dispose();
        return output ?? string.Empty;
    }

    public static async Task WriteSingleLineValueAsync(string filePath, string value, IFileProvider fileProvider)
    {
        if (Directory.Exists(Global.SchemaFileDirectory))
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        else
        {
            Directory.CreateDirectory(Global.SchemaFileDirectory);
        }

        IFileWriter writer = fileProvider.GetFileWriter(filePath);
        await writer.WriteLineAsync(value ?? string.Empty);
        writer.Dispose();
    }

    public static async Task<string> GetSchemaSectionSingleValueAsync(string sectionName, string schemaFilePath, IFileProvider fileProvider)
    {
        if (!File.Exists(schemaFilePath))
        {
            return string.Empty;
        }

        IFileReader reader = fileProvider.GetFileReader(schemaFilePath);
        bool inSection = false;
        while (true)
        {
            string line = await reader.ReadLineAsync();
            if (line == null) break;

            if (string.Equals(line, sectionName, StringComparison.Ordinal))
            {
                inSection = true;
                continue;
            }
            if (inSection)
            {
                if (line.StartsWith("["))
                {
                    // next section reached, no value present
                    break;
                }
                reader.Dispose();
                return line ?? string.Empty;
            }
        }
        reader.Dispose();
        return string.Empty;
    }

    public static async Task UpsertSchemaSectionSingleValueAsync(string sectionName, string value, string schemaFilePath, IFileProvider fileProvider)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(schemaFilePath));

        List<string> lines = new();
        if (File.Exists(schemaFilePath))
        {
            using var sr = new StreamReader(schemaFilePath);
            while (true)
            {
                string line = await sr.ReadLineAsync();
                if (line == null) break;
                lines.Add(line);
            }
        }

        int sectionIndex = lines.FindIndex(l => string.Equals(l, sectionName, StringComparison.Ordinal));
        if (sectionIndex >= 0)
        {
            // ensure a value line right after section; replace or insert
            int valueIndex = sectionIndex + 1;
            if (valueIndex < lines.Count && !lines[valueIndex].StartsWith("["))
            {
                lines[valueIndex] = value ?? string.Empty;
            }
            else
            {
                lines.Insert(valueIndex, value ?? string.Empty);
            }
        }
        else
        {
            // append at end
            lines.Add(sectionName);
            lines.Add(value ?? string.Empty);
        }

        using var sw = new StreamWriter(schemaFilePath, false);
        foreach (string l in lines)
        {
            await sw.WriteLineAsync(l);
        }
        await sw.FlushAsync();
        sw.Close();
    }

    public static async Task UpsertConfigKeyValueAsync(string configFilePath, string key, string value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

        Dictionary<string, string> kv = new(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(configFilePath))
        {
            foreach (string line in await File.ReadAllLinesAsync(configFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                int idx = line.IndexOf('=');
                if (idx <= 0) continue;
                string k = line.Substring(0, idx).Trim();
                string v = line.Substring(idx + 1).Trim();
                if (!kv.ContainsKey(k)) kv[k] = v;
            }
        }
        kv[key ?? string.Empty] = value ?? string.Empty;

        using var sw = new StreamWriter(configFilePath, false);
        foreach (var pair in kv)
        {
            await sw.WriteLineAsync($"{pair.Key}={pair.Value}");
        }
        await sw.FlushAsync();
        sw.Close();
    }

    public static async Task<string> GetConfigValueAsync(string configFilePath, string key)
    {
        if (!File.Exists(configFilePath)) return string.Empty;
        foreach (string line in await File.ReadAllLinesAsync(configFilePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            int idx = line.IndexOf('=');
            if (idx <= 0) continue;
            string k = line.Substring(0, idx).Trim();
            if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(idx + 1).Trim();
            }
        }
        return string.Empty;
    }

    public static void PrintColumns(IEnumerable<ColumnModel> models, IDisplay display)
    {
        display.DisplayColumns(models);
    }

    public static void PrintTables(IEnumerable<TableModel> models, IDisplay display)
    {
        display.DisplayTables(models);
    }

    public static void PrintModel(IEnumerable<string> models, IDisplay display)
    {
        display.ShowInfo("Name ===");

        foreach (string m in models)
        {
            display.ShowInfo(m);
        }
    }
}
