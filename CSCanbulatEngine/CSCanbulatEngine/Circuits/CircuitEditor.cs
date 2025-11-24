using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CSCanbulatEngine.Audio;
using CSCanbulatEngine.EngineComponents;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;
using Silk.NET.Input;
using SixLabors.ImageSharp.ColorSpaces.Companding;

namespace CSCanbulatEngine.Circuits;

public class Values()
{
    public bool Bool = false;
    public List<bool>? BoolList { get; set; }
    public float Float = 0f;
    public List<float>? FloatList { get; set; }
    public int Int = 0;
    public List<int>? IntList { get; set; }
    public string String = "";
    public List<string>? StringList { get; set; }
    public Vector2 Vector2 = Vector2.Zero;
    public List<Vector2>? Vector2List { get; set; }
    public GameObject? GameObject = null;
    public List<GameObject>? GameObjectList { get; set; }

    public Audio.AudioInfo?  AudioInfo = new();
    public List<AudioInfo>? AudioInfoList = new();

    public ComponentHolder?  ComponentHolder = new();
    public List<ComponentHolder>? ComponentHolderList = new();

    public Key? Key = null;
    public List<Key>? KeyList = new();
    public MouseButton? MouseButton = null;
    public List<MouseButton>? MouseButtonList = null;
    
    public Type? ActiveType { get; set; }
}

// Rules
public class ChipPortValue
{
    public ChipPort AssignedChipPort;
    public Func<ChipPort?, Values> ValueFunction;
    public bool? Bool { get; set; }
    public List<bool>? BoolList { get; set; }
    public int? Int { get; set; }
    public List<int>? IntList { get; set; }
    public float? Float { get; set; }
    public List<float>? FloatList { get; set; }
    public string? String { get; set; }
    public List<string>? StringList { get; set; }
    public Vector2? Vector2 { get; set; }
    public List<Vector2>? Vector2List { get; set; }
    public GameObject? GameObject { get; set; }
    public List<GameObject>? GameObjectList { get; set; }
    
    public Audio.AudioInfo? AudioInfo { get; set; }
    
    public List<AudioInfo>? AudioInfoList { get; set; }

    public ComponentHolder? ComponentHolder { get; set; }
    
    public List<ComponentHolder>? ComponentHolderList { get; set; }
    
    public Key? Key { get; set; }
    public List<Key>? KeyList { get; set; }
    public MouseButton? MouseButton { get; set; }
    public List<MouseButton>? MouseButtonList { get; set; }

    public byte[] S_bufer = new byte[100];

    public ChipPortValue(ChipPort assignedChipPort)
    {
        // Setting default values
        Bool = false;
        Int = 0;
        Float = 0;
        String = "";
        Vector2 = System.Numerics.Vector2.Zero;
        GameObject = null;
        AssignedChipPort = assignedChipPort;
    }

