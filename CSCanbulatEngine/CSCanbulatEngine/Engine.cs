using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using CSCanbulatEngine.Audio;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.FileHandling.CircuitHandling;
using CSCanbulatEngine.FileHandling.Game;
using CSCanbulatEngine.FileHandling.ProjectManager;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.Mesh;
using CSCanbulatEngine.Physics;
using CSCanbulatEngine.UIHelperScripts;
using CSCanbulatEngine.Utilities;
using Newtonsoft.Json;

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

/// <summary>
/// The Game Engine
/// </summary>
public class Engine
{
    int minWidth = 1024;
    int minHeight = 720;
    
    public static Project? currentProject;
    public static Scene currentScene;
    
    
    private static IWindow window;

    public static GL gl;

    public static GameObjectScripts.Mesh _squareMesh;

    //--- Core Resources ---
    // Variables for rendering
    private static uint Vbo;
    private static uint Vao;
    
    //Element buffer object, allows indexed drawings
    private static uint Ebo;

    //Reference to Shader.cs class
    public static Shader shader;

    private const float GameAspectRatio = 16f / 9f;
    private static IKeyboard? primaryKeyboard;
    private static IMouse? primaryMouse;

    private static float _cameraZoom = 2f;

    public static uint _whiteTexture;
    
    //Engine state config
    private static EngineState _currentState = EngineState.Editor;
    public static EngineState CurrentState { get {return _currentState; }
        set
        {
            EngineState oldValue = _currentState;
            _currentState = value;
            if (oldValue == EngineState.Pause && value == EngineState.Play) ExecuteOnResume();
            else if (value == EngineState.Play) ExecuteOnPlay();
            else if (value == EngineState.Pause) ExecuteOnPause();
            else if (value == EngineState.Editor) ExecuteOnStop();
        }

    }
    private static string _sceneSnapshotBeforePlay;
    
    //Audio Engine
    public static AudioEngine Audio;

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
    public static ImFontPtr _extraThickFont;

    private bool showInfoWindow = false;
    
    //Texture IDs for the InfoWindow
    private static uint _logoTextureID;
    private static Vector2D<int> _logoSize;

    // For Popups
    public static Byte[] _nameBuffer = new Byte[128];
    
    public static bool renamePopupOpen = false;
    private static bool nameScenePopupOpen = false;
    private static bool nameSceneAsPopup = false;
    public static bool createProjectPopup = false;
    public static bool projectFoundPopup = false;
    public static bool renameFilePopupOpen = false;

    //Ciruit editor popups
    private static bool renameCircuitFileAsPopup = false;
    
    // Circuit Editor Port Config
    public static bool portConfigWindowOpen = false;
    public static Vector2? portConfigWindowPosition = null;
    public static Vector2? portConfigWindowSize = null;
    
    private static bool openSpawnMenuNextFrame = false;
    
    //Project Manager
    public static string projectFilePath = "";
    
    
    
    //Hierarchy stuff
    private bool HierarchyNeedsRefresh = false;
    
    //Gizmo
    private Gizmo _gizmo;

    private RectangleF _projectManagerBounds;
    private string[]? _pendingDroppedFiles = null;
    
    //Console
    public static bool _forceSetConsoleTab = false;

    //Console/ProjectManager stuff
    private bool circuitEditorIsOpen = false;
    private bool _consoleTabActive = true;
    private bool _projectTabActive = true;
    
    //Viewport
    public static bool _isViewportFocused { get; private set; } = false;
    private static string gameObjectClipBoard = "";
    
    //Build menu
    public static bool buildMenuOpen = false;
    
#endif

    /// <summary>
    /// Initialisation of CanbulatEngine
    /// </summary>
    public void Run()
    {
        currentScene = new Scene("ExampleScene");
        currentProject = new Project("", "");
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
        #if EDITOR
        string title = "Canbulat Engine";
        #else
        GameConfig? gameConfig = BuildManager.GetGameConfig();
        string title = "";
        if (gameConfig != null) title = gameConfig.GameName;
        else title = "Game";
        
        #endif
        options.Title = title;
        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Update += OnUpdate;
        window.Closing += OnClose;
        
        #if EDITOR
        window.FileDrop += OnFileDrop;
        window.FramebufferResize += OnFramebufferResize;

        LoadWindowSettings();
        #endif
        
        window.Run();
    }

