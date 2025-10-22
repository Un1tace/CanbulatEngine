using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.FileHandling.CircuitHandling;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Newtonsoft.Json;

namespace CSCanbulatEngine.GameObjectScripts;

public class CircuitScript : Component
{
    public List<Chip> chips = new List<Chip>();
    public string CircuitScriptName;
    public string CircuitScriptDirPath;
    
    public CircuitScript() : base("CircuitScript")
    {
        
    }
    
    public void LoadCircuit(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var circuitInfo = JsonConvert.DeserializeObject<CircuitData.CircuitInfo>(json);
        
        chips.Clear();
        CircuitScriptName = Path.GetFileNameWithoutExtension(filePath);
        CircuitScriptDirPath = Path.GetDirectoryName(filePath);
        
        foreach (var chip in circuitInfo.Chips)
        {
            Type chipType = Type.GetType(chip.ChipType);

            if (chipType != null)
            {
                Chip newChip = (Chip)Activator.CreateInstance(chipType, chip.id, chip.Name, chip.Position);
                newChip.SetCustomProperties(chip.CustomProperties);
                newChip.Color = chip.Color;
                chips.Add(newChip);
                newChip.LoadedInBackground = true;

                if (chipType == typeof(thisChip))
                {
                    var theChip = (thisChip)newChip;
                    theChip.theThisGameObject = AttachedGameObject;
                }
            }
        }

        foreach (var portValueData in circuitInfo.UnconnectedPortValues)
        {
            var chip = FindChip(portValueData.ChipId);
            var port = chip?.InputPorts.FirstOrDefault(p => p.Id == portValueData.PortId);

            if (port != null)
            {
                Type? type = Type.GetType(portValueData.ValueType);
                if (type != null)
                {
                    if (type == typeof(bool)) port.Value.SetValue(bool.Parse(portValueData.Value));
                    else if (type == typeof(int)) port.Value.SetValue(int.Parse(portValueData.Value));
                    else if (type == typeof(float)) port.Value.SetValue(float.Parse(portValueData.Value));
                    else if (type == typeof(string)) port.Value.SetValue(portValueData.Value);
                    else if (type == typeof(Vector2))
                    {
                        var parts = portValueData.Value.Split(',');
                        port.Value.SetValue(new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture),
                            float.Parse(parts[1], CultureInfo.InvariantCulture)));
                    }
                }
            }
        }
        
        foreach (var connectionData in circuitInfo.Connections)
        {
            var outputChip = FindChip(connectionData.OutputChipId);
            var inputChip = FindChip(connectionData.InputChipId);

            if (outputChip != null && inputChip != null)
            {
                var outputPort = outputChip.OutputPorts.Concat(outputChip.OutputExecPorts).FirstOrDefault(p => p.Id == connectionData.OutputPortId);
                var inputPort = inputChip.InputPorts.Concat(inputChip.InputExecPorts).FirstOrDefault(p => p.Id == connectionData.InputPortId);

                if (outputPort != null && inputPort != null)
                {
                    inputPort.ConnectPort(outputPort);
                }
            }
            
        }
        
        Console.WriteLine($"Loaded circuit script: {filePath}");
    }
    
    public Chip? FindChip(int id)
    {
        foreach (var chip in chips)
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
        foreach (var chip in chips)
        {
            if (chip.Name == name) return chip;
        }
        
        return null;
    }
    
    bool searchButtonClicked = false;

    public override void RenderInspector()
    {
        if (ImGui.ImageButton("SearchCircuitScript", (IntPtr)LoadIcons.icons["MagnifyingGlass.png"], new Vector2(20)))
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
            var circuitScripts = ProjectSerialiser.FindAllCircuitScripts();
            foreach (var path in circuitScripts)
            {
                ImGui.BeginGroup();
                if (ImGui.ImageButton(path, (IntPtr)LoadIcons.icons["Circuit.png"], new Vector2(60, 60)))
                {
                    CircuitScriptDirPath = path;
                    LoadCircuit(path);
                    searchButtonClicked = false;
                }
                float textWidth = ImGui.CalcTextSize(Path.GetFileNameWithoutExtension(name)).X;
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

        ImGui.Separator();
        ImGui.Text($"Circuit Script Loaded: {CircuitScriptName}");
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        var circuitProperties = new Dictionary<string, string>
        {
            { "CircuitPath", CircuitScriptDirPath },
            {"CircuitScriptName", CircuitScriptName}
        };
        
        return circuitProperties;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        CircuitScriptDirPath = properties["CircuitPath"];
        CircuitScriptName = properties["CircuitScriptName"];
        LoadCircuit(Path.Combine(CircuitScriptDirPath, CircuitScriptName) + ".ccs");
    }
}