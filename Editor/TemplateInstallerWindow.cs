#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using IKhom.TemplateInstaller;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Main installer window UI
    /// </summary>
    public class TemplateInstallerWindow : EditorWindow
    {
        private TemplateType _selectedTemplate = TemplateType.SingleScene;
        private string _rootNamespace = "Game";
        private bool _includeAddressables = true;
        private bool _includeLocalization = true;
        private bool _includeRemoteConfig = false;
        private bool _generateSampleContent = true;
        private bool _removeInstallerAfterSetup = false;
        private Vector2 _scrollPosition;

        private TemplateDefinition _singleSceneTemplate;
        private TemplateDefinition _modularTemplate;
        private TemplateDefinition _cleanArchitectureTemplate;

        [MenuItem("Tools/Project Template Installer")]
        public static void ShowWindow()
        {
            var window = GetWindow<TemplateInstallerWindow>("Template Installer");
            window.minSize = new Vector2(600, 700);
            window.Show();
        }

        private void OnEnable()
        {
            // Load or create template definitions
            LoadTemplateDefinitions();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(20);

            DrawTemplateSelection();
            EditorGUILayout.Space(20);

            DrawConfiguration();
            EditorGUILayout.Space(20);

            DrawOptions();
            EditorGUILayout.Space(20);

            DrawPreview();
            EditorGUILayout.Space(20);

            DrawInstallButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUILayout.Label("Unity Project Template Installer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Choose an architecture template to generate a production-ready Unity project structure " +
                "with Addressables, Localization, and VContainer integration.",
                MessageType.Info
            );
        }

        private void DrawTemplateSelection()
        {
            GUILayout.Label("Select Template", EditorStyles.boldLabel);

            DrawTemplateOption(
                TemplateType.SingleScene,
                "Single-Scene Prototype",
                "For hypercasual games and rapid prototyping (1-3 weeks)\n" +
                "• One scene architecture\n" +
                "• Simple state machine\n" +
                "• Local Addressables only\n" +
                "• Team: 1-2 developers"
            );

            DrawTemplateOption(
                TemplateType.Modular,
                "Multi-Scene Modular",
                "For hybrid-casual games with LiveOps (3-12 months)\n" +
                "• Four-scene architecture (Bootstrap/Persistent/Shell/Gameplay)\n" +
                "• Feature-based modules\n" +
                "• Local + Remote Addressables\n" +
                "• RemoteConfig integration\n" +
                "• Team: 3-5 developers"
            );

            DrawTemplateOption(
                TemplateType.CleanArchitecture,
                "Clean Architecture",
                "For midcore games with complex business logic (1+ years)\n" +
                "• Domain/Application/Infrastructure separation\n" +
                "• UseCase pattern\n" +
                "• Repository abstraction\n" +
                "• EventBus system\n" +
                "• Full testability\n" +
                "• Team: 5-10+ developers"
            );
        }

        private void DrawTemplateOption(TemplateType type, string title, string description)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bool isSelected = _selectedTemplate == type;
            Color originalColor = GUI.backgroundColor;

            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            }

            if (GUILayout.Button(title, GUILayout.Height(30)))
            {
                _selectedTemplate = type;

                // Auto-enable RemoteConfig for Modular and Clean templates
                if (type == TemplateType.Modular || type == TemplateType.CleanArchitecture)
                {
                    _includeRemoteConfig = true;
                }
            }

            GUI.backgroundColor = originalColor;

            if (isSelected)
            {
                EditorGUILayout.HelpBox(description, MessageType.None);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawConfiguration()
        {
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _rootNamespace = EditorGUILayout.TextField("Root Namespace", _rootNamespace);

            EditorGUILayout.Space(5);

            GUI.enabled = false; // Always enabled
            _includeAddressables = EditorGUILayout.Toggle("Include Addressables", _includeAddressables);
            _includeLocalization = EditorGUILayout.Toggle("Include Localization", _includeLocalization);
            GUI.enabled = true;

            EditorGUILayout.HelpBox(
                "Addressables and Localization are always included. This is the production-ready way.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
        }

        private void DrawOptions()
        {
            GUILayout.Label("Options", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // RemoteConfig only for Modular/Clean
            GUI.enabled = _selectedTemplate == TemplateType.Modular || _selectedTemplate == TemplateType.CleanArchitecture;
            _includeRemoteConfig = EditorGUILayout.Toggle("Include RemoteConfig", _includeRemoteConfig);
            GUI.enabled = true;

            _generateSampleContent = EditorGUILayout.Toggle("Generate Sample Levels", _generateSampleContent);
            _removeInstallerAfterSetup = EditorGUILayout.Toggle("Remove Installer After Setup", _removeInstallerAfterSetup);

            if (_removeInstallerAfterSetup)
            {
                EditorGUILayout.HelpBox(
                    "The installer package will be removed from your project after successful installation.",
                    MessageType.Warning
                );
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreview()
        {
            GUILayout.Label("What Will Be Created", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Folder Structure:", EditorStyles.miniBoldLabel);
            GUILayout.Label(GetFolderPreview(), EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(5);

            GUILayout.Label("Addressables Groups:", EditorStyles.miniBoldLabel);
            GUILayout.Label(GetAddressablesPreview(), EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(5);

            GUILayout.Label("Localization Tables:", EditorStyles.miniBoldLabel);
            GUILayout.Label("• UI (English, Russian)\n• Gameplay (English, Russian)", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawInstallButton()
        {
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);

            if (GUILayout.Button("Install Template", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog(
                    "Install Template",
                    $"This will create a {GetTemplateName()} project structure.\n\n" +
                    "Existing files will not be overwritten.\n\n" +
                    "Continue?",
                    "Install",
                    "Cancel"))
                {
                    InstallTemplate();
                }
            }

            GUI.backgroundColor = Color.white;
        }

        private void InstallTemplate()
        {
            var template = GetSelectedTemplateDefinition();
            if (template == null)
            {
                EditorUtility.DisplayDialog("Error", "Template definition not found", "OK");
                return;
            }

            var options = new InstallationOptions
            {
                InstallDependencies = true,
                ConfigureAddressables = _includeAddressables,
                ConfigureLocalization = _includeLocalization,
                GenerateSampleContent = _generateSampleContent,
                OverwriteExisting = false,
                RemoveInstallerAfterSetup = _removeInstallerAfterSetup
            };

            var installer = new TemplateInstaller(template, _rootNamespace);

            // Show progress
            EditorUtility.DisplayProgressBar("Installing Template", "Starting installation...", 0f);

            installer.Progress.OnProgressChanged += (progress) =>
            {
                EditorUtility.DisplayProgressBar(
                    "Installing Template",
                    progress.CurrentStep,
                    progress.Progress
                );
            };

            var result = installer.Install(options);

            EditorUtility.ClearProgressBar();

            if (result.Success)
            {
                EditorUtility.DisplayDialog(
                    "Installation Complete",
                    result.Message + "\n\nPlease allow Unity to compile the new scripts.",
                    "OK"
                );

                if (_removeInstallerAfterSetup)
                {
                    RemoveInstallerPackage();
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Installation Failed",
                    result.Message + "\n\n" + result.ErrorDetails,
                    "OK"
                );
            }
        }

        private void LoadTemplateDefinitions()
        {
            // Create templates using the factory
            _singleSceneTemplate = TemplateDefinitionFactory.CreateSingleSceneTemplate();
            _modularTemplate = TemplateDefinitionFactory.CreateModularTemplate();
            _cleanArchitectureTemplate = TemplateDefinitionFactory.CreateCleanArchitectureTemplate();
        }

        private TemplateDefinition GetSelectedTemplateDefinition()
        {
            switch (_selectedTemplate)
            {
                case TemplateType.SingleScene:
                    return _singleSceneTemplate;
                case TemplateType.Modular:
                    return _modularTemplate;
                case TemplateType.CleanArchitecture:
                    return _cleanArchitectureTemplate;
                default:
                    return null;
            }
        }

        private string GetTemplateName()
        {
            switch (_selectedTemplate)
            {
                case TemplateType.SingleScene:
                    return "Single-Scene Prototype";
                case TemplateType.Modular:
                    return "Multi-Scene Modular";
                case TemplateType.CleanArchitecture:
                    return "Clean Architecture";
                default:
                    return "Unknown";
            }
        }

        private string GetFolderPreview()
        {
            switch (_selectedTemplate)
            {
                case TemplateType.SingleScene:
                    return "_Project/\n  ├── Bootstrap/\n  ├── Core/\n  │   ├── Content/\n  │   ├── Localization/\n  │   └── StateMachine/\n  └── Scenes/";
                case TemplateType.Modular:
                    return "_Project/\n  ├── 00_Bootstrap/\n  ├── 01_Core/\n  ├── 02_Features/\n  ├── 03_Content/\n  └── 04_SDK/";
                case TemplateType.CleanArchitecture:
                    return "_Project/\n  ├── 00_Bootstrap/\n  ├── 01_Core/\n  │   ├── Domain/\n  │   ├── Application/\n  │   ├── Infrastructure/\n  │   └── Presentation/\n  └── 02_Features/";
                default:
                    return "";
            }
        }

        private string GetAddressablesPreview()
        {
            switch (_selectedTemplate)
            {
                case TemplateType.SingleScene:
                    return "• 00_Static (Local)\n• 01_Localization (Local)\n• 02_Levels_Local (Local)\n• 04_UI (Local)";
                case TemplateType.Modular:
                    return "• 00_Static (Local)\n• 01_Localization (Local)\n• 02_Levels_Local (Local)\n• 03_Levels_Remote (Remote)\n• 04_UI (Local)";
                case TemplateType.CleanArchitecture:
                    return "• 00_Static (Local)\n• 01_Localization (Local)\n• 02_Content_Local (Local)\n• 03_Content_Remote (Remote)\n• 04_UI (Local)\n• 05_Features (Remote)";
                default:
                    return "";
            }
        }

        private void RemoveInstallerPackage()
        {
            if (EditorUtility.DisplayDialog(
                "Remove Installer",
                "Remove the Template Installer package from your project?",
                "Remove",
                "Keep"))
            {
                PackageSelfRemover.RemovePackage();
            }
        }


    }
}
#endif
