using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

public class Camera : Component
{
    public static Camera? Main
    {
        get
        {
            if (Engine.CurrentState == EngineState.Editor)
            {
                return null;
            }

            return _main;
        }

        private set { _main = value; }
    }

    private static Camera? _main { get; set; }

    public float Zoom = 1.0f;

    public Camera() : base("Camera")
    {
        if (Main == null || Main.AttachedGameObject == null) Main = this;
    }

    public void MakeMainCamera()
    {
        Main = this;
    }

    public static void NullMainCamera()
    {
        Main = null;
    }

    public override void DestroyComponent()
    {
        if (Main == this) Main = null;
    }

    public Matrix4x4 GetViewMatrix()
    {
        var t = AttachedGameObject.GetComponent<Transform>();
        if (t == null) return Matrix4x4.Identity;

        Vector2 pos = t.WorldPosition;
        
        Matrix4x4 translation = Matrix4x4.CreateTranslation(-t.WorldPosition.X , -t.WorldPosition.Y, 0f);
        Matrix4x4 rotation = Matrix4x4.CreateRotationZ(-t.WorldRotation);

        return translation * rotation;
    }

    public override void RenderInspector()
    {
        ImGui.DragFloat("Zoom", ref Zoom, 1f, 0.001f, 1000f);
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        return new Dictionary<string, string>()
        {
            { "Zoom", Zoom.ToString(CultureInfo.InvariantCulture) }
        };
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties["Zoom"] != null)
        {
            Zoom = float.Parse(properties["Zoom"]);
        }
    }
}