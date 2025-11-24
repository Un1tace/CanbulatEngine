using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.GameObjectScripts;
using Newtonsoft.Json;
using Silk.NET.Input;

namespace CSCanbulatEngine.FileHandling.CircuitHandling;

public static class CircuitSerialiser
{
    #if EDITOR
    public static void SaveCircuit(string circuitName, string filePath = "")
    {
        if (filePath == "")
        {
            filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Circuits");
        }
        
        Directory.CreateDirectory(filePath);
        CircuitEditor.CircuitScriptName = circuitName;
        CircuitEditor.CircuitScriptDirPath = filePath;

        var circuitInfo = new CircuitData.CircuitInfo
        {
            CircuitScriptName = circuitName
        };

        foreach (var chip in CircuitEditor.chips)
        {
            // circuitInfo.Chips.Add(new CircuitData.ChipData
            // {
            //     id = chip.Id,
            //     Name = chip.Name,
            //     Position = chip.Position,
            //     ChipType = chip.GetType().FullName,
            //     Color = chip.Color,
            //     CustomProperties = chip.GetCustomProperties()
            // });
            
            circuitInfo.Chips.Add(GetChipData(chip));
        }

        foreach (var chip in CircuitEditor.chips)
        {
            var allInputPorts = chip.InputPorts.Concat(chip.InputExecPorts);
            foreach (var inputPort in allInputPorts)
            {
                if (inputPort is ExecPort execPort)
                {
                    if (execPort.InputConnections != null)
                    {
                        foreach (var connectedOutputPort in execPort.InputConnections)
                        {
                            circuitInfo.Connections.Add(new CircuitData.PortConnectionData
                            {
                                InputPortId = execPort.Id,
                                InputChipId = chip.Id,
                                OutputChipId = connectedOutputPort.Parent.Id,
                                OutputPortId = connectedOutputPort.Id
                            });
                        }
                    }
                }
                else if (inputPort.ConnectedPort != null)
                {
                    circuitInfo.Connections.Add(new CircuitData.PortConnectionData
                    {
                        InputPortId = inputPort.Id,
                        InputChipId = chip.Id,
                        OutputChipId = inputPort.ConnectedPort.Parent.Id,
                        OutputPortId = inputPort.ConnectedPort.Id
                    });
                }
                else if (inputPort.PortType != null)
                {
                    // var portValue = inputPort.Value;
                    // string valueStr = "";
                    // if (inputPort.PortType == typeof(bool)) valueStr = portValue.Bool.Value.ToString();
                    // else if (inputPort.PortType == typeof(int)) valueStr = portValue.Int.Value.ToString();
                    // else if (inputPort.PortType == typeof(float)) valueStr = portValue.Float.Value.ToString(CultureInfo.InvariantCulture);
                    // else if (inputPort.PortType == typeof(string)) valueStr = portValue.String;
                    // else if (inputPort.PortType == typeof(Vector2)) valueStr = $"{portValue.Vector2.Value.X},{portValue.Vector2.Value.Y}";
                    // else if (inputPort.PortType == typeof(Key)) valueStr = portValue.Key.ToString();
                    // else if (inputPort.PortType == typeof(MouseButton)) valueStr = portValue.MouseButton.ToString();

                    string valueStr = GetPortValueAsString(inputPort);
                    
                    circuitInfo.UnconnectedPortValues.Add(new CircuitData.UnconnectedPortValueData
                    {
                        ChipId = chip.Id,
                        PortId = inputPort.Id,
                        ValueType = TypeHelper.GetName(inputPort.PortType),
                        Value = valueStr
                    });
                }
            }
        }

        foreach (var theObject in Engine.currentScene.GameObjects)
        {
            if (theObject.HasComponent<CircuitScript>() &&
                theObject.GetComponent<CircuitScript>().CircuitScriptName == Path.GetFileNameWithoutExtension(circuitName))
            {
                theObject.GetComponent<CircuitScript>().LoadCircuit(Path.Combine(filePath, circuitName + ".ccs"));
            }
        }

        string json = JsonConvert.SerializeObject(circuitInfo, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, circuitName + ".ccs"), json);
        EngineLog.Log($"Saved circuit script: {circuitName}");
        Engine.ReloadAllCircuitScripts();
    }

    public static string GetPortValueAsString(ChipPort port)
    {
        var portValue = port.Value;
        string valueStr = "";
        if (port.PortType == typeof(bool)) valueStr = portValue.Bool.Value.ToString();
        else if (port.PortType == typeof(int)) valueStr = portValue.Int.Value.ToString();
        else if (port.PortType == typeof(float)) valueStr = portValue.Float.Value.ToString(CultureInfo.InvariantCulture);
        else if (port.PortType == typeof(string)) valueStr = portValue.String;
        else if (port.PortType == typeof(Vector2)) valueStr = $"{portValue.Vector2.Value.X},{portValue.Vector2.Value.Y}";
        else if (port.PortType == typeof(Key)) valueStr = portValue.Key.ToString();
        else if (port.PortType == typeof(MouseButton)) valueStr = portValue.MouseButton.ToString();

        return valueStr;
    }
