namespace CSCanbulatEngine;

using Silk.NET.Maths;
using Silk.NET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;

public class Engine
{
    private static IWindow window;

    private static GL gl;

    //--- Core Resources ---
    // Variables for rendering
    private static uint Vbo;
    private static uint Vao;

    //Reference to Shader.cs class
    private static Shader shader;

#if EDITOR
    //--- Editor Only Resources ---
    //This is the ImGUI controller, we get this from the Silk.Net Library
    private static ImGuiController imGuiController;

    private static uint Fbo;
    private static uint FboTexture;
    private static uint Rbo;
    private static Silk.NET.Maths.Vector2D<int> ViewportSize = new Vector2D<int>(1280, 720);
#endif

    private static readonly float[] Vertices =
    {
        -0.5f, -0.5f, 0.0f,
        0.5f, -0.5f, 0.0f,
        0.0f, 0.5f, 0.0f
    };

    public void Run()
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
        options.Title = "Canbulat Engine";
        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Update += OnUpdate;
        window.Closing += OnClose;

        window.Run();
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);

        // For inputs
        IInputContext input = window.CreateInput();
        Console.WriteLine("Created an input context");

#if EDITOR
        // ---- Initialising the IMGUI Controller----
        ImGui.CreateContext();
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        imGuiController = new ImGuiController(gl, window, input);
        
        CreateFrameBuffer();
        Console.WriteLine("Initialised IMGUI Controller");
#endif

        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }

        // --- Setting up resources to render ---

        //Create and bind VAO and VBO
        Vao = gl.GenVertexArray();
        gl.BindVertexArray(Vao);
        Console.WriteLine("Created and binded VAO");

        Vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
        Console.WriteLine("Created and binded VBO");

        //Upload Vertices array to the VBO

        fixed (float* buf = Vertices)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), buf,
                BufferUsageARB.StaticDraw);
        }

        //Create instance of shader class and pass file paths
        shader = new Shader(gl,
            "Shaders/shader.vert",
            "Shaders/shader.frag");
        Console.WriteLine("Created base shader");

        // Tell OpenGL (Graphics API) how to read the data in the VBO
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
        gl.EnableVertexAttribArray(0);
    }

    private void OnUpdate(double deltaTime)
    {
        // Later Logic :)
    }

    private void OnRender(double deltaTime)
    {
#if EDITOR
        //-------------------Editor-----------------
        imGuiController.Update((float)deltaTime);

        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                       ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        ImGui.Begin("DockSpaceWindow", windowFlags);
        ImGui.PopStyleVar(2);

        ImGui.DockSpace(ImGui.GetID("MyDockSpace"));

        ImGui.End();

        // Render game scene to off screen FBO
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        gl.Viewport(0, 0, (uint)ViewportSize.X, (uint)ViewportSize.Y);
        gl.ClearColor(Color.CornflowerBlue);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawGameScene();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Render the editor UI
        ImGui.Begin("Game Viewport");
        Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();

        if (ViewportSize.X != (int)viewportPanelSize.X || ViewportSize.Y != (int)viewportPanelSize.Y)
        {
            if (viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
            {
                ViewportSize = new Vector2D<int>((int)viewportPanelSize.X, (int)viewportPanelSize.Y);
                ResizeFramebuffer();
            }
        }

        ImGui.Image((IntPtr)FboTexture, new Vector2(ViewportSize.X, ViewportSize.Y), new Vector2(0, 1),
            new Vector2(1, 0));
        ImGui.End();

        ImGui.Begin("Inspector");
        ImGui.Text("Object properties :)");
        ImGui.End();

        imGuiController.Render();

#else
        //-------------------Game-----------------
        gl.Viewport(0, 0, (uint)window.Size.X, (uint)window.Size.Y);
        gl.ClearColor(Color.DarkSlateBlue);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawGameScene();
#endif
    }

    private void DrawGameScene()
    {
        gl.BindVertexArray(Vao);
        shader.Use();
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    private void OnClose()
    {
#if EDITOR
        imGuiController.Dispose();
        gl.DeleteFramebuffer(Fbo);
        gl.DeleteTexture(FboTexture);
        gl.DeleteRenderbuffer(Rbo);
#endif
        gl.DeleteBuffer(Vbo);
        gl.DeleteVertexArray(Vao);
        shader.Dispose();
        gl.Dispose();

        //Clean up for all the stuff we use
    }

#if EDITOR
    //Creating the frame buffer for the viewport in the editor
    private static unsafe void CreateFrameBuffer()
    {
        Fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

        FboTexture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, FboTexture);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)ViewportSize.X, (uint)ViewportSize.Y, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, FboTexture, 0);

        Rbo = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Rbo);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (uint)ViewportSize.X,
            (uint)ViewportSize.Y);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, Rbo);

        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("The Framebuffer is not complete!");
        }

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private static void ResizeFramebuffer()
    {
        gl.DeleteFramebuffer(Fbo);
        gl.DeleteTexture(FboTexture);
        gl.DeleteRenderbuffer(Rbo);
        CreateFrameBuffer();
    }
#endif

    private void OnFramebufferResize(Vector2D<int> size)
    {
        gl.Viewport(0, 0, (uint) size.X, (uint)size.Y);
    }
    
    // Gets the key hit on the keyboard and exits if its escape
    private void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            window.Close();
        }
    }
}