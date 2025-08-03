using System.Numerics;
using System.Windows.Markup;
using CSCanbulatEngine.GameObjectScripts;

namespace CSCanbulatEngine.Circuits;

public class CircuitChips
{
    
}

public class ConstantChip : Chip
{
    public ConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, new List<Type>() { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(Vector2), typeof(GameObject)}, ChipColor.GetColor(ChipColors.Default));
        AddPort("Output", false, new List<Type>() { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(Vector2), typeof(GameObject)}, ChipColor.GetColor(ChipColors.Default));
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public object? ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort chipPort)
    {
        if (chipPort.IsInput)
        {
            OutputPorts[0].PortType = chipPort.PortType;
        }
        else
        {
            InputPorts[0].PortType = chipPort.PortType;
        }
    }
}

// public class AddChip : Chip
// {
//     
// }