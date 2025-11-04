using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Windows.Markup;
using CSCanbulatEngine.Audio;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Microsoft.IdentityModel.Tokens;
using Silk.NET.Input;
using SixLabors.ImageSharp;
using RectangleF = System.Drawing.RectangleF;

namespace CSCanbulatEngine.Circuits;

public class CircuitChips
{
    private static Vector2 spawnPos = Vector2.Zero;
    
    private static Chip? hoveredChip = null;
    
    private static byte[] ChipSearchBuffer = new  byte[256];
    
    //To add chip: make class, add to all chips, add to context menu
    
    //!! Executing chips require try and catch statement !!
    
    private static readonly List<(string Path, Func<Vector2, Chip> CreateAction)> allChips = new()
    {
        ("Event Chip", (pos) => new EventChip(CircuitEditor.GetNextAvaliableChipID(), "Event Chip", pos)),
        ("Test Button", (pos) => new TestButton(CircuitEditor.GetNextAvaliableChipID(), "Test Button", pos)),
        
        ("Constants/Bool Constant", (pos) => new BoolConstantChip(CircuitEditor.GetNextAvaliableChipID(), "Bool Constant", pos)),
        ("Constants/Float Constant", (pos) => new FloatConstantChip(CircuitEditor.GetNextAvaliableChipID(), "Float Constant", pos)),
        ("Constants/Int Constant", (pos) => new IntConstantChip(CircuitEditor.GetNextAvaliableChipID(), "Int Constant", pos)),
        ("Constants/String Constant", (pos) => new StringConstantChip(CircuitEditor.GetNextAvaliableChipID(), "String Constant", pos)),
        ("Constants/Vector2 Constant", (pos) => new Vector2ConstantChip(CircuitEditor.GetNextAvaliableChipID(), "Vector2 Constant", pos)),
        ("Constants/Audio Info Constant", (pos) => new AudioConstant(CircuitEditor.GetNextAvaliableChipID(), "Audio Constant", pos)),
        
        ("Math/Add", (pos) => new AddChip(CircuitEditor.GetNextAvaliableChipID(), "Add", pos)),
        
        ("Logic/If", (pos) => new IfChip(CircuitEditor.GetNextAvaliableChipID(), "If", pos)),
        ("Logic/Not", (pos) => new NotChip(CircuitEditor.GetNextAvaliableChipID(), "Not Chip", pos)),
        ("Logic/And", (pos) => new AndChip(CircuitEditor.GetNextAvaliableChipID(), "And Chip", pos)),
        ("Logic/Or", (pos) => new OrChip(CircuitEditor.GetNextAvaliableChipID(), "Or Chip", pos)),
        ("Logic/Nor", (pos) => new NorChip(CircuitEditor.GetNextAvaliableChipID(), "Nor Chip", pos)),
        ("Logic/Nand", (pos) => new NandChip(CircuitEditor.GetNextAvaliableChipID(), "Nand Chip", pos)),
        ("Logic/Xor", (pos) => new XorChip(CircuitEditor.GetNextAvaliableChipID(), "Xor Chip", pos)),
        
        ("Comparison/Equals", (pos) => new EqualsChip(CircuitEditor.GetNextAvaliableChipID(), "Equals Chip", pos)),
        ("Comparison/Less Than", (pos) => new LessThanChip(CircuitEditor.GetNextAvaliableChipID(), "Less Than Chip", pos)),
        ("Comparison/Less Than Or Equals", (pos) => new LessThanOrEqualsChip(CircuitEditor.GetNextAvaliableChipID(), "Less Than Or Equals Chip", pos)),
        ("Comparison/Greater Than", (pos) => new GreaterThanChip(CircuitEditor.GetNextAvaliableChipID(), "Greater Than Chip", pos)),
        ("Comparison/Greater Than Or Equals", (pos) => new GreaterThanOrEqualsChip(CircuitEditor.GetNextAvaliableChipID(), "Greater Than Or Equals Chip", pos)),
        
        ("Variables/Bool Variable", (pos) => new BoolVariable(CircuitEditor.GetNextAvaliableChipID(), "Bool Variable", pos)),
        ("Variables/Float Variable", (pos) => new FloatVariable(CircuitEditor.GetNextAvaliableChipID(), "Float Variable", pos)),
        ("Variables/Int Variable", (pos) => new IntVariable(CircuitEditor.GetNextAvaliableChipID(), "Int Variable", pos)),
        ("Variables/String Variable", (pos) => new StringVariable(CircuitEditor.GetNextAvaliableChipID(), "String Variable", pos)),
        ("Variables/Vector2 Variable", (pos) => new Vector2Variable(CircuitEditor.GetNextAvaliableChipID(), "Vector2 Variable", pos)),
        ("Variables/GameObject Variable", (pos) => new GameObjectVariable(CircuitEditor.GetNextAvaliableChipID(), "GameObject Variable", pos)),
        ("Variables/Audio Info Variable", (pos) => new AudioInfoVariable(CircuitEditor.GetNextAvaliableChipID(), "Audio Info Variable", pos)),
        ("Variables/Component Holder Variable", (pos) => new ComponentHolderVariable(CircuitEditor.GetNextAvaliableChipID(), "Component Holder Variable", pos)),

        ("Object/This", (pos) => new thisChip(CircuitEditor.GetNextAvaliableChipID(), "This", pos)),
        ("Object/Find By ID", (pos) => new FindObjectByID(CircuitEditor.GetNextAvaliableChipID(), "Find Object By ID Chip", pos)),
        ("Object/Find First By Tag", (pos) => new FindFirstObjectWithTag(CircuitEditor.GetNextAvaliableChipID(), "Find First Object With Tag", pos)),
        ("Object/Find All By Tag", (pos) => new FindAllObjectsWithTag(CircuitEditor.GetNextAvaliableChipID(), "Find All Objects With Tag", pos)),
        ("Object/Get Component", (pos) => new GetComponentChip(CircuitEditor.GetNextAvaliableChipID(), "Get Component", pos)),
        ("Object/Has Component", (pos) => new HasComponentChip(CircuitEditor.GetNextAvaliableChipID(), "Has Component", pos)),
        
        ("Input/Is Key Down", (pos) => new IsKeyDownChip(CircuitEditor.GetNextAvaliableChipID(), "Is Key Down", pos)),
        ("Input/Is Key Pressed", (pos) => new IsPressedThisFrameChip(CircuitEditor.GetNextAvaliableChipID(), "Is Key Pressed This Frame", pos)),
        ("Input/Is Key Released", (pos) => new IsKeyReleasedThisFrameChip(CircuitEditor.GetNextAvaliableChipID(), "Is Key Released This Frame", pos)),
        
        ("Miscellaneous/Log", (pos) => new LogChip(CircuitEditor.GetNextAvaliableChipID(), "Log Chip", pos)),
        ("Miscellaneous/Log Warning", (pos) => new LogWarningChip(CircuitEditor.GetNextAvaliableChipID(), "Log Warning Chip", pos)),
        ("Miscellaneous/Log Error", (pos) => new LogErrorChip(CircuitEditor.GetNextAvaliableChipID(), "Log Error Chip", pos)),
        ("Miscellaneous/List/Get Element At", (pos) => new GetElementAt(CircuitEditor.GetNextAvaliableChipID(), "Get Element At", pos)),
        ("Miscellaneous/List/Create List", (pos) => new CreateList(CircuitEditor.GetNextAvaliableChipID(), "Create List", pos)),
        ("Miscellaneous/Vector2/Create", (pos) => new Vector2Create(CircuitEditor.GetNextAvaliableChipID(), "Vector2 Create", pos)),
        ("Miscellaneous/Transform/Set World Position", (pos) => new SetWorldPositionChip(CircuitEditor.GetNextAvaliableChipID(), "Set World Position", pos)),
        ("Miscellaneous/Audio/Play Audio", (pos) => new PlayAudioChip(CircuitEditor.GetNextAvaliableChipID(), "Play Audio", pos)),
    };
    
    
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
                CreateContextMenu();
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