#endif

    public static void ParseAndSetPortData(CircuitData.UnconnectedPortValueData portValueData, ChipPort portToSet)
    {
        if (portToSet != null)
        {
            Type? type = TypeHelper.GetType(portValueData.ValueType)?? Type.GetType(portValueData.ValueType);
            if (type != null)
            {
                if (type == typeof(bool)) portToSet.Value.SetValue(bool.Parse(portValueData.Value));
                else if (type == typeof(int)) portToSet.Value.SetValue(int.Parse(portValueData.Value));
                else if (type == typeof(float)) portToSet.Value.SetValue(float.Parse(portValueData.Value));
                else if (type == typeof(string)) portToSet.Value.SetValue(portValueData.Value);
                else if (type == typeof(Vector2))
                {
                    var parts = portValueData.Value.Split(',');
                    portToSet.Value.SetValue(new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture),
                        float.Parse(parts[1], CultureInfo.InvariantCulture)));
                }
                else if (type == typeof(Key))
                {
                    portToSet.Value.SetValue(Enum.GetValues(typeof(Key)).Cast<Key>().ToList().Find(e => e.ToString() == portValueData.Value));
                }
                else if (type == typeof(MouseButton))
                {
                    portToSet.Value.SetValue(Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList().Find(e => e.ToString() == portValueData.Value));
                }
            }
        }
    }
    
    public static CircuitData.ChipData GetChipData(Chip chip)
    {
        var theChip = new CircuitData.ChipData
        {
            id = chip.Id,
            Name = chip.Name,
            Position = chip.Position,
            ChipType = chip.GetType().FullName,
            Color = chip.Color,
            CustomProperties = chip.GetCustomProperties()
        };

        return theChip;
    }

    public static Chip? CreateChipFromData(CircuitData.ChipData data, bool setID = true)
    {
        Type chipType = Type.GetType(data.ChipType);

        if (chipType != null)
        {
            Chip newChip = (Chip)Activator.CreateInstance(chipType, setID? data.id : CircuitEditor.GetNextAvaliableChipID(), data.Name, data.Position);
            newChip.SetCustomProperties(data.CustomProperties);
            newChip.Color = data.Color;
            CircuitEditor.chips.Add(newChip);
            return newChip;
        }

        return null;
    }

    public static void LoadCircuit(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var circuitInfo = JsonConvert.DeserializeObject<CircuitData.CircuitInfo>(json);
        
        CircuitEditor.chips.Clear();
        CircuitEditor.CircuitScriptName = Path.GetFileNameWithoutExtension(filePath);
        CircuitEditor.CircuitScriptDirPath = Path.GetDirectoryName(filePath);
        
        foreach (var chip in circuitInfo.Chips)
        {
            CreateChipFromData(chip);
        }

        foreach (var portValueData in circuitInfo.UnconnectedPortValues)
        {
            var chip = CircuitEditor.FindChip(portValueData.ChipId);
            var port = chip?.InputPorts.FirstOrDefault(p => p.Id == portValueData.PortId);

            ParseAndSetPortData(portValueData, port);
            // if (port != null)
            // {
            //     Type? type = TypeHelper.GetType(portValueData.ValueType)?? Type.GetType(portValueData.ValueType);
            //     if (type != null)
            //     {
            //         if (type == typeof(bool)) port.Value.SetValue(bool.Parse(portValueData.Value));
            //         else if (type == typeof(int)) port.Value.SetValue(int.Parse(portValueData.Value));
            //         else if (type == typeof(float)) port.Value.SetValue(float.Parse(portValueData.Value));
            //         else if (type == typeof(string)) port.Value.SetValue(portValueData.Value);
            //         else if (type == typeof(Vector2))
            //         {
            //             var parts = portValueData.Value.Split(',');
            //             port.Value.SetValue(new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture),
            //                 float.Parse(parts[1], CultureInfo.InvariantCulture)));
            //         }
            //         else if (type == typeof(Key))
            //         {
            //             port.Value.SetValue(Enum.GetValues(typeof(Key)).Cast<Key>().ToList().Find(e => e.ToString() == portValueData.Value));
            //         }
            //         else if (type == typeof(MouseButton))
            //         {
            //             port.Value.SetValue(Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList().Find(e => e.ToString() == portValueData.Value));
            //         }
            //     }
            // }
        }
        
        foreach (var connectionData in circuitInfo.Connections)
        {
            var outputChip = CircuitEditor.FindChip(connectionData.OutputChipId);
            var inputChip = CircuitEditor.FindChip(connectionData.InputChipId);

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

        #if EDITOR
        CircuitEditor.selectedChip = null;
        CircuitEditor.lastSelectedChip = null;
#endif
        EngineLog.Log($"Loaded circuit script: {filePath}");
    }
}