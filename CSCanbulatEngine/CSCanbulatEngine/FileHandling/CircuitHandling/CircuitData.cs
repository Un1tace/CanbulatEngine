using System.Numerics;

namespace CSCanbulatEngine.FileHandling.CircuitHandling;

public class CircuitData
{
    public class ChipData
    {
        public int id { get; set; }
        public string Name { get ; set; }
        public string ChipType { get ; set; }
        public Vector2 Position { get ; set; }
        public Vector4 Color { get ; set; }
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>(); // Holds custom data, e.g event picked
    }

    public class UnconnectedPortValueData
    {
        public int ChipId { get; set; }
        public int PortId { get; set; }
        public string ValueType { get; set; }
        public string Value { get; set; }
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
        public List<UnconnectedPortValueData> UnconnectedPortValues { get; set; } = new();
    }
}