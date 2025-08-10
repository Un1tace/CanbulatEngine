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
    public static List<Event> RegisteredEvents = new List<Event>();

    public static void RegisterEvent(Event values)
    {
        if (!RegisteredEvents.Contains(values)) RegisteredEvents.Add(values);
        if (!s_eventListeners.ContainsKey(values.EventName))  s_eventListeners.Add(values.EventName, new List<Action<EventValues>>());
    }
    
    public static void Subscribe(Event theEvent, Action<EventValues> listener)
    {
        if (!RegisteredEvents.Contains(theEvent)) RegisteredEvents.Add(theEvent);

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
    public Dictionary<string, string> longs = new Dictionary<string, string>();
    public Dictionary<string, Vector2> vectors = new Dictionary<string, Vector2>();
    public Dictionary<string, GameObject> vector3s = new Dictionary<string, GameObject>();
}

public class Event(string eventName)
{
    public string EventName = eventName;
    public EventValues BaseEventValues = new EventValues();
}