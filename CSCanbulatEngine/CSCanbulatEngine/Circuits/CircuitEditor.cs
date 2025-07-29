using System.Numerics;
using System.Runtime.InteropServices;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;

namespace CSCanbulatEngine.Circuits;

public class ChipPortValue
{
    public List<Type> acceptedTypes;
    public Type? typeUsed;
    public bool? b { get; set; }
    public int? i { get; set; }
    public float? f { get; set; }
    public string? s { get; set; }
    public Vector2? v2 { get; set; }
    public GameObject? gObj { get; set; }

    public ChipPortValue(List<Type> acceptedTypes)
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
    }
    
    //Returns if value is accepted or not
    public bool SetValue<T>(T value)
    {
        if (acceptedTypes.Contains(typeof(T)))
        {
            if (typeof(T) == typeof(bool))
            {
                b = value as bool?;
                typeUsed = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(int))
            {
                i = value as int?;
                typeUsed = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(float))
            {
                f = value as float?;
                typeUsed = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(Vector2))
            {
                v2 = value as Vector2?;
                typeUsed = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(GameObject))
            {
                gObj = value as GameObject;
                typeUsed = typeof(T);
                return true;
            }
        }
        else if (acceptedTypes.Contains(typeof(string)))
        {
            s = value.ToString();
            typeUsed = typeof(T);
            return true;
        }

        typeUsed = null;
        return false;
    }
    
    public object? GetValue()
    {
        if (typeUsed == typeof(bool))
        {
            return b;
        }
        else if (typeUsed == typeof(int))
        {
            return i;
        }
        else if (typeUsed == typeof(float))
        {
            return f;
        }
        else if (typeUsed == typeof(Vector2))
        {
            return v2;
        }
        else if (typeUsed == typeof(GameObject))
        {
            return gObj;
        }
        else if (typeUsed == typeof(string))
        {
            return s;
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
    
    public Vector2 Position { get; set; }

    public ChipPortValue Value;

    public ChipPort(int id, string name, Chip parent, bool isInput, List<Type> acceptedValueTypes)
    {
        Id = id;
        Name = name;
        Parent = parent;
        IsInput = isInput;
        Value = new ChipPortValue(acceptedValueTypes);
    }
}

public class Chip
{
    public int Id { get; }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    
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

    public Chip? FindChip(int id)
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

    public Chip? FindChip(string name)
    {
        foreach (var chip in CircuitEditor.chips)
        {
            if (chip.Name == name) return chip;
        }
        
        return null;
    }
    
    

    public ChipPort AddPin(string name, bool isInput, List<Type> acceptedValueTypes)
    {
        int nextAvaliableID = -1;
        bool idFound = false;
        while (nextAvaliableID <= -1 && !idFound)
        {
            nextAvaliableID += 1;
            if (FindChip(nextAvaliableID) == null)
            {
                idFound = true;
            }
        }
        var port = new ChipPort(nextAvaliableID, name, this, isInput, acceptedValueTypes);
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
                                        (ImGui.IsMouseDragging(ImGuiMouseButton.Right) &&
                                         ImGui.IsKeyDown(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                                             ? ImGuiKey.LeftSuper
                                             : ImGuiKey.LeftAlt))))
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

            drawList.AddCircleFilled(portPos, portRadius, ImGui.GetColorU32(new Vector4(0.8f, 0.5f, 0.5f, 1f)));
        }
        
        //Output ports
        for (int i = 0; i < chip.OutputPorts.Count; i++)
        {
            var port = chip.OutputPorts[i];
            var portPos = chipPos + new Vector2(chipSize.X, titleBarHeight + portSpacing * (i + 1));

            port.Position = portPos;

            drawList.AddCircleFilled(portPos, portRadius, ImGui.GetColorU32(new Vector4(0.8f, 0.5f, 0.5f, 1f)));
        }
        ImGui.PopID();
    }
}