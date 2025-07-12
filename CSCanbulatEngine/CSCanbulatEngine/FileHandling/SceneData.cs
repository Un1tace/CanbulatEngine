using System.Numerics;

namespace CSCanbulatEngine.FileHandling;

public class SceneData
{
    public class TransformData : ComponentData
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
    }

    public class MeshRendererData : ComponentData
    {
        public Vector4 Color { get; set; }
        public string? TexturePath { get; set; }
    }
    
    public class ComponentData
    {
        public bool Enabled { get; set; }
        public string Name { get; set; }
    }
    
    public class GameObjectData
    {
        public string Name { get; set; }
        public List<ComponentData> Components { get; set; }
    }
    
    public class SceneInfo
    {
        public string ProjectName { get; set; }
        public List<GameObjectData> GameObjects { get; set; }
    }
}