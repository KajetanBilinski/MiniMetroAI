
using System.Threading;

namespace MiniMetroAI.GameBridge.State;

internal static class GameStateCache
{
    private static string _json;

    public static void Set(string json)
    {
        Volatile.Write(ref _json, json);
    }

    public static string Get()
    {
        return Volatile.Read(ref _json);
    }

    public static void Clear()
    {
        Volatile.Write(ref _json, null);
    }
}
