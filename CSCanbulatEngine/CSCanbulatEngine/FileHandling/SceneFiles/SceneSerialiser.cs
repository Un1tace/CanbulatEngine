using System.Text.Json.Serialization;
using CSCanbulatEngine.Circuits;
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
        var sceneData = SceneDataFromCurrentScene(sceneName);

        string json = JsonConvert.SerializeObject(sceneData, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, sceneName + ".cbs"), json);
        Engine.currentScene.SceneFilePath = Path.Combine(filePath, sceneName + ".cbs");
        Console.WriteLine($"Saved scene: {filePath}");
        Engine.currentScene.SceneSavedOnce = true;
    }
    
    public static SceneData.SceneInfo SceneDataFromCurrentScene(string sceneName = "Example Scene")
    {
        string filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Scenes");
        var sceneData = new SceneData.SceneInfo();
        
        sceneData.SceneFilePath = filePath;
        sceneData.SceneName = sceneName;
        sceneData.GameObjects = new List<SceneData.GameObjectData>();
        sceneData.Events = new List<SceneData.EventData>();

        foreach (var theEvent in EventManager.RegisteredEvents)
        {
            SceneData.EventData eventData = new()
            {
                Name = theEvent.EventName,
                canConfig = theEvent.CanConfig,
                canReceive = theEvent.CanReceive,
                canSend = theEvent.CanSend,
                eventValuesData = theEvent.baseValues
            };
            sceneData.Events.Add(eventData);
        }
        
        foreach (var obj in Engine.currentScene.GameObjects)
        {
            SceneData.TransformData transformData = null;
            SceneData.MeshRendererData meshRendererData = null;
            foreach(var component in obj.Components)
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
            sceneData.GameObjects.Add(new SceneData.GameObjectData(){Name = obj.Name, transformData = transformData, meshRendererData = meshRendererData, ObjectID = obj.ID, Tags = obj.Tags});
        }

        return sceneData;
    }

    public static void LoadSceneFromString(string json)
    {
        var sceneData = JsonConvert.DeserializeObject<SceneData.SceneInfo>(json);
        
        //Reset managers
        VariableManager.Clear();
        EventManager.Clear();

        Engine.currentScene = new Scene(sceneData.SceneName);
        Engine.currentScene.SceneName = sceneData.SceneName;
        Engine.currentScene.SceneFilePath = sceneData.SceneFilePath;
        Engine.currentScene.SceneSavedOnce = true;

        if (sceneData.Events != null)
        {
            foreach (var eventData in sceneData.Events)
            {
                var theEvent = new Event(eventData.Name);
                theEvent.CanConfig = eventData.canConfig;
                theEvent.CanReceive = eventData.canReceive;
                theEvent.CanSend = eventData.canSend;
                theEvent.baseValues = eventData.eventValuesData;
                theEvent.EventName = eventData.Name;

                EventManager.RegisterEvent(theEvent);
            }
        }
        
        foreach (var objData in sceneData.GameObjects)
        {
            GameObject obj = new GameObject(Engine._squareMesh, objData.Name);
            if (objData.ObjectID is not null && GameObject.FindGameObject(objData.ObjectID.Value) is null)
            {
                obj.ID = objData.ObjectID.Value;
            }

            obj.Tags = objData.Tags ?? new List<string>();
            if (objData.transformData != null)
            {
                SceneData.TransformData transformData = objData.transformData;
                Transform transform = new Transform(obj);
                transform.Position = transformData.Position;
                transform.Scale = transformData.Scale;
                transform.Rotation = transformData.Rotation;
                transform.name = transformData.Name;
                transform.isEnabled = transformData.Enabled;
                transform.AttachedGameObject = obj;
                if (obj.GetComponentIndex<Transform>() != -1)
                {
                    obj.Components[obj.GetComponentIndex<Transform>()] = transform;
                }
            }

            if (objData.meshRendererData != null)
            {
                SceneData.MeshRendererData meshRendererData = objData.meshRendererData;
                MeshRenderer meshRenderer = new MeshRenderer(Engine._squareMesh, obj);
                meshRenderer.name = meshRendererData.Name;
                meshRenderer.Color = meshRendererData.Color;
                meshRenderer.TexturePath = meshRendererData.TexturePath;
                if (!String.IsNullOrWhiteSpace(meshRendererData.TexturePath))
                {
                    meshRenderer.AssignTexture(meshRendererData.TexturePath);
                }

                if (obj.GetComponentIndex<MeshRenderer>() != -1)
                {
                    obj.Components[obj.GetComponentIndex<MeshRenderer>()] = meshRenderer;
                }
            }
        }
    }
#endif
    public void LoadScene(string filePath)
    {
        string json = File.ReadAllText(filePath);

        LoadSceneFromString(json);
    }
}