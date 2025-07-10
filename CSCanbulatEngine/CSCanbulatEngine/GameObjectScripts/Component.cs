namespace CSCanbulatEngine.GameObjectScripts;

public class Component
{
    public bool isEnabled = true;
    public bool canBeDisabled = true;
    public bool canBeRemoved = true;
    public string name = "";

    public Component(string ComponentName)
    {
        name = ComponentName;
    }
}