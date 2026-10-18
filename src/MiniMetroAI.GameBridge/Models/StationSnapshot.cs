
namespace MiniMetroAI.GameBridge.Models;

public class StationSnapshot
{
    public int Id { get; set; }
    public string Type { get; set; }

    public float X { get; set; }
    public float Y { get; set; }

    public int WaitingPassengers { get; set; }
    public int Capacity { get; set; }

    public int LineCount { get; set; }

    public bool IsInterchange { get; set; }
    public bool IsOvercrowded { get; set; }
    public float OvercrowdingProgress { get; set; }
}