    public static void CreateContextMenu()
    {
        

        if (ImGui.BeginMenu("Create Chips"))
        {
            ImGui.InputText("Search", ChipSearchBuffer, (uint)ChipSearchBuffer.Length);
            string searchText = Encoding.UTF8.GetString(ChipSearchBuffer).TrimEnd('\0').ToLower();
            ImGui.Separator();
            
            
            if (String.IsNullOrEmpty(searchText))
            {
                if (ImGui.MenuItem("Event Chip"))
                {
                    CircuitEditor.chips.Add(new EventChip(CircuitEditor.GetNextAvaliableChipID(), "Event Chip",
                        spawnPos));
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

                    if (ImGui.MenuItem("Create Audio Info Constant"))
                    {
                        CircuitEditor.chips.Add(new AudioConstant(CircuitEditor.GetNextAvaliableChipID(),
                            "Audio Constant", spawnPos));
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
                    if (ImGui.MenuItem("Create If Chip"))
                    {
                        CircuitEditor.chips.Add(new IfChip(CircuitEditor.GetNextAvaliableChipID(), "If", spawnPos));
                    }

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
                    
                    if (ImGui.MenuItem("Create Audio Info Variable"))
                    {
                        CircuitEditor.chips.Add(new AudioInfoVariable(CircuitEditor.GetNextAvaliableChipID(),
                            "Audio Info Variable", spawnPos));
                    }
                    
                    if (ImGui.MenuItem("Create Component Holder Variable"))
                    {
                        CircuitEditor.chips.Add(new ComponentHolderVariable(CircuitEditor.GetNextAvaliableChipID(),
                            "Component Holder Variable", spawnPos));
                    }


                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Object Chips"))
                {
                    if (ImGui.MenuItem("Create This Chip"))
                    {
                        CircuitEditor.chips.Add(new thisChip(CircuitEditor.GetNextAvaliableChipID(), "This", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Find Object By ID Chip"))
                    {
                        CircuitEditor.chips.Add(new FindObjectByID(CircuitEditor.GetNextAvaliableChipID(),
                            "Find Object By ID Chip", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Find First Object By Tag Chip"))
                    {
                        CircuitEditor.chips.Add(new FindFirstObjectWithTag(CircuitEditor.GetNextAvaliableChipID(),
                            "Find First Object With Tag", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Find All Objects By Tag Chip"))
                    {
                        CircuitEditor.chips.Add(new FindAllObjectsWithTag(CircuitEditor.GetNextAvaliableChipID(),
                            "Find All Objects With Tag", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Get Component Chip"))
                    {
                        CircuitEditor.chips.Add(new GetComponentChip(CircuitEditor.GetNextAvaliableChipID(),
                            "Get Component", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Has Component Chip"))
                    {
                        CircuitEditor.chips.Add(new HasComponentChip(CircuitEditor.GetNextAvaliableChipID(),
                            "Has Component", spawnPos));
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Input Manager Chips"))
                {
                    if (ImGui.MenuItem("Create Is Key Down Chip"))
                    {
                        CircuitEditor.chips.Add(new IsKeyDownChip(CircuitEditor.GetNextAvaliableChipID(), "Is Key Down",
                            spawnPos));
                    }

                    if (ImGui.MenuItem("Create Is Key Pressed This Frame Chip"))
                    {
                        CircuitEditor.chips.Add(new IsPressedThisFrameChip(CircuitEditor.GetNextAvaliableChipID(),
                            "Is Key Pressed This Frame", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Is Key Released This Frame Chip"))
                    {
                        CircuitEditor.chips.Add(new IsKeyReleasedThisFrameChip(CircuitEditor.GetNextAvaliableChipID(),
                            "Is Key Released This Frame", spawnPos));
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

                    if (ImGui.MenuItem("Create Get Element At Chip"))
                    {
                        CircuitEditor.chips.Add(new GetElementAt(CircuitEditor.GetNextAvaliableChipID(),
                            "Get Element At", spawnPos));
                    }

                    if (ImGui.MenuItem("Create List Chip"))
                    {
                        CircuitEditor.chips.Add(new CreateList(CircuitEditor.GetNextAvaliableChipID(), "Create List",
                            spawnPos));
                    }

                    if (ImGui.MenuItem("Create Vector2 Create Chip"))
                    {
                        CircuitEditor.chips.Add(new Vector2Create(CircuitEditor.GetNextAvaliableChipID(),
                            "Vector2 Create", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Set World Position Chip"))
                    {
                        CircuitEditor.chips.Add(new SetWorldPositionChip(CircuitEditor.GetNextAvaliableChipID(),
                            "Set World Position", spawnPos));
                    }

                    if (ImGui.MenuItem("Create Play Audio Chip"))
                    {
                        CircuitEditor.chips.Add(new PlayAudioChip(CircuitEditor.GetNextAvaliableChipID(), "Play Audio",
                            spawnPos));
                    }

                    ImGui.EndMenu();
                }
            }
            else
            {
                foreach (var (path, createAction) in allChips)
                {
                    if (path.ToLower().Contains(searchText.ToLower()))
                    {
                        if (ImGui.MenuItem(path))
                        {
                            var newChip = createAction(spawnPos);
                            CircuitEditor.chips.Add(newChip);
                        }
                    }
                }
            }
            
            ImGui.EndMenu();
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
            value.Float = InputPorts[0].Value.GetValue().Float + InputPorts[1].Value.GetValue().Float;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.Int = InputPorts[0].Value.GetValue().Int + InputPorts[1].Value.GetValue().Int;
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
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values { Bool = false };
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
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
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values { Float = 0 };
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
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
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values { Int = 0 };
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
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
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values { String = "" };
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
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
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values { Vector2 = Vector2.Zero };
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
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
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values { GameObject = null };
    }

    public override void OnExecute()
    {
        if (InputPorts[0].Value.GetValue().GameObject == null)
        {
            GameConsole.Log("[GameObject Variable] GameObject is null", LogType.Error);
            return;
        }
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
    }
}

public class AudioInfoVariable : Chip
{
    private Values varValues;
    public AudioInfoVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(AudioInfo)]);
        AddPort("Output", false, [typeof(AudioInfo)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values {AudioInfo = null};
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
    }
}

public class ComponentHolderVariable : Chip
{
    private Values varValues;
    public ComponentHolderVariable(int id, string name, Vector2 position) : base(id, name, position, true)
    {
        varValues = new Values();
        AddPort("Input", true, [typeof(ComponentHolder)]);
        AddPort("Output", false, [typeof(ComponentHolder)]);
        base.OutputPorts[0].Value.ValueFunction = VarOutput;
        nameBuffer = new byte[128];
        byte[] quickNameBuffer = Encoding.UTF8.GetBytes(name);
        Array.Copy(quickNameBuffer, nameBuffer, quickNameBuffer.Length);
    }

    public Values VarOutput(ChipPort port)
    {
        if (VariableManager.Variables.TryGetValue(Name, out var value))
        {
            return value;
        }
        
        return new Values {ComponentHolder = null};
    }

    public override void OnExecute()
    {
        VariableManager.Variables[Name] = InputPorts[0].Value.GetValue();
        base.OnExecute();
    }
    
    public override void ChipInspectorProperties()
    {
        if (ImGui.InputText("Name", nameBuffer,128))
        {
            
        }
        ImGui.SameLine();
        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            string oldName = Name;
            
            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                if (VariableManager.Variables.TryGetValue(oldName, out var value))
                {
                    VariableManager.Variables.Remove(oldName);
                    VariableManager.Variables[newName] = value;
                }
                Name = newName;
            }
        }
    }
    
    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        
        properties["Name"] = Name;

        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Name", out var name))
        {
            Name = name;
        }
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
            Bool = InputPorts[0].Value.GetValue().Bool &&  InputPorts[1].Value.GetValue().Bool
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
            Bool = InputPorts[0].Value.GetValue().Bool ||  InputPorts[1].Value.GetValue().Bool
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
            Bool = !InputPorts[0].Value.GetValue().Bool
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
            Bool = !(InputPorts[0].Value.GetValue().Bool ||  InputPorts[1].Value.GetValue().Bool)
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
            Bool = !(InputPorts[0].Value.GetValue().Bool &&  InputPorts[1].Value.GetValue().Bool)
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
            Bool = InputPorts[0].Value.GetValue().Bool ^ InputPorts[1].Value.GetValue().Bool
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
            value.Bool = InputPorts[0].Value.GetValue().Float > InputPorts[1].Value.GetValue().Float;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.Bool = InputPorts[0].Value.GetValue().Int > InputPorts[1].Value.GetValue().Int;
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
            value.Bool = InputPorts[0].Value.GetValue().Float >= InputPorts[1].Value.GetValue().Float;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.Bool = InputPorts[0].Value.GetValue().Int >= InputPorts[1].Value.GetValue().Int;
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
            value.Bool = InputPorts[0].Value.GetValue().Float < InputPorts[1].Value.GetValue().Float;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.Bool = InputPorts[0].Value.GetValue().Int < InputPorts[1].Value.GetValue().Int;
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
            value.Bool = InputPorts[0].Value.GetValue().Float <= InputPorts[1].Value.GetValue().Float;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.Bool = InputPorts[0].Value.GetValue().Int <= InputPorts[1].Value.GetValue().Int;
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
            value.Bool = InputPorts[0].Value.GetValue().Float == InputPorts[1].Value.GetValue().Float;
        }
        else if (InputPorts[0].PortType == typeof(int))
        {
            value.Bool = InputPorts[0].Value.GetValue().Int == InputPorts[1].Value.GetValue().Int;
        }
        else if (InputPorts[0].PortType == typeof(string))
        {
            value.Bool = InputPorts[0].Value.GetValue().String == InputPorts[1].Value.GetValue().String;
        }
        else if (InputPorts[0].PortType == typeof(bool))
        {
            value.Bool = InputPorts[0].Value.GetValue().Bool == InputPorts[1].Value.GetValue().Bool;
        }
        else if (InputPorts[0].PortType == typeof(Vector2))
        {
            value.Bool = InputPorts[0].Value.GetValue().Vector2 == InputPorts[1].Value.GetValue().Vector2;
        }
        else if (InputPorts[0].PortType == typeof(GameObject))
        {
            value.Bool = InputPorts[0].Value.GetValue().GameObject == InputPorts[1].Value.GetValue().GameObject;
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

// Event Chip
public class EventChip : Chip
{
    public Event? SelectedEvent;
    private EventMode Mode = EventMode.Receive;
    private Action<EventValues>? ListenerAction;
    private EventValues LastRecievedPayload = new();
    
    public int portSelectedIndex = 0;
    
    private int? portIndexToConfig = null;
    private byte[] portNameChangeBuffer = new byte[128];
    
    private enum EventMode {Receive, Send}

    public EventChip(int id, string name, Vector2 position) : base(id, name, position)
    {
        ConfigurePorts();
    }

    public void ConfigurePorts()
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
            
            foreach (var key in SelectedEvent.baseValues.bools)
                AddPort(key, false, [typeof(bool)], true).Value.ValueFunction = (p) => new Values
                    { Bool = LastRecievedPayload.bools.GetValueOrDefault(p.Name) };
            foreach (var key in SelectedEvent.baseValues.ints)
                AddPort(key, false, [typeof(int)], true).Value.ValueFunction = (p) => new Values
                    { Int = LastRecievedPayload.ints.GetValueOrDefault(p.Name) };
            foreach (var key in SelectedEvent.baseValues.floats)
                AddPort(key, false, [typeof(float)], true).Value.ValueFunction = (p) => new Values
                    { Float = LastRecievedPayload.floats.GetValueOrDefault(p.Name) };
            foreach (var key in SelectedEvent.baseValues.Vector2s)
                AddPort(key, false, [typeof(Vector2)], true).Value.ValueFunction = (p) => new Values
                    { Vector2 = LastRecievedPayload.Vector2s.GetValueOrDefault(p.Name) };
            foreach (var key in SelectedEvent.baseValues.strings)
                AddPort(key, false, [typeof(string)], true).Value.ValueFunction = (p) => new Values
                    { String = LastRecievedPayload.strings.GetValueOrDefault(p.Name) };
            foreach (var key in SelectedEvent.baseValues.GameObjects)
                AddPort(key, false, [typeof(GameObject)], true).Value.ValueFunction = (p) => new Values
                    { GameObject = LastRecievedPayload.GameObjects.GetValueOrDefault(p.Name) };

            ListenerAction = (payload) =>
            {
                if ((SelectedEvent.EventName != "OnStart" && SelectedEvent.EventName != "OnUpdate") || LoadedInBackground)
                {
                    LastRecievedPayload = payload;
                    OutputExecPorts[0].Execute();
                }
            };
            EventManager.Subscribe(SelectedEvent, ListenerAction);
        }
        //Sending Events
        else
        {
            AddExecPort("Execute", true);
            AddExecPort("Then", false);
            
            foreach (var key in SelectedEvent.baseValues.bools)
                AddPort(key, true, [typeof(bool)], true);
            foreach (var key in SelectedEvent.baseValues.ints)
                AddPort(key, true, [typeof(int)], true);
            foreach (var key in SelectedEvent.baseValues.floats)
                AddPort(key, true, [typeof(float)], true);
            foreach (var key in SelectedEvent.baseValues.Vector2s)
                AddPort(key, true, [typeof(Vector2)], true);
            foreach (var key in SelectedEvent.baseValues.strings)
                AddPort(key, true, [typeof(string)], true);
            foreach (var key in SelectedEvent.baseValues.GameObjects)
                AddPort(key, true, [typeof(GameObject)], true);
        }

        Size = new Vector2(Size.X, ((CircuitEditor.portSpacing/CircuitEditor.Zoom) * (SelectedEvent.baseValues.bools.Count() + SelectedEvent.baseValues.floats.Count() + SelectedEvent.baseValues.ints.Count() + SelectedEvent.baseValues.strings.Count() + SelectedEvent.baseValues.Vector2s.Count() + SelectedEvent.baseValues.GameObjects.Count())) + 75);
    }

    public override void OnExecute()
    {
        if (Mode == EventMode.Send && SelectedEvent != null)
        {
            var payload = new EventValues();
            foreach (var port in InputPorts)
            {
                if (port.PortType == typeof(bool)) payload.bools[port.Name] = port.Value.GetValue().Bool;
                else if (port.PortType == typeof(int)) payload.ints[port.Name] = port.Value.GetValue().Int;
                else if (port.PortType == typeof(float)) payload.floats[port.Name] = port.Value.GetValue().Float;
                else if (port.PortType == typeof(string)) payload.strings[port.Name] = port.Value.GetValue().String;
                else if (port.PortType == typeof(Vector2)) payload.Vector2s[port.Name] = port.Value.GetValue().Vector2;
                else if (port.PortType == typeof(GameObject)) payload.GameObjects[port.Name] = port.Value.GetValue().GameObject;
            }
            
            EventManager.Trigger(SelectedEvent, payload);
            OutputExecPorts[0].Execute();
        }
    }

    public List<List<string>> allPortTypes;
    public List<string> ports;
    public List<Type> portTypes;

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

        ImGui.SameLine();
        if (ImGui.ImageButton("Event_Create", (IntPtr)LoadIcons.icons["Plus.png"], new (25)))
        {
            var theEvent = new Event("New Event " +
                                  EventManager.RegisteredEvents.Count(e => e.EventName.Contains("New Event")));
            EventManager.RegisterEvent(theEvent);
            SelectedEvent = theEvent;
            ConfigurePorts();
        }
        
        if (SelectedEvent != null && SelectedEvent.CanConfig)
        {
            ImGui.SameLine();
            if (ImGui.ImageButton("Event_Delete", (IntPtr)LoadIcons.icons["Trash.png"], new (25)))
            {
                if (SelectedEvent != null)
                {
                    EventManager.DeleteEvent(SelectedEvent);
                }
            }
        }

        if (SelectedEvent != null && SelectedEvent.CanConfig)
        {
            if (ImGui.InputText("Name", nameBuffer,128))
            {
            
            }
            ImGui.SameLine();
            if (ImGui.Button("Set"))
            {
                string newName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
                string oldName = Name;
            
                if (!string.IsNullOrWhiteSpace(newName) && oldName != newName && EventManager.RegisteredEvents.All(e => newName != e.EventName))
                {
                    var theEvent = EventManager.RegisteredEvents.Find(e => e == SelectedEvent);
                    theEvent.EventName = newName;
                    Name = newName;
                }
            }
        }

        if (SelectedEvent?.CanSend ?? false)
        {
            if (ImGui.RadioButton("Send", Mode == EventMode.Send))
            {
                Mode = EventMode.Send;
                ConfigurePorts();
            }
        }
        
        if (SelectedEvent?.CanReceive ?? false)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Recieve", Mode == EventMode.Receive))
            {
                Mode = EventMode.Receive;
                ConfigurePorts();
            }
        }
        
        if (SelectedEvent != null && SelectedEvent.CanConfig)
        {
            ports = new List<string>();
            portTypes = new List<Type>();

            BaseEventValues baseValues = SelectedEvent.baseValues;
            allPortTypes = new List<List<string>>() {baseValues.bools, baseValues.floats, baseValues.ints, baseValues.strings, baseValues.Vector2s, baseValues.GameObjects, baseValues.AudioInfos, baseValues.ComponentHolders};
            
            foreach (var port in baseValues.bools)
            {
                ports.Add(port); 
                portTypes.Add(typeof(bool));
            }

            foreach (var port in baseValues.floats)
            {
                ports.Add(port);
                portTypes.Add(typeof(float));
            }

            foreach (var port in baseValues.ints)
            {
                ports.Add(port);
                portTypes.Add(typeof(int));
            }
            
            foreach (var port in baseValues.strings)
            {
                ports.Add(port);
                portTypes.Add(typeof(string));
            }

            foreach (var port in baseValues.Vector2s)
            {
                ports.Add(port);
                portTypes.Add(typeof(Vector2));
            }

            foreach (var port in baseValues.GameObjects)
            {
                ports.Add(port);
                portTypes.Add(typeof(GameObject));
            }

            foreach (var port in baseValues.AudioInfos)
            {
                ports.Add(port);
                portTypes.Add(typeof(AudioInfo));
            }

            foreach (var port in baseValues.ComponentHolders)
            {
                ports.Add(port);
                portTypes.Add(typeof(ComponentHolder));
            }
            
            //Ports Menu
            if (ImGui.ImageButton("InputPortAddButton", (IntPtr)LoadIcons.icons["Plus.png"], new Vector2(25)))
            {
                baseValues.bools.Add("New Port " + ports.FindAll(e => e.Split(" ")[0] + " " + e.Split(" ")[1] == "New Port").Count());
                ConfigureAllChipsToEvent();
            }

            if (ports.Count > 0)
            {
                ImGui.SameLine();
                
                if (portSelectedIndex > ports.Count - 1 || portSelectedIndex == -1)
                {
                    portSelectedIndex = ports.Count - 1;
                }

                if (ImGui.ImageButton("InputPortRemoveButton", (IntPtr)LoadIcons.icons["Trash.png"], new Vector2(25)))
                {
                    int portTypeIndex = GetPortTypeIndex(portTypes[portSelectedIndex]);
                    int selectedIndex = portSelectedIndex;

                    for (int i = portTypeIndex; i > 0; i--)
                    {
                        selectedIndex -= allPortTypes[i].Count;
                    }

                    allPortTypes[portTypeIndex].RemoveAt(selectedIndex);
                    ConfigureAllChipsToEvent();
                }

                ImGui.SameLine();

                if (ImGui.ImageButton("InputPortConfigButton", (IntPtr)LoadIcons.icons["Cog.png"], new Vector2(25)))
                {
                    // Engine.portConfigWindowOpen = false;
                    // ConfigWindows.EnableEventPortConfigWindow(portSelectedIndex, this);

                    portIndexToConfig = portSelectedIndex;
                    
                    var nameBytes = Encoding.UTF8.GetBytes(ports[portIndexToConfig.Value]);
                    Array.Clear(portNameChangeBuffer, 0, portNameChangeBuffer.Length);
                    Array.Copy(nameBytes, portNameChangeBuffer, nameBytes.Length);
                    
                    ImGui.OpenPopup("PortContextMenu");
                }
            }

            if (ImGui.BeginPopup("PortContextMenu"))
            {
                int index = portSelectedIndex;
                
                ImGui.Text("Configure Port");
                ImGui.Separator();

                ImGui.PushItemWidth(200);
                ImGui.InputText("Name", portNameChangeBuffer,128);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (ImGui.Button("Set"))
                {
                    string newName = Encoding.UTF8.GetString(portNameChangeBuffer).TrimEnd('\0');
                    string oldName = ports[portIndexToConfig.Value];

                    // Check if the name is valid and actually changed
                    if (!string.IsNullOrWhiteSpace(newName) && oldName != newName && ports.All(e => e != newName))
                    {
                        int portTypeIndex = GetPortTypeIndex(portTypes[portIndexToConfig.Value]);
                        int selectedIndex = portIndexToConfig.Value;

                        for (int i = 0; i < portTypeIndex; i++)
                        {
                            selectedIndex -= allPortTypes[i].Count;
                        }

                        allPortTypes[portTypeIndex][selectedIndex] = newName;
                        ConfigureAllChipsToEvent();
                    }
                }

                if (ImGui.BeginCombo("Type", TypeHelper.GetName(portTypes[index])))
                {
                    List<Type> availableTypes = [typeof(bool), typeof(float), typeof(int), typeof(string), typeof(Vector2), typeof(GameObject), typeof(AudioInfo), typeof(ComponentHolder)];

                    foreach (var type in availableTypes)
                    {
                        if (ImGui.Selectable(TypeHelper.GetName(type), type == portTypes[index]))
                        {
                            ChangePortType(index, type);
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.EndPopup();
            }

            ImGui.BeginChild("CustomPortList", new Vector2(0, 150), ImGuiChildFlags.Borders);

            for (int i = 0; i < ports.Count; i++)
            {
                string portName = ports[i];
                string typeName = TypeHelper.GetName(portTypes[i]);

                string selectableLabel = $"{portName}##{i}";

                if (ImGui.Selectable(selectableLabel, i == portSelectedIndex, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    portSelectedIndex = i;
                }

                float typeNameWidth = ImGui.CalcTextSize(typeName).X;
                float columnWidth = ImGui.GetContentRegionAvail().X - 5;
                
                ImGui.SameLine(ImGui.GetCursorPos().X + (columnWidth - typeNameWidth));
                
                ImGui.TextDisabled(typeName);
            }
            ImGui.EndChild();
        }
    }

    public void ConfigureAllChipsToEvent()
    {
        List<EventChip>? allEventChips = CircuitEditor.chips.FindAll(e => e is EventChip).ConvertAll(e => e as EventChip);
        if (allEventChips is null && allEventChips.Count == 0)
        {
            return;
        }

        foreach (EventChip chip in allEventChips)
        {
            if (chip.SelectedEvent == SelectedEvent) chip.ConfigurePorts();
        }
    }
    
    public void ConfigureAllChipsToEvent(Event theEvent)
    {
        List<EventChip>? allEventChips = CircuitEditor.chips.FindAll(e => e is EventChip).ConvertAll(e => e as EventChip);
        if (allEventChips is null && allEventChips.Count == 0)
        {
            return;
        }

        foreach (EventChip chip in allEventChips)
        {
            if (chip.SelectedEvent == theEvent) chip.ConfigurePorts();
        }
    }
    
    public void ChangePortType(int portIndex, Type type)
    {
        int portTypeIndex = GetPortTypeIndex(portTypes[portIndex]);
        int selectedIndex = portIndex;

        for (int i = 0; i < portTypeIndex; i++)
        {
            selectedIndex -= allPortTypes[i].Count;
        }

        string portToChange = allPortTypes[portTypeIndex][selectedIndex];
        allPortTypes[portTypeIndex].RemoveAt(selectedIndex);
        allPortTypes[GetPortTypeIndex(type)].Add(portToChange);
        portSelectedIndex = ports.Count() - 1;
        ConfigWindows.portIndexToConfig = ports.Count() - 1;
        portTypes[portSelectedIndex] = type;
        ConfigureAllChipsToEvent();
    }

    public int GetPortTypeIndex(Type type)
    {
        if (type == typeof(bool))
        {
            return 0;
        }
        else if (type == typeof(float))
        {
            return 1;
        }
        else if (type == typeof(int))
        {
            return 2;
        }
        else if (type == typeof(string))
        {
            return 3;
        }
        else if (type == typeof(Vector2))
        {
            return 4;
        }
        else if (type == typeof(GameObject))
        {
            return 5;
        }
        else if (type == typeof(AudioInfo))
        {
            return 6;
        }
        else if (type == typeof(ComponentHolder))
        {
            return 7;
        }

        return 0;
    }
    
    public void ResetToUnconfigured()
    {
        if (ListenerAction != null && SelectedEvent != null)
        {
            EventManager.Unsubscribe(SelectedEvent, ListenerAction);
        }
        ListenerAction = null;
        SelectedEvent = null;
        ConfigurePorts();
    }
    
    public override void OnDestroy()
    {
        if (ListenerAction != null && SelectedEvent != null)
        {
            EventManager.Unsubscribe(SelectedEvent, ListenerAction);
        }
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        var properties = new Dictionary<string, string>();
        if (SelectedEvent != null)
        {
            properties["SelectedEvent"] = SelectedEvent.EventName;
        }

        properties["Mode"] = Mode.ToString();
        return properties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("SelectedEvent", out var eventName))
        {
            SelectedEvent = EventManager.RegisteredEvents.Find(e => e.EventName == eventName);
        }

        if (properties.TryGetValue("Mode", out var modeName))
        {
            Mode = (EventMode)Enum.Parse(typeof(EventMode), modeName);
        }

        ConfigurePorts();
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
        GameConsole.Log(InputPorts[0].Value.GetValue().String);
        base.OnExecute();
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
        GameConsole.Log(InputPorts[0].Value.GetValue().String, LogType.Warning);
        base.OnExecute();
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
        GameConsole.Log(InputPorts[0].Value.GetValue().String, LogType.Error);
        base.OnExecute();
    }
}

public class FindObjectByID : Chip
{
    public FindObjectByID(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("ID", true, [typeof(int)], true);
        AddPort("GameObject", false, [typeof(GameObject)], true);
        OutputPorts[0].Value.ValueFunction = Function;
    }

    public Values Function(ChipPort? chipPort)
    {
        return new Values { GameObject = GameObject.FindGameObject(InputPorts[0].Value.GetValue().Int) };
    }
}

public class FindFirstObjectWithTag : Chip
{
    public FindFirstObjectWithTag(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("Tag", true, [typeof(string)], true);
        AddPort("GameObject", false, [typeof(GameObject)], true);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        return new Values()
            { GameObject = Engine.currentScene.GameObjects.Find(e => e.Tags.Contains(InputPorts[0].Value.GetValue().String)) };
        
    }
}

public class FindAllObjectsWithTag : Chip
{
    public FindAllObjectsWithTag(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        Size = new Vector2(250, 100);
        AddPort("Tag", true, [typeof(string)], true);
        AddPort("List<GameObject>", false, [typeof(List<GameObject>)], true);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        return new Values()
        {
            GameObjectList = Engine.currentScene.GameObjects.FindAll(e => e.Tags.Contains(InputPorts[0].Value.GetValue().String))
        };
    }
}

public class CreateList : Chip
{
    private Type? ChipPortsType = null;
    public CreateList(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("List", false,
        [
            typeof(List<bool>), typeof(List<int>), typeof(List<float>), typeof(List<string>), typeof(List<Vector2>),
            typeof(List<GameObject>), typeof(List<AudioInfo>), typeof(List<ComponentHolder>)
        ], true);
        
        OutputPorts[0].Value.ValueFunction = ListFunction;
    }

    public override void PortTypeChanged(ChipPort port)
    {
    }

    public override void ChildPortIsConnected(ChipPort childPort, ChipPort portConnectedTo)
    {
        if (ChipPortsType == null && childPort.PortType != null)
        {
            ChipPortsType = childPort.PortType;
            foreach (var thePort in InputPorts)
            {
                thePort.PortType = ChipPortsType;
                thePort.UpdateColor();
            }
            
            OutputPorts[0].PortType = TypeHelper.GetListType(ChipPortsType);
            OutputPorts[0].UpdateColor();
        }

        if (ChipPortsType != null && childPort.PortType != null)
        {
            if (ChipPortsType != childPort.PortType)
            {
                childPort.DisconnectPort();
            }
        }
    }

    public override void ChildPortIsDisconnected(ChipPort childPort)
    {
        if (ChipPortsType != null && InputPorts.All(e => e.ConnectedPort == null) && !OutputPorts[0].PortIsConnected())
        {
            ChipPortsType = null;
            foreach (var thePort in InputPorts)
            {
                thePort.PortType = null;
                thePort.UpdateColor();
            }

            Console.WriteLine("Changing all portTypes to null");
            OutputPorts[0].PortType = null;
            OutputPorts[0].UpdateColor();
        }
        else
        {
            childPort.PortType = childPort.IsInput? ChipPortsType : TypeHelper.GetListType(ChipPortsType);
        }
    }

    public override void ChipInspectorProperties()
    {
        if (ImGui.ImageButton("Add Element Port", (IntPtr)LoadIcons.icons["Plus.png"], new Vector2(25)))
        {
            AddElementPort();
        }

        if (ImGui.ImageButton("Remove Element Port", (IntPtr)LoadIcons.icons["Minus.png"], new Vector2(25)))
        {
            RemoveElementPort();
        }
    }
    public ChipPort AddElementPort()
    {
        var thePort = AddPort("Element " + InputPorts.Count(), true, [typeof(bool), typeof(int), typeof(float),
            typeof(string), typeof(Vector2),
            typeof(GameObject), typeof(AudioInfo), typeof(ComponentHolder)], false);
        InputPorts.Last().PortType = ChipPortsType;
        Size = new Vector2(Size.X, (CircuitEditor.portSpacing / CircuitEditor.Zoom) * InputPorts.Count() + 75);
        return thePort;
    }

    public void RemoveElementPort()
    {
        if (!InputPorts.Any())
        {
            return;
        }
        if (InputPorts.Last().ConnectedPort != null)
        {
            InputPorts.Last().DisconnectPort();
        }
        InputPorts.RemoveAt(InputPorts.Count - 1);
        Size = new Vector2(Size.X, (CircuitEditor.portSpacing / CircuitEditor.Zoom) * InputPorts.Count() + 75);
    }

    public Values ListFunction(ChipPort? chipPort)
    {
        Values theValues = new Values();
        if (InputPorts.Count > 0 && ChipPortsType != null)
        {
            if (ChipPortsType == typeof(bool))
            {
                theValues.BoolList = InputPorts.Select(p => p.Value.GetValue().Bool).ToList();
            }
            else if (ChipPortsType == typeof(int))
            {
                theValues.IntList = InputPorts.Select(p => p.Value.GetValue().Int).ToList();
            }
            else if (ChipPortsType == typeof(float))
            {
                theValues.FloatList = InputPorts.Select(p => p.Value.GetValue().Float).ToList();
            }
            else if (ChipPortsType == typeof(string))
            {
                theValues.StringList = InputPorts.Select(p => p.Value.GetValue().String).ToList();
            }
            else if (ChipPortsType == typeof(Vector2))
            {
                theValues.Vector2List = InputPorts.Select(p => p.Value.GetValue().Vector2).ToList();
            }
            else if (ChipPortsType == typeof(GameObject))
            {
                theValues.GameObjectList = InputPorts.Select(p => p.Value.GetValue().GameObject).ToList();
            }
        }
        return theValues;
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        Dictionary<string, string> properties = new Dictionary<string, string>();
        
        if (ChipPortsType != null)
        {
            properties["ChipPortsType"] = ChipPortsType.AssemblyQualifiedName;
        }

        for (int i = 0; i < InputPorts.Count; i++)
        {
            var port = InputPorts[i];
            string portData;

            if (port.ConnectedPort != null)
            {
                portData = $"{port.Id},connected,{port.ConnectedPort.Parent.Id},{port.ConnectedPort.Id}";
            }
            else
            {
                var value = port.Value.GetValue();
                // string valueAsString = value.ToString();
                string valueAsString = $"{value.Bool}^{value.Int}^{value.Float}^{(value.String)}^{value.Vector2.X + "+" + value.Vector2.Y}";

                portData = $"{port.Id},unconnected,{port.PortType?.AssemblyQualifiedName},{valueAsString}";
            }
            
            properties[$"Port{i}"] = portData;
        }

        if (OutputPorts[0].PortType != null)
        {
            properties["OutputPortsType"] = TypeHelper.GetName(OutputPorts[0].PortType).ToLower();
        }

        return properties;
    }

public override void SetCustomProperties(Dictionary<string, string> properties)
{
    if (properties.TryGetValue("ChipPortsType", out var typeName))
    {
        ChipPortsType = Type.GetType(typeName);
    }
    
    var savedPorts = properties
        .Where(kv => kv.Key.StartsWith("Port"))
        .OrderBy(kv => int.Parse(kv.Key.Substring(4))); 

    foreach (var portEntry in savedPorts)
    {
        ChipPort newPort = AddElementPort();
        
        string[] data = portEntry.Value.Split(',');
        
        if (int.TryParse(data[0], out int portId))
        {
        }

        string connectionState = data[1];

        if (connectionState == "connected")
        {
            if (int.TryParse(data[2], out int targetChipId) && int.TryParse(data[3], out int targetPortId))
            {
                var targetChip = CircuitEditor.FindChip(targetChipId);
                var targetPort = targetChip?.FindPort(targetPortId);
                if (targetPort != null)
                {
                    newPort.ConnectPort(targetPort);
                }
            }
        }
        else if (connectionState == "unconnected")
        {
            if (data.Length > 3)
            {
                Type valueType = Type.GetType(data[2]);
                string valueString = data[7];
                
                // if (valueType == typeof(int) && int.TryParse(valueString, out int intValue))
                // {
                //     newPort.Value.SetValue(intValue);
                // }

                if (!valueString.IsNullOrEmpty() && valueString.Contains('^'))
                {
                    string[] values = valueString.Split('^');

                    if (bool.TryParse(values[0], out bool boolValue))
                    {
                        newPort.Value.Bool = boolValue;
                    }

                    if (int.TryParse(values[1], out int intValue))
                    {
                        newPort.Value.Int = intValue;
                    }

                    if (float.TryParse(values[2], out float floatValue))
                    {
                        newPort.Value.Float = floatValue;
                    }

                    newPort.Value.String = values[3];

                    if (values[4].Contains('+'))
                    {
                        string vector2Buffer = values[4];
                        string[] vector2BufferSplit = vector2Buffer.Split('+');
                        if (float.TryParse(vector2BufferSplit[0], out float vector2x) &&
                            float.TryParse(vector2BufferSplit[1], out float vector2y))
                        {
                            newPort.Value.Vector2 = new  Vector2(vector2x, vector2y);
                        }
                    }
                }
            }
        }
    }
    string[] theData = savedPorts.First().Value.Split(',');
    OutputPorts[0].PortType = TypeHelper.GetType( properties.ContainsKey("OutputPortsType")? (String.IsNullOrWhiteSpace(properties["OutputPortsType"]) ? theData[2] :  properties["OutputPortsType"]) : theData[2]);
    
}
}

public class GetElementAt : Chip
{
    public GetElementAt(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("List", true, [typeof(List<bool>), typeof(List<int>), typeof(List<float>), typeof(List<string>), typeof(List<Vector2>), typeof(List<GameObject>)], true);
        AddPort("Index", true, [typeof(int)], true);
        AddPort("Element", false,
            [typeof(bool), typeof(int), typeof(float), typeof(string), typeof(Vector2), typeof(GameObject)], true);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
    }

    public override void PortTypeChanged(ChipPort? port)
    {
        if (port == InputPorts[0]) OutputPorts[0].PortType = TypeHelper.GetNonListType(InputPorts[0].PortType);
        base.PortTypeChanged(port);
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        var listValues = InputPorts[0].Value.GetValue();
        int index = InputPorts[1].Value.GetValue().Int;
        Type? listType = InputPorts[0].PortType;

        if (listType == null) return new Values();

        try
        {
            if (listType == typeof(List<bool>) && listValues.BoolList != null && index < listValues.BoolList.Count)
            {
                return new Values { Bool = listValues.BoolList[index] };
            }
            if (listType == typeof(List<int>) && listValues.IntList != null && index < listValues.IntList.Count)
            {
                return new Values { Int = listValues.IntList[index] };
            }
            if (listType == typeof(List<float>) && listValues.FloatList != null && index < listValues.FloatList.Count)
            {
                return new Values { Float = listValues.FloatList[index] };
            }
            if (listType == typeof(List<string>) && listValues.StringList != null && index < listValues.StringList.Count)
            {
                return new Values { String = listValues.StringList[index] };
            }
            if (listType == typeof(List<Vector2>) && listValues.Vector2List != null && index < listValues.Vector2List.Count)
            {
                return new Values { Vector2 = listValues.Vector2List[index] };
            }
            if (listType == typeof(List<GameObject>) && listValues.GameObjectList != null && index < listValues.GameObjectList.Count)
            {
                return new Values { GameObject = listValues.GameObjectList[index] };
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            GameConsole.Log($"Index {index} is out of range for the list.", LogType.Warning);
            return new Values();
        }
        
        return new Values();
    }
}

public class Vector2Create : Chip
{
    public Vector2Create(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("X", true, [typeof(float)], true);
        AddPort("Y", true, [typeof(float)], true);
        AddPort("Vector2", false, [typeof(Vector2)], true);
        OutputPorts[0].Value.ValueFunction = OutputVector;
    }

    public Values OutputVector(ChipPort? chipPort)
    {
        Vector2 vector = new Vector2(InputPorts[0].Value.Float.Value, InputPorts[1].Value.Float.Value);
        Values theValues = new Values();
        theValues.Vector2 = vector;

        return theValues;
    }
}

public class thisChip : Chip
{
    public GameObject? theThisGameObject;
    public thisChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("Attached GameObject", false, [typeof(GameObject)], true);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        Values theValues = new Values();
        if (theThisGameObject is not null)
        {
            theValues.GameObject = theThisGameObject;
        }
        return theValues;
    }
}

public class SetWorldPositionChip : Chip
{
    public SetWorldPositionChip(int id, string name, Vector2 pos) : base(id, name, pos, true)
    {
        AddPort("GameObject", true, [typeof(GameObject)], true);
        AddPort("Position", true, [typeof(Vector2)], true);
    }

    public override void OnExecute()
    {
        try
        {
            GameObject? targetObject = InputPorts[0].Value.GetValue().GameObject;
            Vector2? targetPosition = InputPorts[1].Value.GetValue().Vector2;

            if (targetObject == null)
            {
                GameConsole.Log("[Set World Position Chip] GameObject is null or invalid", LogType.Error);
            }
            else if (targetPosition == null)
            {
                GameConsole.Log("[Set World Position Chip] Position is null", LogType.Error);
            }
            else
            {
                Transform? targetTransform = targetObject.GetComponent<Transform>();
                if (targetTransform == null)
                {
                    GameConsole.Log("[Set World Position Chip] Target Transform is null", LogType.Error);
                }
                else
                {
                    targetTransform.WorldPosition = targetPosition.Value;
                }
            }
        }
        catch (Exception ex)
        {
            GameConsole.Log($"[Set World Position Chip] Error: {ex.Message}", LogType.Error);
        }

        base.OnExecute();
    }
}

public class SetLocalPositionChip : Chip
{
    public SetLocalPositionChip(int id, string name, Vector2 pos) : base(id, name, pos, true)
    {
        AddPort("GameObject", true, [typeof(GameObject)], true);
        AddPort("Position", true, [typeof(Vector2)], true);
    }
    
    public override void OnExecute()
    {
        try
        {
            GameObject? targetObject = InputPorts[0].Value.GetValue().GameObject;
            Vector2? targetPosition = InputPorts[1].Value.GetValue().Vector2;

            if (targetObject == null)
            {
                GameConsole.Log("[Set Local Position Chip] GameObject is null or invalid", LogType.Error);
            }
            else if (targetPosition == null)
            {
                GameConsole.Log("[Set Local Position Chip] Position is null", LogType.Error);
            }
            else
            {
                Transform? targetTransform = targetObject.GetComponent<Transform>();
                if (targetTransform == null)
                {
                    GameConsole.Log("[Set Local Position Chip] Target Transform is null", LogType.Error);
                }
                else
                {
                    targetTransform.LocalPosition = targetPosition.Value;
                }
            }
        }
        catch (Exception ex)
        {
            GameConsole.Log($"[Set Local Position Chip] Error: {ex.Message}", LogType.Error);
        }

        base.OnExecute();
    }
}

public class GetComponentChip : Chip
{
    private int selectedIndex = 0;

    private string previewValue;
    public GetComponentChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("GameObject", true, [typeof(GameObject)], true);
        AddPort("Component", false, [typeof(ComponentHolder)], true);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
        Size = new Vector2(250, 100);
        ShowCustomItemOnChip = true;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        GameObject? targetObject = InputPorts[0].Value.GetValue().GameObject;
        var typeOfComponent = Component.AllComponents[selectedIndex].Item2;
        bool isValid = targetObject is not null &&
                       targetObject.HasComponent(typeOfComponent);

        if (isValid)
        {
            ComponentHolder componentHolder = new ComponentHolder(targetObject.GetComponent(typeOfComponent));
            Values theValues = new Values();
            theValues.ComponentHolder = componentHolder;
            return theValues;
        }
        else
        {
            return new Values();
        }
    }

    public override void DisplayCustomItem()
    {
        ImGui.PushItemWidth(200);
        if (ImGui.BeginCombo("ComponentDropdown", previewValue))
        {
            for (int i = 0; i < Component.AllComponents.Count(); i++)
            {
                bool isSelected = (i == selectedIndex);

                if (ImGui.Selectable(Component.AllComponents[i].Item1, isSelected))
                {
                    selectedIndex = i;
                    previewValue = (selectedIndex >= 0 && selectedIndex < Component.AllComponents.Count())
                        ? Component.AllComponents[selectedIndex].Item1
                        : "Select...";
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.PopItemWidth();
    }

    public override void OnInstantiation()
    {
        selectedIndex = 0;
        previewValue = (selectedIndex >= 0 && selectedIndex < Component.AllComponents.Count())
            ? Component.AllComponents[selectedIndex].Item1
            : "Select...";
    }
}

public class HasComponentChip : Chip
{
    private int selectedIndex = 0;

    private string previewValue;
    public HasComponentChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("GameObject", true, [typeof(GameObject)], true);
        AddPort("Component", false, [typeof(bool)], true);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
        Size = new Vector2(250, 100);
        ShowCustomItemOnChip = true;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        GameObject? targetObject = InputPorts[0].Value.GetValue().GameObject;
        var typeOfComponent = Component.AllComponents[selectedIndex].Item2;
        bool isValid = targetObject is not null &&
                       targetObject.HasComponent(typeOfComponent);

        if (isValid)
        {
            bool hasComponent = targetObject.HasComponent(typeOfComponent);
            Values theValues = new Values();
            theValues.Bool = hasComponent;
            return theValues;
        }
        else
        {
            return new Values();
        }
    }

    public override void DisplayCustomItem()
    {
        ImGui.PushItemWidth(200);
        if (ImGui.BeginCombo("ComponentDropdown", previewValue))
        {
            for (int i = 0; i < Component.AllComponents.Count(); i++)
            {
                bool isSelected = (i == selectedIndex);

                if (ImGui.Selectable(Component.AllComponents[i].Item1, isSelected))
                {
                    selectedIndex = i;
                    previewValue = (selectedIndex >= 0 && selectedIndex < Component.AllComponents.Count())
                        ? Component.AllComponents[selectedIndex].Item1
                        : "Select...";
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.PopItemWidth();
    }

    public override void OnInstantiation()
    {
        selectedIndex = 0;
        previewValue = (selectedIndex >= 0 && selectedIndex < Component.AllComponents.Count())
            ? Component.AllComponents[selectedIndex].Item1
            : "Select...";
    }
}

public class IfChip : Chip
{
    public IfChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddExecPort("If", true);
        AddPort("Condition", true, [typeof(bool)], false);
        AddExecPort("Then", false);
        AddExecPort("Else", false);
    }

    public override void OnExecute()
    {
        try
        {
            if (InputPorts[0].Value.GetValue().Bool)
            {
                OutputExecPorts.Find(port => port.Name == "Then").Execute();
            }
            else
            {
                OutputExecPorts.Find(port => port.Name == "Else").Execute();
            }
        }
        catch (Exception e)
        {
            GameConsole.Log($"[If] Error executing condition: {e.Message}");
            Console.WriteLine(e);
            throw;
        }
    }
}

public class AudioConstant : Chip
{
    private bool searchButtonClicked = false;
    AudioInfo audioInfo = new AudioInfo();
    public AudioConstant(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("Audio", false, [typeof(AudioInfo)], false);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
    }

    private Values OutputFunction(ChipPort? chipPort)
    {
        Values theValue = new Values();
        if (!String.IsNullOrWhiteSpace(audioInfo.Name) && !String.IsNullOrWhiteSpace(audioInfo.pathToAudio))
        {
            theValue.AudioInfo = audioInfo;
        }

        return theValue;
    }

    public override void ChipInspectorProperties()
    {
        if (ImGui.ImageButton("SearchImage", (IntPtr)LoadIcons.icons["MagnifyingGlass.png"], new Vector2(20, 20)))
        {
            searchButtonClicked = true;
        }

        Vector2 buttonPos = ImGui.GetItemRectMin();
        
        if (searchButtonClicked)
        {
            ImGui.SetNextWindowPos(buttonPos, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(240, 300), ImGuiCond.Appearing);
            ImGui.Begin("Search", ref searchButtonClicked, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);
            ImGui.Columns(3, "Image Column", false);
            var imageFiles = ProjectSerialiser.ScanAssetsForFilesWithExtension([".wav", ".mp3", ".flac", ".ogg"]);
            foreach (var path in imageFiles)
            {
                ImGui.BeginGroup();
                if (ImGui.ImageButton(path, (IntPtr)LoadIcons.icons["Waveform.png"], new Vector2(60, 60)))
                {
                    audioInfo.pathToAudio = path;
                    audioInfo.Name = Path.GetFileNameWithoutExtension(path);
                    if (!Engine.Audio._loadedClips.ContainsKey(audioInfo.Name))
                    {
                        Engine.Audio.LoadSound(audioInfo.Name, audioInfo.pathToAudio, false);
                    }
                    searchButtonClicked = false;
                }
                float textWidth = ImGui.CalcTextSize(Path.GetFileNameWithoutExtension(audioInfo.Name)).X;
                float currentIconWidth = ImGui.GetItemRectSize().X;
                float textPadding = (currentIconWidth - textWidth) * 0.5f;
                if (textPadding > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + textPadding);
                ImGui.Text(Path.GetFileNameWithoutExtension(path));
                ImGui.EndGroup();
                
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
            ImGui.End();
        }
    }
}

public class PlayAudioChip : Chip
{
    public PlayAudioChip(int id, string name, Vector2 pos) : base(id, name, pos, true)
    {
        AddPort("Audio", true, [typeof(AudioInfo)], true);
    }

    public override void OnExecute()
    {
        try
        {
            AudioInfo? audioInfo = InputPorts[0].Value.GetValue().AudioInfo;

            if (audioInfo != null && audioInfo.Name != null && audioInfo.pathToAudio != null)
            {
                Engine.Audio.PlaySound(audioInfo.Name);
            }
            else
            {
                GameConsole.Log($"[Play Audio Chip] Audio is null", LogType.Error);
            }
        }
        catch (Exception e)
        {
           GameConsole.Log($"[Play Audio Chip] Error playing audio: {e.Message}", LogType.Error);
            throw;
        }
        base.OnExecute();
    }
}

public class IsKeyDownChip : Chip
{
    private Key? keyToCheck;

    private static byte[] KeySearchBuffer = new byte[128];
    public IsKeyDownChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("IsDown", false, [typeof(bool)], false);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
        ShowCustomItemOnChip = true;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        Values toOutput = new();

        toOutput.Bool = (keyToCheck != null && InputManager.IsKeyDown(keyToCheck.Value));

        return toOutput;
    }

    public override void DisplayCustomItem()
    {
        ImGui.PushItemWidth(100);
        if (ImGui.BeginCombo("##KeyCombo", keyToCheck.ToString()))
        {
            
            ImGui.InputText("Search", KeySearchBuffer, (uint)KeySearchBuffer.Length);
            string searchText = Encoding.UTF8.GetString(KeySearchBuffer).TrimEnd('\0').ToLower();
            ImGui.Separator();
            
            
            List<Key> allKeys = (List<Key>)Enum.GetValues(typeof(Key)).Cast<Key>().ToList();

            int x = 0;
            while (true)
            {

                if (allKeys.Count(e => e == allKeys[x]) > 1)
                {
                    allKeys.RemoveAt(x);
                }
                else x++;

                if (x >= allKeys.Count())
                {
                    break;
                }
            }

            for (int i = 0; i < allKeys.Count; i++)
            {
                bool isSelected = (keyToCheck != null &&allKeys[i] == keyToCheck.Value);

                if (!String.IsNullOrWhiteSpace(searchText) &&
                    !allKeys[i].ToString().ToLower().Contains(searchText.ToLower()))
                {
                    continue;
                }

                if (ImGui.Selectable(allKeys[i].ToString(), isSelected))
                {
                    keyToCheck = allKeys[i];
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
    }
}

public class IsPressedThisFrameChip : Chip
{
    private Key? keyToCheck;
    private static byte[] KeySearchBuffer = new byte[128];
    public IsPressedThisFrameChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("IsDown", false, [typeof(bool)], false);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
        ShowCustomItemOnChip = true;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        Values toOutput = new();

        toOutput.Bool = (keyToCheck != null && InputManager.IsKeyPressed(keyToCheck.Value));

        return toOutput;
    }

    public override void DisplayCustomItem()
    {
        ImGui.PushItemWidth(100);
        if (ImGui.BeginCombo("##KeyCombo", keyToCheck.ToString()))
        {
            
            ImGui.InputText("Search", KeySearchBuffer, (uint)KeySearchBuffer.Length);
            string searchText = Encoding.UTF8.GetString(KeySearchBuffer).TrimEnd('\0').ToLower();
            ImGui.Separator();
            
            
            List<Key> allKeys = (List<Key>)Enum.GetValues(typeof(Key)).Cast<Key>().ToList();

            int x = 0;
            while (true)
            {

                if (allKeys.Count(e => e == allKeys[x]) > 1)
                {
                    allKeys.RemoveAt(x);
                }
                else x++;

                if (x >= allKeys.Count())
                {
                    break;
                }
            }

            for (int i = 0; i < allKeys.Count; i++)
            {
                bool isSelected = (keyToCheck != null &&allKeys[i] == keyToCheck.Value);

                if (!String.IsNullOrWhiteSpace(searchText) &&
                    !allKeys[i].ToString().ToLower().Contains(searchText.ToLower()))
                {
                    continue;
                }

                if (ImGui.Selectable(allKeys[i].ToString(), isSelected))
                {
                    keyToCheck = allKeys[i];
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
    }
}

public class IsKeyReleasedThisFrameChip : Chip
{
    private Key? keyToCheck;
    private static byte[] KeySearchBuffer = new byte[128];
    public IsKeyReleasedThisFrameChip(int id, string name, Vector2 pos) : base(id, name, pos, false)
    {
        AddPort("IsDown", false, [typeof(bool)], false);
        OutputPorts[0].Value.ValueFunction = OutputFunction;
        ShowCustomItemOnChip = true;
    }

    public Values OutputFunction(ChipPort? chipPort)
    {
        Values toOutput = new();

        toOutput.Bool = (keyToCheck != null && InputManager.IsKeyReleased(keyToCheck.Value));

        return toOutput;
    }

    public override void DisplayCustomItem()
    {
        ImGui.PushItemWidth(100);
        if (ImGui.BeginCombo("##KeyCombo", keyToCheck.ToString()))
        {
            
            ImGui.InputText("Search", KeySearchBuffer, (uint)KeySearchBuffer.Length);
            string searchText = Encoding.UTF8.GetString(KeySearchBuffer).TrimEnd('\0').ToLower();
            ImGui.Separator();
            
            
            List<Key> allKeys = (List<Key>)Enum.GetValues(typeof(Key)).Cast<Key>().ToList();

            int x = 0;
            while (true)
            {

                if (allKeys.Count(e => e == allKeys[x]) > 1)
                {
                    allKeys.RemoveAt(x);
                }
                else x++;

                if (x >= allKeys.Count())
                {
                    break;
                }
            }

            for (int i = 0; i < allKeys.Count; i++)
            {
                bool isSelected = (keyToCheck != null &&allKeys[i] == keyToCheck.Value);

                if (!String.IsNullOrWhiteSpace(searchText) &&
                    !allKeys[i].ToString().ToLower().Contains(searchText.ToLower()))
                {
                    continue;
                }

                if (ImGui.Selectable(allKeys[i].ToString(), isSelected))
                {
                    keyToCheck = allKeys[i];
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
    }
}