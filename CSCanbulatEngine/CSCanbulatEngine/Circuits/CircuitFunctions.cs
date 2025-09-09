using System.ComponentModel;
using System.Data.SqlTypes;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;

namespace CSCanbulatEngine.Circuits;

// <summary>
//Static class for listening and dispatching events.
// Allows different parts of circuits to communicate without direct wired connections
// </summary>
public static class EventManager
{
    private static readonly Dictionary<string, List<Action<EventValues>>> s_eventListeners = new Dictionary<string, List<Action<EventValues>>>();
    public static readonly List<Event> RegisteredEvents = new List<Event>();

    //Pre defined Events
    static EventManager()
    {
        PredefineMainEvents();
    }

    public static void PredefineMainEvents()
    {
        RegisterEvent(new Event("OnStart", false, true, false));
        var updateEvent = new Event("OnUpdate", false, true, false);
        updateEvent.baseValues.floats.Add("Delta Time");
        RegisterEvent(updateEvent);
    }
    
    public static void RegisterEvent(Event values)
    {
        if (RegisteredEvents.Any(e => e.EventName == values.EventName)) return;
        
        RegisteredEvents.Add(values);
        if (!s_eventListeners.ContainsKey(values.EventName))
        {
            s_eventListeners.Add(values.EventName, new List<Action<EventValues>>());
        }
    }

    public static void DeleteEvent(Event values)
    {
        if (values == null || !values.CanConfig) return;

        if (RegisteredEvents.Contains(values))
        {
            RegisteredEvents.Remove(values);
        }

        if (s_eventListeners.ContainsKey(values.EventName))
        {
            s_eventListeners.Remove(values.EventName);
        }
        
        foreach (var chip in CircuitEditor.chips)
        {
            if (chip is EventChip eventChip && eventChip.SelectedEvent == values)
            {
                eventChip.ResetToUnconfigured();
            }
        }
    }
    
    public static void Subscribe(Event theEvent, Action<EventValues> listener)
    {
        if (!s_eventListeners.ContainsKey(theEvent.EventName))
        {
            s_eventListeners[theEvent.EventName] = new List<Action<EventValues>>();
        }
        s_eventListeners[theEvent.EventName].Add(listener);
    }

    public static void Unsubscribe(Event theEvent, Action<EventValues> listener)
    {
        if (theEvent == null || !s_eventListeners.ContainsKey(theEvent.EventName)) return;
        
        s_eventListeners[theEvent.EventName].Remove(listener);
    }

    public static void Trigger(Event theEvent, EventValues payload)
    {
        if (theEvent == null || !s_eventListeners.ContainsKey(theEvent.EventName)) return;

        var listeners = new List<Action<EventValues>>(s_eventListeners[theEvent.EventName]);
        foreach (var listener in listeners)
        {
            listener?.Invoke(payload);
        }
    }

    public static void Clear()
    { 
        var eventsToDelete = new List<Event>(RegisteredEvents);

        foreach (var ev in eventsToDelete) 
        {
            DeleteEvent(ev); 
        }
    

        if (!RegisteredEvents.Any(e => e.EventName == "OnStart" || !RegisteredEvents.Any(e => e.EventName == "OnUpdate"))) 
        {
            PredefineMainEvents(); 
        }
    }
}

//<summary> Class for sending values over events </summary>
public class EventValues
{
    public Dictionary<string, bool> bools = new Dictionary<string, bool>();
    public Dictionary<string, float> floats = new Dictionary<string, float>();
    public Dictionary<string, int> ints = new Dictionary<string, int>();
    public Dictionary<string, string> strings = new Dictionary<string, string>();
    public Dictionary<string, Vector2> Vector2s = new Dictionary<string, Vector2>();
    public Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
}

public class BaseEventValues
{
    public List<string> bools = new List<string>();
    public List<string> floats = new List<string>();
    public List<string> ints = new List<string>();
    public List<string> strings = new List<string>();
    public List<string> Vector2s = new List<string>();
    public List<string> GameObjects = new List<string>();
}

public class Event(string eventName, bool canSend = true, bool canReceive = true, bool canConfig = true)
{
    public string EventName = eventName;
    public BaseEventValues baseValues = new BaseEventValues();
    public bool CanSend = canSend;
    public bool CanReceive = canReceive;
    public bool CanConfig = canConfig;
}

public static class VariableManager
{
    public static Dictionary<string, Values> Variables = new Dictionary<string, Values>();

    public static void Clear()
    {
        Variables.Clear();
    }
}

public static class ConfigWindows
{
    public static int? portIndexToConfig = null;
    public static EventChip MainChip = null;
    public static byte[] portNameChangeBuffer = new byte[128];
    public static void ShowEventPortConfigWindow(Vector2? pos, Vector2? size)
    {
        if (size is null)
        {
            size = new Vector2(200, 250);
        }
        
        if (pos is null)
        {
            pos = ImGui.GetWindowSize()/2 - size/2;
        }
        
        ImGui.SetWindowPos(pos?? Vector2.Zero);
        ImGui.SetNextWindowSize(size ?? Vector2.Zero);
        ImGui.Begin("Port Configuration", ref Engine.portConfigWindowOpen, ImGuiWindowFlags.NoResize);
        
        //Change port name
        if (ImGui.InputText("Port Name", portNameChangeBuffer, 128))
        {
            
        }

        if (ImGui.Button("Set"))
        {
            string newName = Encoding.UTF8.GetString(portNameChangeBuffer).TrimEnd('\0');
            string oldName = MainChip.ports[portIndexToConfig.Value];

            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName && MainChip.ports.All(e => e != newName))
            {
                int portTypeIndex = MainChip.GetPortTypeIndex(MainChip.portTypes[portIndexToConfig.Value]);
                int selectedIndex = portIndexToConfig.Value;

                for (int i = 0; i < portTypeIndex; i++)
                {
                    selectedIndex -= MainChip.allPortTypes[i].Count;
                }

                MainChip.allPortTypes[portTypeIndex][selectedIndex] = newName;
                MainChip.ConfigureAllChipsToEvent();
            }
        }
        
        // Change port type
        if (portIndexToConfig.HasValue && portIndexToConfig.Value < MainChip.portTypes.Count)
        {
            int index = portIndexToConfig.Value;
            if (ImGui.BeginCombo("Port Types", MainChip.portTypes[index].FullName?.Split('.').Last()))
            {
                List<Type> avaliableTypes =
                    [typeof(bool), typeof(float), typeof(int), typeof(string), typeof(Vector2), typeof(GameObject)];

                foreach (var type in avaliableTypes)
                {
                    if (ImGui.Selectable(type.FullName?.Split('.').Last(), type == MainChip.portTypes[index]))
                    {
                        MainChip.ChangePortType(index, type);
                    }
                }
                ImGui.EndCombo();
            }
        }
        else
        {
            ImGui.Text("Invalid port selected.");
        }
        ImGui.End();
    }

    public static void EnableEventPortConfigWindow(int portIndex, EventChip theChip)
    {
        MainChip = theChip;
        portIndexToConfig = portIndex;
        Engine.portConfigWindowPosition = ImGui.GetMousePos();
        Engine.portConfigWindowOpen = true;
        portNameChangeBuffer = UTF8Encoding.UTF8.GetBytes(MainChip.ports[portIndexToConfig.Value].ToString());
    }
}