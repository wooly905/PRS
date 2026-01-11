namespace PRS;

internal static class Global
{
    public static string SchemaFileName => "schema.md";
    public static string ConnectionStringFileName => "prs.txt";
    public static string SchemaFileDirectory => Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".prs");
    public static string SchemasDirectory => Path.Combine(SchemaFileDirectory, "schemas");
    public static string ActiveSchemaPointerFilePath => Path.Combine(SchemaFileDirectory, "active.txt");

    // Dynamic path that resolves to the currently active schema file.
    // Backward compatible: if no active pointer is set, falls back to legacy path under SchemaFileDirectory.
    public static string SchemaFilePath => ResolveActiveSchemaFilePath();

    public static string ConnectionStringFilePath => Path.Combine(SchemaFileDirectory, ConnectionStringFileName);
    public static string ConnectionStringSectionName => "[CS]";
    public static string TableSectionName => "[Table]";
    public static string ColumnSectionName => "[Column]";
    public static string StoredProcedureSectionName => "[StoredProcedure]";

    public static string GetActiveSchemaName()
    {
        try
        {
            if (File.Exists(ActiveSchemaPointerFilePath))
            {
                string name = File.ReadAllText(ActiveSchemaPointerFilePath).Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
        }
        catch
        {
            // ignore and fall through
        }

        return null;
    }

    public static void SetActiveSchema(string schemaFileName)
    {
        Directory.CreateDirectory(SchemaFileDirectory);
        Directory.CreateDirectory(SchemasDirectory);
        File.WriteAllText(ActiveSchemaPointerFilePath, schemaFileName ?? string.Empty);
    }

    public static string CreateSchemaFileNameFromConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return SchemaFileName;
        }

        Dictionary<string, string> kv = new(StringComparer.OrdinalIgnoreCase);
        string[] parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string part in parts)
        {
            int idx = part.IndexOf('=');
            if (idx <= 0 || idx >= part.Length - 1)
            {
                continue;
            }
            string key = part.Substring(0, idx).Trim();
            string value = part.Substring(idx + 1).Trim();
            if (!kv.ContainsKey(key))
            {
                kv[key] = value;
            }
        }

        string server = GetFirst(kv, ["Server", "Data Source", "Addr", "Address", "Network Address"]) ?? "server";
        string database = GetFirst(kv, ["Initial Catalog", "Database"]) ?? "database";

        string safe = $"{SafeFileName(server)}_{SafeFileName(database)}.schema.md";
        return safe;
    }

    private static string GetFirst(Dictionary<string, string> kv, string[] keys)
    {
        foreach (string k in keys)
        {
            if (kv.TryGetValue(k, out string v) && !string.IsNullOrWhiteSpace(v))
            {
                return v;
            }
        }
        return null;
    }

    public static string SafeFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "_";
        }
        char[] invalid = Path.GetInvalidFileNameChars();
        string output = input;
        foreach (char c in invalid)
        {
            output = output.Replace(c, '_');
        }
        output = output.Replace('.', '_');
        return output;
    }

    private static string ResolveActiveSchemaFilePath()
    {
        try
        {
            // If pointer exists and target exists under schemas directory, use it
            string activeName = GetActiveSchemaName();
            if (!string.IsNullOrWhiteSpace(activeName))
            {
                string activePath = Path.Combine(SchemasDirectory, activeName);
                if (File.Exists(activePath))
                {
                    return activePath;
                }
            }

            // Backward compatibility: legacy single-file path
            string legacyPath = Path.Combine(SchemaFileDirectory, SchemaFileName);
            if (File.Exists(legacyPath))
            {
                return legacyPath;
            }

            // Default to schemas directory with default schema name
            return Path.Combine(SchemasDirectory, SchemaFileName);
        }
        catch
        {
            // As a last resort, return legacy path
            return Path.Combine(SchemaFileDirectory, SchemaFileName);
        }
    }
}
