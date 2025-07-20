using System.Text.Json.Serialization;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.InfoHolders;
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
#if EDITOR
    public void SaveScene(string sceneName = "Example Scene")
    {
        string filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Scenes");
        var sceneData = new SceneData.SceneInfo();
        sceneData.SceneFilePath = filePath;
        sceneData.SceneName = sceneName;
        sceneData.GameObjects = new List<SceneData.GameObjectData>();

        foreach (var obj in Engine.currentScene.GameObjects)
        {
            SceneData.TransformData transformData = null;
            SceneData.MeshRendererData meshRendererData = null;
            foreach(var component in obj.components)
            {
                switch (component)
                {
                    case Transform transform:
                        transformData = new SceneData.TransformData();
                        transformData.Name = "Transform";
                        transformData.Enabled = transform.isEnabled;
                        transformData.Position = transform.Position;
                        transformData.Scale = transform.Scale;
                        transformData.Rotation = transform.Rotation;
                        break;
                    case MeshRenderer meshRenderer:
                        meshRendererData = new SceneData.MeshRendererData();
                        meshRendererData.Name = "MeshRenderer";
                        meshRendererData.Enabled = meshRenderer.isEnabled;
                        meshRendererData.Color = meshRenderer.Color;
                        meshRendererData.TexturePath = meshRenderer.TexturePath;
                        break;
                    case null:
                        break;
                }
            }
            sceneData.GameObjects.Add(new SceneData.GameObjectData(){Name = obj.Name, transformData = transformData, meshRendererData = meshRendererData});
        }

        string json = JsonConvert.SerializeObject(sceneData, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, sceneName + ".cbs"), json);
        Engine.currentScene.SceneFilePath = Path.Combine(filePath, sceneName + ".cbs");
        Console.WriteLine($"Saved scene: {filePath}");
        Engine.currentScene.SceneSavedOnce = true;
    }
#endif
    public void LoadScene(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var sceneData = JsonConvert.DeserializeObject<SceneData.SceneInfo>(json);

        Engine.currentScene = new Scene(sceneData.SceneName);
        Engine.currentScene.SceneName = sceneData.SceneName;
        Engine.currentScene.SceneFilePath = sceneData.SceneFilePath;
        Engine.currentScene.SceneSavedOnce = true;
        foreach (var objData in sceneData.GameObjects)
        {
            GameObject obj = new GameObject(Engine._squareMesh, objData.Name);
            if (objData.transformData != null)
            {
                SceneData.TransformData transformData = objData.transformData;
                Transform transform = new Transform();
                transform.Position = transformData.Position;
                transform.Scale = transformData.Scale;
                transform.Rotation = transformData.Rotation;
                transform.name = transformData.Name;
                transform.isEnabled = transformData.Enabled;
                transform.AttachedGameObject = obj;
                if (obj.GetComponentIndex<MeshRenderer>() != -1)
                {
                    obj.components[obj.GetComponentIndex<Transform>()] = transform;
                }
            }

            if (objData.meshRendererData != null)
            {
                SceneData.MeshRendererData meshRendererData = objData.meshRendererData;
                MeshRenderer meshRenderer = new MeshRenderer(Engine._squareMesh);
                meshRenderer.name = meshRendererData.Name;
                meshRenderer.Color = meshRendererData.Color;
                meshRenderer.TexturePath = meshRendererData.TexturePath;
                if (!String.IsNullOrWhiteSpace(meshRendererData.TexturePath))
                {
                    meshRenderer.AssignTexture(meshRendererData.TexturePath);
                }

                if (obj.GetComponentIndex<MeshRenderer>() != -1)
                {
                    obj.components[obj.GetComponentIndex<MeshRenderer>()] = meshRenderer;
                }
            }
        }
    }
}