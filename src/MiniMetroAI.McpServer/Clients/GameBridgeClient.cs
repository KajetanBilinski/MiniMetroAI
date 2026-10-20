
using System.Net;
using System.Net.Http.Json;

namespace MiniMetroAI.McpServer.Clients;

public sealed class GameBridgeClient
{
    private readonly HttpClient _httpClient;

    public GameBridgeClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetGameStateAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                "api/game/state");

            if (response.StatusCode ==
                HttpStatusCode.ServiceUnavailable)
            {
                return "{\"error\":\"no_active_game\"}";
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            return "{\"error\":\"game_bridge_unavailable\"}";
        }
        catch (TaskCanceledException)
        {
            return "{\"error\":\"game_bridge_timeout\"}";
        }
    }


    public async Task<string> SetGamePausedAsync(bool paused)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/game/pause",
                new { paused });

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException)
        {
            return "{\"error\":\"game_bridge_unavailable\"}";
        }
        catch (TaskCanceledException)
        {
            return "{\"error\":\"game_bridge_timeout\"}";
        }
    }

}
