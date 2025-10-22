using System.Numerics;
using CSCanbulatEngine.Circuits;

namespace CSCanbulatEngine.FileHandling;

public class SceneData
{
    // public class TransformData : ComponentData
    // {
    //     public Vector2 Position
    //     {
    //         get { return new Vector2(PositionX, PositionY); }
    //         set { PositionX = value.X; PositionY = value.Y; }
    //     }
    //     public float PositionX { get; set; }
    //     public float PositionY { get; set; }
    //     
    //     public float Rotation { get; set; }
    //     
    //     public Vector2 Scale
    //     {
    //         get { return new Vector2(ScaleX, ScaleY);}
    //         set
    //         {
    //             ScaleX = value.X;
    //             ScaleY = value.Y;
    //         } }
    //     public float ScaleX { get; set; }
    //     public float ScaleY { get; set; }
    // }
    //
    // public class MeshRendererData : ComponentData
    // {
    //     public Vector4 Color
    //     {
    //         get { return new Vector4(ColorR, ColorG, ColorB, ColorA);}
    //         set
    //         {
    //             ColorR = value.X;
    //             ColorG = value.Y;
    //             ColorB = value.Z;
    //             ColorA = value.W;
    //         } }
    //     public float ColorR { get; set; }
    //     public float ColorG { get; set; }
    //     public float ColorB { get; set; }
    //     public float ColorA { get; set; }
    //     public string? TexturePath { get; set; }
    // }
    
    public class ComponentData
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string ComponentType { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; } = new();
    }
    
    public class GameObjectData
    {
        public string Name { get; set; }
        // public TransformData transformData { get; set; }
        // public MeshRendererData meshRendererData { get; set; }
        public List<ComponentData> ComponentData { get; set; } = new();
        public int? ObjectID  { get; set; }
        public int? ParentObjectID { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class EventData
    {
        public string Name { get; set; }
        public BaseEventValues eventValuesData { get; set; }
        public bool canSend { get; set; }
        public bool canReceive { get; set; }
        public bool canConfig { get; set; }
    }
    
    public class SceneInfo
    {
        public string SceneFilePath { get; set; }
        public string SceneName { get; set; }
        public List<GameObjectData> GameObjects { get; set; }
        public List<EventData> Events { get; set; }
    }
}