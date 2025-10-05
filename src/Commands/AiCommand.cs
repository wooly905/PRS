using PRS.Display;
using PRS.FileHandle;
using PRS.Services;

namespace PRS.Commands;

internal class AiCommand(IDisplay display, IFileProvider fileProvider) : ICommand
{
    private readonly IDisplay _display = display;
    private readonly IFileProvider _fileProvider = fileProvider;

    public async Task RunAsync(string[] args)
    {
        // args: prs ai "free text ..."
        if (args == null || args.Length < 2)
        {
            _display.ShowError("Argument mismatch");
            _display.ShowInfo("prs ai \"your question...\"");
            return;
        }

        if (!File.Exists(Global.SchemaFilePath))
        {
            _display.ShowError("Schema doesn't exist locally. Please run dds command first.");
            return;
        }

        // Read endpoint and API key from shared config file (not schema-specific)
        string endpoint = await CommandHelper.GetConfigValueAsync(Global.LlmConfigFilePath, Global.LlmUrlConfigKey);
        string apiKey = await CommandHelper.GetConfigValueAsync(Global.LlmConfigFilePath, Global.LlmApiKeyConfigKey);

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            _display.ShowError("OpenAI endpoint or API key not configured. Use prs llmurl and prs llmapikey to set them.");
            return;
        }

        string userQuestion = string.Join(' ', args.Skip(1));

        // Step 1: keyword/entity extraction
        OpenAIService ai = new(endpoint, apiKey);
        string extractionJson = await ai.ExtractKeywordsAsync(userQuestion);

        // Step 2: schema search within active schema
        SchemaSearchService search = new(_fileProvider);
        var schemaContext = await search.BuildSchemaContextAsync(Global.SchemaFilePath, extractionJson);

        // Step 3: SQL generation restricted to active schema and provided context
        string sql = await ai.GenerateSqlAsync(userQuestion, schemaContext);

        // Local validation: only allowed tables/columns and only active schema
        if (!SchemaSearchService.ValidateSql(sql, schemaContext))
        {
            // Attempt one correction
            sql = await ai.GenerateSqlAsync(userQuestion, schemaContext, true);
            if (!SchemaSearchService.ValidateSql(sql, schemaContext))
            {
                _display.ShowError("Generated SQL references objects outside active schema or unknown columns.");
                return;
            }
        }

        // Output ONLY SQL
        Console.WriteLine(sql);
    }
}


