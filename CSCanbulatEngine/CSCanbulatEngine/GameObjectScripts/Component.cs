using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

public class Component
{
    
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