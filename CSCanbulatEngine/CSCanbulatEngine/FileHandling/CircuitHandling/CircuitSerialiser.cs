using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.Circuits;
using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling.CircuitHandling;

public static class CircuitSerialiser
{
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
            circuitInfo.Chips.Add(new CircuitData.ChipData
            {
                id = chip.Id,
                Name = chip.Name,
                Position = chip.Position,
                ChipType = chip.GetType().FullName,
                Color = chip.Color,
                CustomProperties = chip.GetCustomProperties()
            });
        }

        foreach (var chip in CircuitEditor.chips)
        {
            var allInputPorts = chip.InputPorts.Concat(chip.InputExecPorts);
            foreach (var inputPort in allInputPorts)
            {
                if (inputPort.ConnectedPort != null)
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
                    var portValue = inputPort.Value;
                    string valueStr = "";
                    if (inputPort.PortType == typeof(bool)) valueStr = portValue.b.Value.ToString();
                    else if (inputPort.PortType == typeof(int)) valueStr = portValue.i.Value.ToString();
                    else if (inputPort.PortType == typeof(float)) valueStr = portValue.f.Value.ToString(CultureInfo.InvariantCulture);
                    else if (inputPort.PortType == typeof(string)) valueStr = portValue.s;
                    else if (inputPort.PortType == typeof(Vector2)) valueStr = $"{portValue.v2.Value.X},{portValue.v2.Value.Y}";
                    
                    circuitInfo.UnconnectedPortValues.Add(new CircuitData.UnconnectedPortValueData
                    {
                        ChipId = chip.Id,
                        PortId = inputPort.Id,
                        ValueType = inputPort.PortType.FullName,
                        Value = valueStr
                    });
                }
            }
        }

        string json = JsonConvert.SerializeObject(circuitInfo, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, circuitName + ".ccs"), json);
        Console.WriteLine($"Saved circuit script: {circuitName}");
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
            Type chipType = Type.GetType(chip.ChipType);

            if (chipType != null)
            {
                Chip newChip = (Chip)Activator.CreateInstance(chipType, chip.id, chip.Name, chip.Position);
                newChip.SetCustomProperties(chip.CustomProperties);
                newChip.Color = chip.Color;
                CircuitEditor.chips.Add(newChip);
            }
        }

        foreach (var portValueData in circuitInfo.UnconnectedPortValues)
        {
            var chip = CircuitEditor.FindChip(portValueData.ChipId);
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

        CircuitEditor.selectedChip = null;
        CircuitEditor.lastSelectedChip = null;
        Console.WriteLine($"Loaded circuit script: {filePath}");
    }
}