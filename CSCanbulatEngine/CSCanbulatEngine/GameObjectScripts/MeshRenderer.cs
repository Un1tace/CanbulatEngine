namespace CSCanbulatEngine.GameObjectScripts;

public class MeshRenderer : Component
{
    public Mesh mesh { get; set; }

    public MeshRenderer(Mesh mesh) : base("MeshRenderer")
    {
        this.mesh = mesh;
    }
}