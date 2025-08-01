using System.Runtime.InteropServices;
using System.Text;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.FileHandling.ProjectManager;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.UIHelperScripts;
using CSCanbulatEngine.Utilities;

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
    public static Project? currentProject;
    public static Scene currentScene;
    
    
    private static IWindow window;

    public static GL gl;

    public static Mesh _squareMesh;

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

    private static uint _whiteTexture;

#if EDITOR
    //--- Editor Only Resources ---
    public static StoreObject _selectedGameObject;
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

    private bool showInfoWindow = false;
    
    //Texture IDs for the InfoWindow
    private static uint _logoTextureID;
    private static Vector2D<int> _logoSize;

    // For Popups
    public static Byte[] _nameBuffer = new Byte[128];
    
    public static bool renamePopupOpen = false;
    public static bool nameScenePopupOpen = false;
    public static bool nameSceneAsPopup = false;
    public static bool createProjectPopup = false;
    public static bool projectFoundPopup = false;
    public static bool renameFilePopupOpen = false;
    
    //Project Manager
    public static string projectFilePath = "";
    
    //Gizmo
    private Gizmo _gizmo;

    private RectangleF _projectManagerBounds;
    private string[]? _pendingDroppedFiles = null;
#endif

    public void Run()
    {
        currentScene = new Scene("ExampleScene");
        currentProject = new Project("", "");
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
        window.FileDrop += OnFileDrop;
        
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

        string fontPath = Path.Combine(AppContext.BaseDirectory, "EditorAssets/Fonts/Nunito-Regular.ttf");

        if (File.Exists(fontPath))
        {
            _customFont = io.Fonts.AddFontFromFileTTF(fontPath, 18f);
            Console.WriteLine("Custom Font queued for loading");

            io.Fonts.Build();
            Console.WriteLine("ImGui font atlas built");
        }
        else
        {
            Console.WriteLine("Could not find font file: " + fontPath + ". Using default font");
        }
        
        // 1. Get the font texture data from ImGui
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

        // 2. Create our own OpenGL texture
        uint fontTextureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, fontTextureId);
    
        // 3. Upload the pixel data to the GPU
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);

        // 4. Set texture parameters
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        // --- THIS IS THE FIX ---
        // Explicitly tell OpenGL that our texture has no mipmaps.
        // This is a common requirement on macOS to prevent the "unloadable" texture error.
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);


        // 5. Tell ImGui to use our manually created texture.
        // We cast the uint texture ID to an IntPtr, which is what ImGui expects.
        io.Fonts.SetTexID((IntPtr)fontTextureId);

        // 6. Clear the CPU-side texture data now that it's on the GPU.
        io.Fonts.ClearTexData();
    
        Console.WriteLine("Font texture manually created and uploaded to GPU.");
        
        imGuiController = new ImGuiController(gl, window, input);

        ViewportSize = window.FramebufferSize;
        CreateFrameBuffer();
        Console.WriteLine("Initialised IMGUI Controller and framebuffer");

        try
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "EditorAssets/Images/Logo.png");
            _logoTextureID = TextureLoader.Load(gl, logoPath, out _logoSize);
            Console.WriteLine("Texture Loaded");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load logo texture: {e.Message}");
        }

        LoadIcons.PreloadIcons();
        LoadIcons.LoadImageIcons();
        _gizmo = new Gizmo();
