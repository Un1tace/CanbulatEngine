using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using CSCanbulatEngine.FileHandling.Game;
using CSCanbulatEngine.InfoHolders;
using ImGuiNET;

namespace CSCanbulatEngine.FileHandling;

    public static class BuildManager
    {
        #if EDITOR
        public static void BuildGame()
        {
            string sourceProjectFolder = Engine.currentProject.ProjectFolderPath;
            string sourceAssetsFolder = Path.Combine(sourceProjectFolder, "Assets");
            
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputFolder = FileDialogHelper.ShowSelectFolderDialog(documentsPath, "Select Build Output Folder");

            if (string.IsNullOrEmpty(outputFolder))
            {
                GameConsole.Log("[Build] Build cancelled: No folder selected.");
                EngineLog.Log("[Build] Build cancelled: No folder selected.");
                return;
            }

            string projectPath = FindProjectFile();
            if (string.IsNullOrEmpty(projectPath))
            {
                GameConsole.Log("[Build] Error: Could not find 'CSCanbulatEngine.csproj'. Make sure to run from bin folder");
                EngineLog.Log("[Build] Error: Could not find 'CSCanbulatEngine.csproj'. Make sure to run from bin folder");
                return;
            }
            
            GameConsole.Log($"[Build] Starting build... Output: {outputFolder}");
            EngineLog.Log($"[Build] Starting build... Output: {outputFolder}");

            string rid = "win-x64";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) rid = "osx-x64";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) rid = "linux-x64";

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{projectPath}\" -c Release -r {rid} --self-contained -o \"{outputFolder}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Task.Run(() =>
            {
                try
                {
                    using (Process process = Process.Start(startInfo))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            CreateGameConfig(outputFolder, Engine.currentProject.StartupSceneName);

                            if (Directory.Exists(Path.Combine(sourceProjectFolder, "Assets")))
                            {
                                Console.WriteLine("Copying Assets...");
                                try 
                                {
                                    CopyDirectory(Path.Combine(sourceProjectFolder, "Assets"), Path.Combine(outputFolder, "Assets"), true);
                                    Console.WriteLine("Assets copied successfully.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error copying assets: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Warning: No 'Assets' folder found to copy!");
                            }
                            
                            GameConsole.Log("[Build] Build Successful!");
                            EngineLog.Log("[Build] Build completed successfully.");
                            
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start("explorer.exe", outputFolder);
                            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", outputFolder);
                            
                        }
                        else
                        {
                            GameConsole.Log($"[Build] Failed: \n{error}\n{output}");
                            EngineLog.Log($"[Build] Failed: \n{error}\n{output}");
                        }
                    }
                }
                catch (Exception e)
                {
                    GameConsole.Log($"[Build] Error: {e.Message}. Do you have the .NET SDK installed?");
                    EngineLog.Log($"[Build] Error: {e.Message}");
                }
            });
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

        public static void BuildWindow()
        {
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.Appearing);

            ImGui.Begin("Build Window");

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

            ImGui.Separator();

            ImGui.BeginDisabled(Engine.currentProject.StartupSceneName == null || !projectFiles.Contains(Engine.currentProject.StartupSceneName));

            if (ImGui.Button("Build!"))
            {
                Engine.buildMenuOpen = false;
                BuildGame();
            }
            
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                Engine.buildMenuOpen = false;
            }
            
            ImGui.End();
        }

        public static void CreateGameConfig(string buildDirectory, string startupSceneName)
        {
            GameConfig config = new GameConfig
            {
                StartupSceneName = startupSceneName,
                GameName = Engine.currentProject.ProjectName
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
        
        
        public static void LoadGameConfig()
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "Game.config");

            if (File.Exists(configPath))
            {
                try 
                {
                    string jsonString = File.ReadAllText(configPath);
                    GameConfig config = JsonSerializer.Deserialize<GameConfig>(jsonString);

                    if (!string.IsNullOrEmpty(config.StartupSceneName))
                    {
                        string scenePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Scenes", config.StartupSceneName);
                        Console.WriteLine($"[Engine] Loading startup scene: {scenePath}");

                        SceneSerialiser ss = new SceneSerialiser(Engine.gl, Engine._squareMesh);
                        ss.LoadScene(scenePath); 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Engine] Failed to load Game.config: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("[Engine] No Game.config found. Loading default scene...");
            }
        }
    }