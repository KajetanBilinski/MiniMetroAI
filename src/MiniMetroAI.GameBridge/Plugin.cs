
using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MiniMetroAI.GameBridge.Commands;
using MiniMetroAI.GameBridge.Http;
using MiniMetroAI.GameBridge.Patches;
using MiniMetroAI.GameBridge.Readers;
using MiniMetroAI.GameBridge.State;
using UnityEngine;

namespace MiniMetroAI.GameBridge;

[BepInPlugin(
    "dev.minimetroai.gamebridge",
    "MiniMetroAI GameBridge",
    "0.3.0")]
public sealed class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource LogSource { get; private set; }

    private Harmony _harmony;
    private GameBridgeHttpServer _httpServer;

    private void Awake()
    {
        LogSource = Logger;

        Logger.LogInfo("GameBridge initialized!");

        try
        {
            _harmony = new Harmony(
                "dev.minimetroai.gamebridge");

            _harmony.PatchAll(typeof(Plugin).Assembly);

            Logger.LogInfo("Harmony patches loaded.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }

        try
        {
            _httpServer = new GameBridgeHttpServer();
            _httpServer.Start();
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"Failed to start HTTP API: {ex}");
        }
    }

    private void Update()
    {
        GameCommandQueue.ProcessPending();

        var main = Main.Instance;

        if (main == null || !main.IsInGame)
        {
            GameStateCache.Clear();
        }

        if (!Input.GetKeyDown(KeyCode.F9))
            return;

        var game = GameUpdatePatch.CurrentGame;

        if (main == null ||
            !main.IsInGame ||
            game == null ||
            game.City == null)
        {
            Logger.LogWarning("No active game.");
            return;
        }

        StationReader.Log(game);
    }

    private void OnDestroy()
    {
        _httpServer?.Stop();
        _harmony?.UnpatchSelf();

        GameStateCache.Clear();
    }
}
