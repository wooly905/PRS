using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace PRS.Services;

internal class OpenAIService
{
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _model = "gpt-4.1"; // hard-coded per requirement

    public OpenAIService(string endpoint, string apiKey)
    {
        _endpoint = endpoint?.TrimEnd('/') + "/";
        _apiKey = apiKey;
    }

    private ChatClient CreateClient()
    {
        return new ChatClient(
            credential: new ApiKeyCredential(_apiKey),
            model: _model,
            options: new OpenAIClientOptions()
            {
                Endpoint = new Uri(_endpoint)
            });
    }

    public async Task<string> ExtractKeywordsAsync(string userQuestion)
    {
        ChatClient client = CreateClient();

        ChatCompletion completion = await client.CompleteChatAsync([
            new SystemChatMessage("You are a data understanding assistant. Output JSON only, no prose."),
            new UserChatMessage($"Question: {userQuestion}\nReturn strictly a JSON with keys: entities, candidateTables, candidateColumns, timeFilters, joinHints, synonyms, needsMoreInfo, suggestedQuestions. Do not include any extra text.")
        ]);

        return string.Join("", completion.Content.Select(p => p.Text));
    }

    public async Task<string> GenerateSqlAsync(string userQuestion, string schemaContext, bool attemptCorrection = false)
    {
        ChatClient client = CreateClient();

        string system = "You are a SQL Server T-SQL expert. Use ONLY provided schema. Output one T-SQL statement without comments or explanation.";
        if (attemptCorrection)
        {
            system += " If previous attempt referenced invalid objects, correct it strictly to provided schema.";
        }

        ChatCompletion completion = await client.CompleteChatAsync([
            new SystemChatMessage(system),
            new UserChatMessage($"User question:\n{userQuestion}\n\nActive schema context (use only these tables/columns):\n{schemaContext}\n\nRules:\n- Use fully-qualified names [schema].[table].\n- If filtering by today and column is date/datetime, use CAST(GETDATE() AS date) comparison or proper date range.")
        ]);

        return string.Join("", completion.Content.Select(p => p.Text)).Trim();
    }
}


