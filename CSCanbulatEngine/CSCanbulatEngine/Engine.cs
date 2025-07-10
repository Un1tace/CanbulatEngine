using System.Runtime.InteropServices;

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

//Using if editor to check if they are running the editor or the game for when they build it

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

    private const float GameAspectRatio = 16f / 9f;
    private static IKeyboard? primaryKeyboard;

#if EDITOR
    //--- Editor Only Resources ---
    //This is the ImGUI controller, we get this from the Silk.Net Library
    private static ImGuiController imGuiController;

    // Buffers needed to show in the viewport
    private static uint Fbo;
    private static uint FboTexture;
    private static uint Rbo;
    //Size of the viewport
    private static Silk.NET.Maths.Vector2D<int> ViewportSize;
    
    // Font
    private static ImFontPtr _customFont;

    private static bool _layoutInitialised = false;
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

        window.FramebufferResize += OnFramebufferResize;

        window.Run();
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);

        // For inputs
        IInputContext input = window.CreateInput();
        Console.WriteLine("Created an input context");
        
        primaryKeyboard = input.Keyboards.FirstOrDefault();
        if (primaryKeyboard != null)
        {
            InputManager.Initialize(primaryKeyboard);
        }
        
        // ---- Initialising the IMGUI Controller----
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        // io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

#if EDITOR
        
        
        
        
        SetLook();

        string fontPath = Path.Combine(AppContext.BaseDirectory, "Assets/Fonts/Nunito-Regular.ttf");

        if (File.Exists(fontPath))
        {
            _customFont = io.Fonts.AddFontFromFileTTF(fontPath, 18f);
            Console.WriteLine("Custom Font queued for loading");
        }
        else
        {
            Console.WriteLine("Could not find font file: " + fontPath + ". Using default font");
        }
        
        imGuiController = new ImGuiController(gl, window, input);

        ViewportSize = window.FramebufferSize;
        CreateFrameBuffer();
        Console.WriteLine("Initialised IMGUI Controller and framebuffer");
#endif

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
        
        ImGuiWindowManager.InitialiseDefaults();
    }

    private void OnUpdate(double deltaTime)
    {
#if EDITOR
        //--------Keyboard shortcuts--------
        if (primaryKeyboard != null)
        {
            bool modifierDown;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                modifierDown = InputManager.IsKeyDown(Key.SuperLeft) || InputManager.IsKeyDown(Key.SuperRight);
            }
            else
            {
                modifierDown = InputManager.IsKeyDown(Key.ControlLeft) || InputManager.IsKeyDown(Key.ControlRight);
            }

            if (modifierDown)
            {
                if (InputManager.IsKeyPressed(Key.S))
                {
                    SaveScene();
                }
            }
        }
       
#endif
        // ! ! ! Keep at last update ! ! !
        InputManager.LateUpdate();
    }

    private unsafe void OnRender(double deltaTime)
    {
#if EDITOR
        //-------------------Editor-----------------
        imGuiController.Update((float)deltaTime);
        // Render game scene to off screen FBO
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        
        gl.Viewport(0, 0, (uint)ViewportSize.X, (uint)ViewportSize.Y);
        

        float viewportAspectRatio = (float)ViewportSize.X / ViewportSize.Y;

        float scaleX = 1f;
        float scaleY = 1f;

        if (viewportAspectRatio > GameAspectRatio)
        {
            scaleX = viewportAspectRatio / GameAspectRatio;
        }
        else
        {
            scaleY = GameAspectRatio / viewportAspectRatio;
        }
        
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(-1 * scaleX, 1 * scaleX, -1 * scaleY, 1 * scaleY, -1f, 1f);
        shader.Use();
        shader.SetUniform("projection", projection);
        
        DrawGameScene();
        
                
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        gl.Viewport(0, 0, (uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);

        ImGuiWindowFlags editorPanelFlags = ImGuiWindowFlags.None;
        editorPanelFlags |= ImGuiWindowFlags.NoMove;      // Uncomment to prevent moving
        editorPanelFlags |= ImGuiWindowFlags.NoResize;    // Uncomment to prevent resizing
        editorPanelFlags |= ImGuiWindowFlags.NoCollapse;  // Uncomment to prevent collapsing
        
        gl.ClearColor(Color.Black);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        

        // bool fontLoaded = _customFont.NativePtr != null;
        //
        // if (fontLoaded)
        // {
        //     ImGui.PushFont(_customFont);
        // }

        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                       ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.MenuBar;

        ImGui.Begin("DockSpaceWindow", windowFlags);
        
        
        ImGui.End();

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                string superKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "CMD" : "CTRL";
                if (ImGui.MenuItem("Save", superKey + "+S"))
                {
                    SaveScene();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Debug"))
            {
                bool dockingEnabled = ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable);
                ImGui.MenuItem("Docking Enabled", "", dockingEnabled);
                ImGui.EndMenu();
            }
            ImGuiWindowManager.menuBarHeight = ImGui.GetFrameHeight();
            ImGui.EndMainMenuBar();
        }

        // Render the editor UI
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[0]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[0]);
        ImGui.Begin("Game Viewport", editorPanelFlags);
        Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();

        var dpiScaleX = (float)window.FramebufferSize.X / window.Size.X;
        var dpiScaleY = (float)window.FramebufferSize.Y / window.Size.Y;
        
        var newPixelSize = new Vector2D<int>((int)(viewportPanelSize.X * dpiScaleX), (int)(viewportPanelSize.Y * dpiScaleY));

        if (ViewportSize != newPixelSize)
        {
            if (newPixelSize.X > 0 && newPixelSize.Y > 0)
            {
                ViewportSize = newPixelSize;
                ResizeFramebuffer();
            }
        }

        ImGui.Image((IntPtr)FboTexture, viewportPanelSize, new Vector2(0, 1),
            new Vector2(1, 0));
        ImGui.End();

        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[1]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[1]);
        ImGui.Begin("Inspector", editorPanelFlags);
        ImGui.Text("Object properties :)");
        ImGui.End();

        // if (fontLoaded)
        // {
        //     ImGui.PopFont();
        // }

        imGuiController.Render();

#else
        //-------------------Game-----------------
        gl.Viewport(0, 0, (uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);
        float viewportAspectRatio = (float)window.FramebufferSize.X / window.FramebufferSize.Y;

        float scaleX = 1f;
        float scaleY = 1f;

        if (viewportAspectRatio > GameAspectRatio)
        {
            scaleX = viewportAspectRatio / GameAspectRatio;
        }
        else
        {
            scaleY = GameAspectRatio / viewportAspectRatio;
        }
        
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(-1 * scaleX, 1 * scaleX, -1 * scaleY, 1 * scaleY, -1f, 1f);
        shader.Use();
        shader.SetUniform("projection", projection);
        // gl.ClearColor(Color.DarkSlateBlue);
        // gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawGameScene();
#endif
    }

    private void DrawGameScene()
    {
        gl.ClearColor(Color.CornflowerBlue);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
        ImGuiWindowManager.OnFrameBufferResize();
    }

    private static void SetLook()
    {
        var style = ImGui.GetStyle();
        style.WindowRounding = 5.3f;
        style.FrameRounding = 2.3f;
        style.ScrollbarRounding = 5f;
    }

    private static void SaveScene()
    {
        Console.WriteLine("Save Scene Initiated");
    }
}