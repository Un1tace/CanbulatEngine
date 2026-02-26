using System.Globalization;
using System.Numerics;
using System.Text;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.FileHandling.CircuitHandling;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Newtonsoft.Json;

namespace CSCanbulatEngine.GameObjectScripts;

/// <summary>
/// Load circuit script to do logic on an object within the game engine
/// </summary>
public class CircuitScript : Component
{
    public List<Chip> chips = new List<Chip>();
    public string CircuitScriptName;
    public string CircuitScriptDirPath;

    private SerialisationChip[] allSerialisedChips =>
        chips.FindAll(e => e.GetType() == typeof(SerialisationChip)).Cast<SerialisationChip>().ToArray();
    
    public CircuitScript() : base("CircuitScript")
    {
        
    }
    
    /// <summary>
    /// Load circuit script into a component
    /// </summary>
    /// <param name="filePath">File path to circuit script</param>
    public void LoadCircuit(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var circuitInfo = JsonConvert.DeserializeObject<CircuitData.CircuitInfo>(json);

        foreach (Chip chip in chips)
        {
            chip.OnDestroy();
        }
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

        foreach (var chip in allSerialisedChips)
        {
            chip.valuesHeld = chip.defaultValues;
            chip.originalValuesHeld = chip.valuesHeld;
        }
        
        EngineLog.Log($"Loaded serialised chip values into circuit script: {CircuitScriptName}");
    }
    
    /// <summary>
    /// Create chip from data
    /// </summary>
    /// <param name="data">Chip data</param>
    /// <param name="setID">Set ID of chip</param>
    /// <returns>Chip created</returns>
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
    
    /// <summary>
    /// Get next available chip ID in circuit script
    /// </summary>
    /// <returns>ID</returns>
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
    
    /// <summary>
    /// Finds chip with ID in circuit script
    /// </summary>
    /// <param name="id">Chip ID</param>
    /// <returns>Chip</returns>
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
    
    /// <summary>
    /// Find all chips by type of chip
    /// </summary>
    /// <param name="typeOfChip">Chip type to look for</param>
    /// <returns>Chips of that type in script</returns>
    public static Chip[]? FindAllChipsByType(Type typeOfChip)
    {
        List<Chip> chips = new List<Chip>();

        foreach (var chip in chips)
        {
            if (chip.GetType() == typeOfChip) chips.Add(chip);
        }
        
        return chips.ToArray();
    }

    /// <summary>
    /// Find first chip with name
    /// </summary>
    /// <param name="name">Chip name</param>
    /// <returns>Chip</returns>
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
    /// <summary>
    /// Render the inspector for the circuit script component
    /// </summary>
    public override void RenderInspector()
    {
        int id = AttachedGameObject.Components.FindIndex(e => e == this);
        
        ImGui.PushID(id);
        if (ImGui.ImageButton("SearchCircuitScript", (IntPtr)LoadIcons.icons["MagnifyingGlass.png"], new Vector2(20)))
        {
            searchButtonClicked = true;
        }
        ImGui.PopID();
        
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

        if (allSerialisedChips.Any())
        {
            ImGui.Separator();
            ImGui.Text("Serialisation Values");

            foreach (var chip in allSerialisedChips)
            {
                ImGui.Text(chip.Name);

                ImGui.SameLine();
                ImGui.PushID(chip.Id);
                if (chip.serialisationType == typeof(bool))
                {
                    if (ImGui.Checkbox("##Value", ref chip.valuesHeld.Bool))
                    {
                    }
                }
                else if (chip.serialisationType == typeof(float))
                {
                    if (ImGui.InputFloat("##Value", ref chip.valuesHeld.Float))
                    {
                    }
                }
                else if (chip.serialisationType == typeof(int))
                {
                    if (ImGui.InputInt("##Value", ref chip.valuesHeld.Int))
                    {
                    }
                }
                else if (chip.serialisationType == typeof(string))
                {
                    if (ImGui.InputText("##Value", chip.stringSerialisationValue, (uint)chip.stringSerialisationValue.Count()))
                    {
                        chip.valuesHeld.String = Encoding.UTF8.GetString(chip.stringSerialisationValue).TrimEnd('\0');
                    }
                }
                else if (chip.serialisationType == typeof(Vector2))
                {
                    if (ImGui.InputFloat2("##Value", ref chip.valuesHeld.Vector2))
                    {
                    }
                }

                ImGui.PopID();
            }
        }
    }

    /// <summary>
    /// Get properties of circuit script component
    /// </summary>
    /// <returns>Properties of the component</returns>
    public override Dictionary<string, string> GetCustomProperties()
    {
        var circuitProperties = new Dictionary<string, string>
        {
            { "CircuitScriptDirPath", CircuitScriptDirPath },
            {"CircuitScriptName", CircuitScriptName}
        };

        foreach (var chip in allSerialisedChips)
        {
            circuitProperties[$"SER:{chip.Id.ToString()}"] = chip.GetValuesAsString(chip.valuesHeld);
        }
        
        return circuitProperties;
    }
#endif

    /// <summary>
    /// Set properties of the component
    /// </summary>
    /// <param name="properties">Properties to set</param>
    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("CircuitScriptName", out var n)) CircuitScriptName = n;
        if (properties.TryGetValue("CircuitScriptDirPath", out var p)) CircuitScriptDirPath = p;

        if (!string.IsNullOrWhiteSpace(CircuitScriptDirPath) && !string.IsNullOrWhiteSpace(CircuitScriptName))
        {
            var path = Path.Combine(CircuitScriptDirPath, CircuitScriptName + ".ccs");
            if (File.Exists(path)) LoadCircuit(path);
            CircuitScriptDirPath = p;
            CircuitScriptName = n;
        }

        foreach (var chip in allSerialisedChips)
        {
            if (properties.TryGetValue($"SER:{chip.Id.ToString()}", out var v))
            {
                chip.valuesHeld = chip.ParseValues(v);
            }
            else chip.valuesHeld = chip.defaultValues;
        }
    }

    /// <summary>
    /// Destroy and clean up component
    /// </summary>
    public override void DestroyComponent()
    {
        foreach (var chip in chips)
        {
            chip.OnDestroy();
        }

        chips.Clear();
        base.DestroyComponent();
    }
}