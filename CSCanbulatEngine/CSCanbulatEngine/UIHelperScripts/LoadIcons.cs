using CSCanbulatEngine.FileHandling;
using Silk.NET.Maths;

namespace CSCanbulatEngine.UIHelperScripts;

#if EDITOR
public class LoadIcons
{
    public static Dictionary<string, uint> icons = new Dictionary<string, uint>();
    public static Dictionary<string, Vector2D<int>> iconSizes = new Dictionary<string, Vector2D<int>>();
    
    //Images
    public static Dictionary<string, uint> imageIcons = new Dictionary<string, uint>();
    public static Dictionary<string, Vector2D<int>> imageIconSizes = new Dictionary<string, Vector2D<int>>();

    private static string IconDirectory = "Resources/Icons";

    public static void PreloadIcons()
    {
        try
        {
            string fullDirectory = Path.Combine(AppContext.BaseDirectory, IconDirectory);
            List<string> filesFound = Directory.GetFiles(fullDirectory).ToList();

            foreach (string file in filesFound)
            {
                string baseFile = FileHandling.FileHandling.GetNameOfFile(file);
                if (!file.EndsWith("png"))
                {
                    filesFound.Remove(file);
                }
                else
                {
                    Vector2D<int> size = new Vector2D<int>(0, 0);
                    uint textureID = TextureLoader.Load(Engine.gl, file, out size);
                    icons.Add(baseFile, textureID);
                    iconSizes.Add(baseFile, size);
                    EngineLog.Log($"[IconLoader] {baseFile} has been loaded");
                }
            }
        }
        catch (Exception ex)
        {
            EngineLog.Log($"[IconLoader] Failed to load icons: {ex.Message}");
        }
        
        
    }

    public static void LoadImageIcons()
    {
        string assetsFolder = ProjectSerialiser.GetAssetsFolder();
        
        FindImagesInDirectory(assetsFolder);
        
        FindImagesInSubDirectory(assetsFolder);
    }

    private static void FindImagesInSubDirectory(string path)
    {
        string[] subDirectories = Directory.GetDirectories(path);
        foreach (string subDirectory in subDirectories)
        {
            FindImagesInDirectory(subDirectory);
            FindImagesInSubDirectory(subDirectory);
        }
    }

    private static void FindImagesInDirectory(string path)
    {
        string[] files = Directory.GetFiles(path);

        if (files.Length != 0)
        {
            foreach (string file in files)
            {
                if (file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpg") ||
                    file.ToLower().EndsWith(".jpeg"))
                {
                    if (!imageIcons.ContainsKey(file))
                    {
                        uint id = TextureLoader.Load(Engine.gl, file, out Vector2D<int> size);
                        imageIcons.Add(file, id);
                        imageIconSizes.Add(file, size);
                        EngineLog.Log($"[IconLoader] {file} has been loaded");
                    }
                }
            }
        }
    }
}
#endif