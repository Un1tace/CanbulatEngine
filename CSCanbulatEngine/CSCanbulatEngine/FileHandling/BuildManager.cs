using System.Diagnostics;
using System.Net.Mime;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.FileHandling.Game;
using CSCanbulatEngine.InfoHolders;
using ImGuiNET;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CSCanbulatEngine.FileHandling;

/// <summary>
/// Manages building and loading built games
/// </summary>
    public static class BuildManager
    {
        private static string buildForOS = "";
        public static byte[] BuildNameBuffer = new byte[128];
        
        private const string EngineName = "CSCanbulatEngine";
        
        #if EDITOR
        public static void BuildGame(string OS, string Name, string startupStringName)
        {
            string sourceProjectFolder = Engine.currentProject.ProjectFolderPath;
            string sourceAssetsFolder = Path.Combine(sourceProjectFolder, "Assets");
            
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputFolder = FileDialogHelper.ShowSelectFolderDialog(documentsPath, "Select Build Output Folder");

            if (string.IsNullOrEmpty(outputFolder))
            {
                GameConsole.Log("[Build] Build cancelled: No folder selected");
                return;
            }
            
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", OS == "MacOS-arm64"? "Mac" : "Windows");

            if (!Directory.Exists(templatePath))
            {
                GameConsole.Log($"[Build] Error: GameTemplate folder not found at {templatePath}");
            }
            
            GameConsole.Log($"[Build] Building '{Name}' for {OS} to: {outputFolder}...");

            try
            {
                // if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                // {
                //     RunShellCommand($"cp -a \"{templatePath}/.\" \"{outputFolder}\"");
                //     RunShellCommand($"chmod -R 777 \"{outputFolder}\"");
                // }
                // else
                // {
                //     CopyDirectory(templatePath, outputFolder, true);
                // }
                CopyDirectory(templatePath, outputFolder, true);

                string targetDir = outputFolder;

                if (OS == "MacOS-arm64")
                {
                    string appBundlePath = Path.Combine(outputFolder, EngineName + ".app");
                    string newAppBundlePath = Path.Combine(outputFolder, Name + ".app");

                    if (Directory.Exists(appBundlePath))
                    {
                        if (Name != EngineName)
                        {
                            if (Directory.Exists(newAppBundlePath)) Directory.Delete(newAppBundlePath, true);
                            Directory.Move(appBundlePath, newAppBundlePath);
                            GameConsole.Log($"[Build] Renamed Bundle to {Name}.app");
                        }

                        targetDir = Path.Combine(newAppBundlePath, "Contents", "MacOS");
                        
                        string plistPath = Path.Combine(newAppBundlePath, "Contents", "Info.plist");

                        if (File.Exists(plistPath))
                        {
                            string content = File.ReadAllText(plistPath);
                            content = content.Replace(EngineName, Name);
                            content = content.Replace("com.company.CSCanbulatEngine", $"com.company.{Name.Replace(" ", "")}");
                            File.WriteAllText(plistPath, content);
                            GameConsole.Log("[Build] Patched Info.plist");
                        }
                    }
                    else
                    {
                        GameConsole.Log("[Build] Warning: .app bundle not found in template. Building as flat file?");
                    }
                }
                else if (OS == "Windows-x64")
                {
                    GameConsole.Log("[Build] Configuring for Windows-x64 structure");
                }

                string outputAssets = Path.Combine(targetDir, "Assets");
                Directory.CreateDirectory(outputAssets);

                // if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                // {
                //     RunShellCommand($"cp -a \"{sourceAssetsFolder}/.\" \"{outputAssets}\"");
                // }
                // else
                // {
                //     CopyDirectory(sourceAssetsFolder, outputAssets, true);
                // }
                CopyDirectory(sourceAssetsFolder, outputAssets, true);

                if (Name != EngineName)
                {
                    RenameGameFiles(targetDir, Name, OS);
                }
                
                Engine.currentProject.StartupSceneName = startupStringName;
                string configJson = JsonConvert.SerializeObject(Engine.currentProject, Formatting.Indented);
                File.WriteAllText(Path.Combine(targetDir, "Game.config"),configJson);


                if (OS == "MacOS-arm64" && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string newAppBundlePath = Path.Combine(outputFolder, Name + ".app");
                    string appRoot = Directory.Exists(newAppBundlePath) ? newAppBundlePath : targetDir;
                    string exePath = Path.Combine(targetDir, Name);
                    
                    RunShellCommand($"xattr -cr \"{appRoot}\"");
                    
                    RunShellCommand($"find \"{appRoot}\" -type f -name \"._*\" -delete");
                    RunShellCommand($"find \"{appRoot}\" -type f -name \".DS_Store\" -delete");

                    if (File.Exists(exePath))
                    {
                        RunShellCommand($"chmod +x \"{exePath}\"");
                        RunShellCommand($"codesign --force --deep -s - \"{appRoot}\"");
                        GameConsole.Log("[Build] App Signed & Quarantines Removed.");
                    }
                }
                else if (OS == "MacOS-arm64") GameConsole.Log("[Build] Warning: Build files will need to be signed");
                
                GameConsole.Log("[Build] SUCCESS!");
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start("explorer.exe", outputFolder);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", outputFolder);
            }
            catch (Exception e)
            {
                GameConsole.Log($"[Build] Failed: {e.Message}");
            }
        }

        private static void RenameGameFiles(string folder, string name, string OS)
        {
            string[] childDirs = Directory.GetDirectories(folder);
            List<string> extensions = new List<String>() {".pdb", ".deps.json", ".runtimeconfig.json" };

            if (OS == "Windows-x64") extensions.Add(".exe");
            else if (OS == "MacOS-arm64") extensions.Add("");
            
            foreach (var ext in extensions)
            {
                string oldFile = Path.Combine(folder, $"{EngineName}{ext}");
                string newFile = Path.Combine(folder, $"{name}{ext}");

                if (File.Exists(oldFile))
                {
                    if (File.Exists(newFile)) File.Delete(newFile);
                    
                    File.Move(oldFile, newFile);
                    GameConsole.Log($"[Build] Renamed: {Path.GetFileName(oldFile)} -> {Path.GetFileName(newFile)}");
                }
            }
            
            string depsFile = Path.Combine(folder, name + ".deps.json");
            if (File.Exists(depsFile))
            {
                string content =  File.ReadAllText(depsFile);

                content.Replace(EngineName, name);
                File.WriteAllText(depsFile, content);
                GameConsole.Log("[Build] Patched .deps.json references");
            }

            foreach (string childDir in childDirs)
            {
                RenameGameFiles(childDir, name, OS);
            }
        }

        public static string FindProjectFile()
        {
            string currentDir = AppContext.BaseDirectory;

            for (int i = 0; i < 5; i++)
            {
                string[] files = Directory.GetFiles(currentDir, "CSCanbulatEngine.csproj");
                if (files.Length > 0)
                {
                    return files[0];
                }
                
                DirectoryInfo parent = Directory.GetParent(currentDir);
                if (parent == null) break;
                currentDir = parent.FullName;
            }

            return null;
        }
        
        private static void RunShellCommand(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        public static void BuildWindow()
        {
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.Appearing);

            ImGui.Begin("Build Window");
            
            ImGui.Text("Build Name");
            ImGui.SameLine();
            ImGui.InputText("##GameName", BuildNameBuffer, (uint)BuildNameBuffer.Length);

            string[] projectFiles = ProjectSerialiser.ScanAssetsForFilesWithExtension([".cbs"]).ToArray<string>();

            for (int i = 0; i < projectFiles.Length; i++)
            {
                projectFiles[i] = Path.GetFileName(projectFiles[i]);
            }

            ImGui.Text("Start Scene");

            ImGui.SameLine();
            
            if (ImGui.BeginCombo("StartSceneSelector", Engine.currentProject.StartupSceneName)) 
            {
                foreach (var file in projectFiles)
                {
                    if (ImGui.Selectable(file, (file == Engine.currentProject.StartupSceneName)))
                    {
                        Engine.currentProject.StartupSceneName = file;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Text("Operating System");
            ImGui.SameLine();

            if (buildForOS == "")
            {
                buildForOS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "Windows-x64" : "MacOS-arm64";
            }

            if (ImGui.BeginCombo("OSPicker", buildForOS))
            {
                if (ImGui.Selectable("Windows-x64", buildForOS == "Windows-x64"))
                {
                    buildForOS = "Windows-x64";
                }

                if (ImGui.Selectable("MacOS-arm64", buildForOS == "MacOS-arm64"))
                {
                    buildForOS = "MacOS-arm64";
                }

                ImGui.EndCombo();
            }

            ImGui.Separator();

            ImGui.BeginDisabled(Engine.currentProject.StartupSceneName == null || !projectFiles.Contains(Engine.currentProject.StartupSceneName) || (buildForOS != "Windows-x64" && buildForOS != "MacOS-arm64"));

            if (ImGui.Button("Build!"))
            {
                string gameName = Encoding.UTF8.GetString(BuildNameBuffer).TrimEnd('\0');
                if (!String.IsNullOrWhiteSpace(gameName))
                {
                    Engine.buildMenuOpen = false;
                    BuildGame(buildForOS, gameName, Engine.currentProject.StartupSceneName);
                    ImGui.CloseCurrentPopup();
                }
            }
            
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                Engine.buildMenuOpen = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.End();
        }

        public static void CreateGameConfig(string buildDirectory, string startupSceneName, string buildName)
        {
            GameConfig config = new GameConfig
            {
                StartupSceneName = startupSceneName,
                GameName = buildName
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(config, options);

            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }
            
            string configPath = Path.Combine(buildDirectory, "Game.config");
            File.WriteAllText(configPath, jsonString);
            
            EngineLog.Log($"[Build] Created game config at {configPath}");
        }
        
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            
            DirectoryInfo[] dirs = dir.GetDirectories();
            
            Directory.CreateDirectory(destinationDir);
            
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }
            
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
        
#endif
        
        
        public static string? LoadGameConfig()
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName;
            string baseDirectory = Path.GetDirectoryName(exePath);
            
            EngineLog.Log($"Base directory: {baseDirectory}");

            string configPath = Path.Combine(baseDirectory, "Game.config");
            
            if (string.IsNullOrEmpty(baseDirectory)) baseDirectory = AppContext.BaseDirectory;

            if (File.Exists(configPath))
            {
                try 
                {
                    string jsonString = File.ReadAllText(configPath);
                    GameConfig config = JsonSerializer.Deserialize<GameConfig>(jsonString);

                    if (!string.IsNullOrEmpty(config.StartupSceneName))
                    {
                        string scenePath = Path.Combine(baseDirectory, "Assets", "Scenes", config.StartupSceneName);
                        Console.WriteLine($"[Engine] Loading startup scene: {scenePath}");

                        SceneSerialiser ss = new SceneSerialiser(Engine.gl, Engine._squareMesh);
                        ss.LoadScene(scenePath);
                        return config.GameName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Engine] Failed to load Game.config: {e.Message}");
                    return null;
                    throw;
                }
            }
            else
            {
                Console.WriteLine("[Engine] No Game.config found! Closing program...");
                System.Environment.Exit(0);
                return null;
            }
            return null;
        }

        public static GameConfig? GetGameConfig()
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName;
            string baseDirectory = Path.GetDirectoryName(exePath);
            
            EngineLog.Log($"Base directory: {baseDirectory}");

            string configPath = Path.Combine(baseDirectory, "Game.config");
            
            if (string.IsNullOrEmpty(baseDirectory)) baseDirectory = AppContext.BaseDirectory;

            if (File.Exists(configPath))
            {
                try 
                {
                    string jsonString = File.ReadAllText(configPath);
                    GameConfig config = JsonSerializer.Deserialize<GameConfig>(jsonString);

                    return config;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Engine] Failed to load Game.config: {e.Message}");
                    return null;
                    throw;
                }
            }
            else
            {
                Console.WriteLine("[Engine] No Game.config found! Closing program...");
                System.Environment.Exit(0);
                return null;
            }
            return null;
        }
    }