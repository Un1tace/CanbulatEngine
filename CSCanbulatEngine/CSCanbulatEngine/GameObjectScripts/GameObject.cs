using System.ComponentModel;

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
        components.Add(Mesh);
        int nameCheck = 0;
        foreach (var gameObject in Engine._gameObjects)
        {
            if (gameObject.Name == Name)
            {
                nameCheck++;
                Name = Name + nameCheck;
            }
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
            components.Remove(component);
        }
        Console.WriteLine($"Component ({component.name}) cannot be removed");
    }
}