using System.Runtime.InteropServices;
using CSCanbulatEngine.GameObjectScripts;

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

    private static List<GameObject> _gameObjects;
    private static Mesh _squareMesh;

    //--- Core Resources ---
    // Variables for rendering
    private static uint Vbo;
    private static uint Vao;
    
    //Element buffer object, allows indexed drawings
    private static uint Ebo;

    //Reference to Shader.cs class
    private static Shader shader;

    private const float GameAspectRatio = 16f / 9f;
    private static IKeyboard? primaryKeyboard;

    private static float _cameraZoom = 2f;

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
    
    //Thread for the info window
    private static Thread _infoWindowThread;

    private bool showInfoWindow = false;
    
    //Texture IDs for the InfoWindow
    private static uint _logoTextureID;
    private static Vector2D<int> _logoSize;
#endif

    public void Run()
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
        #if EDITOR
        string title = "Canbulat Engine";
        #else
        string title = "Game";
        #endif
        options.Title = title;
        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Update += OnUpdate;
        window.Closing += OnClose;
        
        #if EDITOR
        window.FramebufferResize += OnFramebufferResize;
        #endif

        window.Run();
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);
        
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

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

        try
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "Assets/Images/Logo.png");
            _logoTextureID = TextureLoader.Load(gl, logoPath, out _logoSize);
            Console.WriteLine("Texture Loaded");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load logo texture: {e.Message}");
        }
