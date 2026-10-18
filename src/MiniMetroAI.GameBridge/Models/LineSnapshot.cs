
using System.Collections.Generic;

namespace MiniMetroAI.GameBridge.Models;

public class LineSnapshot
{
    public int Index { get; set; }

    public int TrainCount { get; set; }

    public bool IsMothballed { get; set; }

    public List<LinkSnapshot> Links { get; set; }
        = new List<LinkSnapshot>();
}
