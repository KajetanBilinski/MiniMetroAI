
using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MiniMetroAI.GameBridge.Patches;
using MiniMetroAI.GameBridge.Readers;
using UnityEngine;

namespace MiniMetroAI.GameBridge;

[BepInPlugin(
    "dev.minimetroai.gamebridge",
    "MiniMetroAI GameBridge",
    "0.2.0")]
public sealed class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource LogSource { get; private set; }

    private Harmony _harmony;

    private void Awake()
    {
        LogSource = Logger;

        Logger.LogInfo("GameBridge initialized!");

        try
        {
            _harmony = new Harmony(
                "dev.minimetroai.gamebridge");

            _harmony.PatchAll(
                typeof(Plugin).Assembly);

            Logger.LogInfo("Harmony patches loaded.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F9))
            return;

        Game game = GameUpdatePatch.CurrentGame;

        if (Main.Instance == null ||
            !Main.Instance.IsInGame ||
            game == null ||
            game.City == null)
        {
            Logger.LogInfo("No active game.");
            return;
        }

        StationReader.Log(game);
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