#endif

        shader = new Shader(gl, "Shaders/shader.vert", "Shaders/shader.frag");
        
        _gameObjects = new List<GameObject>();

        float[] squareVertices =
        {
            0.5f, 0.5f, 0f,
            0.5f, -0.5f, 0f,
            - 0.5f, -0.5f, 0f, 
            - 0.5f, 0.5f, 0f
        };
        uint[] squareIndices = { 0, 1, 2, 2, 3, 0 };
        
        //Create our example mesh resource
        _squareMesh = new Mesh(gl, squareVertices, squareIndices);
        
        var gameObject1 = new GameObject(_squareMesh);
        gameObject1.Transform.Position = new Vector2(-0.75f, 0f);
        _gameObjects.Add(gameObject1);
        
        //Second game object
        var gameObject2 = new GameObject(_squareMesh);
        gameObject2.Transform.Position = new Vector2(0.75f, 0);
        gameObject2.Transform.Scale = new(0.5f, 0.5f);
        _gameObjects.Add(gameObject2);
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
        
        DrawGameScene(ViewportSize, _cameraZoom);
        
                
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        gl.Viewport(0, 0, (uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);
        
        gl.ClearColor(Color.Black);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        RenderEditorUI();

        imGuiController.Render();

#else
        //-------------------Game-----------------
        gl.Viewport(0, 0, (uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);
        DrawGameScene(window.FramebufferSize, _cameraZoom);
#endif
    }
#if EDITOR
    private void RenderEditorUI()
    {
        ImGuiWindowFlags editorPanelFlags = ImGuiWindowFlags.None;
        editorPanelFlags |= ImGuiWindowFlags.NoMove;      // Uncomment to prevent moving
        editorPanelFlags |= ImGuiWindowFlags.NoResize;    // Uncomment to prevent resizing
        editorPanelFlags |= ImGuiWindowFlags.NoCollapse;  // Uncomment to prevent collapsing
                // bool fontLoaded = _customFont.NativePtr != null;
        //
        // if (fontLoaded)
        // {
        //     ImGui.PushFont(_customFont);
        // }
        //--Dock Space Window--
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                       ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.MenuBar;

        ImGui.Begin("DockSpaceWindow", windowFlags);
        
        
        ImGui.End();
        
        // -- Menu Bar --

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

            if (ImGui.MenuItem("Info"))
            {
                showInfoWindow = !showInfoWindow;
            }
            ImGuiWindowManager.menuBarHeight = ImGui.GetFrameHeight();
            ImGuiWindowManager.InitialiseDefaults();
            ImGui.EndMainMenuBar();
        }
        
        // -- Info Window --
        if (showInfoWindow)
        {
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.Appearing);
            
            ImGui.Begin("Info", ref showInfoWindow, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

            if (_logoTextureID != 0)
            {
                //Centre the image horizontally
                float contentWidth = 400f;
                float logoAspectRatio = (float)_logoSize.X/_logoSize.Y;
                float desiredWidth = contentWidth*0.6f;
                float scaledHeight = desiredWidth/logoAspectRatio;
                
                Vector2 imageDisplaySize = new Vector2(desiredWidth, scaledHeight);
                
                float horizontalPadding = (contentWidth - desiredWidth)/2;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + horizontalPadding);
                
                ImGui.Image((IntPtr)_logoTextureID, imageDisplaySize);
                ImGui.Separator();
            }
        
            ImGui.Text("Application Info");
            ImGui.Separator();
            ImGui.Text($"Vendor: {gl.GetStringS(StringName.Vendor)}");
            ImGui.Text($"Renderer: {gl.GetStringS(StringName.Renderer)}");
            ImGui.Text($"Version: {gl.GetStringS(StringName.Version)}");
            ImGui.Text($"FPS: {ImGui.GetIO().Framerate:F1}");

            ImGui.End();
        }

        // Render the editor UI
        // -- Viewport --
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
        
        // -- Inspector --
        ImGuiWindowFlags inspectorWindowPanelFlags = ImGuiWindowFlags.None;
        inspectorWindowPanelFlags |= ImGuiWindowFlags.NoMove;      // Uncomment to prevent moving
        inspectorWindowPanelFlags |= ImGuiWindowFlags.NoResize;    // Uncomment to prevent resizing
        inspectorWindowPanelFlags |= ImGuiWindowFlags.NoCollapse;
        inspectorWindowPanelFlags |= ImGuiWindowFlags.NoTitleBar;
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[1]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[1]);
        ImGui.Begin("Inspector", inspectorWindowPanelFlags);
        
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.36f, 0.55f, 0.85f, 0.8f));

        if (ImGui.Button("Inspector"))
        {

        }

        ImGui.PopStyleColor(1);

        ImGui.SameLine();
        if (ImGui.Button("Scene Properties"))
        {
            
        }
        
        ImGui.Separator();
        
        ImGui.Text("Object properties :)");

        if (ImGui.Button("Test"))
        {
            
        }
        ImGui.End();
        
        // -- Hierarchy --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[2]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[2]);
        ImGui.Begin("Hierarchy", editorPanelFlags);
        ImGui.End();
        
        // -- Project File Manager --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[3]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[3]);
        ImGui.Begin("Project File Manager", editorPanelFlags);
        ImGui.End();

        // if (fontLoaded)
        // {
        //     ImGui.PopFont();
        // }
        
        SetLook();
    }
    #endif

    private unsafe void DrawGameScene(Vector2D<int> currentViewportSize, float cameraZoom)
    {
        gl.ClearColor(Color.CornflowerBlue);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();
        
        float viewportAspectRatio = (float)currentViewportSize.X / currentViewportSize.Y;

        float orthoWidth, orthoHeight;
        if (viewportAspectRatio > 1.0f)
        {
            orthoWidth = 2f * viewportAspectRatio * cameraZoom;
            orthoHeight = 2f * cameraZoom;
        }
        else
        {
            orthoWidth = 2f * cameraZoom;
            orthoHeight = 2f / viewportAspectRatio * cameraZoom;
        }
        Matrix4x4 projection = Matrix4x4.CreateOrthographic(orthoWidth, orthoHeight, -1f, 1f);
        shader.Use();
        shader.SetUniform("projection", projection);

        foreach (var gameObject in _gameObjects)
        {
            //Get the matrix from the transform
            Matrix4x4 modelMatrix = gameObject.Transform.GetModelMatrix();
            //Set model uniform in the shader for the object
            shader.SetUniform("model", modelMatrix);

            gameObject.Mesh.Draw();
        }
    }

    private void OnClose()
    {
#if EDITOR
        imGuiController.Dispose();
        gl.DeleteFramebuffer(Fbo);
        gl.DeleteTexture(FboTexture);
        gl.DeleteRenderbuffer(Rbo);
        gl.DeleteTexture(_logoTextureID);
#endif
        _squareMesh.Dispose();
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

        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.15f, 0.4f, 0.75f, 0.8f);
    }

    private static void SaveScene()
    {
        Console.WriteLine("Save Scene Initiated");
    }
#endif
}