    /// <summary>
    /// Ran when ImGui, Silk.Net and OpenGL loads
    /// </summary>
    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);
        
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // For inputs
        IInputContext input = window.CreateInput();
        EngineLog.Log("[Engine] Created an input context");
        
        primaryKeyboard = input.Keyboards.FirstOrDefault();
        if (primaryKeyboard != null)
        {
            InputManager.InitializeKeyboard(primaryKeyboard);
        }

        primaryMouse = input.Mice.FirstOrDefault();
        if (primaryMouse != null)
        {
            InputManager.InitializeMouse(primaryMouse);
        }
        
        // ---- Initialising the IMGUI Controller----
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        // io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        try
        {
            Audio = new AudioEngine();
            
            string soundPath = Path.Combine(AppContext.BaseDirectory, "Assets/Sounds/roblox-sword.wav");
            if (File.Exists(soundPath))
            {
                Audio.LoadSound("testSound", soundPath);
            }
            else
            {
                EngineLog.Log($"[Engine] Could not find sound to load at: {soundPath}");
            }
        }
        catch (Exception e)
        {
            EngineLog.Log($"[Engine] Failed to initialize AudioEngine: {e.Message}");
            throw;
        }

#if EDITOR
        
        
        
        
        SetLook();

        string fontPath = Path.Combine(AppContext.BaseDirectory, "EditorAssets/Fonts/Nunito-Regular.ttf");

        if (File.Exists(fontPath))
        {
            _customFont = io.Fonts.AddFontFromFileTTF(fontPath, 18f);
            EngineLog.Log("[Engine] Custom Font queued for loading");

            io.Fonts.Build();
            EngineLog.Log("[Engine] ImGui font atlas built");
        }
        else
        {
            EngineLog.Log("[Engine] Could not find font file: " + fontPath + ". Using default font");
        }
        
        fontPath = Path.Combine(AppContext.BaseDirectory, "EditorAssets/Fonts/Nunito-ExtraBold.ttf");

        if (File.Exists(fontPath))
        {
            _extraThickFont = io.Fonts.AddFontFromFileTTF(fontPath, 18f);
            EngineLog.Log("[Engine] Extra-Bold Font queued for loading");

            io.Fonts.Build();
            EngineLog.Log("[Engine] ImGui font atlas built");
        }
        else
        {
            EngineLog.Log("[Engine] Could not find font file: " + fontPath + ". Using default font");
        }
        
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);
        
        uint fontTextureId = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, fontTextureId);
        
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);
        
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

        
        io.Fonts.SetTexID((IntPtr)fontTextureId);
        
        io.Fonts.ClearTexData();
    
        EngineLog.Log("[Engine] Font texture manually created and uploaded to GPU.");
        
        imGuiController = new ImGuiController(gl, window, input);

        ViewportSize = window.FramebufferSize;
        CreateFrameBuffer();
        EngineLog.Log("[Engine] Initialised IMGUI Controller and framebuffer");

        try
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "EditorAssets/Images/Logo.png");
            _logoTextureID = TextureLoader.Load(gl, logoPath, out _logoSize);
            EngineLog.Log("[Engine] Texture Loaded");
        }
        catch (Exception e)
        {
            EngineLog.Log($"[Engine] Failed to load logo texture: {e.Message}");
        }

        _gizmo = new Gizmo();
        LoadIcons.PreloadIcons();
#endif

        shader = new Shader(gl, "Shaders/shader.vert", "Shaders/shader.frag");
        
        //Format: X, Y, Z, U, V
        float[] squareVertices =
        {
            // Position           // UV
            0.5f,  0.5f, 0.0f,   1.0f, 0.0f, // Top Right (0)
            0.5f, -0.5f, 0.0f,   1.0f, 1.0f, // Bottom Right (1)
            -0.5f, -0.5f, 0.0f,   0.0f, 1.0f, // Bottom Left (2)
            -0.5f,  0.5f, 0.0f,   0.0f, 0.0f  // Top Left (3)
        };
        uint[] squareIndices = {0, 3, 2, 
            2, 1, 0};
        
        //Create our example mesh resource
        _squareMesh = new GameObjectScripts.Mesh(gl, squareVertices, squareIndices);
        
        // If no texture assigned use 1x1 white texture
        _whiteTexture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _whiteTexture);
        byte[] whitePixel = { 255, 255, 255, 255 };
        fixed (byte* p = whitePixel)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }
        gl.BindTexture(TextureTarget.Texture2D, 0);
        
        var gameObject1 = new GameObject(_squareMesh, ObjectType.Quad);
        gameObject1.GetComponent<Transform>().WorldPosition = new Vector2(-0.75f, 0f);
        var renderer1 = gameObject1.GetComponent<MeshRenderer>();
        if (renderer1 != null) renderer1.Color = new Vector4(1, 0, 0, 1); // <- Red
        
        var gameObject2 = new GameObject(_squareMesh, ObjectType.Quad);
        gameObject2.GetComponent<Transform>().WorldPosition = new Vector2(-0.75f, 0.5f);
        gameObject1.MakeParentOfObject(gameObject2);
        
        #if GAME
        LoadIcons.LoadImageIcons();
        
        window.Title = BuildManager.LoadGameConfig()?? "CSCanbulatEngine";

        CurrentState = EngineState.Play;
