
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniMetroAI.McpServer.Clients;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddHttpClient<GameBridgeClient>(client =>
{
    client.BaseAddress = new Uri(
        Environment.GetEnvironmentVariable(
            "MINIMETROAI_BRIDGE_URL")
        ?? "http://127.0.0.1:8765/");

    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
