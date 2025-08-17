using CSCanbulatEngine.Circuits;
using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling.CircuitHandling;

public static class CircuitSerialiser
{
    public static void SaveCircuit(string circuitName)
    {
        string filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Circuits");
        Directory.CreateDirectory(filePath);

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
                ChipType = chip.GetType().FullName
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

        foreach (var chip in circuitInfo.Chips)
        {
            Type chipType = Type.GetType(chip.ChipType);

            if (chipType != null)
            {
                Chip newChip = (Chip)Activator.CreateInstance(chipType, chip.id, chip.Name, chip.Position);
                CircuitEditor.chips.Add(newChip);
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
        
        Console.WriteLine($"Loaded circuit script: {filePath}");
    }
}