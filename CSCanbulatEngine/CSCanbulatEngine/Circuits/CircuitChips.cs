using System.Numerics;
using System.Windows.Markup;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;

namespace CSCanbulatEngine.Circuits;

public class CircuitChips
{
    public static void ChipsMenu(ImGuiIOPtr io, Vector2 canvasPos, Vector2 panning)
    {
        if (ImGui.BeginPopupContextWindow("SpawnChipMenu"))
        {
            Vector2 spawnPos = io.MousePos - canvasPos - panning;
            if (ImGui.MenuItem("Create Add Chip"))
            {
                CircuitEditor.chips.Add(new AddChip(CircuitEditor.GetNextAvaliableChipID(), "Add", spawnPos));
            }

            if (ImGui.MenuItem("Create Float Constant"))
            {
                CircuitEditor.chips.Add(new FloatConstantChip(CircuitEditor.GetNextAvaliableChipID(), "Float Constant", spawnPos));
            }

            if (ImGui.MenuItem("Create Int Constant"))
            {
                CircuitEditor.chips.Add(new IntConstantChip(CircuitEditor.GetNextAvaliableChipID(), "Int Constant", spawnPos));
            }

            ImGui.EndPopup();
        }
    }
}

public class FloatConstantChip : Chip
{
    public FloatConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, new List<Type>() { typeof(float)});
        AddPort("Output", false, new List<Type>() { typeof(float)});
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort chipPort)
    {

    }
}

public class IntConstantChip : Chip
{
    public IntConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, new List<Type>() { typeof(int)});
        AddPort("Output", false, new List<Type>() { typeof(int)});
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort chipPort)
    {

    }
}

public class AddChip : Chip
{
    public AddChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, new List<Type>() { typeof(int), typeof(float)});
        AddPort("B", true, new List<Type>() { typeof(int), typeof(float)});
        AddPort("Output", false, new List<Type>() { typeof(int), typeof(float) });
        base.OutputPorts[0].Value.ValueFunction = AddOutput;
    }

    public Values AddOutput(ChipPort? chipPort)
    {
        Values value = new Values();
        if (InputPorts[0].PortType == typeof(float))
        {
            value.f = InputPorts[0].Value.GetValue().f + InputPorts[1].Value.GetValue().f;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.i = InputPorts[0].Value.GetValue().i + InputPorts[1].Value.GetValue().i;
        }

        return value;
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort? chipPort)
    {
        if (chipPort == null)
        {
            foreach (var port in InputPorts)
            {
                port._PortType = null;
                port.UpdateColor();
            }
            foreach (var port in OutputPorts)
            {
                port._PortType = null;
                port.UpdateColor();
            }
        }

        {
            foreach (var port in InputPorts)
            {
                port._PortType = chipPort.PortType;
                port.UpdateColor();
            }
            foreach (var port in OutputPorts)
            {
                port._PortType = chipPort.PortType;
                port.UpdateColor();
            }
        }
    }
}