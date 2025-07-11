using Silk.NET.Maths;

namespace CSCanbulatEngine.UIHelperScripts;

#if EDITOR
public class LoadIcons
{
    public static Dictionary<string, uint> icons = new Dictionary<string, uint>();
    public static Dictionary<string, Vector2D<int>> iconSizes = new Dictionary<string, Vector2D<int>>();

    private static string IconDirectory = "Resources/Icons";

    public static void PreloadIcons()
    {
        string fullDirectory = Path.Combine(AppContext.BaseDirectory, IconDirectory);
        List<string> filesFound = Directory.GetFiles(fullDirectory).ToList();

        foreach (string file in filesFound)
        {
            string baseFile = file.TrimStart(fullDirectory.ToCharArray());
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
                Console.WriteLine($"{baseFile} has been loaded");
            }
        }
        
        
    }
}
#endif