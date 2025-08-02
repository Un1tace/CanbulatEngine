using System.Numerics;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;

namespace CSCanbulatEngine.GameObjectScripts;

public class MeshRenderer : Component
{
    public Mesh Mesh { get; set; }

    public Vector4 Color = Vector4.One;

    public uint TextureID { get; private set; }

    private static Vector2D<int> _imageResolution = Vector2D<int>.Zero;

    public string? TexturePath;

    public Vector2D<int> ImageResolution
    {
        get
        {
            if (TextureID != 0)
            {
                return _imageResolution;
            }
            else
            {
                return Vector2D<int>.One;
            }
        }
        set => _imageResolution = value;
    }

    public MeshRenderer(Mesh mesh) : base("MeshRenderer")
    {
        this.Mesh = mesh;
        TextureID = 0;
    }

    public void AssignTexture(string path)
    {
        try
        {
            string fullPath = path;
            Vector2D<int> sizeOutput = new Vector2D<int>();
            TextureID = TextureLoader.Load(Engine.gl, fullPath, out sizeOutput);
            ImageResolution = sizeOutput;
            TexturePath = path;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to load texture: {e.Message}");
        }
    }
    
#if EDITOR
    //Color picker
    public override unsafe void RenderInspector()
    {
        if (ImGui.ColorEdit4("Color", ref Color))
        {
            //?Possible different options like hex or colour picker
        }
        ImGui.Text($"Texture ID: {TextureID}");
        ImGui.Text("Image: ");
        ImGui.SameLine();
        if (ImGui.ImageButton("SearchImage", (IntPtr)LoadIcons.icons["MagnifyingGlass.png"], new Vector2(20, 20)))
        {}

        ImGui.SameLine();
        if (ImGui.ImageButton("ClearImage", (IntPtr)LoadIcons.icons["Cross.png"], new Vector2(20, 20)))
        {
            TextureID = 0;
        }
        ImGui.SameLine();
        ImGui.ImageButton("Image", (IntPtr)TextureID, new Vector2(100, 100));
        

        if (ImGui.BeginDragDropTarget())
        {
            ImGuiPayloadPtr payloadPtr = ImGui.AcceptDragDropPayload("DND_ASSET_PATH");

            if (payloadPtr.NativePtr != null)
            {
                string assetPath = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(payloadPtr.Data);

                if (assetPath.ToLower().EndsWith(".png") || assetPath.ToLower().EndsWith(".jpg") || assetPath.ToLower().EndsWith(".jpeg"))
                {
                    try
                    {
                        AssignTexture(assetPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to load dropped texture: {e.Message}");
                    }
                }
            }
            
            ImGui.EndDragDropTarget();
        }
    }
#endif
}