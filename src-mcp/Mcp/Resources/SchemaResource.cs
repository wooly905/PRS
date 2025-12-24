using PRS.FileHandle;

namespace PRS.McpServer.Mcp.Resources;

internal class SchemaResource
{
    private readonly IFileProvider _fileProvider;

    public SchemaResource(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public string Uri => "prs://schema/{schemaName}";
    public string Name => "Database Schema";
    public string Description => "Read the content of a database schema file";

    public async Task<object> ReadAsync(string uri)
    {
        // Parse URI: prs://schema/{schemaName}
        if (!uri.StartsWith("prs://schema/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid URI format: {uri}");
        }

        string schemaName = uri.Substring("prs://schema/".Length);
        string safeName = Global.SafeFileName(schemaName);
        string fileName = safeName.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
            ? safeName
            : safeName + ".schema.md";
        string filePath = Path.Combine(Global.SchemasDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Schema file not found: {schemaName}");
        }

        // Read the schema file content
        string content = await File.ReadAllTextAsync(filePath);

        // MCP protocol requires contents to be an array
        return new
        {
            contents = new[]
            {
                new
                {
                    uri = uri,
                    mimeType = "text/markdown",
                    text = content
                }
            }
        };
    }

    public List<object> ListResources()
    {
        var resources = new List<object>();

        if (!Directory.Exists(Global.SchemasDirectory))
        {
            return resources;
        }

        string[] files = Directory.GetFiles(Global.SchemasDirectory, "*.schema.md", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string shortName = name.EndsWith(".schema.md", StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - ".schema.md".Length)
                : name;

            resources.Add(new
            {
                uri = $"prs://schema/{shortName}",
                name = shortName,
                mimeType = "text/markdown",
                description = $"Database schema: {shortName}"
            });
        }

        return resources;
    }
}