#endif

    }
    
    /// <summary>
    /// Executes Every Frame
    /// </summary>
    /// <param name="deltaTime"></param>
    private void OnUpdate(double deltaTime)
    {
        if (window.Size.X < minWidth || window.Size.Y < minHeight)
        {
            window.Size = new Vector2D<int>(
                Math.Max(window.Size.X, minWidth), 
                Math.Max(window.Size.Y, minHeight)
            );
        }
        
        EngineLog.PrintLogs();
#if EDITOR
        //--------Keyboard shortcuts--------
        if (primaryKeyboard != null && !ImGui.GetIO().WantCaptureKeyboard)
        {
            bool modifierDown;
            bool altKeyDown = InputManager.IsKeyDown(Key.AltLeft) || InputManager.IsKeyDown(Key.AltRight);
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
                if (altKeyDown && InputManager.IsKeyPressed(Key.N))
                {
                    CircuitEditor.chips.Clear();
                    CircuitEditor.CircuitScriptName = "";
                    CircuitEditor.CircuitScriptDirPath = "";
                }
                else if (altKeyDown && InputManager.IsKeyPressed(Key.O))
                {
                    OpenCircuitScript();
                }
                else if (altKeyDown && InputManager.IsKeyPressed(Key.S))
                {
                    SaveCircuitScript();
                }
                else if (InputManager.IsKeyPressed(Key.S) &&
                         (InputManager.IsKeyDown(Key.ShiftLeft) || InputManager.IsKeyDown(Key.ShiftRight)))
                {
                    SaveSceneAs();
                }
                else if (InputManager.IsKeyPressed(Key.S))
                {
                    SaveScene();
                }
                else if (InputManager.IsKeyPressed(Key.O))
                {
                    LoadProject();
                }
                else if (InputManager.IsKeyPressed(Key.Backspace))
                {
                    _selectedGameObject?.gameObject.DeleteObject();
                }
                else if (InputManager.IsKeyPressed(Key.A) && !renamePopupOpen && !circuitEditorIsOpen)
                {
                    new GameObject(ChunFactory.CreateQuad(), ObjectType.Quad);
                }
                else if (InputManager.IsKeyPressed(Key.A) && !renamePopupOpen && circuitEditorIsOpen)
                {
                    openSpawnMenuNextFrame = true;
                }
                else if (InputManager.IsKeyPressed(Key.Number2) && _selectedGameObject != null)
                {
                    renamePopupOpen = true;
                }
                else if (InputManager.IsKeyPressed(Key.C))
                {
                    if (!circuitEditorIsOpen && _selectedGameObject != null)
                    {
                        gameObjectClipBoard = JsonConvert.SerializeObject(SceneSerialiser.GetGameObjectData(_selectedGameObject.gameObject));
                        EngineLog.Log("[Clipboard] Copied selected GameObject to clipboard");
                    }
                    else if (circuitEditorIsOpen && CircuitEditor.lastSelectedChip != null)
                    {
                        CircuitEditor.chipClipboard =
                            JsonConvert.SerializeObject(CircuitSerialiser.GetChipData(CircuitEditor.lastSelectedChip));
                        CircuitEditor.chipPosClipboard = CircuitEditor.lastSelectedChip.Position;
                        EngineLog.Log("[Clipboard] Copied selected chip to clipboard");
                    }
                }
                else if (InputManager.IsKeyPressed(Key.V))
                {
                    if (!circuitEditorIsOpen && !String.IsNullOrWhiteSpace(gameObjectClipBoard))
                    {
                        var theData = JsonConvert.DeserializeObject<SceneData.GameObjectData>(gameObjectClipBoard);
                        
                        if (theData != null)
                        {
                            var theGameObject = SceneSerialiser.CreateGameObjectFromData(theData);
                            theGameObject.GetComponent<Transform>().WorldPosition = theGameObject.GetComponent<Transform>().WorldPosition + new Vector2(0.5f);
                            EngineLog.Log("[Clipboard] Pasted selected GameObject to clipboard");
                        }
                    }
                    else if (circuitEditorIsOpen && !String.IsNullOrWhiteSpace(CircuitEditor.chipClipboard))
                    {
                        var theData = JsonConvert.DeserializeObject<CircuitData.ChipData>(CircuitEditor.chipClipboard);

                        if (theData != null)
                        {
                            var theChip = CircuitSerialiser.CreateChipFromData(theData, false);
                            theChip.Position = CircuitEditor.chipPosClipboard + new Vector2(5);
                            EngineLog.Log("[Clipboard] Pasted selected chip to clipboard");
                        }
                        else
                        {
                            EngineLog.Log("[Clipboard] Tried to paste but nothing was in the clipboard.");
                        }
                    }
                }
            }
        }
       
