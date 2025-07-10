using System.ComponentModel;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

//Represents object in game world, with transform and mesh
public class GameObject
{
    public string Name { get; set; }
    public Transform Transform { get; set; }
    public Mesh Mesh { get; set; }

    public List<Component> components { get; }

    public GameObject(Mesh mesh, string name = "GameObject")
    {
        Mesh = mesh;
        Transform = new Transform();
        Name = name;
        components = new List<Component>();
        components.Add(Transform);
        components.Add(new MeshRenderer(mesh));
        int nameCheck = 0;
        foreach (var gameObject in Engine._gameObjects)
        {
            if (gameObject.Name == Name || gameObject.Name == (Name + nameCheck))
            {
                nameCheck++;
                
            }
        }
        Name = Name + nameCheck;
        if (!Engine._gameObjects.Contains(this))
        {
            Engine._gameObjects.Add(this);
        }
    }

    public void AddComponent(Component component)
    {
        components.Add(component);
    }

    public void RemoveComponent(Component component)
    {
        if (component.canBeRemoved)
        {
            component.DestroyComponent();
            components.Remove(component);
        }

        Console.WriteLine($"Component ({component.name}) cannot be removed");
    }
    

    public void DeleteObject()
    {
        while (components.Count > 0)
        {
            components[0].DestroyComponent();
            components.RemoveAt(0);
        }

        Engine._gameObjects.Remove(this);
#if EDITOR
        if (Engine._selectedGameObject == this)
        {
            Engine._selectedGameObject = null;
        }
#endif
    }

#if EDITOR
    public void RenderObjectOptionBar(string superKey)
    {
        if (ImGui.BeginMenu(Name))
        {

            if (ImGui.MenuItem("Remove", superKey + "+Back"))
            {
                DeleteObject();
            }
            ImGui.EndMenu();
        }
    }
#endif

}