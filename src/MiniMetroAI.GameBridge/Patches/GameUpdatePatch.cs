
using System;
using HarmonyLib;
using MiniMetroAI.GameBridge.Readers;

namespace MiniMetroAI.GameBridge.Patches;

[HarmonyPatch(typeof(Game), nameof(Game.Update))]
internal static class GameUpdatePatch
{
    internal static Game CurrentGame { get; private set; }

    private static int _lastStationCount = -1;

    [HarmonyPostfix]
    private static void AfterUpdate(Game __instance)
    {
        try
        {
            if (__instance == null ||
                __instance.City == null)
                return;

            if (!ReferenceEquals(CurrentGame, __instance))
            {
                CurrentGame = __instance;
                _lastStationCount = -1;
            }

            int count = __instance.City.StationCount;

            if (count == _lastStationCount)
                return;

            _lastStationCount = count;

            StationReader.Log(__instance);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError(ex);
        }
    }
}
