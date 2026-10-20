
using System.ComponentModel;
using MiniMetroAI.McpServer.Clients;
using ModelContextProtocol.Server;

namespace MiniMetroAI.McpServer.Tools;

[McpServerToolType]
public static class GameTools
{
    [McpServerTool(Name = "get_game_state")]
    [Description(
        "Returns the current Mini Metro game state, " +
        "including stations, lines, trains, passengers " +
        "and score as JSON.")]
    public static Task<string> GetGameState(
        GameBridgeClient client)
    {
        return client.GetGameStateAsync();
    }


    [McpServerTool(Name = "set_game_paused")]
    [Description(
        "Pauses or resumes the active Mini Metro game. " +
        "Use only when game control has been requested.")]
    public static Task<string> SetGamePaused(
        GameBridgeClient client,
        [Description(
        "True pauses the game, false resumes it.")]
    bool paused)
    {
        return client.SetGamePausedAsync(paused);
    }

}
