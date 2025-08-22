using System.ComponentModel;
using System.Data.SqlTypes;
using System.Numerics;
using CSCanbulatEngine.GameObjectScripts;

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
        RegisterEvent(new Event("OnStart", false, true, false));
        var updateEvent = new Event("OnUpdate", false, true, false);
        updateEvent.BaseEventValues.floats.Add("Delta Time", 0);
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

public class Event(string eventName, bool canSend = true, bool canReceive = true, bool canConfig = true)
{
    public string EventName = eventName;
    public EventValues BaseEventValues = new EventValues();
    public bool CanSend = canSend;
    public bool CanReceive = canReceive;
    public bool CanConfig = canConfig;
}