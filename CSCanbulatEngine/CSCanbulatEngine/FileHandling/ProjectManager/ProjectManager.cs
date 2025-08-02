using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;

namespace CSCanbulatEngine.FileHandling.ProjectManager;

public static class ProjectManager
{
    private static readonly string[] _ignoredFileTypes = new[] { ".DS_Store" };
    
#if EDITOR

    public static string selectedDir = "";

    public static float maxZoom = -1f;
    
    private static string selectedFileToRename = "";

    
    public static float zoom
    {
        get => _zoom;
        set { _zoom = value;
            iconSize = refIconSize * value;
        }
    }
    private static float _zoom = 1f;
    
    public static float SliderZoom = zoom;

    private static float iconSize
    {
        get => _iconSize;
        set { _iconSize = value;
            totalCellWidth = iconSize + cellPadding;
        }
    }
    
    private static float _iconSize = 60f;
    private static float refIconSize = 60f;
    static float cellPadding = 20;
    static float totalCellWidth = iconSize + cellPadding;

    public static void LoadManagerDefaults()
    {
        maxZoom = (float)ImGui.GetContentRegionAvail().X / (float)iconSize;
    }
    public static void RenderDirectories()
    {
        foreach (string dir in Directory.GetDirectories(ProjectSerialiser.GetAssetsFolder()))
        {
            RenderDirNode(dir);
        }
    }
    
    // Project file manager
    // List on side
    public static void RenderDirNode(string dir)
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;

        if (selectedDir == dir)
        {  
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (Directory.GetDirectories(dir).Length == 0)
        {
            flags = ImGuiTreeNodeFlags.Leaf;
        }

        bool nodeOpen = ImGui.TreeNodeEx(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? dir.Split('\\').Last():dir.Split("/").Last(), flags);
        
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            selectedDir = Path.Combine(ProjectSerialiser.GetAssetsFolder(), FileHandling.GetNameOfFile(dir));
            Console.WriteLine(selectedDir);
        }

