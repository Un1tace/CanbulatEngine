using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CSCanbulatEngine.EngineComponents;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;
using Silk.NET.Input;

namespace CSCanbulatEngine.Circuits;

// Rules
public class ChipPortValue
{
    public ChipPort AssignedChipPort;
    public Func<ChipPort?, Values> ValueFunction;
    public bool? b { get; set;  }
    public int? i { get; set; }
    public float? f { get; set; }
    public string? s { get; set; }
    public Vector2? v2 { get; set; }
    public GameObject? gObj { get; set; }

    public byte[] S_bufer = new byte[100];

    public ChipPortValue(ChipPort assignedChipPort)
    {
        // Setting default values
        b = false;
        i = 0;
        f = 0;
        s = "";
        v2 = Vector2.Zero;
        gObj = null;
        AssignedChipPort = assignedChipPort;
    }

    public void UpdateSBuffer()
    {
        var bytes = Encoding.UTF8.GetBytes(s ?? "");
        Array.Clear(S_bufer, 0, S_bufer.Length);
        
        var lengthToCopy = Math.Min(bytes.Length, S_bufer.Length - 1);
        Array.Copy(bytes, S_bufer, lengthToCopy);
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
            UpdateSBuffer();
            AssignedChipPort.PortType = typeof(T);
            return true;
        }

