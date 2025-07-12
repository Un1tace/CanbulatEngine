using System.Text.Json.Serialization;
using CSCanbulatEngine.GameObjectScripts;
using Silk.NET.OpenGL;
using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling;

public class SceneSerialiser
{
    private readonly GL _gl;
    private readonly Mesh _defaultMesh;

    public SceneSerialiser(GL gl, Mesh defaultMesh)
    {
        _gl = gl;
        _defaultMesh = defaultMesh;
    }

    public void SaveScene(string filePath)
    {
        var sceneData = new SceneData.SceneInfo();
        sceneData.GameObjects = new List<SceneData.GameObjectData>();

        foreach (var obj in Engine._gameObjects)
        {
            List<SceneData.ComponentData> components = new();
            foreach(var component in obj.components)
            {
                SceneData.ComponentData? componentData = null;
                switch (component)
                {
                    case Transform transform:
                        SceneData.TransformData transformData = new SceneData.TransformData();
                        transformData.Name = "Transform";
                        transformData.Enabled = transform.isEnabled;
                        transformData.Position = transform.Position;
                        transformData.Scale = transform.Scale;
                        transformData.Rotation = transform.Rotation;
                        componentData = transformData;
                        break;
                    case MeshRenderer meshRenderer:
                        SceneData.MeshRendererData meshRendererData = new SceneData.MeshRendererData();
                        meshRendererData.Name = "MeshRenderer";
                        meshRendererData.Enabled = meshRenderer.isEnabled;
                        meshRendererData.Color = meshRenderer.Color;
                        meshRendererData.TexturePath = meshRenderer.TexturePath;
                        componentData = meshRendererData;
                        break;
                    case null:
                        componentData = null;
                        break;
                }
                if (componentData != null) components.Add(componentData);
            }
            sceneData.GameObjects.Add(new SceneData.GameObjectData(){Name = obj.Name, Components = components});
        }

        string json = JsonConvert.SerializeObject(sceneData, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, "Scene.txt"), json);
        Console.WriteLine($"Saved scene: {filePath}");
    }
}