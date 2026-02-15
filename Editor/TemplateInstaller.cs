#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Core template installation engine - orchestrates the entire installation process
    /// Safe, idempotent, and reports detailed progress
    /// </summary>
    public class TemplateInstaller
    {
        private readonly TemplateDefinition _template;
        private readonly string _rootNamespace;
        private readonly InstallationProgress _progress;
        private readonly List<string> _createdFolders = new List<string>();
        private readonly List<string> _createdScenes = new List<string>();

        public TemplateInstaller(TemplateDefinition template, string rootNamespace)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
            _rootNamespace = string.IsNullOrEmpty(rootNamespace) ? "Game" : rootNamespace;
            _progress = new InstallationProgress();
        }

        public InstallationProgress Progress => _progress;

        /// <summary>
        /// Execute the full template installation
        /// </summary>
        public InstallationResult Install(InstallationOptions options)
        {
            try
            {
                // Validate template
                if (!_template.Validate(out string validationError))
                {
                    return InstallationResult.Failure("Template validation failed", validationError);
                }

                // Validate environment
                _progress.UpdateProgress("Validating environment...", 0.05f);
                if (!ValidateEnvironment(out string envError))
                {
                    return InstallationResult.Failure("Environment validation failed", envError);
                }

                // Check for existing installation
                if (DetectExistingInstallation() && !options.OverwriteExisting)
                {
                    return InstallationResult.Failure(
                        "Existing installation detected",
                        "Enable 'Overwrite Existing' option to proceed"
                    );
                }

                // Step 1: Create folder structure
                _progress.UpdateProgress("Creating folder structure...", 0.1f);
                CreateFolderStructure();

                // Step 2: Install dependencies
                if (options.InstallDependencies)
                {
                    _progress.UpdateProgress("Installing dependencies...", 0.2f);
                    InstallDependencies();
                }

                // Step 3: Configure Addressables
                if (options.ConfigureAddressables)
                {
                    _progress.UpdateProgress("Configuring Addressables...", 0.4f);
                    ConfigureAddressables();
                }

                // Step 4: Configure Localization
                if (options.ConfigureLocalization)
                {
                    _progress.UpdateProgress("Configuring Localization...", 0.6f);
                    ConfigureLocalization();
                }

                // Step 5: Create scenes
                _progress.UpdateProgress("Creating scenes...", 0.7f);
                CreateScenes();

                // Step 6: Generate runtime code
                _progress.UpdateProgress("Generating runtime code...", 0.85f);
                GenerateRuntimeCode(options);

                // Step 7: Configure build settings
                _progress.UpdateProgress("Configuring build settings...", 0.95f);
                ConfigureBuildSettings();

                // Complete
                _progress.Complete();
                AssetDatabase.Refresh();

                return InstallationResult.SuccessResult($"Template '{_template.TemplateName}' installed successfully!");
            }
            catch (Exception ex)
            {
                _progress.ReportError(ex.Message);
                return InstallationResult.Failure("Installation failed with exception", ex.ToString());
            }
        }

        private bool ValidateEnvironment(out string error)
        {
            error = string.Empty;

            // Check Unity version
            if (!Application.unityVersion.StartsWith("2022") && !Application.unityVersion.StartsWith("2023"))
            {
                error = $"Unity 2022.3 LTS or newer required. Current: {Application.unityVersion}";
                return false;
            }

            // Check if project is in play mode
            if (EditorApplication.isPlaying)
            {
                error = "Cannot install template while in Play Mode";
                return false;
            }

            // Check write permissions
            string assetsPath = Application.dataPath;
            if (!Directory.Exists(assetsPath))
            {
                error = "Assets directory not accessible";
                return false;
            }

            return true;
        }

        private bool DetectExistingInstallation()
        {
            // Check if _Project folder exists
            string projectRoot = Path.Combine(Application.dataPath, "_Project");
            return Directory.Exists(projectRoot);
        }

        private void CreateFolderStructure()
        {
            foreach (var structure in _template.FolderStructures)
            {
                CreateFolderHierarchy(structure);
            }
        }

        private void CreateFolderHierarchy(FolderStructure structure)
        {
            string basePath = Path.Combine(Application.dataPath, structure.RootFolder);

            // Create root if it doesn't exist
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                _createdFolders.Add(structure.RootFolder);
            }

            // Create all subfolders
            foreach (var subfolder in structure.Subfolders)
            {
                CreateNestedFolders(basePath, subfolder);
            }
        }

        private void CreateNestedFolders(string basePath, string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string currentPath = basePath;

            foreach (var part in parts)
            {
                currentPath = Path.Combine(currentPath, part);
                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                    string relativePath = currentPath.Replace(Application.dataPath, "Assets");
                    _createdFolders.Add(relativePath);
                }
            }
        }

        private void InstallDependencies()
        {
            foreach (var package in _template.RequiredPackages)
            {
                // Check if package is already installed
                if (!PackageValidator.IsPackageInstalled(package))
                {
                    Debug.Log($"[TemplateInstaller] Installing package: {package}");
                    PackageInstallationHelper.InstallPackage(package);
                }
            }
        }

        private void ConfigureAddressables()
        {
            try
            {
                var configurator = new AddressablesConfigurator();
                foreach (var groupDef in _template.AddressableGroups)
                {
                    configurator.CreateGroup(groupDef);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TemplateInstaller] Failed to configure Addressables: {ex.Message}");
            }
        }

        private void ConfigureLocalization()
        {
            try
            {
                var configurator = new LocalizationConfigurator();
                foreach (var tableDef in _template.LocalizationTables)
                {
                    configurator.CreateTable(tableDef);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[TemplateInstaller] Failed to configure Localization: {ex.Message}");
            }
        }

        private void CreateScenes()
        {
            var sceneGenerator = new SceneGenerator(_template.TemplateType, _rootNamespace);
            foreach (var sceneDef in _template.Scenes)
            {
                string scenePath = sceneGenerator.CreateScene(sceneDef);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    _createdScenes.Add(scenePath);
                }
            }
        }

        private void GenerateRuntimeCode(InstallationOptions options)
        {
            var codeGenerator = new RuntimeCodeGenerator(_template, _rootNamespace);

            if (_template.IncludeContentService)
            {
                codeGenerator.GenerateContentService();
            }

            if (_template.IncludeLocalizationService)
            {
                codeGenerator.GenerateLocalizationService();
            }

            if (_template.IncludeStateMachine)
            {
                codeGenerator.GenerateStateMachine();
            }

            if (_template.IncludeBootstrapInstaller)
            {
                codeGenerator.GenerateBootstrapInstaller();
            }

            if (options.GenerateSampleContent)
            {
                codeGenerator.GenerateSampleContent();
            }
        }

        private void ConfigureBuildSettings()
        {
            var buildConfigurator = new BuildSettingsConfigurator();
            buildConfigurator.ConfigureScenes(_createdScenes);
        }
    }

    /// <summary>
    /// Options for template installation
    /// </summary>
    public class InstallationOptions
    {
        public bool InstallDependencies { get; set; } = true;
        public bool ConfigureAddressables { get; set; } = true;
        public bool ConfigureLocalization { get; set; } = true;
        public bool GenerateSampleContent { get; set; } = false;
        public bool OverwriteExisting { get; set; } = false;
        public bool RemoveInstallerAfterSetup { get; set; } = false;
    }
}
#endif
