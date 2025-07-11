using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

public class MeshRenderer : Component
{
    public Mesh Mesh { get; set; }

    public Vector4 Color = Vector4.One;

    public uint TextureID = 0;

    public MeshRenderer(Mesh mesh) : base("MeshRenderer")
    {
        this.Mesh = mesh;
    }

    public void AssignTexture(string path)
    {
        try
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, "EditorAssets/Images/Logo.png");
            TextureID = TextureLoader.Load(Engine.gl, fullPath, out _);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to load texture: {e.Message}");
        }
    }
    
#if EDITOR
    //Color picker
    public override void RenderInspector()
    {
        if (ImGui.ColorEdit4("Color", ref Color))
        {
            
        }
        
        ImGui.Text($"Texture ID: {TextureID}");
        if (TextureID != 0)
        {
            ImGui.Image((IntPtr)TextureID, new Vector2(100, 100)); //Show preview
        }
    }
#endif
}