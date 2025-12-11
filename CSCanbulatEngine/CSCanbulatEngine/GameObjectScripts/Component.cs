using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

/// <summary>
/// Add-ons to an object to make it do functions.
/// </summary>
public class Component
{
    public static readonly List<(string, Type)> AllComponents = new()
    {
        ("Transform", typeof(Transform)),
        ("MeshRenderer", typeof(MeshRenderer)),
        ("CircuitScript", typeof(CircuitScript)),
        ("Rigidbody", typeof(Rigidbody))
    };
    
    public bool _isEnabled = true;
    public bool canBeDisabled = true;
    public bool canBeRemoved = true;

    public GameObject AttachedGameObject;
    
    public bool isEnabled
    {
        get => _isEnabled;
        set => _isEnabled = !canBeDisabled || value;
    }
    
    public string name = "";

    public Component(string ComponentName)
    {
        name = ComponentName;
    }

    public virtual void RenderInspector()
    {
       ImGui.Text("Empty Component :)");
    }

    public virtual void DestroyComponent()
    {
    }
    
    // Used for saving custom properties on the specific component that will be needed in the scene
    public virtual Dictionary<string, string> GetCustomProperties()
    {
        return new Dictionary<string, string>();
    }

    // Used for setting custom properties on the specific component that will be needed in the scene
    public virtual void SetCustomProperties(Dictionary<string, string> properties) {}
    
}

public unsafe class ComponentHolder
{
    public Component Component;

    public ComponentHolder(Component? component)
    {
        Component = component;
    }

    public ComponentHolder()
    {
        Component = null;
    }
}