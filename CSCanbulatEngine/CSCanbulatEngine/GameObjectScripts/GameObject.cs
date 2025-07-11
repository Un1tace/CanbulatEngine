using System.ComponentModel;
using System.Text;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

//Represents object in game world, with transform and mesh
public class GameObject
{
    public string Name { get; set; }
    public Transform Transform { get; set; }

    public List<Component> components { get; }

    public GameObject(Mesh mesh, string name = "GameObject")
    {
        Transform = new Transform();
        Name = name;
        components = new List<Component>();
        
        //Add Core Components
        AddComponent(Transform);
        AddComponent(new MeshRenderer(mesh));
        
        //
        int nameCheck = 0;
        string baseName = name;
        while (Engine._gameObjects.Any(go => go.Name == Name))
        {
            nameCheck++;
            Name = $"{baseName}{nameCheck}";
        }
        
        
        if (!Engine._gameObjects.Contains(this))
        {
            Engine._gameObjects.Add(this);
        }
#if EDITOR
        Engine._selectedGameObject = this;
#endif
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in components)
        {
            if (component is T typedComponent)
            {
                return typedComponent;
            }
        }

        return null;
    }

    public void AddComponent(Component component) {
        
        components.Add(component);
        component.AttachedGameObject = this;
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
            if (ImGui.MenuItem("Rename", superKey + "+2"))
            {
                Array.Clear(Engine._nameBuffer, 0, Engine._nameBuffer.Length);
                if (Engine._selectedGameObject != null)
                {
                    byte[] currentNameBytes = Encoding.UTF8.GetBytes(Engine._selectedGameObject.Name);
                    Array.Copy(currentNameBytes, Engine._nameBuffer, currentNameBytes.Length);
                }
                // ImGui.OpenPopup("Rename Object");
                Engine.renamePopupOpen = true;
            }
            ImGui.EndMenu();
        }
    }
#endif

}