using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;
using Silk.NET.Input;

namespace CSCanbulatEngine.Circuits;

public class ChipPortValue
{
    public ChipPort AssignedChipPort;
    public Func<ChipPort?, object?> ValueFunction;
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
            return ValueFunction(AssignedChipPort);
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
    public Type? PortType {
        get
        {
            return _PortType;
        }
        set
        {
            UpdateColor();
            Parent.PortTypeChanged(this);
            _PortType = value;
        }}
    public Type? _PortType;
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

    public void UpdateColor()
    {
        Color = ChipColor.GetColor(PortType);
    }

    // !! Need to check if they are the same type
    public bool ConnectPort(ChipPort port)
    {
        if (!IsInput) port.ConnectPort(this);
        if (port.IsInput == IsInput) return false;
        if (this.Parent == port.Parent) return false;

        if (port == ConnectedPort)
        {
            ConnectedPort = null;
        }
        else ConnectedPort = port;
        Parent.UpdateChipConfig();
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

    public void RenderFakeWire()
    {
        var drawList = ImGui.GetWindowDrawList();

        Vector2 outputPortPos = ImGui.GetIO().MousePos;

        List<Vector2> linePositions = new List<Vector2>();
        List<Vector4> lineColors = new List<Vector4>();

        for (float i = -0.1f; i <= 1.1; i += 0.1f)
        {
            linePositions.Add(new (float.Lerp(outputPortPos.X, Position.X, i), SineLerpFunction(outputPortPos.Y, Position.Y, i)));
            lineColors.Add(new Vector4(Color.X, Color.Y, Color.Z, Color.W - 0.1f));
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
    
    public virtual void UpdateChipConfig()
    {
        
    }

    public virtual void PortTypeChanged(ChipPort port)
    {
        
    }
}

public static class CircuitEditor
{
    public static List<Chip> chips = new List<Chip>();
    private static Vector2 panning = Vector2.Zero;
    private static Chip? selectedChip = null;
    private static ChipPort? _portDragSource = null;

    public static void Render()
    {
        ImGui.BeginChild("NodeEditorCanvas", Vector2.Zero);
        
        var drawList = ImGui.GetWindowDrawList();
        var canvasPos = ImGui.GetCursorScreenPos();
        var io = ImGui.GetIO();

        if (ImGui.IsWindowHovered() && (ImGui.IsMouseDragging(ImGuiMouseButton.Middle) ||
                                         (InputManager.IsKeyDown(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)? Key.SuperLeft : Key.AltLeft) && ImGui.IsMouseDragging(ImGuiMouseButton.Right))))
        {
            panning += ImGui.GetIO().MouseDelta;
        }
        
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
        {
            ChipPort port = GetPortAt(io.MousePos);
            if (port != null)
            {
                _portDragSource = port;
            }
        }
        
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _portDragSource != null)
        {
            ChipPort targetPort = GetPortAt(io.MousePos);
            if (targetPort != null)
            {
                Console.WriteLine("attempting to connect to port");
                // Attempt to connect the source to the target
                if (_portDragSource.ConnectPort(targetPort))
                {
                    Console.WriteLine($"Connected '{_portDragSource.Name}' to '{targetPort.Name}'");
                }
            }
            // End the drag operation regardless of success
            _portDragSource = null;
        }
        
        if (_portDragSource != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            // We need a dummy port to call the render function
            var tempPort = new ChipPort(-1, "", null, !_portDragSource.IsInput, new List<Type>(), _portDragSource.Color);
            tempPort.Position = _portDragSource.Position;
            tempPort.RenderFakeWire();
        }

        foreach (var chip in chips)
        {
            RenderChip(chip, canvasPos, drawList);
        }

        Chip? closestChip = null;
        foreach (var chip in chips)
        {
            if (closestChip == null || (Vector2.Distance(chip.Position, ImGui.GetIO().MousePos) <
                                        Vector2.Distance(closestChip.Position, chip.Position)))
            {
                closestChip = chip;
            }
        }

        // if (closestChip != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        // {
        //     ChipPort closestPort = null;
        //     List<ChipPort> ports =  closestChip.InputPorts;
        //     ports.AddRange(closestChip.OutputPorts);
        //     foreach (var chipPort in ports)
        //     {
        //         if (closestChip == null || (Vector2.Distance(chipPort.Position, ImGui.GetIO().MousePos) <
        //                                     Vector2.Distance(closestChip.Position, chipPort.Position)))
        //         {
        //             closestPort = chipPort;
        //         }
        //     }
        //
        //     if (Vector2.Distance(closestPort.Position, ImGui.GetIO().MousePos) < 25f && selectedChip == null)
        //     {
        //         selectedPort = closestPort;
        //         Console.WriteLine($"Selected port: {selectedPort.Name}");
        //     }
        // }
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && selectedChip != null)
        {
            selectedChip.Position += ImGui.GetIO().MouseDelta;
        }
        else
        {
            selectedChip = null;
        }

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            // Vector2 mousePos = ImGui.GetIO().MousePos;
            // ChipPort portReleasedOn = null;
            // ChipPort closestPort = null;
            // List<ChipPort> ports = new List<ChipPort>();
            // ports.AddRange(closestChip.InputPorts);
            // ports.AddRange(closestChip.OutputPorts);
            // foreach (var chipPort in ports)
            // {
            //     if (portReleasedOn == null || (Vector2.Distance(chipPort.Position, mousePos) <
            //                                 Vector2.Distance(closestChip.Position, chipPort.Position)))
            //     {
            //         closestPort = chipPort;
            //     }
            // }
            //
            // if (Vector2.Distance(closestPort.Position, ImGui.GetIO().MousePos) < 25f && portReleasedOn != null)
            // {
            //     portReleasedOn = closestPort;
            //     Console.WriteLine($"Connecting to {closestPort.Name}");
            //     selectedPort.ConnectPort(portReleasedOn);
            // }
            
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
        ImGui.InvisibleButton("chip_drag_area", new Vector2(chipSize.X-5f, chipSize.Y-5f));
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
    
    private static ChipPort GetPortAt(Vector2 mousePos)
    {
        foreach (var chip in chips)
        {
            foreach (var port in chip.InputPorts)
            {
                if (Vector2.Distance(port.Position, mousePos) < 10f) return port;
            }
            foreach (var port in chip.OutputPorts)
            {
                if (Vector2.Distance(port.Position, mousePos) < 10f) return port;
            }
        }
        return null;
    }
}

public enum ChipColors
{
    Default, Bool, Int, Float, String, Vector2, GameObject
}

public static class ChipColor
{
    public static Vector4 GetColor(ChipColors color)
    {
        switch (color)
        {
            case ChipColors.Default:
                return Vector4.One;
                break;
            case ChipColors.Bool:
                return new Vector4(0.5f, 0, 0, 1f);
                break;
            case ChipColors.Int:
                return new Vector4(0, 0.5f, 0, 1f);
                break;
            case ChipColors.Float:
                return new Vector4(0, 0, 0.5f, 1f);
                break;
            case ChipColors.String:
                return new Vector4(0.56f, 0.35f, 0.88f, 1f);
                break;
            case ChipColors.Vector2:
                return new Vector4(0.35f, 0.88f, 0.8f, 1f);
                break;
            case ChipColors.GameObject:
                return new Vector4(1f, 0.89f, 0.15f, 1f);
                break;
            default:
                return Vector4.One;
                break;
        }
    }

    public static Vector4 GetColor(Type? type)
    {
        if (type == typeof(bool))
        {
            return new Vector4(0.5f, 0, 0, 1f);
        }
        else if (type == typeof(int))
        {
            return new Vector4(0, 0.5f, 0, 1f);
        }
        else if (type == typeof(float))
        {
            return new Vector4(0, 0, 0.5f, 1f);
        }
        else if (type == typeof(string))
        {
            return new Vector4(0.56f, 0.35f, 0.88f, 1f);
        }
        else if (type == typeof(Vector2))
        {
            return new Vector4(0.35f, 0.88f, 0.8f, 1f);
        }
        else if (type == typeof(GameObject))
        {
            return new Vector4(1f, 0.89f, 0.15f, 1f);
        }
        else
        {
            return Vector4.One;
        }
    }
}