        if (nodeOpen)
        {
            foreach (string childDir in Directory.GetDirectories(dir))
            {
                RenderDirNode(childDir);
            }

            ImGui.TreePop();
        }
    }

    public static void RenderProjectManagerIcons()
    {
        zoom = SliderZoom;
        if (String.IsNullOrWhiteSpace(selectedDir)) selectedDir = ProjectSerialiser.GetAssetsFolder();
        
        if (maxZoom == -1) LoadManagerDefaults();
        
        float avaliableWidth = ImGui.GetContentRegionAvail().X;
        
        int columnCount = (int)(avaliableWidth / totalCellWidth);

        if (columnCount < 1)
        {
            columnCount = 1;
        }
        
        ImGui.Columns(columnCount, "FileGrid", false);

        string[] files = Directory.GetFiles(selectedDir);
        string[] dirs = Directory.GetDirectories(selectedDir);
        
        List<string> listedItems = new List<string>();
        listedItems.AddRange(files);
        listedItems.AddRange(dirs);

        foreach (var name in listedItems)
        {
            if (_ignoredFileTypes.Contains(Path.GetExtension(name)))
            {
                continue;
            }
            
            FileAttributes attr = File.GetAttributes(name);

            bool isDir = attr.HasFlag(FileAttributes.Directory);

            string iconName = isDir ? "Folder.png" : "Page.png";
            
            switch (Path.GetExtension(name))
            {
                case ".mp3":
                    iconName = "Waveform.png";
                    break;
                case ".cbs":
                    iconName = "Photoframe.png";
                    break;
                case null:
                    break;
                
            }
            
            uint iconId = LoadIcons.icons[iconName];

            Vector2D<int> actualSize = LoadIcons.iconSizes[iconName];

            if (name.ToLower().EndsWith(".png") ||  name.ToLower().EndsWith(".jpg") || name.ToLower().EndsWith(".jpeg"))
            {
                if (!LoadIcons.imageIcons.ContainsKey(name))
                {
                    LoadIcons.LoadImageIcons();
                }

                iconId = LoadIcons.imageIcons[name];
                actualSize = LoadIcons.imageIconSizes[name];
            }

            bool isHeightBigger = actualSize.Y > actualSize.X;

            Vector2 size = isHeightBigger ? new Vector2((int)((float)iconSize * ((float)actualSize.X/(float)actualSize.Y)),iconSize) : new Vector2(iconSize, (int)((float)iconSize * ((float)actualSize.Y / (float)actualSize.X)));
            
            ImGui.PushID(FileHandling.GetNameOfFile(name));
            
            ImGui.BeginGroup();
            {
                ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                if (ImGui.ImageButton(name, (IntPtr)iconId, new Vector2(iconSize, iconSize)))
                {
                    Console.WriteLine($"Clicked on {name}");
                    if (isDir)
                    {
                        selectedDir = name;
                    }
                    else if (Path.GetExtension(name) == ".cbs")
                    {
                        SceneSerialiser ss = new SceneSerialiser(Engine.gl, Engine._squareMesh);
                        ss.LoadScene(name);
                    }
                }
                ImGui.PopStyleColor();
                
                if (!isDir && ImGui.BeginDragDropSource())
                {
                    byte[] pathBytes = Encoding.UTF8.GetBytes(name);
                    IntPtr pathPtr = Marshal.AllocHGlobal(pathBytes.Length + 1);
                    Marshal.Copy(pathBytes, 0, pathPtr, pathBytes.Length);
                    Marshal.WriteByte(pathPtr, pathBytes.Length, 0);

                    ImGui.SetDragDropPayload("DND_ASSET_PATH", pathPtr, (uint)pathBytes.Length + 1);
                
                    ImGui.Image((IntPtr)iconId, new Vector2(32, 32));
                    ImGui.SameLine();
                    ImGui.Text(Path.GetFileName(name));
                
                    ImGui.EndDragDropSource();
                }

                float textWidth = ImGui.CalcTextSize(Path.GetFileNameWithoutExtension(name)).X;
                float currentIconWidth = ImGui.GetItemRectSize().X;
                float textPadding = (currentIconWidth - textWidth) * 0.5f;
                if (textPadding > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + textPadding);
                ImGui.Text(Path.GetFileNameWithoutExtension(name));
            }
            ImGui.EndGroup();

           
            
            
            
            Vector2 itemMin = ImGui.GetItemRectMin();
            Vector2 itemMax = ImGui.GetItemRectMax();
            
            if (ImGui.IsMouseHoveringRect(itemMin, itemMax))
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup($"ContextMenu_{name}");
                }
            }

            if (ImGui.BeginPopupContextItem($"ContextMenu_{name}"))
            {
                ImGui.Text(Path.GetFileName(name));
                ImGui.Separator();

                if (ImGui.MenuItem("Rename"))
                {
                    selectedFileToRename = name;
                    
                    Array.Clear(Engine._nameBuffer, 0, Engine._nameBuffer.Length);
                        byte[] currentNameBytes = Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(name));
                        Array.Copy(currentNameBytes, Engine._nameBuffer, currentNameBytes.Length);
                        Engine.renameFilePopupOpen = true;
                }

                if (ImGui.MenuItem("Delete"))
                {
                    string filePath = Path.Combine(selectedDir, name);
                    if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                    {
                        Directory.Delete(filePath, true);
                    }
                    else
                    {
                        File.Delete(filePath);
                    }
                }
                
                if (ImGui.MenuItem($"Open in {(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)? "Finder" : "Explorer")}"))
                {
                    FileHandling.ShowInFileManager(name);
                }
                
                ImGui.EndPopup();
            }

            ImGui.PopID();
            ImGui.NextColumn();
        }
        
        ImGui.Columns(1);
    }

    public static void RenameFileContinued(string name)
    {
        string filePath = Path.Combine(selectedDir, name);
        if (File.GetAttributes(selectedFileToRename).HasFlag(FileAttributes.Directory))
        {
            Directory.Move(selectedFileToRename, filePath);
        }
        else
        {
            File.Move(selectedFileToRename, filePath + Path.GetExtension(selectedFileToRename), true);
        }
        
    }
    #endif
}