#endif

        if (CurrentState == EngineState.Play)
        {
            var updateEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnUpdate");
            if (updateEvent != null)
            {
                var payload = new EventValues();
                payload.floats["Delta Time"] = (float)deltaTime;
                EventManager.Trigger(updateEvent, payload);
            }
            
            ChernikovEngine.Step((float)deltaTime);
        }

        Audio?.Update();
        
        // ! ! ! Keep at last update ! ! !
        InputManager.LateUpdate();
    }

    /// <summary>
    /// Renders GUI Elements
    /// </summary>
    /// <param name="deltaTime"></param>
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
        if (ImGui.BeginPopupModal("Name Scene As", ImGuiWindowFlags.AlwaysAutoResize))
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
        
        if (ImGui.BeginPopupModal("Name Circuit Script As", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter a name for the circuit script");
            ImGui.InputText("##NameInput", Engine._nameBuffer, (uint)Engine._nameBuffer.Length);

            if (ImGui.Button("OK"))
            {
                string newName = Encoding.UTF8.GetString(Engine._nameBuffer).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    CircuitEditor.CircuitScriptName = newName;
                    SaveAsCircuitScriptContinued();
                }

                renameCircuitFileAsPopup = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                renameCircuitFileAsPopup = false;
                ImGui.CloseCurrentPopup();
                
            }
            ImGui.EndPopup();
        }
        
        if (openSpawnMenuNextFrame)
        {
            var canvasPos = ImGui.GetCursorScreenPos();
            var io = ImGui.GetIO();
            
            Vector2 mousePosInWorld = (io.MousePos - canvasPos - CircuitEditor.panning) / CircuitEditor.Zoom;
            
            CircuitChips.SetSpawnPos(mousePosInWorld);
            
            ImGui.BeginPopupContextWindow("SpawnChipMenu");

            openSpawnMenuNextFrame = false;
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

        if (buildMenuOpen)
        {
            BuildManager.BuildWindow();
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
    private unsafe void RenderEditorUI()
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

                if (ImGui.MenuItem("Build Project"))
                {
                    Array.Clear(BuildManager.BuildNameBuffer, 0, BuildManager.BuildNameBuffer.Length);
                    byte[] nameBytes = Encoding.UTF8.GetBytes(currentProject.ProjectName);
                    Array.Copy(nameBytes, BuildManager.BuildNameBuffer, Math.Min(nameBytes.Length, BuildManager.BuildNameBuffer.Length));
                    
                    buildMenuOpen = true;
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

            if (circuitEditorIsOpen)
            {
                CircuitEditor.MainMenuBar();
            }
            else
            {
                if (_selectedGameObject != null)
                {
                    _selectedGameObject.gameObject.RenderObjectOptionBar(superKey);
                }

                GameObject.RenderCreateObjectMenu(superKey);

                if (ImGui.BeginMenu("Debug"))
                {
                    bool dockingEnabled = ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable);
                    ImGui.MenuItem("Docking Enabled", "", dockingEnabled);
                    ImGui.EndMenu();
                }
            }

            if (ImGui.BeginMenu("Windows"))
            {
                if (ImGui.MenuItem("Project Manager Open", "", ref _projectTabActive))
                {
                }
                
                if (ImGui.MenuItem("Console Open", "", ref _consoleTabActive))
                {
                }

                if (ImGui.MenuItem("Project Settings Window Open", "", ref ProjectSettings.isOpen))
                {
                    
                }
                
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

        if (ProjectSettings.isOpen)
        {
            ProjectSettings.ProjectSettingsWindow();
        }
        
        // Render the editor UI
        // -- Viewport --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[0]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[0]);
        ImGui.Begin("Game Viewport", editorPanelFlags | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar);
        if (ImGui.BeginTabBar("MainWindowTabs"))
        {
            if (ImGui.BeginTabItem("Viewport"))
            {
                _isViewportFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
                circuitEditorIsOpen = false;
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
            else
            {
                _isViewportFocused = false;
            }

            if (ImGui.BeginTabItem("Circuit Editor"))
            {
                circuitEditorIsOpen = true;
                CircuitEditor.Render(); 
                ImGui.EndTabItem();
                ImGui.SetWindowFontScale(1f);
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

        if (!circuitEditorIsOpen)
        {
            //Puts all the properties in the inspector visible and rendered
            GameObject.RenderGameObjectInspector();
        }
        else
        {
            CircuitEditor.RenderChipInspector();
        }
        ImGui.End();
        
        // -- Hierarchy --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[2]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[2]);
        ImGui.Begin((circuitEditorIsOpen? "Circuit Editor" : "Hierarchy") + $" - {(circuitEditorIsOpen? CircuitEditor.CircuitScriptName : currentScene.SceneName)}", editorPanelFlags);

        if (!circuitEditorIsOpen)
        {
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("HIERARCHY_GAMEOBJECT");

                if (payload.NativePtr != null && payload.Data != IntPtr.Zero)
                {
                    int draggedId = *(int*)payload.Data;

                    GameObject draggedObject = GameObject.FindGameObject(draggedId);

                    if (draggedObject != null)
                    {
                        draggedObject.RemoveParentObject();
                    }
                }

                ImGui.EndDragDropTarget();
            }

            while (true)
            {
                HierarchyNeedsRefresh = false;

                foreach (var gameObject in currentScene.GameObjects.ToList())
                {
                    if (gameObject is null || GameObject.FindGameObject(gameObject.ID) == null) continue;

                    if (gameObject.ParentObject == null)
                    {
                        RenderHierarchyNode(gameObject);
                    }

                    if (HierarchyNeedsRefresh) break;
                }

                if (!HierarchyNeedsRefresh) break;

            }


            Vector2 remainingSpace = ImGui.GetContentRegionAvail();
            
            if (remainingSpace.Y < 0) remainingSpace.Y = 0;
            if (remainingSpace.X < 1) remainingSpace.X = 1;
            
            ImGui.InvisibleButton("###HierarchyEmptySpace", remainingSpace);
            
            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("HIERARCHY_GAMEOBJECT");

                if (payload.NativePtr != null && payload.Data != IntPtr.Zero)
                {
                    int draggedId = *(int*)payload.Data;

                    GameObject draggedObject = GameObject.FindGameObject(draggedId);

                    if (draggedObject != null)
                    {
                        draggedObject.RemoveParentObject();
                    }
                }

                ImGui.EndDragDropTarget();
            }

            if (ImGui.BeginPopupContextItem())
            {
                GameObject.RenderCreateObjectMenu(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)? "CMD" : "CTRL");
                ImGui.EndPopup();
            }
        }
        else
        {
            foreach (var chip in CircuitEditor.chips)
            {
                bool selected = chip == CircuitEditor.lastSelectedChip;
                ImGui.PushID(chip.Id);
                if (ImGui.Selectable($"{chip.Name}", selected))
                {
                    CircuitEditor.selectedChip = chip;
                    CircuitEditor.lastSelectedChip = chip;
                }

                ImGui.PopID();
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
            ImGui.OpenPopup("Name Scene As");
        }

        if (createProjectPopup)
        {
            ImGui.OpenPopup("Name Project");
        }

        if (projectFoundPopup)
        {
            ImGui.OpenPopup("Project Found");
        }

        if (renameCircuitFileAsPopup)
        {
            ImGui.OpenPopup("Name Circuit Script As");
        }

        if (renameFilePopupOpen)
        {
            ImGui.OpenPopup("Rename File");
        }
        
        // -- Project File Manager --
        ImGui.SetNextWindowPos(ImGuiWindowManager.windowPosition[3]);
        ImGui.SetNextWindowSize(ImGuiWindowManager.windowSize[3]);
        ImGui.Begin("Project File Manager", editorPanelFlags |  ImGuiWindowFlags.NoTitleBar);
        
        if (ImGui.BeginTabBar("Bottom Window"))
        {
            ImGuiTabItemFlags projectTabFlags = ImGuiTabItemFlags.NoCloseWithMiddleMouseButton;
            if (ImGui.BeginTabItem("Project Manager", ref _projectTabActive, projectTabFlags))
            {
                float leftPanelWidth = ImGui.GetContentRegionAvail().X * 0.2f;
                ImGui.BeginChild("Directories", new Vector2(leftPanelWidth, ImGui.GetContentRegionAvail().Y),
                    ImGuiChildFlags.AutoResizeY);
                if (ImGui.Selectable("Assets"))
                {
                    ProjectManager.selectedDir = ProjectSerialiser.GetAssetsFolder();
                    EngineLog.Log(ProjectManager.selectedDir);
                }

                _projectManagerBounds = new RectangleF(new(ImGui.GetWindowPos()), new(ImGui.GetWindowSize()));

                if (_pendingDroppedFiles != null)
                {
                    if (ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows))
                    {
                        EngineLog.Log("Files dropped onto Project Manager!");

                        foreach (var path in _pendingDroppedFiles)
                        {
                            try
                            {
                                string fileName = Path.GetFileName(path);
                                string destPath = Path.Combine(ProjectManager.selectedDir, fileName);
                                File.Copy(path, destPath, true);
                                EngineLog.Log($"Imported '{fileName}' to assets.");
                            }
                            catch (Exception e)
                            {
                                EngineLog.Log($"Failed to import file '{path}': {e.Message}");
                            }
                        }
                    }

                    _pendingDroppedFiles = null;
                }

                ProjectManager.RenderDirectories();
                ImGui.EndChild();

                ImGui.SameLine();

                //File Icons
                ImGui.BeginChild("File Icons",
                    new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y),
                    ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AutoResizeX);

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
                        newName += (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/") +
                                   sepDirectories[i];
                    }
                }
                else newName = name;

                Array.Clear(sepDirectories, 0, sepDirectories.Length);

                ImGui.Text(newName);
                ImGui.SameLine();

                float sliderWidth = 150f;

                float newCursorPosX = ImGui.GetCursorPos().X + ImGui.GetContentRegionAvail().X - sliderWidth;

                ImGui.SetCursorPosX(newCursorPosX);

                ImGui.PushItemWidth(sliderWidth);

                if (ImGui.SliderFloat("Zoom", ref ProjectManager.SliderZoom, 0.5f, ProjectManager.maxZoom))
                {
                }

                ImGui.PopItemWidth();
                ImGui.Separator();
                ProjectManager.RenderProjectManagerIcons();
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            
            ImGuiTabItemFlags consoleFlags = ImGuiTabItemFlags.NoCloseWithMiddleMouseButton;

            if (_forceSetConsoleTab)
            {
                _consoleTabActive = true;
                consoleFlags |= ImGuiTabItemFlags.SetSelected;
                _forceSetConsoleTab = false; // Reset the flag
            }
            
            //Game Console
            if (ImGui.BeginTabItem("Console", ref _consoleTabActive, consoleFlags))
            {
                GameConsole.RenderConsole();
                ImGui.EndTabItem();
            }
            
            ImGui.EndTabBar();
        }
        ImGui.End();

        // if (fontLoaded)
        // {
        //     ImGui.PopFont();
        // }
        
        SetLook();
    }

    private unsafe void RenderHierarchyNode(GameObject gameObject)
    {
        bool isSelected = (_selectedGameObject?.gameObject == gameObject);

        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen |
                                   ImGuiTreeNodeFlags.SpanAvailWidth;

        if (isSelected)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        bool isLeaf = (gameObject.ChildObjects.Count == 0);

        if (isLeaf)
        {
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        string label = $"{gameObject.Name}##{gameObject.ID}";

        bool NodeOpen = ImGui.TreeNodeEx(label, flags);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            _selectedGameObject = new(gameObject);
        }

        if (ImGui.BeginDragDropSource())
        {
            int draggedId = gameObject.ID;
            ImGui.SetDragDropPayload("HIERARCHY_GAMEOBJECT", (IntPtr)(&draggedId), (uint)sizeof(int));
            ImGui.Text($"Parenting: {gameObject.Name}");
            
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("HIERARCHY_GAMEOBJECT");

            if (payload.NativePtr != null && payload.Data != IntPtr.Zero)
            {
                int draggedId = *(int*)payload.Data;

                GameObject draggedObject = GameObject.FindGameObject(draggedId);

                if (draggedObject != null)
                {
                    draggedObject.MakeChildOfObject(gameObject);
                }
            }
            ImGui.EndDragDropTarget();
        }
        
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.MenuItem("Delete"))
            {
                gameObject.DeleteObject();
                HierarchyNeedsRefresh = true;
                ImGui.EndPopup();
                
                if (NodeOpen && !isLeaf)
                {
                    ImGui.TreePop();
                }
                
                return;
            }

            if (gameObject.ParentObject != null)
            {
                if (ImGui.MenuItem("Unparent Object"))
                {
                    gameObject.RemoveParentObject();
                    ImGui.EndPopup();
                    
                    if (NodeOpen && !isLeaf)
                    {
                        ImGui.TreePop();
                    }
                    
                    return;
                }
            }
            ImGui.EndPopup();
        }
        
        if (NodeOpen)
        {
            foreach (var child in gameObject.ChildObjects.ToList())
            {
                if (HierarchyNeedsRefresh) break;
                RenderHierarchyNode(child);
            }

            if (!isLeaf)
            {
                ImGui.TreePop();
            }
        }
    }

    public static void RenderToolbar()
    {
        // float toolbarHeight = ImGuiWindowManager.menuBarHeight * 1.5f;
        // Vector2 viewportPos = ImGui.GetMainViewport().Pos;
        // Vector2 viewportSize = ImGui.GetMainViewport().Size;
        // ImGui.SetNextWindowPos(new Vector2(viewportPos.X + viewportSize.X * 0.5f, viewportPos.Y + ImGuiWindowManager.menuBarHeight), ImGuiCond.Always, new Vector2(0.5f, 0f));
        // ImGui.SetNextWindowSize(new Vector2(0, toolbarHeight));
        //
        // ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
        //                          ImGuiWindowFlags.NoScrollWithMouse;
        // ImGui.Begin("Toolbar", flags);
        float buttonSize = ImGui.GetContentRegionAvail().X - 5f;

        bool twoButtonsPerRow = buttonSize >= 50f;

        if (twoButtonsPerRow) buttonSize = (buttonSize / 2f) - 2.5f;
        
        if (twoButtonsPerRow)
        {
            ImGui.SameLine();
        }

        if (CurrentState == EngineState.Editor)
        {
            if (ImGui.ImageButton("Play", (IntPtr)LoadIcons.icons["Play.png"], new Vector2(buttonSize)))
            {
                CurrentState = EngineState.Play;
                ChernikovEngine.ResetRigidbodyValues();
                var sceneData = SceneSerialiser.SceneDataFromCurrentScene();
                _sceneSnapshotBeforePlay = JsonConvert.SerializeObject(sceneData, Formatting.Indented);

                var startEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnStart");
                if (startEvent != null) EventManager.Trigger(startEvent, new EventValues());
            }
        }
        else
        {
            IntPtr pauseButtonImage = CurrentState == EngineState.Pause ? (IntPtr)LoadIcons.icons["Play.png"] : (IntPtr)LoadIcons.icons["Pause.png"];
            if (ImGui.ImageButton("Pause", (IntPtr)pauseButtonImage, new Vector2(buttonSize)))
            {
                CurrentState = CurrentState == EngineState.Play ? EngineState.Pause : EngineState.Play;
            }
        }

        if (CurrentState != EngineState.Editor)
        {
            if (ImGui.ImageButton("Stop", (IntPtr)LoadIcons.icons["Stop.png"], new Vector2(buttonSize)))
            {
                CurrentState = EngineState.Editor;
                
                SceneSerialiser.LoadSceneFromString(_sceneSnapshotBeforePlay);
                
                Engine.ReloadAllCircuitScripts();
            }
        }
        
        // ImGui.End();
    }

    private void OnFileDrop(string[] paths)
    {
        _pendingDroppedFiles = paths;
        EngineLog.Log("File drop detected by OS, pending processing");
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
            
            renderer.Draw();
            
            // //Set color in shader :)
            // shader.SetUniform("uColor", renderer.Color);
            //
            // uint textureToBind = renderer.TextureID != 0 ? renderer.TextureID : _whiteTexture;
            // gl.BindTexture(TextureTarget.Texture2D, textureToBind);
            //
            // //Get the matrix from the transform
            // Matrix4x4 modelMatrix = gameObject.GetComponent<Transform>().GetModelMatrix();
            // //Set model uniform in the shader for the object
            // shader.SetUniform("model", modelMatrix);
            //
            // renderer.Mesh.Draw();
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
        SaveWindowSettings();
#endif
        Audio?.Dispose();
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
            EngineLog.Log("The Framebuffer is not complete!");
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
            if (currentProject.ProjectFolderPath != null && currentProject.ProjectName != null)
            {
                ProjectSerialiser.SaveProjectFile(currentProject.ProjectFolderPath, currentProject.ProjectName);
            }
        }
        else
        {
            SaveSceneAs();
        }
    }

    private static void LoadProject()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        EngineLog.Log($"Folder Selected: {FileDialogHelper.ShowSelectFolderDialog(documentsPath, "Select The Folder To Load Project")}");
    }

    private static void LoadScene()
    {
        string documentsPath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Scenes");
        string? projectPath = FileDialogHelper.ShowOpenFileDialog(documentsPath, new [] {"*.cbs"});
        EngineLog.Log($"Folder Selected: {projectPath}");
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

    public static void LoadCircuitScript(string filePath)
    {
        CircuitSerialiser.LoadCircuit(filePath);
    }

    public static void OpenCircuitScript()
    {
        string circuitPathFolder = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Circuits");
        string? circuitScriptPath = FileDialogHelper.ShowOpenFileDialog(circuitPathFolder, new [] {"*.ccs"});
        EngineLog.Log($"File Selected: {circuitScriptPath}");
        if (!String.IsNullOrWhiteSpace(circuitScriptPath))
        {
            LoadCircuitScript(circuitScriptPath);
        }
    }

    public static void SaveCircuitScript()
    {
        if (!String.IsNullOrWhiteSpace(CircuitEditor.CircuitScriptName) && !String.IsNullOrWhiteSpace(CircuitEditor.CircuitScriptDirPath))
        {
            CircuitSerialiser.SaveCircuit(CircuitEditor.CircuitScriptName, CircuitEditor.CircuitScriptDirPath);
        }
        else
        {
            SaveAsCircuitScript();
        }
    }

    public static void SaveAsCircuitScript()
    {
        renameCircuitFileAsPopup = true;
    }

    private static void SaveAsCircuitScriptContinued()
    {
        if (!String.IsNullOrWhiteSpace(CircuitEditor.CircuitScriptName))
        {
            CircuitSerialiser.SaveCircuit(CircuitEditor.CircuitScriptName, CircuitEditor.CircuitScriptDirPath);
        }
        else
        {
            GameConsole.Log("The circuit script name is empty! It has not been saved");
        }
    }
    
#endif

    public static void ReloadAllCircuitScripts()
    {
        foreach (var gameObject in Engine.currentScene.GameObjects)
        {
            foreach (var component in gameObject.Components)
            {
                if (component is CircuitScript circuitScript)
                {
                    circuitScript.LoadCircuit(Path.Combine(circuitScript.CircuitScriptDirPath, circuitScript.CircuitScriptName) + ".ccs");
                }
            }
        }
    }

    public static void ExecuteOnPlay()
    {
        foreach (var gameObject in Engine.currentScene.GameObjects)
        {
            if (gameObject.HasComponent<CircuitScript>())
            {
                foreach (var chip in gameObject.GetComponent<CircuitScript>().chips)
                {
                    chip.OnPlay();
                }
            }
        }
    }

    public static void ExecuteOnPause()
    {
        foreach (var gameObject in Engine.currentScene.GameObjects)
        {
            if (gameObject.HasComponent<CircuitScript>())
            {
                foreach (var chip in gameObject.GetComponent<CircuitScript>().chips)
                {
                    chip.OnPause();
                }
            }
        }
    }

    public static void ExecuteOnResume()
    {
        foreach (var gameObject in Engine.currentScene.GameObjects)
        {
            if (gameObject.HasComponent<CircuitScript>())
            {
                foreach (var chip in gameObject.GetComponent<CircuitScript>().chips)
                {
                    chip.OnResume();
                }
            }
        }
    }

    public static void ExecuteOnStop()
    {
        foreach (var gameObject in Engine.currentScene.GameObjects)
        {
            if (gameObject.HasComponent<CircuitScript>())
            {
                foreach (var chip in gameObject.GetComponent<CircuitScript>().chips)
                {
                    chip.OnStop();
                }
            }
        }
    }

    
    //Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanbulatEngine", "Logs")
    
    /// <summary>
    /// Save settings of current window to WindowConfig.cseetings
    /// </summary>
    public void SaveWindowSettings()
    {
        IWindow window = Engine.window;

        Dictionary<string, string> Config = new Dictionary<string, string>()
        {
            {"SizeX", window.Size.X.ToString(CultureInfo.InvariantCulture)},
            {"SizeY", window.Size.Y.ToString(CultureInfo.InvariantCulture)},
            {"PositionX", window.Position.X.ToString(CultureInfo.InvariantCulture)},
            {"PositionY", window.Position.Y.ToString(CultureInfo.InvariantCulture)},
            {"WindowState", (window.WindowState.ToString())}
        };
        
        string savePlace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanbulatEngine", "WindowConfig.csettings");
        
        string jsonSettings = JsonConvert.SerializeObject(Config, Formatting.Indented);
        
        File.WriteAllText(savePlace, jsonSettings);
        EngineLog.Log($"Window Settings Saved: {savePlace}");
    }

    public void LoadWindowSettings()
    {
        string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanbulatEngine", "WindowConfig.csettings");

        if (!File.Exists(settingsPath))
        {
            EngineLog.Log($"Window Settings not found: {settingsPath}, loading default config...");
            return;
        }
        
        IWindow window = Engine.window;
        
        string jsonSettings = File.ReadAllText(settingsPath);
        Dictionary<string, string> Config = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSettings);

        bool Fullscreen = false;
        bool Maximized = false;
        
        if (Config.ContainsKey("WindowState"))
        {
            Config.TryGetValue("WindowState", out var windowStateStr);
            Fullscreen = windowStateStr == "Fullscreen";
            Maximized = windowStateStr == "Maximized";
            
            if (Fullscreen) window.WindowState = WindowState.Fullscreen;
            if (Maximized) window.WindowState = WindowState.Maximized;
        }

        Vector2D<int> size = window.Size;
        if (Config.ContainsKey("SizeX"))
        {
            size.X = Config.TryGetValue("SizeX", out var sxStr)? int.Parse(sxStr, CultureInfo.InvariantCulture) : 0;
        }

        if (Config.ContainsKey("SizeY"))
        {
            size.Y = Config.TryGetValue("SizeY", out var syStr)? int.Parse(syStr, CultureInfo.InvariantCulture) : 0;
        }

        window.Size = size;
        
        Vector2D<int> position = window.Position;

        if (Config.ContainsKey("PositionX"))
        {
            position.X = Config.TryGetValue("PositionX", out var posStr)? int.Parse(posStr, CultureInfo.InvariantCulture) : 0;
        }

        if (Config.ContainsKey("PositionY"))
        {
            position.Y = Config.TryGetValue("PositionY", out var posStr)? int.Parse(posStr, CultureInfo.InvariantCulture) : 0;
        }
        
        window.Position = position;
        
        
    }
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

public enum EngineState
{
    Editor, Play, Pause
}