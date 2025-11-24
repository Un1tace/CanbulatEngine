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
            CreateChipFromData(chip);
        }

        foreach (var portValueData in circuitInfo.UnconnectedPortValues)
        {
            var chip = FindChip(portValueData.ChipId);
            var port = chip?.InputPorts.FirstOrDefault(p => p.Id == portValueData.PortId);

            CircuitSerialiser.ParseAndSetPortData(portValueData, port);
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
        EngineLog.Log($"Loaded circuit script: {filePath} in object {AttachedGameObject.Name}");
    }
    
    public Chip? CreateChipFromData(CircuitData.ChipData data, bool setID = true)
    {
        Type chipType = Type.GetType(data.ChipType);

        if (chipType != null)
        {
            Chip newChip = (Chip)Activator.CreateInstance(chipType, setID? data.id : GetNextAvaliableChipID(), data.Name, data.Position);
            newChip.SetCustomProperties(data.CustomProperties);
            newChip.Color = data.Color;
            newChip.LoadedInBackground = true;
            if (newChip.GetType() == typeof(thisChip))
            {
                thisChip theChip = (thisChip)newChip;
                theChip.theThisGameObject = AttachedGameObject;
            }
            chips.Add(newChip);
            return newChip;
        }

        return null;
    }
    
    public int GetNextAvaliableChipID()
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

    #if EDITOR
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
#endif

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        CircuitScriptDirPath = properties["CircuitPath"];
        CircuitScriptName = properties["CircuitScriptName"];
        LoadCircuit(Path.Combine(CircuitScriptDirPath, CircuitScriptName) + ".ccs");
    }
}