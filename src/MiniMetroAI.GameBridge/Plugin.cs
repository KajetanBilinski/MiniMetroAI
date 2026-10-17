
using BepInEx;

namespace MiniMetroAI.GameBridge
{
    [BepInPlugin(
        "dev.minimetroai.gamebridge",
        "MiniMetroAI GameBridge",
        "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo(
                "MiniMetroAI GameBridge initialized!");
        }
    }
}
