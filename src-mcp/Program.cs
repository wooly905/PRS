using System.Text;
using PRS.FileHandle;
using PRS.McpServer.Mcp;
using PRS.McpServer.Mcp.Resources;
using PRS.McpServer.Mcp.Tools;
using PRS.McpServer.Services;

namespace PRS.McpServer;

class Program
{
    static async Task Main(string[] args)
    {
        // Redirect all standard output to standard error by default
        // This ensures that any accidental Console.WriteLine from libraries
        // goes to stderr and doesn't break the MCP JSON-RPC protocol on stdout.
        var stdout = Console.Out;
        var stderr = Console.Error;
        Console.SetOut(stderr);

        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        
        try
        {
            // Initialize services
            IFileProvider fileProvider = new FileProvider();
            SchemaService schemaService = new SchemaService(fileProvider);
            SchemaResource schemaResource = new SchemaResource(fileProvider);

            // Create and configure MCP server
            // We pass the original stdout to the server so it can still send JSON-RPC
            Mcp.McpServer server = new Mcp.McpServer(schemaService, schemaResource, stdout);

            // Register all tools
            server.RegisterTool(new FindTableTool(schemaService));
            server.RegisterTool(new FindColumnTool(schemaService));
            server.RegisterTool(new FindStoredProcedureTool(schemaService));
            server.RegisterTool(new GetTableSchemaTool(schemaService));
            server.RegisterTool(new ListSchemasTool(schemaService));
            server.RegisterTool(new UseSchemaTool(schemaService));

            // Run the server (this will block and handle stdio)
            await server.RunAsync();
        }
        catch (Exception ex)
        {
            // If we get here, something went wrong during initialization
            await stderr.WriteLineAsync($"Fatal error during initialization: {ex.Message}");
            if (ex.StackTrace != null)
            {
                await stderr.WriteLineAsync(ex.StackTrace);
            }
            await stderr.FlushAsync();
            Environment.Exit(1);
        }
    }
}

