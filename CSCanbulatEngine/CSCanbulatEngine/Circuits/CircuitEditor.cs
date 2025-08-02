using System.Numerics;
using System.Runtime.InteropServices;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;
using Silk.NET.Input;

namespace CSCanbulatEngine.Circuits;

public class ChipPortValue
{
    public ChipPort AssignedChipPort;
    public bool? b { get; set;  }
    public int? i { get; set; }
    public float? f { get; set; }
    public string? s { get; set; }
    public Vector2? v2 { get; set; }
    public GameObject? gObj { get; set; }

    public ChipPortValue(List<Type> acceptedTypes, ChipPort assignedChipPort)
    {
        //Allows for multiple values
        acceptedTypes = acceptedTypes;
        // Setting default values
        b = false;
        i = 0;
        f = 0;
        s = "";
        v2 = Vector2.Zero;
        gObj = null;
        AssignedChipPort = assignedChipPort;
    }
    
    //Returns if value is accepted or not
    public bool SetValue<T>(T value)
    {
        if (AssignedChipPort.acceptedTypes.Contains(typeof(T)))
        {
            if (typeof(T) == typeof(bool))
            {
                b = value as bool?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(int))
            {
                i = value as int?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(float))
            {
                f = value as float?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(Vector2))
            {
                v2 = value as Vector2?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(GameObject))
            {
                gObj = value as GameObject;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
        }
        else if (AssignedChipPort.acceptedTypes.Contains(typeof(string)))
        {
            s = value.ToString();
            AssignedChipPort.PortType = typeof(T);
            return true;
        }

        AssignedChipPort.PortType = null;
        return false;
    }
    
    public object? GetValue()
    {
        if (AssignedChipPort.IsInput)
        {
            if (AssignedChipPort.ConnectedPort != null)
            {
                return AssignedChipPort.ConnectedPort.Value.GetValue();
            }
            else
            {
                if (AssignedChipPort.PortType == typeof(bool))
                {
                    return b;
                }
                else if (AssignedChipPort.PortType == typeof(int))
                {
                    return i;
                }
                else if (AssignedChipPort.PortType == typeof(float))
                {
                    return f;
                }
                else if (AssignedChipPort.PortType == typeof(Vector2))
                {
                    return v2;
                }
                else if (AssignedChipPort.PortType == typeof(GameObject))
                {
                    return gObj;
                }
                else if (AssignedChipPort.PortType == typeof(string))
                {
                    return s;
                }
        
                return null;
            }
        }
        else
        {
            
        }

        return null;

    }
}

public class ChipPort
{
    public int Id { get; }
    public string Name { get; }
    public Chip Parent { get; set; }
    public bool IsInput { get; }
    public Type? PortType { get; set; }
    public List<Type> acceptedTypes;
    public Vector4 Color { get; set; }
    public ChipPort? ConnectedPort { get; set; } //Only used for ports that are inputs
    
    public Vector2 Position { get; set; }

    public ChipPortValue Value;

    public ChipPort(int id, string name, Chip parent, bool isInput, List<Type> acceptedValueTypes, Vector4 color)
    {
        Id = id;
        Name = name;
        Parent = parent;
        IsInput = isInput;
        Value = new ChipPortValue(acceptedValueTypes, this);
        Color = color;
    }

    // !! Need to check if they are the same type
    public bool ConnectPort(ChipPort port)
    {
        if (!IsInput) return false;
        if (port.IsInput == IsInput) return false;

        ConnectedPort = port;
        return true;
    }

    public void RenderWire()
    {
        if (ConnectedPort == null)
        {
            return;
        }
        
        var drawList = ImGui.GetWindowDrawList();

        Vector2 outputPortPos = ConnectedPort.Position;

        List<Vector2> linePositions = new List<Vector2>();
        List<Vector4> lineColors = new List<Vector4>();

        for (float i = -0.1f; i <= 1.1; i += 0.1f)
        {
            linePositions.Add(new (float.Lerp(outputPortPos.X, Position.X, i), SineLerpFunction(outputPortPos.Y, Position.Y, i)));
            lineColors.Add(Vector4.Lerp(ConnectedPort.Color, Color, i));
        }

        for (int i = 1; i < linePositions.Count; i++)
        {
            drawList.AddLine(linePositions[i - 1], linePositions[i], ImGui.GetColorU32(lineColors[i]), 2.5f);
        }
    }