    public void UpdateSBuffer()
    {
        var bytes = Encoding.UTF8.GetBytes(String ?? "");
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
                Bool = value as bool?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(int))
            {
                Int = value as int?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(float))
            {
                Float = value as float?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(Vector2))
            {
                Vector2 = value as Vector2?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(GameObject))
            {
                GameObject = value as GameObject;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(string))
            {
                String = value.ToString();
                UpdateSBuffer();
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<bool>))
            {
                BoolList = value as List<bool>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<int>))
            {
                IntList = value as List<int>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<float>))
            {
                FloatList = value as List<float>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<string>))
            {
                StringList = value as List<string>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<Vector2>))
            {
                Vector2List = value as List<Vector2>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<GameObject>))
            {
                GameObjectList = value as List<GameObject>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(Audio.AudioInfo))
            {
                AudioInfo = value as Audio.AudioInfo;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<AudioInfo>))
            {
                AudioInfoList = value as List<AudioInfo>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(ComponentHolder))
            {
                ComponentHolder = value as ComponentHolder;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<ComponentHolder>))
            {
                ComponentHolderList = value as List<ComponentHolder>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(Key))
            {
                Key = value as Key?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<Key>))
            {
                KeyList = value as List<Key>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(MouseButton))
            {
                MouseButton = value as MouseButton?;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            else if (typeof(T) == typeof(List<MouseButton>))
            {
                MouseButtonList = value as List<MouseButton>;
                AssignedChipPort.PortType = typeof(T);
                return true;
            }
            
        }
        else if (AssignedChipPort.acceptedTypes.Contains(typeof(string)))
        {
            String = value.ToString();
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
                values.Bool = Bool ?? false;
                values.Int = Int ?? 0;
                values.Float = Float ?? 0;
                values.String = String ?? "";
                values.Vector2 = Vector2 ?? System.Numerics.Vector2.Zero;
                values.GameObject = GameObject ?? null;
                values.BoolList = BoolList ?? null;
                values.IntList = IntList ?? null;
                values.FloatList = FloatList ?? null;
                values.StringList = StringList ?? null;
                values.Vector2List = Vector2List ?? null;
                values.GameObjectList = GameObjectList ?? null;
                values.AudioInfo = AudioInfo ?? null;
                values.AudioInfoList = AudioInfoList ?? null;
                values.ComponentHolder = ComponentHolder ?? null;
                values.ComponentHolderList = ComponentHolderList ?? null;
                values.Key = Key ?? null;
                values.KeyList = KeyList ?? null;
                values.MouseButton = MouseButton ?? null;
                values.MouseButtonList = MouseButtonList ?? null;
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
    public int Id { get; set; }
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

    private byte[] portNameBuffer;

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

        Type? typeToSet = null;
        if (this.PortType != null && port.PortType != null)
        {
            if (this.PortType != port.PortType) return false;
            typeToSet = this.PortType;
        }
        else if (this.PortType != null)
        {
            if (port.acceptedTypes.Contains(this.PortType))
            {
                typeToSet = this.PortType;
            }
            else return false;
        }
        else if (port.PortType != null)
        {
            if (this.acceptedTypes.Contains(port.PortType))
            {
                typeToSet = port.PortType;
            }
            else return false;
        }

        
        if (port == ConnectedPort)
        {
            var disconnectedPortBuffer = ConnectedPort;
            ConnectedPort = null;
            if (acceptedTypes.Count != 1)
            {
                _PortType = null;
            }
            port.outputConnectedPorts.Remove(this);
            Parent.PortTypeChanged(this);
            UpdateColor();
            Parent.ChildPortIsDisconnected(this);
            disconnectedPortBuffer.Parent.ChildPortIsDisconnected(disconnectedPortBuffer);
        }
        else
        {
            ConnectedPort = port;
            PortType = port.PortType;
            port.outputConnectedPorts.Add(this);
            UpdateColor();
            Parent.ChildPortIsConnected(this, port);
        }
        Parent.UpdateChipConfig();
        return true;
    }

    public virtual bool PortIsConnected()
    {
        return this.ConnectedPort != null || this.outputConnectedPorts.Count() != 0;
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
        var disconnectedPortBuffer = ConnectedPort;
        ConnectedPort = null;
        if ((acceptedTypes?.Count ?? -1) != 1)
        {
            _PortType = null;
        }
        Parent.PortTypeChanged(this);
        Parent.ChildPortIsDisconnected(this);
        disconnectedPortBuffer.Parent.ChildPortIsDisconnected(disconnectedPortBuffer);
        UpdateColor();
    }

    public virtual void RenderWire()
    {
        if (ConnectedPort == null)
        {
            return;
        }
        
        var drawList = ImGui.GetWindowDrawList();

        Vector2 outputPortPos = ConnectedPort.Position;

        List<Vector2> linePositions = new List<Vector2>();
        List<Vector4> lineColors = new List<Vector4>();

        for (float i = 0f; i <= 1.1; i += 0.1f)
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

        for (float i = 0f; i <= 1.1; i += 0.1f)
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
    public List<ExecPort> InputConnections;
    public ExecPort(int id, string name, Chip parent, bool isInput, bool showName = false) : base(id, name, parent, isInput, [], showName)
    {
        animationManagerStartWire = new EngineAnimationManager();
        animationManagerEndWire = new EngineAnimationManager();

        if (isInput) InputConnections = new();
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

        if (InputConnections.Contains(execPort))
        {
            execPort.outputConnectedPorts.Remove(this);
            InputConnections.Remove(execPort);
            UpdateColor();
            Parent.ChildPortIsDisconnected(this);
        }
        else
        {
            InputConnections.Add(execPort);
            port.outputConnectedPorts.Add(this);
            UpdateColor();
            Parent.ChildPortIsConnected(this, port);
        }

        Parent.UpdateChipConfig();
        return true;
    }
    
    public override void RenderWire()
    {
        if (!IsInput) return;

        var drawList = ImGui.GetWindowDrawList();
        
        foreach (var outputPort in InputConnections)
        {
            Vector2 outputPortPos = outputPort.Position;

            List<Vector2> linePositions = new List<Vector2>();
            List<Vector4> lineColors = new List<Vector4>();

            for (float i = 0f; i <= 1.1; i += 0.1f)
            {
                Vector4 start = outputPort.animationManagerStartWire?.GetPulseAnimationColor() ?? outputPort.Color;
                Vector4 end = this.animationManagerEndWire?.GetPulseAnimationColor() ?? this.Color;
            
                linePositions.Add(new Vector2(float.Lerp(outputPortPos.X, Position.X, i), SineLerpFunction(outputPortPos.Y, Position.Y, i)));
                lineColors.Add(Vector4.Lerp(start, end , i));
            }

            for (int i = 1; i < linePositions.Count; i++)
            {
                drawList.AddLine(linePositions[i - 1], linePositions[i], ImGui.GetColorU32(lineColors[i]), 2.5f);
            }
        }
    }

    public void Execute()
    {
        if (IsInput)
        {
            if (InputConnections != null)
            {
                foreach(var outputPort in InputConnections)
                {
                    outputPort.animationManagerStartWire?.SetUpPulseAnimation(outputPort.Color,
                        Vector4.Clamp(outputPort.Color + new Vector4(1f, 1f, 1f, 0f), Vector4.Zero, Vector4.One),
                        200);
                }
            }
            
            animationManagerEndWire.SetUpPulseAnimation(Color, Vector4.Clamp(Color + new Vector4(1f, 1f, 1f, 0f), Vector4.Zero, Vector4.One), 200);
            
            Parent.OnExecute();
        }
        else
        {
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

    public override bool PortIsConnected()
    {
        if (IsInput)
        {
            return InputConnections.Count > 0;
        }
        else
        {
            return outputConnectedPorts.Count > 0;
        }
    }
    
    public override void DisconnectPort()
    {
        if (IsInput)
        {
            foreach (var outputPort in InputConnections.ToList())
            {
                outputPort.outputConnectedPorts.Remove(this);
                Parent.ChildPortIsDisconnected(this);
            }
            InputConnections.Clear();
            UpdateColor();
        }
        else
        {
            foreach (var inputPort in outputConnectedPorts.ToList())
            {
                (inputPort as ExecPort)?.InputConnections.Remove(this);
                inputPort.Parent.ChildPortIsDisconnected(inputPort);
                inputPort.UpdateColor();
            }
            outputConnectedPorts.Clear();
            UpdateColor();
        }
    }
}

public class Chip
{
    public bool LoadedInBackground = false;
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
    
    public byte[] nameBuffer = new byte[128];

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

        OnInstantiation();
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

    public ExecPort AddExecPort(string name, bool isInput, bool showName = false)
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
        var port = new ExecPort(nextAvaliableID, name, this, isInput, showName);
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

    public ChipPort? FindPort(int id)
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
        
        while (!idFound)
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

    // Renders custom inspector options on the inspector if the chip is selected and needed
    public virtual void ChipInspectorProperties() {}

    // Any clean up that is needed before the chip is deleted
    public virtual void OnDestroy() {}
    
    // Executed when any child port is connected
    public virtual void ChildPortIsConnected(ChipPort childPort, ChipPort portConnectedTo) {}
    
    // Executed when any child port is disconnected
    public virtual void ChildPortIsDisconnected(ChipPort childPort) {}
    
    // Used for saving custom properties on the specific chip that will be needed in the circuit editor
    public virtual Dictionary<string, string> GetCustomProperties()
    {
        return new Dictionary<string, string>();
    }

    // Used for setting custom properties on the specific chip when loaded
    public virtual void SetCustomProperties(Dictionary<string, string> properties) {}

    public virtual void OnInstantiation() {}
}

public static class CircuitEditor
{
    public static List<Chip> chips = new List<Chip>();
    
    public static string CircuitScriptName = "";
    public static string CircuitScriptDirPath = "";
    
    #if EDITOR
    public static Vector2 panning = Vector2.Zero;
    public static Chip? selectedChip = null;
    public static Chip? lastSelectedChip = null;
    private static ChipPort? _portDragSource = null;
    private static ChipPort? HoveredPort = null;
    
    public static float Zoom = 1f;
    public const float MinZoom = 0.3f;
    public const float MaxZoom = 2f;
    
    public static float portSpacing = 25f * Zoom;
    
    //Clipboard
    public static string chipClipboard = "";
    public static Vector2 chipPosClipboard = Vector2.Zero;
    public static string chipUnconnectedPortClipboard = "";
    
    public unsafe static void Render()
    {
        ImGui.SetWindowFontScale(CircuitEditor.Zoom);
        
        ImGui.BeginChild("NodeEditorCanvas", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar);
        
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
                    EngineLog.Log($"Connected '{_portDragSource.Name}' to '{targetPort.Name}'");
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
        ImGui.SetWindowFontScale(1f);
        
        RenderStatusBar();
    }

    private static unsafe void RenderChip(Chip chip, Vector2 canvasPos, ImDrawListPtr drawList)
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
        portSpacing = 25f * Zoom;
        
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
                                         (new Vector2(nameTextWidth > 50 ? nameTextWidth + 10 : 50, nameTextHeight / 2) * Zoom));
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
                    float val = port.Value.Float ?? 0f;
                    if (ImGui.InputFloat($"##{port.Id}", ref val, 0, 0, "%.2f"))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(int))
                {
                    ImGui.PushItemWidth(90 * Zoom);
                    int val = port.Value.Int ?? 0;
                    if (ImGui.InputInt($"##{port.Id}", ref val))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(bool))
                {
                    ImGui.PushItemWidth(60 * Zoom);
                    bool val = port.Value.Bool ?? false;
                    if (ImGui.Checkbox($"##{port.Id}", ref val))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(Vector2))
                {
                    ImGui.PushItemWidth(90 * Zoom);
                    Vector2 val = port.Value.Vector2 ?? Vector2.Zero;
                    if (ImGui.InputFloat2($"##{port.Id}", ref val))
                    {
                        port.Value.SetValue(val);
                    }
                }
                else if (portType == typeof(Key))
                {
                    ImGui.PushItemWidth(60 * Zoom);
                    
                    if (ImGui.BeginCombo("##KeyCombo", port.Value.Key.ToString()))
                    {
            
                        ImGui.InputText("Search", port.Value.S_bufer, (uint)port.Value.S_bufer.Length);
                        string searchText = Encoding.UTF8.GetString(port.Value.S_bufer).TrimEnd('\0').ToLower();
                        ImGui.Separator();


                        Key[] allKeys = InputManager.GetAllKeys();

                        for (int x = 0; x < allKeys.Length; x++)
                        {
                            bool isSelected = (port.Value.Key != null &&allKeys[x] == port.Value.Key);

                            if (!String.IsNullOrWhiteSpace(searchText) &&
                                !allKeys[x].ToString().ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }

                            if (ImGui.Selectable(allKeys[x].ToString(), isSelected))
                            {
                                port.Value.Key = allKeys[x];
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
            
                        ImGui.EndCombo();
                    }
                }
                else if (portType == typeof(MouseButton))
                {
                    ImGui.PushItemWidth(60 * Zoom);
                    
                    if (ImGui.BeginCombo("##KeyCombo", port.Value.MouseButton.ToString()))
                    {
            
                        ImGui.InputText("Search", port.Value.S_bufer, (uint)port.Value.S_bufer.Length);
                        string searchText = Encoding.UTF8.GetString(port.Value.S_bufer).TrimEnd('\0').ToLower();
                        ImGui.Separator();


                        MouseButton[] allMouseButtons = InputManager.GetAllMouseButtons();

                        for (int x = 0; x < allMouseButtons.Length; x++)
                        {
                            bool isSelected = (port.Value.MouseButton != null &&allMouseButtons[x] == port.Value.MouseButton);

                            if (!String.IsNullOrWhiteSpace(searchText) &&
                                !allMouseButtons[x].ToString().ToLower().Contains(searchText.ToLower()))
                            {
                                continue;
                            }

                            if (ImGui.Selectable(allMouseButtons[x].ToString(), isSelected))
                            {
                                port.Value.MouseButton = allMouseButtons[x];
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
            
                        ImGui.EndCombo();
                    }
                }
                else if (portType == typeof(string))
                {
                    ImGui.PushItemWidth(100 * Zoom);
                    if (ImGui.InputText($"##{port.Name}{chip.Id}", port.Value.S_bufer, (uint)port.Value.S_bufer.Length))
                    {
                        port.Value.String = Encoding.UTF8.GetString(port.Value.S_bufer).TrimEnd('\0');
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
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Float.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Float.ToString());
                    }
                    else if (port.PortType == typeof(int))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Int.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Int.ToString());
                    }
                    else if (port.PortType == typeof(string))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().String).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", "\"" + port.Value.GetValue().String + "\"");
                    }
                    else if (port.PortType == typeof(bool))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Bool.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Bool.ToString());
                    }
                    else if (port.PortType == typeof(Vector2))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Vector2.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Vector2.ToString());
                    }
                    else if (port.PortType == typeof(GameObject))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().GameObject?.Name ?? "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().GameObject?.Name ?? "");
                    }
                    else if (port.PortType == typeof(AudioInfo))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().AudioInfo?.Name ?? "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().AudioInfo?.Name ?? "");
                    }
                    else if (port.PortType == typeof(ComponentHolder))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().ComponentHolder is not null && port.Value.GetValue().ComponentHolder.Component is not null? port.Value.GetValue().ComponentHolder.Component.name : "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().ComponentHolder is not null && port.Value.GetValue().ComponentHolder.Component is not null? port.Value.GetValue().ComponentHolder.Component.name : "");
                    }
                    else if (port.PortType == typeof(Key))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Key.ToString()).X;
                        Vector2 drawPos = portPos + new Vector2(-textWidth / 2, -25);
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Key.ToString());
                    }
                    else if (port.PortType == typeof(MouseButton))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().MouseButton.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().MouseButton.ToString());
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
                                         (new Vector2(nameTextWidth > 100 ? -nameTextWidth - 10 : -100,
                                             -nameTextHeight / 2) * Zoom));
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
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Float.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Float.ToString());
                    }
                    else if (port.PortType == typeof(int))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Int.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Int.ToString());
                    }
                    else if (port.PortType == typeof(string))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().String).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", "\"" + port.Value.GetValue().String + "\"");
                    }
                    else if (port.PortType == typeof(bool))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Bool.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Bool.ToString());
                    }
                    else if (port.PortType == typeof(Vector2))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Vector2.ToString()).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Vector2.ToString());
                    }
                    else if (port.PortType == typeof(GameObject))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().GameObject is not null? port.Value.GetValue().GameObject.Name : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().GameObject is not null? port.Value.GetValue().GameObject.Name : "null");
                    }
                    else if (port.PortType == typeof(List<bool>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().BoolList is not null? port.Value.GetValue().BoolList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().BoolList is not null? port.Value.GetValue().BoolList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(List<float>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().FloatList is not null? port.Value.GetValue().FloatList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().FloatList is not null? port.Value.GetValue().FloatList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(List<int>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().IntList is not null? port.Value.GetValue().IntList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().IntList is not null? port.Value.GetValue().IntList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(List<string>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().StringList is not null? port.Value.GetValue().StringList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().StringList is not null? port.Value.GetValue().StringList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(List<Vector2>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Vector2List is not null? port.Value.GetValue().Vector2List.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().Vector2List is not null? port.Value.GetValue().Vector2List.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(List<GameObject>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().GameObjectList is not null? port.Value.GetValue().GameObjectList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().GameObjectList is not null? port.Value.GetValue().GameObjectList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(AudioInfo))
                    {
                        AudioInfo? theAudio = port.Value.GetValue().AudioInfo;

                        string theDisplay = "null";

                        if (theAudio != null && !String.IsNullOrWhiteSpace(theAudio.Name) && !String.IsNullOrWhiteSpace(theAudio.pathToAudio))
                        {
                            theDisplay = theAudio.Name;
                        }
                        
                        textWidth = ImGui.CalcTextSize(theDisplay).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", theDisplay);
                    }
                    else if (port.PortType == typeof(List<AudioInfo>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().AudioInfoList is not null? port.Value.GetValue().AudioInfoList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().AudioInfoList is not null? port.Value.GetValue().AudioInfoList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(ComponentHolder))
                    {
                        ComponentHolder? holder = port.Value.GetValue().ComponentHolder;

                        string componentName = "null";

                        if (holder != null)
                        {
                            Component theComponent = holder.Component;
                            if (theComponent is not null)
                            {
                                componentName = theComponent.name;
                            }
                        }
                        
                        textWidth = ImGui.CalcTextSize(componentName).X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", componentName);
                    }
                    else if (port.PortType == typeof(List<ComponentHolder>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().ComponentHolderList is not null? port.Value.GetValue().ComponentHolderList.Count().ToString() : "null").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", (port.Value.GetValue().ComponentHolderList is not null? port.Value.GetValue().ComponentHolderList.Count().ToString() : "null"));
                    }
                    else if (port.PortType == typeof(Key))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().Key is not null && port.Value.GetValue().Key is not null? port.Value.GetValue().Key.ToString() : "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().Key is not null && port.Value.GetValue().Key is not null? port.Value.GetValue().Key.ToString() : "");
                    }
                    else if (port.PortType == typeof(MouseButton))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().MouseButton is not null && port.Value.GetValue().MouseButton is not null? port.Value.GetValue().MouseButton.ToString() : "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().MouseButton is not null && port.Value.GetValue().MouseButton is not null? port.Value.GetValue().MouseButton.ToString() : "");
                    }
                    else if (port.PortType == typeof(List<Key>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().KeyList is not null && port.Value.GetValue().KeyList is not null? port.Value.GetValue().KeyList.Count().ToString() : "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().KeyList is not null && port.Value.GetValue().KeyList is not null? port.Value.GetValue().KeyList.Count().ToString() : "");
                    }
                    else if (port.PortType == typeof(List<MouseButton>))
                    {
                        textWidth = ImGui.CalcTextSize(port.Value.GetValue().MouseButtonList is not null && port.Value.GetValue().MouseButtonList is not null? port.Value.GetValue().MouseButtonList.Count().ToString() : "").X;
                        ImGui.SetCursorScreenPos(portPos + new Vector2(-textWidth/2, -25));
                        ImGui.LabelText($"##{port.Id}", port.Value.GetValue().MouseButtonList is not null && port.Value.GetValue().MouseButtonList is not null? port.Value.GetValue().MouseButtonList.Count().ToString() : "");
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
    
    private static void RenderStatusBar()
    {
        float statusBarHeight = ImGui.GetFrameHeight();
        var contentRegionAvail = ImGui.GetContentRegionAvail();
        
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + contentRegionAvail.Y - statusBarHeight);

        ImGui.BeginChild("StatusBarChild", new Vector2(0, statusBarHeight), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar);
        ImGui.Separator();
        
        if (_portDragSource != null)
        {
            ImGui.Text("Click on a compatible port to connect, or release in empty space to cancel.");
        }
        else if (HoveredPort != null)
        {
            string portInfo = $"Hovering: {HoveredPort.Name}";
            if (HoveredPort.PortType != null)
            {
                portInfo += $" ({TypeHelper.GetName(HoveredPort.PortType)})";
            }
            ImGui.Text(portInfo);
        }
        else
        {
           // Default text
            string superKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "CMD" : "CTRL";
            ImGui.Text($"Middle Mouse or {superKey}+Right Click: Pan | Scroll: Zoom | Right Click on canvas: Open Menu");
        }

        ImGui.EndChild();
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

        CircuitChips.CreateContextMenu();

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
    
    // Inspector Stuff For Chips
    public static void RenderChipInspector()
    {
        if (lastSelectedChip != null)
        {
            ImGui.ColorEdit4("Chip Color", ref lastSelectedChip.Color);
            lastSelectedChip.ChipInspectorProperties();
        }
    }
#endif
    
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
}

public enum ChipTypes
{
    Default, Bool, Int, Float, String, Vector2, GameObject, Exec, BoolList, IntList, FloatList, StringList, Vector2List, GameObjectList, AudioInfo, AudioInfoList, ComponentHolder, ComponentHolderList, Key, KeyList, MouseButton, MouseButtonList
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
            else if (type == typeof(List<bool>))
            {
                return "List<bool>";
            }
            else if (type == typeof(List<int>))
            {
                return "List<int>";
            }
            else if (type == typeof(List<float>))
            {
                return "List<float>";
            }
            else if (type == typeof(List<string>))
            {
                return "List<string>";
            }
            else if (type == typeof(List<Vector2>))
            {
                return "List<Vector2>";
            }
            else if (type == typeof(List<GameObject>))
            {
                return "List<GameObject>";
            }
            else if (type == typeof(AudioInfo))
            {
                return "AudioInfo";
            }
            else if (type == typeof(List<AudioInfo>))
            {
                return "List<AudioInfo>";
            }
            else if (type == typeof(ComponentHolder))
            {
                return "ComponentHolder";
            }
            else if (type == typeof(List<ComponentHolder>))
            {
                return "List<ComponentHolder>";
            }
            else if (type == typeof(Key))
            {
                return "Key";
            }
            else if (type == typeof(List<Key>))
            {
                return "List<Key>";
            }
            else if (type == typeof(MouseButton))
            {
                return "MouseButton";
            }
            else if (type == typeof(List<MouseButton>))
            {
                return "List<MouseButton>";
            }
            else
            {
                return "";
            }
    }

    public static Type? GetType(string name)
    {
        name = name.ToLower();
        switch (name)
        {
            case "bool":
                return typeof(bool);
            case "int":
                return typeof(int);
            case "float":
                return typeof(float);
            case "string":
                return typeof(string);
            case "vector2":
                return typeof(Vector2);
            case "gameobject":
                return typeof(GameObject);
            case "list<bool>":
                return typeof(List<bool>);
            case "list<int>":
                return typeof(List<int>);
            case "list<float>":
                return typeof(List<float>);
            case "list<string>":
                return typeof(List<string>);
            case "list<vector2>":
                return typeof(List<Vector2>);
            case "list<gameobject>":
                return typeof(List<GameObject>);
            case "audioinfo":
                return typeof(AudioInfo);
            case "list<audioinfo>":
                return typeof(List<AudioInfo>);
            case "componentholder":
                return typeof(ComponentHolder);
            case "list<componentholder>":
                return typeof(List<ComponentHolder>);
            case "key":
                return typeof(Key);
            case "list<key>":
                return typeof(List<Key>);
            case "mouseButton":
                return typeof(MouseButton);
            case "list<mousebutton>":
                return typeof(List<MouseButton>);
        }

        return null;
    }

    public static Type? GetNonListType(Type listType)
    {
        if (listType == typeof(List<bool>)) return typeof(bool);
        else if (listType == typeof(List<int>)) return typeof(int);
        else if (listType == typeof(List<float>)) return typeof(float);
        else if (listType == typeof(List<string>)) return typeof(string);
        else if (listType == typeof(List<Vector2>)) return typeof(Vector2);
        else if (listType == typeof(List<GameObject>)) return typeof(GameObject);
        else if (listType == typeof(List<AudioInfo>)) return typeof(AudioInfo);
        else if (listType == typeof(List<ComponentHolder>)) return typeof(ComponentHolder);
        else if (listType == typeof(List<Key>)) return typeof(Key);
        else if (listType == typeof(List<MouseButton>)) return typeof(MouseButton);
        else return null;
    }

    public static Type? GetListType(Type type)
    {
        if (type == typeof(bool)) return typeof(List<bool>);
        else if (type == typeof(int)) return typeof(List<int>);
        else if (type == typeof(float)) return typeof(List<float>);
        else if (type == typeof(string)) return typeof(List<string>);
        else if (type == typeof(Vector2)) return typeof(List<Vector2>);
        else if (type == typeof(GameObject)) return typeof(List<GameObject>);
        else if (type == typeof(AudioInfo)) return typeof(List<AudioInfo>);
        else if (type == typeof(ComponentHolder)) return typeof(List<ComponentHolder>);
        else if (type == typeof(Key)) return typeof(List<Key>);
        else if (type == typeof(MouseButton)) return typeof(List<MouseButton>);
        else return null;
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
            case ChipTypes.BoolList:
                return new Vector4(0.7f, 0.2f, 0.2f, 1f);
                break;
            case ChipTypes.IntList:
                return new Vector4(0.2f, 0.7f, 0.2f, 1f);
                break;
            case ChipTypes.FloatList:
                return new Vector4(0.2f, 0.2f, 0.7f, 1f);
                break;
            case ChipTypes.StringList:
                return new Vector4(0.76f, 0.55f, 1f, 1f);
                break;
            case ChipTypes.Vector2List:
                return new Vector4(0.55f, 1f, 1f, 1f);
                break;
            case ChipTypes.GameObjectList:
                return new Vector4(1f, 1f, 0.35f, 1f);
                break;
            case ChipTypes.AudioInfo:
                return new Vector4(1f, 0.89f, 0.15f, 1f);
                break;
            case ChipTypes.AudioInfoList:
                return new Vector4(1f, 1f, 0.35f, 1f);
                break;
            case ChipTypes.ComponentHolder:
                return new Vector4(1f, 0.89f, 0.15f, 1f);
                break;
            case ChipTypes.ComponentHolderList:
                return new Vector4(1f, 1f, 0.35f, 1f);
                break;
            case ChipTypes.Key:
                return new Vector4(1f, 0.89f, 0.15f, 1f);
                break;
            case ChipTypes.KeyList:
                return new Vector4(1f, 1f, 0.35f, 1f);
                break;
            case ChipTypes.MouseButton:
                return new Vector4(1f, 0.89f, 0.15f, 1f);
                break;
            case ChipTypes.MouseButtonList:
                return new Vector4(1f, 1f, 0.35f, 1f);
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
        else if (type == typeof(List<bool>))
        {
            return new Vector4(0.7f, 0.2f, 0.2f, 1f);
        }
        else if (type == typeof(List<int>))
        {
            return new Vector4(0.2f, 0.7f, 0.2f, 1f);
        }
        else if (type == typeof(List<float>))
        {
            return new Vector4(0.2f, 0.2f, 0.7f, 1f);
        }
        else if (type == typeof(List<string>))
        {
            return new Vector4(0.76f, 0.55f, 1f, 1f);
        }
        else if (type == typeof(List<Vector2>))
        {
            return new Vector4(0.55f, 1f, 1f, 1f);
        }
        else if (type == typeof(List<GameObject>))
        {
            return new Vector4(1f, 1f, 0.35f, 1f);
        }
        else if (type == typeof(AudioInfo))
        {
            return new Vector4(1f, 0.89f, 0.15f, 1f);
        }
        else if (type == typeof(List<AudioInfo>))
        {
            return new Vector4(1f, 1f, 0.35f, 1f);
        }
        else if (type == typeof(ComponentHolder))
        {
            return new Vector4(1f, 0.89f, 0.15f, 1f);
        }
        else if (type == typeof(List<ComponentHolder>))
        {
            return new Vector4(1f, 1f, 0.35f, 1f);
        }
        else if (type == typeof(Key))
        {
            return new Vector4(1f, 0.89f, 0.15f, 1f);
        }
        else if (type == typeof(List<Key>))
        {
            return new Vector4(1f, 1f, 0.35f, 1f);
        }
        else if (type == typeof(MouseButton))
        {
            return new Vector4(1f, 0.89f, 0.15f, 1f);
        }
        else if (type == typeof(List<MouseButton>))
        {
            return new Vector4(1f, 1f, 0.35f, 1f);
        }
        else
        {
            return Vector4.One;
        }
    }
}