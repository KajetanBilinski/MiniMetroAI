
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using MiniMetroAI.GameBridge.Commands;
using MiniMetroAI.GameBridge.State;
using Newtonsoft.Json;

namespace MiniMetroAI.GameBridge.Http;

internal sealed class GameBridgeHttpServer
{
    private readonly HttpListener _listener = new HttpListener();
    private Thread _thread;

    public void Start()
    {
        _listener.Prefixes.Add("http://127.0.0.1:8765/");
        _listener.Start();

        _thread = new Thread(Listen)
        {
            IsBackground = true,
            Name = "MiniMetroAI HTTP Server"
        };

        _thread.Start();

        Plugin.LogSource.LogInfo(
            "HTTP API started on http://127.0.0.1:8765/");
    }

    private void Listen()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = _listener.GetContext();
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            try
            {
                HandleRequest(context);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError(ex);

                try
                {
                    context.Response.Abort();
                }
                catch
                {
                    // Connection already closed.
                }
            }
        }
    }

    private static void HandleRequest(
        HttpListenerContext context)
    {
        var request = context.Request;
        string path = request.Url.AbsolutePath;

        if (path == "/api/game/pause")
        {
            HandlePause(context);
            return;
        }
        if (request.HttpMethod != "GET")
        {
            SendJson(
                context,
                405,
                "{\"error\":\"method_not_allowed\"}");
            return;
        }

        if (path == "/health")
        {
            SendJson(
                context,
                200,
                "{\"status\":\"ok\"}");
            return;
        }

        if (path == "/api/game/state")
        {
            string json = GameStateCache.Get();

            if (json == null)
            {
                SendJson(
                    context,
                    503,
                    "{\"error\":\"no_active_game\"}");
                return;
            }

            SendJson(context, 200, json);
            return;
        }

        SendJson(
            context,
            404,
            "{\"error\":\"not_found\"}");
    }

    private static void SendJson(
        HttpListenerContext context,
        int statusCode,
        string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        var response = context.Response;

        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF8;
        response.ContentLength64 = bytes.Length;

        using (var stream = response.OutputStream)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    public void Stop()
    {
        if (_listener.IsListening)
            _listener.Stop();

        _listener.Close();
    }


    private sealed class PauseRequest
    {
        [JsonProperty("paused")]
        public bool? Paused { get; set; }
    }

    private static string ReadSmallJsonBody(HttpListenerRequest request)
    {
        using (var reader = new StreamReader(
            request.InputStream,
            request.ContentEncoding ?? Encoding.UTF8))
        {
            char[] buffer = new char[257];
            int count = reader.ReadBlock(buffer, 0, buffer.Length);

            if (count > 256)
                return null;

            return new string(buffer, 0, count);
        }
    }

    private static void HandlePause(HttpListenerContext context)
    {
        var request = context.Request;

        if (request.HttpMethod != "POST")
        {
            SendJson(context, 405,
                "{\"error\":\"method_not_allowed\"}");
            return;
        }

        if (!string.IsNullOrEmpty(request.Headers["Origin"]))
        {
            SendJson(context, 403,
                "{\"error\":\"browser_origin_not_allowed\"}");
            return;
        }

        if (request.ContentType == null ||
            !request.ContentType.StartsWith(
                "application/json",
                StringComparison.OrdinalIgnoreCase))
        {
            SendJson(context, 415,
                "{\"error\":\"json_required\"}");
            return;
        }

        PauseRequest payload;

        try
        {
            string body = ReadSmallJsonBody(request);

            if (body == null)
            {
                SendJson(context, 413,
                    "{\"error\":\"payload_too_large\"}");
                return;
            }

            payload = JsonConvert.DeserializeObject<PauseRequest>(body);
        }
        catch (JsonException)
        {
            SendJson(context, 400,
                "{\"error\":\"invalid_json\"}");
            return;
        }

        if (payload == null || !payload.Paused.HasValue)
        {
            SendJson(context, 400,
                "{\"error\":\"paused_required\"}");
            return;
        }

        var command = GameCommandQueue.EnqueuePause(
            payload.Paused.Value);

        if (!command.Completion.Task.Wait(10000))
        {
            command.Cancel();

            SendJson(context, 504,
                "{\"error\":\"command_timeout\"}");
            return;
        }

        var result = command.Completion.Task.Result;

        SendJson(context, result.StatusCode, result.Json);
    }

}
