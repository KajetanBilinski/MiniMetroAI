
namespace MiniMetroAI.GameBridge.Models;

public class TrainSnapshot
{
    public int Id { get; set; }

    public int LineIndex { get; set; }

    public int RailcarCount { get; set; }

    public int ActiveRailcarCount { get; set; }

    public int PassengerCount { get; set; }
}
