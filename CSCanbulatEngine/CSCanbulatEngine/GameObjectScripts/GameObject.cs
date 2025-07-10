namespace CSCanbulatEngine.GameObjectScripts;

//Represents object in game world, with transform and mesh
public class GameObject
{
    public Transform Transform { get; set; }
    public Mesh Mesh { get; set; }

    public GameObject(Mesh mesh)
    {
        Mesh = mesh;
        Transform = new Transform();
    }
}