        AssignedChipPort.PortType = null;
        return false;
    }
    
    public Values GetValue()
    {
        if (AssignedChipPort.IsInput)
        {
            if (AssignedChipPort.ConnectedPort != null)
            {
                return AssignedChipPort.ConnectedPort.Value.GetValue();
            }
            else
            {
                Values values = new Values();
                values.b = b ?? false;
                values.i = i ?? 0;
                values.f = f ?? 0;
                values.s = s ?? "";
                values.v2 = v2 ?? Vector2.Zero;
                values.gObj = gObj ?? null;
                return values;
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
            _PortType = value;
            UpdateColor();
            Parent.PortTypeChanged(this);
        }}
    public Type? _PortType;
    public List<Type> acceptedTypes;
    public Vector4 Color { get; set; }
    public virtual ChipPort? ConnectedPort { get; set; } //Only used for ports that are inputs
    
    public Vector2 Position { get; set; }

    public ChipPortValue Value;

    public bool ShowName = false;
    
    public EngineAnimationManager animationManagerStartWire;
    public EngineAnimationManager animationManagerEndWire;
    
    public List<ChipPort> outputConnectedPorts;

    public ChipPort(int id, string name, Chip parent, bool isInput, List<Type> acceptedValueTypes,
        bool showName = false)
    {
        Id = id;
        Name = name;
        Parent = parent;
        IsInput = isInput;
        Value = new ChipPortValue(this);
        acceptedTypes = acceptedValueTypes;
        ShowName = showName;

        if (acceptedValueTypes.Count == 1)
        {
            PortType = acceptedValueTypes[0];
        }
        if (!isInput)
        {
            outputConnectedPorts = new List<ChipPort>();
        }
        
        UpdateColor();
    }
    
    public ChipPort(int id, string name, Chip parent, bool isInput, Type? portType = null, bool showName = false)
    {
        Id = id;
        Name = name;
        Parent = parent;
        IsInput = isInput;
        Value = new ChipPortValue(this);
        ShowName = showName;
        if (portType != null)
        {
            PortType = portType;

            acceptedTypes = new List<Type>() { portType };
        }
        if (!isInput)
        {
            outputConnectedPorts = new List<ChipPort>();
        }
        
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (this is ExecPort)
        {
            Color = ChipColor.GetColor(ChipTypes.Exec);
        }
        else
        {
            if (PortType == null)
            {
                if (acceptedTypes.Count == 1)
                {
                    Color = ChipColor.GetColor(acceptedTypes[0]);
                }
                else Color = ChipColor.GetColor(ChipTypes.Default);
            }
            else Color = ChipColor.GetColor(PortType);
        }
    }

    // !! Need to check if they are the same type
    public virtual bool ConnectPort(ChipPort port)
    {
        if (!IsInput)
        {
            return port.ConnectPort(this);
        }
        if (port is ExecPort execPort) return false;
        if (port.IsInput == IsInput) return false;
        if (this.Parent == port.Parent) return false;
        if (!this.acceptedTypes.Intersect(port.acceptedTypes).Any()) return false;
        if (PortType != null)  if (PortType != port.PortType) return false;

        if (port == ConnectedPort)
        {
            
            ConnectedPort = null;
            if (acceptedTypes.Count != 1)
            {
                _PortType = null;
            }
            port.outputConnectedPorts.Remove(this);
            Parent.PortTypeChanged(this);
            UpdateColor();
        }
        else
        {
            ConnectedPort = port;
            PortType = port.PortType;
            port.outputConnectedPorts.Add(this);
            UpdateColor();
        }
        Parent.UpdateChipConfig();
        return true;
    }

    public virtual void DisconnectPort()
    {
        if (ConnectedPort == null) return;
        if (!IsInput)
        {
            ConnectedPort.DisconnectPort();
            return;
        }
        ConnectedPort.outputConnectedPorts.Remove(this);
        ConnectedPort = null;
        if ((acceptedTypes?.Count ?? -1) != 1)
        {
            _PortType = null;
        }
        Parent.PortTypeChanged(this);
        UpdateColor();
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
            Vector4 start = animationManagerStartWire?.GetPulseAnimationColor() ?? ConnectedPort.Color;
            Vector4 end = animationManagerEndWire?.GetPulseAnimationColor() ?? Color;
            
            linePositions.Add(new Vector2(float.Lerp(outputPortPos.X, Position.X, i), SineLerpFunction(outputPortPos.Y, Position.Y, i)));
            lineColors.Add(Vector4.Lerp(start, end , i));
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

public class ExecPort : ChipPort
{
    public ExecPort(int id, string name, Chip parent, bool isInput) : base(id, name, parent, isInput)
    {
        animationManagerStartWire = new EngineAnimationManager();
        animationManagerEndWire = new EngineAnimationManager();
    }

    public override bool ConnectPort(ChipPort port)
    {
        if (!IsInput)
        {
            return port.ConnectPort(this);
        }
        if (port.IsInput == IsInput) return false;
        if (this.Parent == port.Parent) return false;
        if (port is not ExecPort execPort) return false;

        if (execPort == ConnectedPort)
        {
            ConnectedPort = null;
            port.outputConnectedPorts.Remove(this);
            UpdateColor();
        }
        else
        {
            ConnectedPort = port;
            port.outputConnectedPorts.Add(this);
            UpdateColor();
        }

        Parent.UpdateChipConfig();
        return true;
    }

    public void Execute()
    {
        if (IsInput)
        {
            if (ConnectedPort != null)
            {
                animationManagerStartWire.SetUpPulseAnimation(ConnectedPort.Color,
                             Vector4.Clamp(ConnectedPort.Color + new Vector4(1f, 1f, 1f, 0f), Vector4.Zero, Vector4.One),
                             200);
            }
            animationManagerEndWire.SetUpPulseAnimation(Color, Vector4.Clamp(Color + new Vector4(1f, 1f, 1f, 0f), Vector4.Zero, Vector4.One), 200);
                
            Parent.OnExecute();
        }
        else
        {
            // if (ConnectedPort is ExecPort connectedExecPort)
            // {
            //     connectedExecPort.Execute();
            // }
            foreach (ChipPort port in outputConnectedPorts)
            {
                ExecPort? execPort = port as ExecPort;
                if (execPort != null)
                {
                    execPort.Execute();
                }
            }
        }
    }

    
}

public class Chip
{
    public int Id { get; }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Action CircuitFunction { get; set; }
    public bool ShowCustomItemOnChip { get; set; }

    public Vector4 Color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f); 
    
    // Ports
    public List<ChipPort> InputPorts = new List<ChipPort>();
    public List<ChipPort> OutputPorts = new List<ChipPort>();
    public List<ExecPort> InputExecPorts = new List<ExecPort>();
    public List<ExecPort> OutputExecPorts = new List<ExecPort>();

    public Chip(int id, string name, Vector2 position, bool requiresExec = false)
    {
        Id = id;
        Name = name;
        Position = position;
        Size = new Vector2(150, 100);
        if (requiresExec)
        {
            InputExecPorts.Add(new ExecPort(NextAvaliablePortIDFunc(), "Chip Execution Input", this, true));
            OutputExecPorts.Add(new ExecPort(NextAvaliablePortIDFunc(), "Chip Execution Output", this, false));
        }
    }

    public ChipPort AddPort(string name, bool isInput, List<Type> acceptedValueTypes, bool showName = false)
    {
        int nextAvaliableID = -1;
        bool idFound = false;
        while (!idFound)
        {
            nextAvaliableID += 1;
            if (FindPort(nextAvaliableID) == null)
            {
                idFound = true;
            }
        }
        var port = new ChipPort(nextAvaliableID, name, this, isInput, acceptedValueTypes, showName);
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

    public ExecPort AddExecPort(string name, bool isInput)
    {
        int nextAvaliableID = -1;
        bool idFound = false;
        while (nextAvaliableID <= -1 && !idFound)
        {
            nextAvaliableID += 1;
            if (FindPort(nextAvaliableID) == null)
            {
                idFound = true;
            }
        }
        var port = new ExecPort(nextAvaliableID, name, this, isInput);
        if (isInput) InputExecPorts.Add(port);
        else OutputExecPorts.Add(port);
        return port;
    }
    
    public virtual void OnExecute()
    {
        OutputExecPorts[0].Execute();
    }
    
    public virtual void UpdateChipConfig()
    {
        foreach (var port in InputPorts)
        {
            port.UpdateColor();
        }
    }

    public virtual void PortTypeChanged(ChipPort? port)
    {
        if (port == null) return;
        port.UpdateColor();
    }

    private ChipPort? FindPort(int id)
    {
        foreach (var port in new List<ChipPort>().Concat(InputPorts).Concat(OutputPorts).Concat(InputExecPorts).Concat(OutputExecPorts))
        {
            if (port.Id == id)
            {
                return port;
            }
        }

        return null;
    }

    private int NextAvaliablePortIDFunc()
    {
        int nextAvaliableID = -1;
        bool idFound = false;
        while (nextAvaliableID <= -1 && !idFound)
        {
            nextAvaliableID += 1;
            if (FindPort(nextAvaliableID) == null)
            {
                idFound = true;
            }
        }
        return nextAvaliableID;
    }

    // Used for displaying custom stuff on chips, used in override on chips
    public virtual void DisplayCustomItem() {}

    public virtual void ChipInspectorProperties() {}

    public virtual void OnDestroy() {}
}

public static class CircuitEditor
{
    public static List<Chip> chips = new List<Chip>();
    private static Vector2 panning = Vector2.Zero;
    private static Chip? selectedChip = null;
    private static Chip? lastSelectedChip = null;
    private static ChipPort? _portDragSource = null;
    private static ChipPort? HoveredPort = null;
    public static string CircuitScriptName = "";
    public static string CircuitScriptDirPath = "";

    public static float Zoom = 1f;
    public const float MinZoom = 0.3f;
    public const float MaxZoom = 2f;

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

        if (ImGui.IsWindowHovered())
        {
            if (io.MouseWheel != 0)
            {
                Vector2 mousePosInCanvas = (io.MousePos - canvasPos - panning) / Zoom;

                float oldZoom = Zoom;
                Zoom += io.MouseWheel * Zoom * 0.1f;
                Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);
                
                panning += mousePosInCanvas * (oldZoom - Zoom);
            }
        }

        if (ImGui.IsWindowHovered())
        {
            HoveredPort = GetPortAt(io.MousePos);
        }
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
        {
            if (HoveredPort != null)
            {
                _portDragSource = HoveredPort;
            }
        }
        
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _portDragSource != null)
        {
            ChipPort targetPort = GetPortAt(io.MousePos);
            if (targetPort != null)
            {
                if (_portDragSource == targetPort)
                {
                    _portDragSource.DisconnectPort();
                }
                else if (_portDragSource.ConnectPort(targetPort))
                {
                    Console.WriteLine($"Connected '{_portDragSource.Name}' to '{targetPort.Name}'");
                }
            }
            _portDragSource = null;
        }
        
        if (_portDragSource != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            _portDragSource.RenderFakeWire();
        }

        CircuitChips.ChipsMenu(io, canvasPos, panning);

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
        
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && selectedChip != null)
        {
            selectedChip.Position += ImGui.GetIO().MouseDelta/Zoom;
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

        var chipPos = canvasPos + (chip.Position * Zoom) + panning;
        var chipSize = chip.Size * Zoom;
        var titleBarHeight = 30f * Zoom;
        
        drawList.AddRectFilled(chipPos, chipPos + chipSize, ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f)));
        drawList.AddRectFilled(chipPos, chipPos + new Vector2(chipSize.X, titleBarHeight), ImGui.GetColorU32(chip.Color), 5f * Zoom, ImDrawFlags.RoundCornersTop);
        drawList.AddText(chipPos + new Vector2(5f, 5f), ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1.0f)), chip.Name);
        
        ImGui.SetCursorScreenPos(chipPos);
        ImGui.InvisibleButton("chip_drag_area", new Vector2(chipSize.X-5f, titleBarHeight));
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            selectedChip = chip;
            lastSelectedChip = chip;
        }

        float portRadius = 5f * Zoom;
        float portSpacing = 25f * Zoom;
        
        List<ChipPort> inputChipPorts = new List<ChipPort>();
        inputChipPorts.AddRange(chip.InputExecPorts);
        inputChipPorts.AddRange(chip.InputPorts);
        
        List<ChipPort> outputChipPorts = new List<ChipPort>();
        outputChipPorts.AddRange(chip.OutputExecPorts);
        outputChipPorts.AddRange(chip.OutputPorts);

        //Input ports
        for (int i = 0; i < inputChipPorts.Count; i++)
        {
            ChipPort port = inputChipPorts[i];
            var portPos = chipPos + new Vector2(0, titleBarHeight + portSpacing * (i + 1));

            port.Position = portPos;

            if (port is ExecPort execPort)
            {
                float multiplier = 2f;
                float half = MathF.Sqrt(MathF.Pow(portRadius, 2) / 2) * multiplier;
                drawList.AddTriangleFilled(new Vector2(portPos.X - half, portPos.Y + half),  new Vector2(portPos.X - half, portPos.Y - half), new Vector2(portPos.X +
                    (portRadius * multiplier) - half, portPos.Y), ImGui.GetColorU32(port.animationManagerEndWire?.GetPulseAnimationColor() ?? port.Color));
                
            }
            else
            {
                drawList.AddCircleFilled(portPos, portRadius, ImGui.GetColorU32(port.Color));
            }

            if (port.ShowName)
            {
                float nameTextWidth = ImGui.CalcTextSize(port.Name).X;
                float nameTextHeight = ImGui.CalcTextSize(port.Name).Y;
                ImGui.SetCursorScreenPos(portPos +
                                         new Vector2(nameTextWidth > 50 ? nameTextWidth + 10 : 50, nameTextHeight / 2));
                ImGui.LabelText($"##{port.Id}", port.Name);
            }
            
            port.RenderWire();

            if ((port.ConnectedPort == null && port.PortType != null) && port.PortType != typeof(GameObject)) 
            {
                ImGui.SetCursorScreenPos(portPos + new Vector2(30 * Zoom, -10 * Zoom));

                Type portType = port.PortType;
                if (portType == typeof(float))
                {
                    ImGui.PushItemWidth(60 * Zoom);
                    float val = port.Value.f ?? 0f;
                    if (ImGui.InputFloat($"##{port.Id}", ref val, 0, 0, "%.2f"))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(int))
                {
                    ImGui.PushItemWidth(90 * Zoom);
                    int val = port.Value.i ?? 0;
                    if (ImGui.InputInt($"##{port.Id}", ref val))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(bool))
                {
                    ImGui.PushItemWidth(60 * Zoom);
                    bool val = port.Value.b ?? false;
                    if (ImGui.Checkbox($"##{port.Id}", ref val))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(Vector2))
                {
                    ImGui.PushItemWidth(90 * Zoom);
                    Vector2 val = port.Value.v2 ?? Vector2.Zero;
                    if (ImGui.InputFloat2($"##{port.Id}", ref val))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(string))
                {
                    ImGui.PushItemWidth(100 * Zoom);
                    if (ImGui.InputText($"##{port.Name}{chip.Id}", port.Value.S_bufer, (uint)port.Value.S_bufer.Length))
                    {
                        port.Value.s = Encoding.UTF8.GetString(port.Value.S_bufer).TrimEnd('\0');
                    }
                }
                else ImGui.PushItemWidth(60 * Zoom);
                ImGui.PopItemWidth();
            }

            if (HoveredPort == port)
            {
                ImGui.PushFont(Engine._extraThickFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(new Vector4(0.788f, 0.788f, 0.125f, 1f)));
                if (port is ExecPort)
                {
                    ExecPort portAsExecPort = port as ExecPort;
                    float textWidth = ImGui.CalcTextSize("Exec").X;
                    ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -40));
                    ImGui.LabelText($"##{port.Id}", "Exec");
                }
                else if (port.PortType != null)
                {
                    float textWidth = ImGui.CalcTextSize(TypeHelper.GetName(port.PortType)).X;
                    ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -40));
                    ImGui.LabelText($"##{port.Id}", TypeHelper.GetName(port.PortType));
                    if (port.PortType == typeof(float))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().f.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().f.ToString());
                    }
                    else if (port.PortType == typeof(int))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().i.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().i.ToString());
                    }
                    else if (port.PortType == typeof(string))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().s).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", "\"" + port.Value.GetValue().s + "\"");
                    }
                    else if (port.PortType == typeof(bool))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().b.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().b.ToString());
                    }
                    else if (port.PortType == typeof(Vector2))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().v2.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().v2.ToString());
                    }
                    else if (port.PortType == typeof(GameObject))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().gObj?.Name ?? "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().gObj?.Name ?? "");
                    }
                }
                else if (port.acceptedTypes != null)
                {
                    string typeString = "";
                    for (int x = 0; x < port.acceptedTypes.Count; x++)
                    {
                        typeString += x == 0? TypeHelper.GetName(port.acceptedTypes[x]) : " | " + TypeHelper.GetName(port.acceptedTypes[x]);
                    }
                    float textWidth = ImGui.CalcTextSize(typeString).X;
                    ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -40));
                    ImGui.LabelText($"##{port.Id}", typeString);
                }
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
        }
        
        //Output ports
        for (int i = 0; i < outputChipPorts.Count; i++)
        {
            var port = outputChipPorts[i];
            var portPos = chipPos + new Vector2(chipSize.X, titleBarHeight + portSpacing * (i + 1));

            port.Position = portPos;

            if (port is ExecPort execPort)
            {
                float multiplier = 2f;
                ChipPort? connectedPort = port.ConnectedPort;
                Vector4 theColor;
                if (connectedPort == null)
                {
                    theColor = port.Color;
                }
                else theColor = connectedPort.animationManagerStartWire?.GetPulseAnimationColor() ?? port.Color;
                float half = MathF.Sqrt(MathF.Pow(portRadius, 2) / 2) * multiplier;
                drawList.AddTriangleFilled(new Vector2(portPos.X - half, portPos.Y + half),  new Vector2(portPos.X - half, portPos.Y - half), new Vector2(portPos.X +
                    (portRadius * multiplier) - half, portPos.Y), ImGui.GetColorU32(theColor));
            }
            else
            {
                drawList.AddCircleFilled(portPos, portRadius, ImGui.GetColorU32(port.Color));
            }

            if (port.ShowName)
            {
                float nameTextWidth = ImGui.CalcTextSize(port.Name).X;
                float nameTextHeight = ImGui.CalcTextSize(port.Name).Y;
                ImGui.SetCursorScreenPos(portPos +
                                         new Vector2(nameTextWidth > 50 ? -nameTextWidth - 10 : -50,
                                             -nameTextHeight / 2));
                ImGui.LabelText($"##{port.Id}", port.Name);
            }
            
            if (HoveredPort == port)
            {
                ImGui.PushFont(Engine._extraThickFont);
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(new Vector4(0.788f, 0.788f, 0.125f, 1f)));
                if (port is ExecPort)
                {
                    ExecPort portAsExecPort = port as ExecPort;
                    float textWidth = ImGui.CalcTextSize("Exec").X;
                    ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -40));
                    ImGui.LabelText($"##{port.Id}", "Exec");
                }
                else if (port.PortType != null)
                {
                    float textWidth = ImGui.CalcTextSize(TypeHelper.GetName(port.PortType)).X;
                    ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -40));
                    ImGui.LabelText($"##{port.Id}", TypeHelper.GetName(port.PortType));
                    if (port.PortType == typeof(float))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().f.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().f.ToString());
                    }
                    else if (port.PortType == typeof(int))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().i.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().i.ToString());
                    }
                    else if (port.PortType == typeof(string))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().s).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", "\"" + port.Value.GetValue().s + "\"");
                    }
                    else if (port.PortType == typeof(bool))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().b.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().b.ToString());
                    }
                    else if (port.PortType == typeof(Vector2))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().v2.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().v2.ToString());
                    }
                    else if (port.PortType == typeof(GameObject))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().gObj?.Name ?? "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().gObj?.Name ?? "");
                    }
                }
                else if (port.acceptedTypes != null)
                {
                    string typeString = "";
                    for (int x = 0; x < port.acceptedTypes.Count; x++)
                    {
                        typeString += x == 0? TypeHelper.GetName(port.acceptedTypes[x]) : " | " + TypeHelper.GetName(port.acceptedTypes[x]);
                    }
                    float textWidth = ImGui.CalcTextSize(typeString).X;
                    ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -40));
                    ImGui.LabelText($"##{port.Id}", typeString);
                }
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
        }

        if (chip.ShowCustomItemOnChip)
        {
            int highestAmountOfPorts = inputChipPorts.Count > outputChipPorts.Count
                ? inputChipPorts.Count
                : outputChipPorts.Count;
            var portLatePos = chipPos + new Vector2(0, titleBarHeight + portSpacing * (highestAmountOfPorts + 1));

            ImGui.SetCursorScreenPos(portLatePos + new Vector2(30 * Zoom, -10 * Zoom));
            chip.DisplayCustomItem();
        }
        
        ImGui.PopID();
    }

    public static void MainMenuBar()
    {
        string superKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "CMD" : "CTRL";
        string altKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Option" : "ALT"; 
        if (ImGui.BeginMenu("Circuit Script"))
        {
            if (ImGui.MenuItem("New Circuit Script", superKey + "+" + altKey + "+" + "N"))
            {
                chips.Clear();
                CircuitScriptName = "";
                CircuitScriptDirPath = "";
            }

            if (ImGui.MenuItem("Open Circuit Script", superKey + "+" + altKey + "+" + "O"))
            {
                Engine.OpenCircuitScript();
            }

            if (ImGui.MenuItem("Save Circuit Script", superKey + "+" + altKey + "+" + "S"))
            {
                Engine.SaveCircuitScript();
            }
                
            ImGui.EndMenu();
        }
        
    }

    public static void DeleteChip(Chip chipToDelete)
    {
        if (chipToDelete == null) return;

        chipToDelete.OnDestroy();
        
        var allInputPorts = chipToDelete.InputPorts.Concat(chipToDelete.InputExecPorts);
        foreach (var inputPort in allInputPorts.ToList())
        {
            if (inputPort.ConnectedPort != null)
            {
                inputPort.DisconnectPort();
            }
        }
        
        var allOutputPorts = chipToDelete.OutputPorts.Concat(chipToDelete.OutputExecPorts);
        foreach (var outputPort in allOutputPorts.ToList())
        {
            if (outputPort.outputConnectedPorts != null)
            {
                foreach (var connectedInputPort in outputPort.outputConnectedPorts.ToList())
                {
                    connectedInputPort.DisconnectPort();
                }
            }
        }

        if (selectedChip == chipToDelete)
        {
            selectedChip = null;
        }

        if (lastSelectedChip == chipToDelete)
        {
            lastSelectedChip = null;
        }
        
        chips.Remove(chipToDelete);
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
    
    private static ChipPort? GetPortAt(Vector2 mousePos)
    {
        foreach (var chip in chips)
        {
            foreach (var port in chip.InputExecPorts)
            {
                if (Vector2.Distance(port.Position, mousePos) < 10f * Zoom) return port;
            }
            foreach (var port in chip.InputPorts)
            {
                if (Vector2.Distance(port.Position, mousePos) < 10f * Zoom) return port;
            }
            foreach (var port in chip.OutputExecPorts)
            {
                if (Vector2.Distance(port.Position, mousePos) < 10f * Zoom) return port;
            }
            foreach (var port in chip.OutputPorts)
            {
                if (Vector2.Distance(port.Position, mousePos) < 10f * Zoom) return port;
            }
        }
        return null;
    }

    public static int GetNextAvaliableChipID()
    {
        bool found = false;
        int id = 0;
        while (!found)
        {
            if (FindChip(id) == null)
            {
                return id;
                found = true;
            }
            else
            {
                id++;
            }
        }

        return id;
    }
    
    
    // Inspector Stuff For Chips
    public static void RenderChipInspector()
    {
        if (lastSelectedChip != null)
        {
            ImGui.ColorEdit4("Chip Color", ref lastSelectedChip.Color);
            lastSelectedChip.ChipInspectorProperties();
        }
    }
}

