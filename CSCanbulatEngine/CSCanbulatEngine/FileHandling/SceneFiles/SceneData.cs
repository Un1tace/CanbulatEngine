using System.Numerics;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.GameObjectScripts;

namespace CSCanbulatEngine.FileHandling;

public class SceneData
{
    public record ComponentData
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string ComponentType { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; } = new();
    }
    
    public record GameObjectData
    {
        public string Name { get; set; }
        public List<ComponentData> ComponentData { get; set; } = new();
        public int? ObjectID  { get; set; }
        public int? ParentObjectID { get; set; }
        public List<string>? Tags { get; set; }
        public ObjectType? ObjectType { get; set; }
        
        // Used in prefabs
        public List<GameObjectData>? Children { get; set; }
    }

    public record EventData
    {
        public string Name { get; set; }
        public BaseEventValues eventValuesData { get; set; }
        public bool canSend { get; set; }
        public bool canReceive { get; set; }
        public bool canConfig { get; set; }
    }
    
    public record SceneInfo
    {
        public string SceneFilePath { get; set; }
        public string SceneName { get; set; }
        public List<GameObjectData> GameObjects { get; set; }
        public List<EventData> Events { get; set; }
    }
}