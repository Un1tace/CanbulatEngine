using System.Drawing;
using System.Numerics;
using System.Windows.Markup;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using SixLabors.ImageSharp;
using RectangleF = System.Drawing.RectangleF;

namespace CSCanbulatEngine.Circuits;

public class CircuitChips
{
    private static Vector2 spawnPos = Vector2.Zero;
    
    private static Chip? hoveredChip = null;
    public static void ChipsMenu(ImGuiIOPtr io, Vector2 canvasPos, Vector2 panning)
    {
        
        Vector2 mousePosInWorld = (io.MousePos - canvasPos - panning) / CircuitEditor.Zoom;
        

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            spawnPos = io.MousePos - canvasPos - panning;
            
            foreach (var chip in CircuitEditor.chips)
            {
                var chipRectangle = new RectangleF(chip.Position.X, chip.Position.Y, chip.Size.X, chip.Size.Y);
                if (chipRectangle.Contains(mousePosInWorld.X, mousePosInWorld.Y))
                {
                    hoveredChip = chip;
                    continue;
                }
            }
        }
        
        if (hoveredChip == null)
        {
            if (ImGui.BeginPopupContextWindow("SpawnChipMenu"))
            {
                if (ImGui.BeginMenu("Create Chips"))
                {
                    if (ImGui.MenuItem("Spawn Event Chip"))
                    {
                        CircuitEditor.chips.Add(new EventChip(CircuitEditor.GetNextAvaliableChipID(), "Event Chip", spawnPos));
                    }
                    
                    if (ImGui.MenuItem("Test Button"))
                    {
                        CircuitEditor.chips.Add(new TestButton(CircuitEditor.GetNextAvaliableChipID(), "Test Button",
                            spawnPos));
                    }

                    if (ImGui.BeginMenu("Constant Chips"))
                    {
                        if (ImGui.MenuItem("Create Bool Constant"))
                        {
                            CircuitEditor.chips.Add(new BoolConstantChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Bool Constant", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Float Constant"))
                        {
                            CircuitEditor.chips.Add(new FloatConstantChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Float Constant", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Int Constant"))
                        {
                            CircuitEditor.chips.Add(new IntConstantChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Int Constant", spawnPos));
                        }

                        if (ImGui.MenuItem("Create String Constant"))
                        {
                            CircuitEditor.chips.Add(new StringConstantChip(CircuitEditor.GetNextAvaliableChipID(),
                                "String Constant", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Vector2 Constant"))
                        {
                            CircuitEditor.chips.Add(new Vector2ConstantChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Vector2 Constant", spawnPos));
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Math Chips"))
                    {
                        if (ImGui.MenuItem("Create Add Chip"))
                        {
                            CircuitEditor.chips.Add(
                                new AddChip(CircuitEditor.GetNextAvaliableChipID(), "Add", spawnPos));
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Logic & Comparison Chips"))
                    {
                        if (ImGui.MenuItem("Create Not Chip"))
                        {
                            CircuitEditor.chips.Add(new NotChip(CircuitEditor.GetNextAvaliableChipID(), "Not Chip",
                                spawnPos));
                        }

                        if (ImGui.MenuItem("Create And Chip"))
                        {
                            CircuitEditor.chips.Add(new AndChip(CircuitEditor.GetNextAvaliableChipID(), "And Chip",
                                spawnPos));
                        }

                        if (ImGui.MenuItem("Create Or Chip"))
                        {
                            CircuitEditor.chips.Add(new OrChip(CircuitEditor.GetNextAvaliableChipID(), "Or Chip",
                                spawnPos));
                        }

                        if (ImGui.MenuItem("Create Nor Chip"))
                        {
                            CircuitEditor.chips.Add(new NorChip(CircuitEditor.GetNextAvaliableChipID(), "Nor Chip",
                                spawnPos));
                        }

                        if (ImGui.MenuItem("Create Nand Chip"))
                        {
                            CircuitEditor.chips.Add(new NandChip(CircuitEditor.GetNextAvaliableChipID(), "Nand Chip",
                                spawnPos));
                        }

                        if (ImGui.MenuItem("Create Xor Chip"))
                        {
                            CircuitEditor.chips.Add(new XorChip(CircuitEditor.GetNextAvaliableChipID(), "Xor Chip",
                                spawnPos));
                        }

                        if (ImGui.MenuItem("Create Equals Chip"))
                        {
                            CircuitEditor.chips.Add(new EqualsChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Equals Chip", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Less Than Chip"))
                        {
                            CircuitEditor.chips.Add(new LessThanChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Less Than Chip", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Less Than Or Equals Chip"))
                        {
                            CircuitEditor.chips.Add(new LessThanOrEqualsChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Less Than Or Equals Chip", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Greater Than Chip"))
                        {
                            CircuitEditor.chips.Add(new GreaterThanChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Greater Than Chip", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Greater Than Or Equals Chip"))
                        {
                            CircuitEditor.chips.Add(new GreaterThanOrEqualsChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Greater Than Or Equals Chip", spawnPos));
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Variable Chips"))
                    {
                        if (ImGui.MenuItem("Create Bool Variable"))
                        {
                            CircuitEditor.chips.Add(new BoolVariable(CircuitEditor.GetNextAvaliableChipID(),
                                "Bool Variable", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Float Variable"))
                        {
                            CircuitEditor.chips.Add(new FloatVariable(CircuitEditor.GetNextAvaliableChipID(),
                                "Float Variable", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Int Variable"))
                        {
                            CircuitEditor.chips.Add(new IntVariable(CircuitEditor.GetNextAvaliableChipID(),
                                "Int Variable", spawnPos));
                        }

                        if (ImGui.MenuItem("Create String Variable"))
                        {
                            CircuitEditor.chips.Add(new StringVariable(CircuitEditor.GetNextAvaliableChipID(),
                                "String Variable", spawnPos));
                        }

                        if (ImGui.MenuItem("Create Vector2 Variable"))
                        {
                            CircuitEditor.chips.Add(new Vector2Variable(CircuitEditor.GetNextAvaliableChipID(),
                                "Vector2 Variable", spawnPos));
                        }

                        if (ImGui.MenuItem("Create GameObject Variable"))
                        {
                            CircuitEditor.chips.Add(new GameObjectVariable(CircuitEditor.GetNextAvaliableChipID(),
                                "GameObject Variable", spawnPos));
                        }


                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Miscellaneous Chips"))
                    {
                        if (ImGui.MenuItem("Create Log Chip"))
                        {
                            CircuitEditor.chips.Add(new LogChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Log Chip", spawnPos));
                        }
                        
                        if (ImGui.MenuItem("Create Log Warning Chip"))
                        {
                            CircuitEditor.chips.Add(new LogWarningChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Log Warning Chip", spawnPos));
                        }
                        
                        if (ImGui.MenuItem("Create Log Error Chip"))
                        {
                            CircuitEditor.chips.Add(new LogErrorChip(CircuitEditor.GetNextAvaliableChipID(),
                                "Log Error Chip", spawnPos));
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }
        else
        {
            if (ImGui.BeginPopupContextWindow("Chip Menu"))
            {
                if (ImGui.MenuItem("Delete Chip"))
                {
                    CircuitEditor.DeleteChip(hoveredChip);
                    hoveredChip = null;
                }
                ImGui.EndPopup();
            }
        }
    }
}


//Constants

public class BoolConstantChip : Chip
{
    public BoolConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }
}

public class FloatConstantChip : Chip
{
    public FloatConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, [typeof(float)]);
        AddPort("Output", false, [typeof(float)]);
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }
}

public class IntConstantChip : Chip
{
    public IntConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, [typeof(int)]);
        AddPort("Output", false, [typeof(int)]);
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }
}

public class StringConstantChip : Chip
{
    public StringConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, [typeof(string)]);
        AddPort("Output", false, [typeof(string)]);
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }
}

public class Vector2ConstantChip : Chip
{
    public Vector2ConstantChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, [typeof(Vector2)]);
        AddPort("Output", false, [typeof(Vector2)]);
        base.OutputPorts[0].Value.ValueFunction = ConstantOutput;
    }

    public Values ConstantOutput(ChipPort? chipPort)
    {
        return base.InputPorts[0].Value.GetValue();
    }
}

public class AddChip : Chip
{
    public AddChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, [typeof(int), typeof(float)]);
        AddPort("B", true, [typeof(int), typeof(float)]);
        AddPort("Output", false, [typeof(int), typeof(float)]);
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
        else
        {
            foreach (var port in InputPorts)
            {
                port._PortType = chipPort?.PortType ?? null;
                port.UpdateColor();
            }
            foreach (var port in OutputPorts)
            {
                port._PortType = chipPort?.PortType ?? null;
                port.UpdateColor();
            }
        }
    }
}

public class TestButton : Chip
{
    public TestButton(int id, string name, Vector2 position) : base(id, name, position)
    {
        ShowCustomItemOnChip = true;
        AddExecPort("Output", false);
    }

    public override void DisplayCustomItem()
    {
        if (ImGui.Button("Button"))
        {
            OutputExecPorts[0].Execute();
        }
    }
}

// Variables :)
public class BoolVariable : Chip
{
    private Values varValues;
    public BoolVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
    }

    public Values VarOutput(ChipPort port)
    {
        return varValues;
    }

    public override void OnExecute()
    {
        varValues = InputPorts[0].Value.GetValue();
        if (InputPorts[0] is ExecPort)
        {
            Console.WriteLine("Is a exec port");
        }
        base.OnExecute();
    }
}

public class FloatVariable : Chip
{
    private Values varValues;
    public FloatVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(float)]);
        AddPort("Output", false, [typeof(float)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
    }

    public Values VarOutput(ChipPort port)
    {
        return varValues;
    }

    public override void OnExecute()
    {
        varValues = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
}

public class IntVariable : Chip
{
    private Values varValues;
    public IntVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(int)]);
        AddPort("Output", false, [typeof(int)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
    }

    public Values VarOutput(ChipPort port)
    {
        return varValues;
    }

    public override void OnExecute()
    {
        varValues = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
}

public class StringVariable : Chip
{
    private Values varValues;
    public StringVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(string)]);
        AddPort("Output", false, [typeof(string)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
    }

    public Values VarOutput(ChipPort port)
    {
        return varValues;
    }

    public override void OnExecute()
    {
        varValues = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
}

public class Vector2Variable : Chip
{
    private Values varValues;
    public Vector2Variable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(Vector2)]);
        AddPort("Output", false, [typeof(Vector2)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
    }

    public Values VarOutput(ChipPort port)
    {
        return varValues;
    }

    public override void OnExecute()
    {
        varValues = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
}

public class GameObjectVariable : Chip
{
    private Values varValues;
    public GameObjectVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(GameObject)]);
        AddPort("Output", false, [typeof(GameObject)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
    }

    public Values VarOutput(ChipPort port)
    {
        return varValues;
    }

    public override void OnExecute()
    {
        if (InputPorts[0].Value.GetValue().gObj == null)
        {
            GameConsole.Log("Game Object Variable doesn't have a acceptable value", LogType.Error);
            return;
        }
        varValues = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
}

// Logic Chips
public class AndChip : Chip
{
    public AndChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input A", true, [typeof(bool)]);
        AddPort("Input B", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? port)
    {
        Values values = new Values
        {
            b = InputPorts[0].Value.GetValue().b &&  InputPorts[1].Value.GetValue().b
        };
        return values;
    }
}

public class OrChip : Chip
{
    public OrChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input A", true, [typeof(bool)]);
        AddPort("Input B", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? port)
    {
        Values values = new Values
        {
            b = InputPorts[0].Value.GetValue().b ||  InputPorts[1].Value.GetValue().b
        };
        return values;
    }
}

public class NotChip : Chip
{
    public NotChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? port)
    {
        Values values = new Values
        {
            b = !InputPorts[0].Value.GetValue().b
        };
        return values;
    }
}

public class NorChip : Chip
{
    public NorChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input A", true, [typeof(bool)]);
        AddPort("Input B", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? port)
    {
        Values values = new Values
        {
            b = !(InputPorts[0].Value.GetValue().b ||  InputPorts[1].Value.GetValue().b)
        };
        return values;
    }
}

public class NandChip : Chip
{
    public NandChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input A", true, [typeof(bool)]);
        AddPort("Input B", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? port)
    {
        Values values = new Values
        {
            b = !(InputPorts[0].Value.GetValue().b &&  InputPorts[1].Value.GetValue().b)
        };
        return values;
    }
}

public class XorChip : Chip
{
    public XorChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("Input A", true, [typeof(bool)]);
        AddPort("Input B", true, [typeof(bool)]);
        AddPort("Output", false, [typeof(bool)]);
        OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? port)
    {
        Values values = new Values
        {
            b = InputPorts[0].Value.GetValue().b ^ InputPorts[1].Value.GetValue().b
        };
        return values;
    }
} 

//Comparison
public class GreaterThanChip : Chip
{
    public GreaterThanChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, [typeof(int), typeof(float)]);
        AddPort("B", true, [typeof(int), typeof(float)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? chipPort)
    {
        Values value = new Values();
        if (InputPorts[0].PortType == typeof(float))
        {
            value.b = InputPorts[0].Value.GetValue().f > InputPorts[1].Value.GetValue().f;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.b = InputPorts[0].Value.GetValue().i > InputPorts[1].Value.GetValue().i;
        }

        return value;
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort? chipPort)
    {
        if (chipPort?.ConnectedPort == null)
        {
            if ((chipPort == InputPorts[0] && InputPorts[1].ConnectedPort == null) ||
                (chipPort == InputPorts[1] && InputPorts[0].ConnectedPort == null))
            {
                InputPorts[0]._PortType = null;
                InputPorts[1]._PortType = null;
                InputPorts[0].UpdateColor();
                InputPorts[1].UpdateColor();
                return;
            }
        }
        
        if (chipPort == InputPorts[0])
        {
            InputPorts[1]._PortType = InputPorts[0].PortType;
            InputPorts[1].UpdateColor();
        }
        else
        {
            InputPorts[0]._PortType = InputPorts[1].PortType;
            InputPorts[0].UpdateColor();
        }
    }
}

public class GreaterThanOrEqualsChip : Chip
{
    public GreaterThanOrEqualsChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, [typeof(int), typeof(float)]);
        AddPort("B", true, [typeof(int), typeof(float)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? chipPort)
    {
        Values value = new Values();
        if (InputPorts[0].PortType == typeof(float))
        {
            value.b = InputPorts[0].Value.GetValue().f >= InputPorts[1].Value.GetValue().f;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.b = InputPorts[0].Value.GetValue().i >= InputPorts[1].Value.GetValue().i;
        }

        return value;
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort? chipPort)
    {
        if (chipPort?.ConnectedPort == null)
        {
            if ((chipPort == InputPorts[0] && InputPorts[1].ConnectedPort == null) ||
                (chipPort == InputPorts[1] && InputPorts[0].ConnectedPort == null))
            {
                InputPorts[0]._PortType = null;
                InputPorts[1]._PortType = null;
                InputPorts[0].UpdateColor();
                InputPorts[1].UpdateColor();
                return;
            }
        }
        
        if (chipPort == InputPorts[0])
        {
            InputPorts[1]._PortType = InputPorts[0].PortType;
            InputPorts[1].UpdateColor();
        }
        else
        {
            InputPorts[0]._PortType = InputPorts[1].PortType;
            InputPorts[0].UpdateColor();
        }
    }
}

public class LessThanChip : Chip
{
    public LessThanChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, [typeof(int), typeof(float)]);
        AddPort("B", true, [typeof(int), typeof(float)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? chipPort)
    {
        Values value = new Values();
        if (InputPorts[0].PortType == typeof(float))
        {
            value.b = InputPorts[0].Value.GetValue().f < InputPorts[1].Value.GetValue().f;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.b = InputPorts[0].Value.GetValue().i < InputPorts[1].Value.GetValue().i;
        }

        return value;
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort? chipPort)
    {
        if (chipPort?.ConnectedPort == null)
        {
            if ((chipPort == InputPorts[0] && InputPorts[1].ConnectedPort == null) ||
                (chipPort == InputPorts[1] && InputPorts[0].ConnectedPort == null))
            {
                InputPorts[0]._PortType = null;
                InputPorts[1]._PortType = null;
                InputPorts[0].UpdateColor();
                InputPorts[1].UpdateColor();
                return;
            }
        }
        
        if (chipPort == InputPorts[0])
        {
            InputPorts[1]._PortType = InputPorts[0].PortType;
            InputPorts[1].UpdateColor();
        }
        else
        {
            InputPorts[0]._PortType = InputPorts[1].PortType;
            InputPorts[0].UpdateColor();
        }
    }
}

public class LessThanOrEqualsChip : Chip
{
    public LessThanOrEqualsChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, [typeof(int), typeof(float)]);
        AddPort("B", true, [typeof(int), typeof(float)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? chipPort)
    {
        Values value = new Values();
        if (InputPorts[0].PortType == typeof(float))
        {
            value.b = InputPorts[0].Value.GetValue().f <= InputPorts[1].Value.GetValue().f;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.b = InputPorts[0].Value.GetValue().i <= InputPorts[1].Value.GetValue().i;
        }

        return value;
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort? chipPort)
    {
        if (chipPort?.ConnectedPort == null)
        {
            if ((chipPort == InputPorts[0] && InputPorts[1].ConnectedPort == null) ||
                (chipPort == InputPorts[1] && InputPorts[0].ConnectedPort == null))
            {
                InputPorts[0]._PortType = null;
                InputPorts[1]._PortType = null;
                InputPorts[0].UpdateColor();
                InputPorts[1].UpdateColor();
                return;
            }
        }
        
        if (chipPort == InputPorts[0])
        {
            InputPorts[1]._PortType = InputPorts[0].PortType;
            InputPorts[1].UpdateColor();
        }
        else
        {
            InputPorts[0]._PortType = InputPorts[1].PortType;
            InputPorts[0].UpdateColor();
        }
    }
}

public class EqualsChip : Chip
{
    public EqualsChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        AddPort("A", true, [typeof(bool), typeof(int), typeof(float), typeof(string), typeof(Vector2), typeof(GameObject)]);
        AddPort("B", true, [typeof(bool), typeof(int), typeof(float), typeof(string), typeof(Vector2), typeof(GameObject)]);
        AddPort("Output", false, [typeof(bool)]);
        base.OutputPorts[0].Value.ValueFunction = ChipOutput;
    }

    public Values ChipOutput(ChipPort? chipPort)
    {
        Values value = new Values();
        if (InputPorts[0].PortType == typeof(float))
        {
            value.b = InputPorts[0].Value.GetValue().f == InputPorts[1].Value.GetValue().f;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.b = InputPorts[0].Value.GetValue().i == InputPorts[1].Value.GetValue().i;
        }
        else if (InputPorts[0].PortType == typeof(string))
        {
            value.b = InputPorts[0].Value.GetValue().s == InputPorts[1].Value.GetValue().s;
        }
        else if (InputPorts[0].PortType == typeof(bool))
        {
            value.b = InputPorts[0].Value.GetValue().b == InputPorts[1].Value.GetValue().b;
        }
        else if (InputPorts[0].PortType == typeof(Vector2))
        {
            value.b = InputPorts[0].Value.GetValue().v2 == InputPorts[1].Value.GetValue().v2;
        }
        else if (InputPorts[0].PortType == typeof(GameObject))
        {
            value.b = InputPorts[0].Value.GetValue().gObj == InputPorts[1].Value.GetValue().gObj;
        }

        return value;
    }

    public override void UpdateChipConfig()
    {
        
    }

    public override void PortTypeChanged(ChipPort? chipPort)
    {
        if (chipPort?.ConnectedPort == null)
        {
            if ((chipPort == InputPorts[0] && InputPorts[1].ConnectedPort == null) ||
                (chipPort == InputPorts[1] && InputPorts[0].ConnectedPort == null))
            {
                InputPorts[0]._PortType = null;
                InputPorts[1]._PortType = null;
                InputPorts[0].UpdateColor();
                InputPorts[1].UpdateColor();
                return;
            }
        }
        
        if (chipPort == InputPorts[0])
        {
            InputPorts[1]._PortType = InputPorts[0].PortType;
            InputPorts[1].UpdateColor();
        }
        else
        {
            InputPorts[0]._PortType = InputPorts[1].PortType;
            InputPorts[0].UpdateColor();
        }
    }
}

// Event Chips
public class EventChip : Chip
{
    private Event? SelectedEvent;
    private EventMode Mode = EventMode.Receive;
    private Action<EventValues>? ListenerAction;
    private EventValues LastRecievedPayload = new();
    
    private enum EventMode {Receive, Send}

    public EventChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        ConfigurePorts();
    }

    private void ConfigurePorts()
    {
        if (Mode == EventMode.Receive && (!SelectedEvent?.CanReceive ?? false) && SelectedEvent.CanSend)
        {
            Mode = EventMode.Send;
        }
        else if (Mode == EventMode.Send && (!SelectedEvent?.CanSend ?? false) && SelectedEvent.CanReceive)
        {
            Mode = EventMode.Receive;
        }
        InputPorts.Clear();
        OutputPorts.Clear();
        InputExecPorts.Clear();
        OutputExecPorts.Clear();

        if (ListenerAction != null && SelectedEvent != null)
        {
            EventManager.Unsubscribe(SelectedEvent, ListenerAction);
            ListenerAction = null;
        }

        if (SelectedEvent == null)
        {
            Name = "Event (UnConfigured)";
            return;
        }

        Name = SelectedEvent.EventName;

        //Receiving events
        if (Mode == EventMode.Receive)
        {
            AddExecPort("Then", false);
            
            foreach (var (key, value) in SelectedEvent.BaseEventValues.bools)
                AddPort(key, false, [typeof(bool)], true).Value.ValueFunction = (p) => new Values
                    { b = LastRecievedPayload.bools.GetValueOrDefault(p.Name) };
            foreach (var (key, value) in SelectedEvent.BaseEventValues.ints)
                AddPort(key, false, [typeof(int)], true).Value.ValueFunction = (p) => new Values
                    { i = LastRecievedPayload.ints.GetValueOrDefault(p.Name) };
            foreach (var (key, value) in SelectedEvent.BaseEventValues.floats)
                AddPort(key, false, [typeof(float)], true).Value.ValueFunction = (p) => new Values
                    { f = LastRecievedPayload.floats.GetValueOrDefault(p.Name) };
            foreach (var (key, value) in SelectedEvent.BaseEventValues.Vector2s)
                AddPort(key, false, [typeof(Vector2)], true).Value.ValueFunction = (p) => new Values
                    { v2 = LastRecievedPayload.Vector2s.GetValueOrDefault(p.Name) };
            foreach (var (key, value) in SelectedEvent.BaseEventValues.strings)
                AddPort(key, false, [typeof(string)], true).Value.ValueFunction = (p) => new Values
                    { s = LastRecievedPayload.strings.GetValueOrDefault(p.Name) };
            foreach (var (key, value) in SelectedEvent.BaseEventValues.GameObjects)
                AddPort(key, false, [typeof(GameObject)], true).Value.ValueFunction = (p) => new Values
                    { gObj = LastRecievedPayload.GameObjects.GetValueOrDefault(p.Name) };

            ListenerAction = (payload) =>
            {
                LastRecievedPayload = payload;
                OutputExecPorts[0].Execute();
            };
            EventManager.Subscribe(SelectedEvent, ListenerAction);
        }
        //Sending Events
        else
        {
            AddExecPort("Execute", true);
            
            foreach (var (key, value) in SelectedEvent.BaseEventValues.bools)
                AddPort(key, true, [typeof(bool)], true);
            foreach (var (key, value) in SelectedEvent.BaseEventValues.ints)
                AddPort(key, true, [typeof(int)], true);
            foreach (var (key, value) in SelectedEvent.BaseEventValues.floats)
                AddPort(key, true, [typeof(float)], true);
            foreach (var (key, value) in SelectedEvent.BaseEventValues.Vector2s)
                AddPort(key, true, [typeof(Vector2)], true);
            foreach (var (key, value) in SelectedEvent.BaseEventValues.strings)
                AddPort(key, true, [typeof(string)], true);
            foreach (var (key, value) in SelectedEvent.BaseEventValues.GameObjects)
                AddPort(key, true, [typeof(GameObject)], true);
        }
    }

    public override void OnExecute()
    {
        if (Mode == EventMode.Send && SelectedEvent != null)
        {
            var payload = new EventValues();
            foreach (var port in InputPorts)
            {
                if (port.PortType == typeof(bool)) payload.bools[port.Name] = port.Value.GetValue().b;
                else if (port.PortType == typeof(int)) payload.ints[port.Name] = port.Value.GetValue().i;
                else if (port.PortType == typeof(float)) payload.floats[port.Name] = port.Value.GetValue().f;
                else if (port.PortType == typeof(string)) payload.strings[port.Name] = port.Value.GetValue().s;
                else if (port.PortType == typeof(Vector2)) payload.Vector2s[port.Name] = port.Value.GetValue().v2;
                else if (port.PortType == typeof(GameObject)) payload.GameObjects[port.Name] = port.Value.GetValue().gObj;
            }
            
            EventManager.Trigger(SelectedEvent, payload);
        }
    }

    public override void ChipInspectorProperties()
    {
        string preview = SelectedEvent?.EventName ?? "Select an Event...";
        if (ImGui.BeginCombo("Event", preview))
        {
            foreach (var registeredEvent in EventManager.RegisteredEvents)
            {
                if (ImGui.Selectable(registeredEvent.EventName, registeredEvent == SelectedEvent))
                {
                    SelectedEvent = registeredEvent;
                    ConfigurePorts();
                }
            }
            ImGui.EndCombo();
        }

        if (SelectedEvent != null && SelectedEvent.CanConfig)
        {
            ImGui.SameLine();
            if (ImGui.ImageButton("Event_Config", (IntPtr)LoadIcons.icons["Cog.png"], new (10, 10)))
            {
                
            }
        }

        ImGui.SameLine();
        if (ImGui.ImageButton("Event_Create", (IntPtr)LoadIcons.icons["Plus.png"], new (10, 10)))
        {
                
        }

        if (SelectedEvent?.CanReceive ?? false)
        {
            if (ImGui.RadioButton("Recieve", Mode == EventMode.Receive))
            {
                Mode = EventMode.Receive;
                ConfigurePorts();
            }
        }

        if (SelectedEvent?.CanSend ?? false)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Send", Mode == EventMode.Send))
            {
                Mode = EventMode.Send;
                ConfigurePorts();
            }
        }
    }
    
    public override void OnDestroy()
    {
        if (ListenerAction != null && SelectedEvent != null)
        {
            EventManager.Unsubscribe(SelectedEvent, ListenerAction);
        }
    }
}

public class LogChip : Chip
{
    public LogChip(int id, string name, Vector2 pos) : base(id, name, pos, true)
    {
        AddPort("Log", true, [typeof(string)], false);
    }

    public override void OnExecute()
    {
        GameConsole.Log(InputPorts[0].Value.GetValue().s);
    }
}

public class LogWarningChip : Chip
{
    public LogWarningChip(int id, string name, Vector2 pos) : base(id, name, pos, true)
    {
        AddPort("Log", true, [typeof(string)], false);
    }

    public override void OnExecute()
    {
        GameConsole.Log(InputPorts[0].Value.GetValue().s, LogType.Warning);
    }
}

public class LogErrorChip : Chip
{
    public LogErrorChip(int id, string name, Vector2 pos) : base(id, name, pos, true)
    {
        AddPort("Log", true, [typeof(string)], false);
    }

    public override void OnExecute()
    {
        GameConsole.Log(InputPorts[0].Value.GetValue().s, LogType.Error);
    }
}