public enum ChipTypes
{
    Default, Bool, Int, Float, String, Vector2, GameObject, Exec
}

public static class TypeHelper
{
    public static string GetName(Type type)
    {
            if (type == typeof(bool))
            {
                return "Bool";
            }
            else if (type == typeof(int))
            {
                return "Int";
            }
            else if (type == typeof(float))
            {
                return "Float";
            }
            else if (type == typeof(string))
            {
                return "String";
            }
            else if (type == typeof(Vector2))
            {
                return "Vector2";
            }
            else if (type == typeof(GameObject))
            {
                return "GameObject";
            }
            else
            {
                return "";
            }
    }
    
    
}

public static class ChipColor
{
    public static Vector4 GetColor(ChipTypes type)
    {
        switch (type)
        {
            case ChipTypes.Default:
                return Vector4.One;
                break;
            case ChipTypes.Bool:
                return new Vector4(0.5f, 0, 0, 1f);
                break;
            case ChipTypes.Int:
                return new Vector4(0, 0.5f, 0, 1f);
                break;
            case ChipTypes.Float:
                return new Vector4(0, 0, 0.5f, 1f);
                break;
            case ChipTypes.String:
                return new Vector4(0.56f, 0.35f, 0.88f, 1f);
                break;
            case ChipTypes.Vector2:
                return new Vector4(0.35f, 0.88f, 0.8f, 1f);
                break;
            case ChipTypes.GameObject:
                return new Vector4(1f, 0.89f, 0.15f, 1f);
                break;
            case ChipTypes.Exec:
                return new Vector4(1f, 0.29f, 0.13f, 1f);
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

public class Values()
{
    public bool b = false;
    public float f = 0f;
    public int i = 0;
    public string s = "";
    public Vector2 v2 = Vector2.Zero;
    public GameObject? gObj = null;
    
    public Type? ActiveType { get; set; }
}