using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace CSCanbulatEngine.GameObjectScripts;

public class MeshRenderer : Component
{
    public Mesh Mesh { get; set; }

    public Vector4 Color = Vector4.One;

    public uint TextureID { get; private set; }

    private static Vector2D<int> _imageResolution = Vector2D<int>.Zero;

    public string? TexturePath;

    private bool searchButtonClicked = false;

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

    public void Draw()
    {
        if (isEnabled)
        {
            //Set color in shader :)
            Engine.shader.SetUniform("uColor", Color);

            uint textureToBind = TextureID != 0 ? TextureID : Engine._whiteTexture;
            Engine.gl.BindTexture(TextureTarget.Texture2D, textureToBind);

            //Get the matrix from the transform
            Matrix4x4 modelMatrix = AttachedGameObject.GetComponent<Transform>().GetModelMatrix();
            //Set model uniform in the shader for the object
            Engine.shader.SetUniform("model", modelMatrix);

            Mesh.Draw();
        }
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        var props = new Dictionary<string, string>
        {
            { "Color.R", Color.X.ToString(CultureInfo.InvariantCulture) },
            { "Color.G", Color.Y.ToString(CultureInfo.InvariantCulture) },
            { "Color.B", Color.Z.ToString(CultureInfo.InvariantCulture) },
            { "Color.A", Color.W.ToString(CultureInfo.InvariantCulture) }
        };

        if (!string.IsNullOrEmpty(TexturePath))
        {
            props["TexturePath"] = TexturePath;
        }

        return props;
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        Color = new Vector4(float.Parse(properties["Color.R"], CultureInfo.InvariantCulture),
            float.Parse(properties["Color.G"], CultureInfo.InvariantCulture),
            float.Parse(properties["Color.B"], CultureInfo.InvariantCulture),
            float.Parse(properties["Color.A"], CultureInfo.InvariantCulture));

        if (properties.TryGetValue("TexturePath", out var path))
        {
            AssignTexture(path);
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
        {
            searchButtonClicked = true;
        }

        Vector2 buttonPos = ImGui.GetItemRectMin();
        
        if (searchButtonClicked)
        {
            ImGui.SetNextWindowPos(buttonPos, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(240, 300), ImGuiCond.Appearing);
            ImGui.Begin("Search", ref searchButtonClicked, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);
            ImGui.Columns(3, "Image Column", false);
            var imageFiles = ProjectSerialiser.FindAllImageFiles();
            foreach (var path in imageFiles)
            {
                ImGui.BeginGroup();
                if (!LoadIcons.icons.ContainsKey(path))
                {
                    LoadIcons.LoadImageIcons();
                }
                if (ImGui.ImageButton(path, (IntPtr)LoadIcons.imageIcons[path], new Vector2(60, 60)))
                {
                    AssignTexture(path);
                    searchButtonClicked = false;
                }
                float textWidth = ImGui.CalcTextSize(Path.GetFileNameWithoutExtension(name)).X;
                float currentIconWidth = ImGui.GetItemRectSize().X;
                float textPadding = (currentIconWidth - textWidth) * 0.5f;
                if (textPadding > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + textPadding);
                ImGui.Text(Path.GetFileNameWithoutExtension(path));
                ImGui.EndGroup();
                
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
            ImGui.End();
        }

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