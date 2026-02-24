using System.Numerics;

namespace CSCanbulatEngine.FileHandling.CircuitHandling;

public class CircuitData
{
    public record ChipData
    {
        public int id { get; set; }
        public string Name { get ; set; }
        public string ChipType { get ; set; }
        public Vector2 Position { get ; set; }
        public Vector4 Color { get ; set; }
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>(); // Holds custom data, e.g event picked
    }

    public record UnconnectedClipboardChipData
    {
        public List<UnconnectedPortValueData> inputPorts { get; set; }
        public List<UnconnectedPortValueData> outputPorts { get; set; }
    }

    public record UnconnectedPortValueData
    {
        public int ChipId { get; set; }
        public int PortId { get; set; }
        public string? ValueType { get; set; }
        public string? Value { get; set; }
    }
    
    public record PortConnectionData
    {
        public int OutputChipId { get; set; }
        public int OutputPortId { get; set; }
        public int InputChipId { get; set; }
        public int InputPortId { get; set; }
    }
    
    public record CircuitInfo
    {
        public string CircuitScriptName { get; set; }
        public List<ChipData> Chips { get; set; } = new();
        public List<PortConnectionData> Connections { get; set; } = new();
        public List<UnconnectedPortValueData> UnconnectedPortValues { get; set; } = new();
    }
}