    public float SineLerpFunction(float start, float end, float t)
    {
        float tInPi = MathF.PI * float.Clamp(t, 0, 1);
        float sineValue = (MathF.Sin(tInPi - MathF.PI/2) + 1)/2;
        return float.Lerp(start, end, sineValue);
    }
}

public class Chip
{
    public int Id { get; }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Action CircuitFunction { get; set; }
    
    // Ports
    public List<ChipPort> InputPorts = new List<ChipPort>();
    public List<ChipPort> OutputPorts = new List<ChipPort>();

    public Chip(int id, string name, Vector2 position)
    {
        Id = id;
        Name = name;
        Position = position;
        Size = new Vector2(150, 100);
    }

    public ChipPort AddPort(string name, bool isInput, List<Type> acceptedValueTypes, Vector4 color)
    {
        int nextAvaliableID = -1;
        bool idFound = false;
        while (nextAvaliableID <= -1 && !idFound)
        {
            nextAvaliableID += 1;
            if (CircuitEditor.FindChip(nextAvaliableID) == null)
            {
                idFound = true;
            }
        }
        var port = new ChipPort(nextAvaliableID, name, this, isInput, acceptedValueTypes, color);
        if (isInput)
        {
            InputPorts.Add(port);
        }
        else
        {
            OutputPorts.Add(port);
        }

        return port;
    }
}

public static class CircuitEditor
{
    public static List<Chip> chips = new List<Chip>();
    private static Vector2 panning = Vector2.Zero;
    private static Chip? selectedChip = null;

    public static void Render()
    {
        ImGui.BeginChild("NodeEditorCanvas", Vector2.Zero);
        
        var drawList = ImGui.GetWindowDrawList();
        var canvasPos = ImGui.GetCursorScreenPos();

        if (ImGui.IsWindowHovered() && (ImGui.IsMouseDragging(ImGuiMouseButton.Middle) ||
                                         (InputManager.IsKeyDown(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)? Key.SuperLeft : Key.AltLeft) && ImGui.IsMouseDragging(ImGuiMouseButton.Right))))
        {
            panning += ImGui.GetIO().MouseDelta;
        }

        foreach (var chip in chips)
        {
            RenderChip(chip, canvasPos, drawList);
        }

        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && selectedChip != null)
        {
            selectedChip.Position += ImGui.GetIO().MouseDelta;
        }
        else
        {
            selectedChip = null;
        }
        
        ImGui.EndChild();
    }

    private static void RenderChip(Chip chip, Vector2 canvasPos, ImDrawListPtr drawList)
    {
        ImGui.PushID(chip.Id);

        var chipPos = canvasPos + chip.Position + panning;
        var chipSize = chip.Size;
        var titleBarHeight = 30f;
        
        drawList.AddRectFilled(chipPos, chipPos + chipSize, ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f)));
        drawList.AddRectFilled(chipPos, chipPos + new Vector2(chipSize.X, titleBarHeight), ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 1.0f)), 5f, ImDrawFlags.RoundCornersTop);
        drawList.AddText(chipPos + new Vector2(5f, 5f), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1.0f)), chip.Name);
        
        ImGui.SetCursorScreenPos(chipPos);
        ImGui.InvisibleButton("chip_drag_area", chipSize);
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            selectedChip = chip;
        }

        float portRadius = 5f;
        float portSpacing = 25f;

        //Input ports
        for (int i = 0; i < chip.InputPorts.Count; i++)
        {
            var port = chip.InputPorts[i];
            var portPos = chipPos + new Vector2(0, titleBarHeight + portSpacing * (i + 1));

            port.Position = portPos;

            drawList.AddCircleFilled(portPos, portRadius, ImGui.GetColorU32(port.Color));
            port.RenderWire();
        }
        
        //Output ports
        for (int i = 0; i < chip.OutputPorts.Count; i++)
        {
            var port = chip.OutputPorts[i];
            var portPos = chipPos + new Vector2(chipSize.X, titleBarHeight + portSpacing * (i + 1));

            port.Position = portPos;

            drawList.AddCircleFilled(portPos, portRadius, ImGui.GetColorU32(port.Color));
        }
        ImGui.PopID();
    }
    
    public static Chip? FindChip(int id)
    {
        foreach (var chip in CircuitEditor.chips)
        {
            if (chip.Id == id)
            {
                return chip;
            }
        }

        return null;
    }

    public static Chip? FindChip(string name)
    {
        foreach (var chip in CircuitEditor.chips)
        {
            if (chip.Name == name) return chip;
        }
        
        return null;
    }
}