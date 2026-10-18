
using System.Collections.Generic;

namespace MiniMetroAI.GameBridge.Models;

public class GameStateSnapshot
{
    public int Score { get; set; }
    public bool IsPaused { get; set; }
    public bool IsGameOver { get; set; }

    public string Mode { get; set; }

    public float GameTime { get; set; }
    public int Day { get; set; }

    public int LineCount { get; set; }
    public int StationCount { get; set; }

    public List<StationSnapshot> Stations { get; set; }
        = new List<StationSnapshot>();
    public List<LineSnapshot> Lines { get; set; }
    = new List<LineSnapshot>();
}
