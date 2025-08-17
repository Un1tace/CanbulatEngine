using System.Numerics;

namespace CSCanbulatEngine.FileHandling.CircuitHandling;

public class CircuitData
{
    public class ChipData
    {
        public int id { get; set; }
        public string Name;
        public string ChipType;
        public Vector2 Position;
    }

    public class PortConnectionData
    {
        public int OutputChipId { get; set; }
        public int OutputPortId { get; set; }
        public int InputChipId { get; set; }
        public int InputPortId { get; set; }
    }
    
    public class CircuitInfo
    {
        public string CircuitScriptName { get; set; }
        public List<ChipData> Chips { get; set; } = new();
        public List<PortConnectionData> Connections { get; set; } = new();
    }
}