
using System;
using HarmonyLib;
using MiniMetroAI.GameBridge.Readers;
using MiniMetroAI.GameBridge.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace MiniMetroAI.GameBridge.Patches;

[HarmonyPatch(typeof(Game), nameof(Game.Update))]
internal static class GameUpdatePatch
{
    internal static Game CurrentGame { get; private set; }

    private static int _lastStationCount = -1;
    private static float _nextSnapshotAt;

    private static readonly JsonSerializerSettings JsonSettings =
        new JsonSerializerSettings
        {
            ContractResolver =
                new CamelCasePropertyNamesContractResolver()
        };

    [HarmonyPostfix]
    private static void AfterUpdate(Game __instance)
    {
        try
        {
            if (__instance == null || __instance.City == null)
                return;

            if (!ReferenceEquals(CurrentGame, __instance))
            {
                CurrentGame = __instance;
                _lastStationCount = -1;
                _nextSnapshotAt = 0f;
            }

            float now = Time.realtimeSinceStartup;

            if (now >= _nextSnapshotAt)
            {
                _nextSnapshotAt = now + 1f;

                var snapshot = GameStateReader.Read(__instance);

                string json = JsonConvert.SerializeObject(
                    snapshot,
                    JsonSettings);

                GameStateCache.Set(json);
            }

            int count = __instance.City.StationCount;

            if (count != _lastStationCount)
            {
                _lastStationCount = count;
                StationReader.Log(__instance);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError(ex);
        }
    }
}