#endif

        shader = new Shader(gl, "Shaders/shader.vert", "Shaders/shader.frag");
        
        //Format: X, Y, Z, U, V
        float[] squareVertices =
        {
            // Position         //UV Coords
            0.5f, 0.5f, 0f,     1.0f, 0.0f, //Top Right
            0.5f, -0.5f, 0f,    1.0f, 1.0f, //Bottom Right
            - 0.5f, -0.5f, 0f,  0.0f, 1.0f, //Bottom Left
            - 0.5f, 0.5f, 0f,    0.0f, 0.0f //Top Left
        };
        uint[] squareIndices = { 0, 1, 2, 2, 3, 0 };
        
        //Create our example mesh resource
        _squareMesh = new Mesh(gl, squareVertices, squareIndices);
        
        // If no texture assigned use 1x1 white texture
        _whiteTexture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _whiteTexture);
        byte[] whitePixel = { 255, 255, 255, 255 };
        fixed (byte* p = whitePixel)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }
        gl.BindTexture(TextureTarget.Texture2D, 0);
        
        var gameObject1 = new GameObject(_squareMesh);
        gameObject1.GetComponent<Transform>().Position = new Vector2(-0.75f, 0f);
        var renderer1 = gameObject1.GetComponent<MeshRenderer>();
        if (renderer1 != null) renderer1.Color = new Vector4(1, 0, 0, 1); // <- Red

        var addChip = new Chip(1, "Add", new Vector2(50, 50));
        addChip.AddPort("A", true, [typeof(float), typeof(int)], new Vector4(0.5f, 0f, 0f, 1f));
        addChip.AddPort("B", true, [typeof(float), typeof(int)], new Vector4(0.5f, 0f, 0f, 1f));
        addChip.AddPort("Result", false, [typeof(float), typeof(int)], new Vector4(0.5f, 0f, 0f, 1f));
        CircuitEditor.chips.Add(addChip);

        var constChip = new Chip(2, "Constant", new Vector2(300, 150));
        constChip.AddPort("Value", false , [typeof(float), typeof(int)], new Vector4(0f, 0.5f, 0f, 1f));
        CircuitEditor.chips.Add(constChip);

        var constantChip = CircuitEditor.FindChip("Constant");
        var addChip2 = CircuitEditor.FindChip("Add");
        
        addChip2.InputPorts[0].ConnectPort(constantChip.OutputPorts[0]);

    }
    

    private void OnUpdate(double deltaTime)
    {
#if EDITOR
        //--------Keyboard shortcuts--------
        if (primaryKeyboard != null && !ImGui.GetIO().WantCaptureKeyboard)
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
                else if (InputManager.IsKeyPressed(Key.S) &&
                         (InputManager.IsKeyDown(Key.ShiftLeft) || InputManager.IsKeyDown(Key.ShiftRight)))
                {
                    SaveSceneAs();
                }
                else if (InputManager.IsKeyPressed(Key.O))
                {
                    LoadProject();
                }
                else if (InputManager.IsKeyPressed(Key.Backspace))
                {
                    _selectedGameObject?.gameObject.DeleteObject();
                }
                else if (InputManager.IsKeyPressed(Key.A) && !renamePopupOpen)
                {
                    new GameObject(_squareMesh);
                }
                else if (InputManager.IsKeyPressed(Key.Number2) && _selectedGameObject != null)
                {
                    renamePopupOpen = true;
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
        
        ImGui.PushFont(_customFont);
        // Render game scene to off screen FBO
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        
        gl.Viewport(0, 0, (uint)ViewportSize.X, (uint)ViewportSize.Y);
        
        DrawGameScene(ViewportSize, _cameraZoom);
        
                
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        gl.Viewport(0, 0, (uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);
        
        gl.ClearColor(Color.Black);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        RenderEditorUI();
        
        // For Menu Bar > {Object.name} > Rename Object 
        if (ImGui.BeginPopupModal("Rename Object", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter a new name for the object.");
            ImGui.InputText("##NameInput", Engine._nameBuffer, (uint)Engine._nameBuffer.Length);

            if (ImGui.Button("OK"))
            {
                string newName = Encoding.UTF8.GetString(Engine._nameBuffer).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(newName) && Engine._selectedGameObject != null)
                {
                    _selectedGameObject.gameObject.Name = newName;
                }

                renamePopupOpen = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                renamePopupOpen = false;
                ImGui.CloseCurrentPopup();
                
            }
            ImGui.EndPopup();
        }
        
        // For Menu Bar > File > Name Scene
        if (ImGui.BeginPopupModal("Name Scene", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter a name for the scene");
            ImGui.InputText("##NameInput", Engine._nameBuffer, (uint)Engine._nameBuffer.Length);

            if (ImGui.Button("OK"))
            {
                string newName = Encoding.UTF8.GetString(Engine._nameBuffer).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    currentScene.SceneName = newName;
                }

                nameScenePopupOpen = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                nameScenePopupOpen = false;
                ImGui.CloseCurrentPopup();
                
            }
            ImGui.EndPopup();
        }
        
        //Before using Save as
        if (ImGui.BeginPopupModal("Name Scene as", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter a name for the scene");
            ImGui.InputText("##NameInput", Engine._nameBuffer, (uint)Engine._nameBuffer.Length);

            if (ImGui.Button("OK"))
            {
                string newName = Encoding.UTF8.GetString(Engine._nameBuffer).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    currentScene.SceneName = newName;
                    SaveSceneAsContinued();
                }

                nameSceneAsPopup = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                nameSceneAsPopup = false;
                ImGui.CloseCurrentPopup();
                
            }
            ImGui.EndPopup();
        }
        
        //Renaming a file in project manager
        if (ImGui.BeginPopupModal("Rename File", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter a new name for the file.");
            ImGui.InputText("##NameInput", Engine._nameBuffer, (uint)Engine._nameBuffer.Length);

            if (ImGui.Button("OK"))
            {
                string newName = Encoding.UTF8.GetString(Engine._nameBuffer).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(newName) && Engine._selectedGameObject != null)
                {
                    ProjectManager.RenameFileContinued(newName);
                }

                renameFilePopupOpen = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                renameFilePopupOpen = false;
                ImGui.CloseCurrentPopup();
                
            }
            ImGui.EndPopup();
        }
        
        ProjectSerialiser.CreateProjectPopUp();
        
        ProjectSerialiser.ProjectAlreadyHerePopup();
        
        ImGui.PopFont();

        imGuiController.Render();
        
        if (currentProject.ProjectName == "")
        {
            currentProject.ProjectName = " ";
            ProjectSerialiser.CreateOrLoadProjectFile();
        }

        
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
            string superKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "CMD" : "CTRL";
            if (ImGui.BeginMenu($"Project - {currentProject.ProjectName}"))
            {
                if (ImGui.MenuItem("Create Project"))
                {
                    CreateProject();
                }

                if (ImGui.MenuItem("Open Project"))
                {
                    ProjectSerialiser.LoadProject();
                }
                if (ImGui.MenuItem("Load Scene"))
                {
                    LoadScene();
                }

                if (ImGui.MenuItem("Create Scene"))
                {
                    CreateScene();
                }
                if (ImGui.MenuItem("Name Scene"))
                {
                    Array.Clear(Engine._nameBuffer, 0, Engine._nameBuffer.Length);
                    if (currentScene != null)
                    {
                        byte[] currentNameBytes = Encoding.UTF8.GetBytes(currentScene.SceneName);
                        Array.Copy(currentNameBytes, Engine._nameBuffer, currentNameBytes.Length);
                        nameScenePopupOpen = true;
                    }
                    // ImGui.OpenPopup("Rename Object");
                    
                }
                if (ImGui.MenuItem("Save Scene", superKey + "+S"))
                {
                    SaveScene();
                }
                if (ImGui.MenuItem("Save Scene As", superKey + "+Shift+S"))
                {
                    SaveSceneAs();
                }
                ImGui.EndMenu();
            }
            
            if (_selectedGameObject != null)
            {
                _selectedGameObject.gameObject.RenderObjectOptionBar(superKey);
            }

            RenderObjectMenu(superKey);

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
        ImGui.Begin("Game Viewport", editorPanelFlags | ImGuiWindowFlags.NoTitleBar);
        if (ImGui.BeginTabBar("MainWindowTabs"))
        {
            if (ImGui.BeginTabItem("Viewport"))
            {
                Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                // Vector2 viewportPos = ImGui.GetWindowPos();
                Vector2 viewportPos = ImGui.GetCursorScreenPos();

                var dpiScaleX = (float)window.FramebufferSize.X / window.Size.X;
                var dpiScaleY = (float)window.FramebufferSize.Y / window.Size.Y;

                var newPixelSize = new Vector2D<int>((int)(viewportPanelSize.X * dpiScaleX),
                    (int)(viewportPanelSize.Y * dpiScaleY));

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

                if (_selectedGameObject != null)
                {
                    var viewMatrix = Matrix4x4.Identity;

                    float viewportAspectRatio =
                        viewportPanelSize.X > 0 ? viewportPanelSize.X / viewportPanelSize.Y : 1.0f;
                    float orthoWidth = 2f * (viewportAspectRatio > 1.0f ? viewportAspectRatio : 1.0f) * _cameraZoom;
                    float orthoHeight = 2f * (viewportAspectRatio < 1.0f ? 1.0f / viewportAspectRatio : 1.0f) *
                                        _cameraZoom;
                    var projMatrix = Matrix4x4.CreateOrthographic(orthoWidth, orthoHeight, -1f, 100f);

                    _gizmo.UpdateAndRender(_selectedGameObject.gameObject, viewMatrix, projMatrix, viewportPos, viewportPanelSize);
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Circuit Editor"))
            {
                CircuitEditor.Render(); 
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        
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
        
        ImGui.Separator();
        
        //Puts all the properties in the inspector visible and rendered
        if (_selectedGameObject != null)
        {
            ImGui.Text($"Editing {_selectedGameObject.gameObject.Name}");
            ImGui.Separator();

            foreach (Component component in _selectedGameObject.gameObject.components)
            {
                if (ImGui.CollapsingHeader(component.name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    component.RenderInspector();
                }
            }
        }
        ImGui.End();
        
        // -- Hierarchy --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[2]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[2]);
        ImGui.Begin($"Hierarchy - {currentScene.SceneName}", editorPanelFlags);
        foreach(var gameObject in currentScene.GameObjects)
        {
            
            bool isSelected = (_selectedGameObject.gameObject == gameObject);
            if (ImGui.Selectable(gameObject.Name, isSelected))
            {
                _selectedGameObject = new (gameObject);
            }
        }
        ImGui.End();
        
        var toolbarFlags = editorPanelFlags;
        toolbarFlags |= ImGuiWindowFlags.NoTitleBar;
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[4]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[4]);
        ImGui.Begin("Toolbar", toolbarFlags);
        Gizmo.RenderToolbar();
        ImGui.End();

        if (renamePopupOpen)
        {
            ImGui.OpenPopup("Rename Object");
        }
        
        if (nameScenePopupOpen)
        {
            ImGui.OpenPopup("Name Scene");
        }
        
        if (nameSceneAsPopup)
        {
            ImGui.OpenPopup("Name Scene as");
        }

        if (createProjectPopup)
        {
            ImGui.OpenPopup("Name Project");
        }

        if (projectFoundPopup)
        {
            ImGui.OpenPopup("Project Found");
        }

        if (renameFilePopupOpen)
        {
            ImGui.OpenPopup("Rename File");
        }
        
        // -- Project File Manager --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[3]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[3]);
        ImGui.Begin("Project File Manager", editorPanelFlags);
        float leftPanelWidth = ImGui.GetContentRegionAvail().X * 0.2f;
        ImGui.BeginChild("Directories", new Vector2(leftPanelWidth, ImGui.GetContentRegionAvail().Y), ImGuiChildFlags.AutoResizeY);
        if (ImGui.Selectable("Assets"))
        {
            ProjectManager.selectedDir = ProjectSerialiser.GetAssetsFolder();
            Console.WriteLine(ProjectManager.selectedDir);
        }

        _projectManagerBounds = new RectangleF(new(ImGui.GetWindowPos()), new (ImGui.GetWindowSize()));

        if (_pendingDroppedFiles != null)
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows))
            {
                Console.WriteLine("Files dropped onto Project Manager!");

                foreach (var path in _pendingDroppedFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(path);
                        string destPath = Path.Combine(ProjectManager.selectedDir, fileName);
                        File.Copy(path, destPath, true);
                        Console.WriteLine($"Imported '{fileName}' to assets.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to import file '{path}': {e.Message}");
                    }
                }
            }
            _pendingDroppedFiles = null;
        }
        
        ProjectManager.RenderDirectories();
        ImGui.EndChild();
        
        ImGui.SameLine();
        
        //File Icons
        ImGui.BeginChild("File Icons", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y), ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AutoResizeX);

        string name = ProjectManager.selectedDir;
        
        string[] sepDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? name.Split('\\')
            : name.Split("/");

        int assetsFolderIndex = -1;

        for (int i = 0; i < sepDirectories.Length; i++)
        {
            if (sepDirectories[i] == "Assets") assetsFolderIndex = i;
        }

        string newName = "Assets";

        if (assetsFolderIndex != -1)
        {
            for (int i = assetsFolderIndex + 1; i < sepDirectories.Length; i++)
            {
                newName += (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "\\" : "/") + sepDirectories[i];
            }
        }
        else newName = name;
        
        Array.Clear(sepDirectories,0 , sepDirectories.Length);
        
        ImGui.Text(newName);
        ImGui.SameLine();

        float sliderWidth = 150f;
        
        float newCursorPosX = ImGui.GetCursorPos().X + ImGui.GetContentRegionAvail().X - sliderWidth;
        
        ImGui.SetCursorPosX(newCursorPosX);
        
        ImGui.PushItemWidth(sliderWidth);
        
        if (ImGui.SliderFloat("Zoom", ref ProjectManager.SliderZoom, 0.5f, ProjectManager.maxZoom))  {}
        ImGui.PopItemWidth();
        ImGui.Separator();
        ProjectManager.RenderProjectManagerIcons();
        ImGui.EndChild();
        ImGui.End();

        // if (fontLoaded)
        // {
        //     ImGui.PopFont();
        // }
        
        SetLook();
    }

    private void OnFileDrop(string[] paths)
    {
        _pendingDroppedFiles = paths;
        Console.WriteLine("File drop detected by OS, pending processing");
    }
    #endif

    #if EDITOR
    private void RenderObjectMenu(string superKey)
    {
        if (ImGui.BeginMenu("Object"))
        {
            if (ImGui.MenuItem("Create GameObject", superKey + "+A"))
            {
                new GameObject(_squareMesh);
                
            }
            ImGui.EndMenu();
        }
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
        shader.SetUniform("projection", projection);
        
        shader.SetUniform("uTexture", 0);
        gl.ActiveTexture(TextureUnit.Texture0);

        foreach (var gameObject in currentScene.GameObjects)
        {
            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer == null) continue;
            
            //Set color in shader :)
            shader.SetUniform("uColor", renderer.Color);

            uint textureToBind = renderer.TextureID != 0 ? renderer.TextureID : _whiteTexture;
            gl.BindTexture(TextureTarget.Texture2D, textureToBind);
            
            //Get the matrix from the transform
            Matrix4x4 modelMatrix = gameObject.GetComponent<Transform>().GetModelMatrix();
            //Set model uniform in the shader for the object
            shader.SetUniform("model", modelMatrix);

            renderer.Mesh.Draw();
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
        gl.DeleteTexture(_whiteTexture);
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

    private static void SaveSceneAs()
    {
        nameSceneAsPopup = true;
    }
    
    private static void SaveSceneAsContinued()
    {
            if (String.IsNullOrWhiteSpace(currentScene.SceneName)) currentScene.SceneName = "ExampleScene";
            SceneSerialiser ss = new SceneSerialiser(gl, _squareMesh);
            ss.SaveScene(currentScene.SceneName);
    }

    private static void SaveScene()
    {
        if (!String.IsNullOrWhiteSpace(currentScene.SceneFilePath) && !String.IsNullOrWhiteSpace(currentScene.SceneName) && currentScene.SceneSavedOnce)
        {
            SceneSerialiser ss = new SceneSerialiser(gl, _squareMesh);
            ss.SaveScene(currentScene.SceneName);
        }
        else
        {
            SaveSceneAs();
        }
    }

    private static void LoadProject()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Console.WriteLine($"Folder Selected: {FileDialogHelper.ShowSelectFolderDialog(documentsPath, "Select The Folder To Load Project")}");
    }

    private static void LoadScene()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string? projectPath = FileDialogHelper.ShowOpenFileDialog(documentsPath, new [] {"*.cbs"});
        Console.WriteLine($"Folder Selected: {projectPath}");
        if (!String.IsNullOrWhiteSpace(projectPath))
        {
            SceneSerialiser ss = new SceneSerialiser(gl, _squareMesh);
            ss.LoadScene(projectPath);
        }
    }

    private static void CreateProject()
    {
        ProjectSerialiser.CreateProjectFiles();
    }

    private static void CreateScene()
    {
        currentScene = new Scene("New Scene");
        _selectedGameObject = null;
    }
#endif
}

public class StoreObject
{
    public GameObject gameObject { get; set; }
    public int timeCreated { get; private set; }
    
    public StoreObject(GameObject gameObject)
    {
        this.gameObject = gameObject;
        timeCreated = DateTime.UtcNow.Second;
    }
}