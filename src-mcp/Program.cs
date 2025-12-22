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
        // Initialize services
        IFileProvider fileProvider = new FileProvider();
        SchemaService schemaService = new SchemaService(fileProvider);
        SchemaResource schemaResource = new SchemaResource(fileProvider);

        // Create and configure MCP server
        Mcp.McpServer server = new Mcp.McpServer(schemaService, schemaResource);

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
}

