#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Tutorial window that opens after template installation
    /// Provides detailed, actionable instructions for working with the installed template
    /// </summary>
    public class TemplateTutorialWindow : EditorWindow
    {
        private TemplateType _templateType;
        private Vector2 _scrollPosition;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionHeaderStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _codeStyle;
        private GUIStyle _stepStyle;
        private bool _stylesInitialized;

        private static readonly Dictionary<TemplateType, TemplateTutorialData> _tutorials = new Dictionary<TemplateType, TemplateTutorialData>();

        [MenuItem("Tools/Template Installer/Show Tutorial")]
        public static void ShowTutorialMenu()
        {
            var window = GetWindow<TemplateTutorialWindow>("Template Tutorial");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        public static void ShowTutorial(TemplateType templateType)
        {
            var window = GetWindow<TemplateTutorialWindow>("Template Tutorial");
            window.minSize = new Vector2(800, 600);
            window._templateType = templateType;
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            InitializeTutorials();
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(10, 10, 10, 10)
            };

            _sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(5, 5, 8, 8)
            };

            _bodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 12,
                margin = new RectOffset(5, 5, 3, 3),
                richText = true
            };

            _codeStyle = new GUIStyle(EditorStyles.textArea)
            {
                fontSize = 11,
                wordWrap = false,
                margin = new RectOffset(10, 10, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };

            _stepStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(10, 10, 10, 10),
                richText = true
            };

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            if (_tutorials.ContainsKey(_templateType))
            {
                DrawTutorial(_tutorials[_templateType]);
            }
            else
            {
                EditorGUILayout.HelpBox("No tutorial available for this template type.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string templateName = GetTemplateName(_templateType);
            GUILayout.Label($"📚 {templateName} - Quick Start Guide", _headerStyle);

            EditorGUILayout.HelpBox(
                "This guide will help you understand how to work with your newly installed template. " +
                "Follow the steps below to start building your game.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
        }

        private void DrawTutorial(TemplateTutorialData tutorial)
        {
            // Overview
            DrawSection("📋 Overview", tutorial.Overview);

            // What Was Created
            DrawSection("✅ What Was Created", tutorial.WhatWasCreated);

            // Quick Start Steps
            DrawQuickStartSteps(tutorial.QuickStartSteps);

            // How to Work With
            DrawHowToWorkWith(tutorial.HowToWorkWith);

            // Common Tasks
            DrawCommonTasks(tutorial.CommonTasks);

            // Next Steps
            DrawSection("🚀 Next Steps", tutorial.NextSteps);

            // Additional Resources
            DrawAdditionalResources(tutorial.AdditionalResources);
        }

        private void DrawSection(string title, string content)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label(title, _sectionHeaderStyle);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(content, _bodyStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawQuickStartSteps(List<TutorialStep> steps)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("🎯 Quick Start Steps", _sectionHeaderStyle);

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                EditorGUILayout.BeginVertical(_stepStyle);

                GUILayout.Label($"<b>Step {i + 1}: {step.Title}</b>", _bodyStyle);
                GUILayout.Label(step.Description, _bodyStyle);

                if (!string.IsNullOrEmpty(step.CodeExample))
                {
                    EditorGUILayout.Space(5);
                    GUILayout.Label("Code Example:", EditorStyles.miniLabel);
                    EditorGUILayout.TextArea(step.CodeExample, _codeStyle, GUILayout.Height(step.CodeHeight));
                }

                if (!string.IsNullOrEmpty(step.Tip))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox($"💡 Tip: {step.Tip}", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawHowToWorkWith(List<WorkflowGuide> guides)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("🛠 How to Work With This Template", _sectionHeaderStyle);

            foreach (var guide in guides)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label($"<b>{guide.Title}</b>", _bodyStyle);
                GUILayout.Label(guide.Description, _bodyStyle);

                if (guide.Steps != null && guide.Steps.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    foreach (var step in guide.Steps)
                    {
                        GUILayout.Label($"  • {step}", _bodyStyle);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawCommonTasks(List<CommonTask> tasks)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("📝 Common Tasks", _sectionHeaderStyle);

            foreach (var task in tasks)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label($"<b>{task.TaskName}</b>", _bodyStyle);

                foreach (var step in task.Steps)
                {
                    GUILayout.Label($"  {step}", _bodyStyle);
                }

                if (!string.IsNullOrEmpty(task.Example))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.TextArea(task.Example, _codeStyle, GUILayout.Height(task.ExampleHeight));
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawAdditionalResources(List<string> resources)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("📚 Additional Resources", _sectionHeaderStyle);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var resource in resources)
            {
                GUILayout.Label($"  • {resource}", _bodyStyle);
            }
            EditorGUILayout.EndVertical();
        }

        private string GetTemplateName(TemplateType type)
        {
            switch (type)
            {
                case TemplateType.SingleScene:
                    return "Single-Scene Prototype";
                case TemplateType.Modular:
                    return "Multi-Scene Modular";
                case TemplateType.CleanArchitecture:
                    return "Clean Architecture";
                default:
                    return "Unknown Template";
            }
        }

        private void InitializeTutorials()
        {
            if (_tutorials.Count > 0) return;

            _tutorials[TemplateType.SingleScene] = CreateSingleSceneTutorial();
            _tutorials[TemplateType.Modular] = CreateModularTutorial();
            _tutorials[TemplateType.CleanArchitecture] = CreateCleanArchitectureTutorial();
        }

        private TemplateTutorialData CreateSingleSceneTutorial()
        {
            return new TemplateTutorialData
            {
                Overview = "The Single-Scene Prototype template is designed for rapid prototyping and hypercasual games. " +
                          "It uses a single scene architecture with Addressables and Localization built-in from day one, " +
                          "making it production-ready and scalable.",

                WhatWasCreated = "✓ Folder structure: _Project/Bootstrap, Core, Game, Scenes, Content\n" +
                                "✓ Scene: Game.unity with SceneContext\n" +
                                "✓ Addressable Groups: Static, Localization, Levels_Local, UI\n" +
                                "✓ Localization Tables: UI and Gameplay (English, Russian)\n" +
                                "✓ Core services: ContentService, LocalizationService, StateMachine",

                QuickStartSteps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        Title = "Open the Game Scene",
                        Description = "Navigate to _Project/Scenes/Game.unity and open it. This is your main scene.",
                        Tip = "This scene contains the BootstrapInstaller which sets up all your services."
                    },
                    new TutorialStep
                    {
                        Title = "Create Your First Level",
                        Description = "Create a level prefab in _Project/Game/Levels/Prefabs/",
                        CodeExample = "1. Right-click in Project → Create → Prefab\n" +
                                     "2. Name it 'Level_1'\n" +
                                     "3. Add your gameplay objects to the prefab\n" +
                                     "4. Mark as Addressable (Inspector → Addressable checkbox)\n" +
                                     "5. Set Address: 'Level_1'\n" +
                                     "6. Set Group: 02_Levels_Local",
                        CodeHeight = 120,
                        Tip = "Use the naming convention 'Level_X' for automatic loading by LevelLoader."
                    },
                    new TutorialStep
                    {
                        Title = "Create a UI Screen",
                        Description = "Create UI screens in _Project/Game/UI/Screens/",
                        CodeExample = "1. Create Canvas prefab (UI → Canvas)\n" +
                                     "2. Name it 'MainMenuScreen'\n" +
                                     "3. Add UI elements (buttons, text, etc.)\n" +
                                     "4. Mark as Addressable\n" +
                                     "5. Set Address: 'UI_MainMenu'\n" +
                                     "6. Set Group: 04_UI",
                        CodeHeight = 120,
                        Tip = "Use LocalizedText component on TextMeshPro elements for automatic translation."
                    },
                    new TutorialStep
                    {
                        Title = "Add Localized Text",
                        Description = "Add localization to your UI text elements",
                        CodeExample = "1. Select TextMeshProUGUI component\n" +
                                     "2. Add LocalizedText component\n" +
                                     "3. Set Table Name: 'UI'\n" +
                                     "4. Set Key: 'your_text_key'\n" +
                                     "5. Add the key to Localization Tables (Window → Asset Management → Localization Tables)",
                        CodeHeight = 100,
                        Tip = "Text will automatically update when language changes."
                    },
                    new TutorialStep
                    {
                        Title = "Test Your Game",
                        Description = "Press Play to test the bootstrap flow",
                        CodeExample = "Expected flow:\n" +
                                     "1. Game.unity loads\n" +
                                     "2. BootstrapInstaller initializes services\n" +
                                     "3. ContentBootstrap loads localization\n" +
                                     "4. GameEntryPoint starts state machine\n" +
                                     "5. LoadingState loads your level",
                        CodeHeight = 100,
                        Tip = "Check the Console for '[ContentBootstrap] Ready' and '[GameEntryPoint] Starting' messages."
                    }
                },

                HowToWorkWith = new List<WorkflowGuide>
                {
                    new WorkflowGuide
                    {
                        Title = "Loading Content via Addressables",
                        Description = "All content should be loaded through ContentService, never use Resources.",
                        Steps = new List<string>
                        {
                            "Inject ContentService via VContainer",
                            "Use await contentService.LoadAsync<T>(\"key\") to load assets",
                            "Use await contentService.InstantiateAsync(\"key\") for prefabs",
                            "Always mark assets as Addressable and assign proper groups"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Adding New States",
                        Description = "Extend the state machine for new game flows",
                        Steps = new List<string>
                        {
                            "Create new state class implementing IState",
                            "Register in BootstrapInstaller: builder.Register<YourState>(Lifetime.Transient)",
                            "Inject dependencies (ContentService, UIManager, etc.)",
                            "Transition: _stateMachine.Enter<YourState>()"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Managing Localization",
                        Description = "Add and manage translations for your game",
                        Steps = new List<string>
                        {
                            "Open: Window → Asset Management → Localization Tables",
                            "Select your table (UI or Gameplay)",
                            "Add new entry with key and translations",
                            "Use LocalizationService.GetString(\"TableName\", \"key\") in code",
                            "Or use LocalizedText component on UI elements"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Organizing Content",
                        Description = "Keep your project organized as it grows",
                        Steps = new List<string>
                        {
                            "Levels → _Project/Game/Levels/Prefabs/ (Addressable: 02_Levels_Local)",
                            "UI Screens → _Project/Game/UI/Screens/ (Addressable: 04_UI)",
                            "Configs → _Project/Content/Configs/ (ScriptableObjects)",
                            "Shared prefabs → _Project/Content/ (Addressable: 00_Static)"
                        }
                    }
                },

                CommonTasks = new List<CommonTask>
                {
                    new CommonTask
                    {
                        TaskName = "How to: Load a Level",
                        Steps = new List<string>
                        {
                            "1. Create level prefab and mark as Addressable with key 'Level_X'",
                            "2. In your state, inject LevelLoader",
                            "3. Call: var level = await _levelLoader.LoadCurrentLevelAsync()",
                            "4. Level will be instantiated in the scene"
                        },
                        Example = "public class LoadingState : IState\n{\n    private readonly LevelLoader _levelLoader;\n    \n    public async void Enter()\n    {\n        var level = await _levelLoader.LoadCurrentLevelAsync();\n        // Level is now in the scene\n    }\n}",
                        ExampleHeight = 120
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Show a UI Screen",
                        Steps = new List<string>
                        {
                            "1. Create UI prefab and mark as Addressable with key 'UI_ScreenName'",
                            "2. Inject UIManager into your state/service",
                            "3. Call: var screen = await _uiManager.ShowScreenAsync<T>(\"UI_ScreenName\")",
                            "4. Screen will be instantiated under UIRoot"
                        },
                        Example = "public class MenuState : IState\n{\n    private readonly UIManager _uiManager;\n    \n    public async void Enter()\n    {\n        var menu = await _uiManager.ShowScreenAsync<MainMenuScreen>(\"UI_MainMenu\");\n        menu.OnPlayClicked += OnPlayClicked;\n    }\n}",
                        ExampleHeight = 140
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Add a New Service",
                        Steps = new List<string>
                        {
                            "1. Create your service class in _Project/Core/Services/",
                            "2. Add dependencies via constructor injection",
                            "3. Register in BootstrapInstaller:",
                            "   builder.Register<YourService>(Lifetime.Singleton)",
                            "4. Inject into other classes that need it"
                        },
                        Example = "public class AudioService\n{\n    private readonly ContentService _content;\n    \n    public AudioService(ContentService content)\n    {\n        _content = content;\n    }\n    \n    public async void PlaySound(string soundKey)\n    {\n        var clip = await _content.LoadAsync<AudioClip>(soundKey);\n        // Play the clip\n    }\n}",
                        ExampleHeight = 160
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Add Localized Text to UI",
                        Steps = new List<string>
                        {
                            "1. Add TextMeshProUGUI to your UI",
                            "2. Add LocalizedText component",
                            "3. Set Table Name (e.g., 'UI') and Key (e.g., 'play_button')",
                            "4. Add the key to Localization Tables with translations",
                            "5. Text will auto-update when language changes"
                        },
                        Example = "// In code (alternative to component):\npublic class MenuScreen : MonoBehaviour\n{\n    [SerializeField] private TextMeshProUGUI _titleText;\n    private LocalizationService _localization;\n    \n    public void Initialize(LocalizationService localization)\n    {\n        _localization = localization;\n        _titleText.text = _localization.GetString(\"UI\", \"main_menu_title\");\n    }\n}",
                        ExampleHeight = 160
                    }
                },

                NextSteps = "• Create more levels and mark them as Addressable\n" +
                           "• Add game-specific services (Audio, Analytics, Ads)\n" +
                           "• Implement gameplay states (Gameplay, Results, Pause)\n" +
                           "• Add more UI screens for your game flow\n" +
                           "• Test localization by switching languages\n" +
                           "• When ready for remote content, create 03_Levels_Remote group\n" +
                           "• Build Addressables: Window → Asset Management → Addressables → Build → Build Player Content",

                AdditionalResources = new List<string>
                {
                    "Architecture Guide: Assets/TempalateInstaller/unity-arch-single-scene.md",
                    "VContainer Documentation: https://vcontainer.hadashikick.jp/",
                    "Addressables Documentation: https://docs.unity3d.com/Packages/com.unity.addressables@latest",
                    "Localization Documentation: https://docs.unity3d.com/Packages/com.unity.localization@latest",
                    "UniTask Documentation: https://github.com/Cysharp/UniTask"
                }
            };
        }

        private TemplateTutorialData CreateModularTutorial()
        {
            return new TemplateTutorialData
            {
                Overview = "The Multi-Scene Modular template is designed for hybrid-casual games with LiveOps. " +
                          "It uses a four-scene architecture (Bootstrap/Persistent/Shell/Gameplay) with feature-based modules, " +
                          "local + remote Addressables, and RemoteConfig integration.",

                WhatWasCreated = "✓ Folder structure: 00_Bootstrap, 01_Core, 02_Features, 03_Content, 04_SDK, 05_LiveOps\n" +
                                "✓ Scenes: Bootstrap.unity, Persistent.unity, Shell.unity, Gameplay.unity\n" +
                                "✓ Addressable Groups: Static, Localization, Levels_Local, Levels_Remote, UI, Audio\n" +
                                "✓ Localization Tables: UI, Shop, Gameplay, UI_Assets (English, Russian, Chinese)\n" +
                                "✓ Services: ContentService, LocalizationService, RemoteConfigService, FeatureFlagService",

                QuickStartSteps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        Title = "Understand the Scene Flow",
                        Description = "The template uses 4 scenes that load in sequence",
                        CodeExample = "Bootstrap.unity → Initializes core services\n" +
                                     "  ↓\n" +
                                     "Persistent.unity → Loads additively, never unloads (Audio, Analytics)\n" +
                                     "  ↓\n" +
                                     "Shell.unity → Meta UI (main menu, shop, progression)\n" +
                                     "  ↓\n" +
                                     "Gameplay.unity → Loads when playing a level, unloads when returning to Shell",
                        CodeHeight = 120,
                        Tip = "Bootstrap unloads after initialization. Persistent stays loaded throughout the game."
                    },
                    new TutorialStep
                    {
                        Title = "Configure Remote Addressables",
                        Description = "Set up remote content delivery for levels 11+",
                        CodeExample = "1. Open: Window → Asset Management → Addressables → Groups\n" +
                                     "2. Select group: 03_Levels_Remote\n" +
                                     "3. Set Build Path: ServerData/[BuildTarget]\n" +
                                     "4. Set Load Path: https://your-cdn.com/[BuildTarget]\n" +
                                     "5. Move Level_11+ prefabs to this group",
                        CodeHeight = 100,
                        Tip = "Levels 1-10 stay local, 11+ are remote and downloaded on-demand."
                    },
                    new TutorialStep
                    {
                        Title = "Create Feature Modules",
                        Description = "Organize code into feature-based modules",
                        CodeExample = "Example: Shop Feature\n" +
                                     "1. Create folder: _Project/02_Features/Shop/\n" +
                                     "2. Add ShopFeature.cs (business logic)\n" +
                                     "3. Add ShopInstaller.cs (DI registration)\n" +
                                     "4. Register in PersistentInstaller:\n" +
                                     "   builder.Register<ShopFeature>(Lifetime.Singleton)",
                        CodeHeight = 120,
                        Tip = "Each feature is self-contained with its own installer."
                    },
                    new TutorialStep
                    {
                        Title = "Setup RemoteConfig",
                        Description = "Configure remote configuration for A/B testing and balance",
                        CodeExample = "1. Create RemoteConfig keys in your backend/Unity Gaming Services\n" +
                                     "2. Fetch in BootstrapState:\n" +
                                     "   await _remoteConfig.FetchAsync();\n" +
                                     "3. Use values:\n" +
                                     "   int timer = _remoteConfig.GetValue<int>(\"level_timer\", 60);\n" +
                                     "   bool shopEnabled = _remoteConfig.GetValue<bool>(\"feature_shop\", true);",
                        CodeHeight = 120,
                        Tip = "RemoteConfig allows changing game balance without app updates."
                    },
                    new TutorialStep
                    {
                        Title = "Test the Full Flow",
                        Description = "Play from Bootstrap scene to test the complete flow",
                        CodeExample = "Expected flow:\n" +
                                     "1. Bootstrap → Initialize services, fetch RemoteConfig\n" +
                                     "2. Persistent → Load additively, initialize features\n" +
                                     "3. Shell → Show main menu\n" +
                                     "4. User clicks Play → Load level (download if remote)\n" +
                                     "5. Gameplay → Play level\n" +
                                     "6. Results → Return to Shell",
                        CodeHeight = 120,
                        Tip = "Always start from Bootstrap.unity when testing."
                    }
                },

                HowToWorkWith = new List<WorkflowGuide>
                {
                    new WorkflowGuide
                    {
                        Title = "Working with Remote Content",
                        Description = "Load content with automatic download handling",
                        Steps = new List<string>
                        {
                            "Check download size: await _content.GetDownloadSizeAsync(key)",
                            "Show download prompt to user if size > 0",
                            "Download with progress: await _content.DownloadAsync(key, progress)",
                            "Load normally: await _content.LoadAsync<T>(key)",
                            "Content is cached after first download"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Creating Feature Modules",
                        Description = "Organize features as self-contained modules",
                        Steps = new List<string>
                        {
                            "Create folder: _Project/02_Features/YourFeature/",
                            "Add YourFeature.cs with IFeature interface",
                            "Add YourFeatureInstaller.cs for DI registration",
                            "Implement Initialize() method",
                            "Register in PersistentInstaller or feature-specific scope"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Using Feature Flags",
                        Description = "Enable/disable features remotely",
                        Steps = new List<string>
                        {
                            "Define flag in RemoteConfig: \"feature_shop\": true",
                            "Check in code: bool enabled = _featureFlags.IsEnabled(\"shop\")",
                            "Show/hide UI based on flag",
                            "Toggle features without app updates"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Scene Management",
                        Description = "Load and unload scenes properly",
                        Steps = new List<string>
                        {
                            "Use SceneLoader service, never SceneManager directly",
                            "Load additive: await _sceneLoader.LoadAdditiveAsync(\"SceneName\")",
                            "Unload: await _sceneLoader.UnloadAsync(\"SceneName\")",
                            "Bootstrap unloads after Persistent loads",
                            "Shell unloads when Gameplay loads, reloads when returning"
                        }
                    }
                },

                CommonTasks = new List<CommonTask>
                {
                    new CommonTask
                    {
                        TaskName = "How to: Load Remote Level with Download UI",
                        Steps = new List<string>
                        {
                            "1. Check if level is remote (level > 10)",
                            "2. Get download size",
                            "3. Show download prompt if needed",
                            "4. Download with progress bar",
                            "5. Load level normally"
                        },
                        Example = "public async UniTask<GameObject> LoadLevelAsync(int levelIndex)\n{\n    string key = $\"Level_{levelIndex}\";\n    \n    // Check download size\n    var size = await _content.GetDownloadSizeAsync(key);\n    \n    if (size > 0)\n    {\n        // Show download prompt\n        bool confirmed = await ShowDownloadPrompt(size);\n        if (!confirmed) return null;\n        \n        // Download with progress\n        await _content.DownloadAsync(key, new Progress<float>(OnProgress));\n    }\n    \n    // Load level\n    return await _content.InstantiateAsync(key);\n}",
                        ExampleHeight = 200
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Create a Shop Feature",
                        Steps = new List<string>
                        {
                            "1. Create ShopFeature.cs in _Project/02_Features/Shop/",
                            "2. Load shop config from RemoteConfig",
                            "3. Create shop items with localized names/icons",
                            "4. Handle purchases with IAP integration",
                            "5. Register in PersistentInstaller"
                        },
                        Example = "public class ShopFeature : IFeature\n{\n    private readonly RemoteConfigService _remoteConfig;\n    private readonly LocalizationService _localization;\n    private readonly ContentService _content;\n    \n    public void Initialize()\n    {\n        var itemsJson = _remoteConfig.GetValue<string>(\"shop_items\", \"[]\");\n        _items = JsonConvert.DeserializeObject<List<ShopItem>>(itemsJson);\n    }\n    \n    public async UniTask<ShopItemView> CreateItemViewAsync(ShopItem item)\n    {\n        var view = await _content.InstantiateAsync(\"UI_ShopItem\");\n        var name = _localization.GetString(\"Shop\", item.NameKey);\n        var icon = await _localization.GetAssetAsync<Sprite>(\"Shop_Assets\", item.IconKey);\n        view.Initialize(item, name, icon);\n        return view;\n    }\n}",
                        ExampleHeight = 220
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Use Feature Flags for A/B Testing",
                        Steps = new List<string>
                        {
                            "1. Define feature flag in RemoteConfig",
                            "2. Fetch RemoteConfig in BootstrapState",
                            "3. Check flag before showing feature",
                            "4. Toggle remotely for A/B testing"
                        },
                        Example = "// In RemoteConfig:\n// \"feature_daily_reward\": true\n\npublic class ShellState : IState\n{\n    private readonly FeatureFlagService _features;\n    private readonly UIManager _ui;\n    \n    public async void Enter()\n    {\n        var menu = await _ui.ShowScreenAsync<MainMenuScreen>(\"UI_MainMenu\");\n        \n        // Show daily reward button only if enabled\n        bool dailyRewardEnabled = _features.IsEnabled(\"daily_reward\");\n        menu.SetDailyRewardButtonVisible(dailyRewardEnabled);\n    }\n}",
                        ExampleHeight = 180
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Build and Deploy Remote Content",
                        Steps = new List<string>
                        {
                            "1. Window → Asset Management → Addressables → Build → Build Player Content",
                            "2. Find ServerData folder in project root",
                            "3. Upload ServerData to your CDN",
                            "4. Update Load Path in Addressables Groups to match CDN URL",
                            "5. Test catalog updates in game"
                        },
                        Example = "# Upload to CDN (example with AWS S3)\naws s3 sync ./ServerData s3://yourgame-cdn/ --acl public-read\n\n# Or use your CDN provider's CLI/dashboard\n# Make sure Load Path in Addressables matches:\n# https://yourgame-cdn.s3.amazonaws.com/[BuildTarget]",
                        ExampleHeight = 120
                    }
                },

                NextSteps = "• Configure RemoteConfig in Unity Gaming Services or your backend\n" +
                           "• Create feature modules (Shop, Progression, Events)\n" +
                           "• Set up remote Addressables groups and CDN\n" +
                           "• Build and upload remote content\n" +
                           "• Test catalog updates and remote downloads\n" +
                           "• Implement analytics tracking for features\n" +
                           "• Add A/B testing with feature flags\n" +
                           "• Create localized content for all supported languages",

                AdditionalResources = new List<string>
                {
                    "Architecture Guide: Assets/TempalateInstaller/unity-arch-modular.md",
                    "LiveOps Guide: Assets/TempalateInstaller/unity-arch-liveops-2.md",
                    "VContainer Documentation: https://vcontainer.hadashikick.jp/",
                    "Addressables Remote Content: https://docs.unity3d.com/Packages/com.unity.addressables@latest/manual/RemoteContentDistribution.html",
                    "Unity Gaming Services: https://unity.com/solutions/gaming-services",
                    "RemoteConfig Documentation: https://docs.unity.com/remote-config/"
                }
            };
        }

        private TemplateTutorialData CreateCleanArchitectureTutorial()
        {
            return new TemplateTutorialData
            {
                Overview = "The Clean Architecture template is designed for midcore games with complex business logic. " +
                          "It uses Domain/Application/Infrastructure/Presentation separation with UseCase pattern, " +
                          "Repository abstraction, EventBus system, and full testability.",

                WhatWasCreated = "✓ Folder structure: Domain, Application, Infrastructure, Presentation layers\n" +
                                "✓ Scenes: Bootstrap.unity, Persistent.unity, Shell.unity, Gameplay.unity\n" +
                                "✓ Addressable Groups: Static, Localization, Content_Local, Content_Remote, UI, Features\n" +
                                "✓ Localization Tables: UI, Shop, Battle, Inventory, Errors + Asset Tables\n" +
                                "✓ Architecture: IContentRepository, UseCases, EventBus, ViewModels",

                QuickStartSteps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        Title = "Understand the Architecture Layers",
                        Description = "Clean Architecture separates concerns into distinct layers",
                        CodeExample = "Domain Layer (Inner):\n" +
                                     "  - Entities (Player, Item, Currency)\n" +
                                     "  - UseCases (PurchaseItemUseCase, EquipItemUseCase)\n" +
                                     "  - Interfaces (IContentRepository, IInventoryRepository)\n" +
                                     "\n" +
                                     "Application Layer:\n" +
                                     "  - Services (ContentService, LocalizationService)\n" +
                                     "  - Events (EventBus, domain events)\n" +
                                     "\n" +
                                     "Infrastructure Layer:\n" +
                                     "  - Repositories (ContentRepository wraps Addressables)\n" +
                                     "  - Network (Backend integration)\n" +
                                     "  - Persistence (Save system)\n" +
                                     "\n" +
                                     "Presentation Layer:\n" +
                                     "  - Views (MonoBehaviour UI)\n" +
                                     "  - ViewModels (Presentation logic)",
                        CodeHeight = 240,
                        Tip = "Domain layer has NO dependencies on Unity or infrastructure. It's pure C# business logic."
                    },
                    new TutorialStep
                    {
                        Title = "Create Your First UseCase",
                        Description = "UseCases contain business logic and orchestrate repositories",
                        CodeExample = "// In _Project/01_Core/Domain/UseCases/\npublic class PurchaseItemUseCase\n{\n    private readonly IInventoryRepository _inventory;\n    private readonly IWalletRepository _wallet;\n    private readonly IContentRepository _content;\n    \n    public PurchaseItemUseCase(\n        IInventoryRepository inventory,\n        IWalletRepository wallet,\n        IContentRepository content)\n    {\n        _inventory = inventory;\n        _wallet = wallet;\n        _content = content;\n    }\n    \n    public async Task<Result<Item>> ExecuteAsync(string itemId)\n    {\n        // 1. Load item definition via IContentRepository\n        var itemDef = await _inventory.GetItemDefinitionAsync(itemId);\n        \n        // 2. Check funds\n        if (!_wallet.CanAfford(itemDef.Price))\n            return Result<Item>.Failure(\"insufficient_funds\");\n        \n        // 3. Process purchase\n        _wallet.Spend(itemDef.Price);\n        var item = itemDef.CreateInstance();\n        await _inventory.AddItemAsync(item);\n        \n        return Result<Item>.Success(item);\n    }\n}",
                        CodeHeight = 320,
                        Tip = "UseCases are testable with mocked repositories. No Unity dependencies!"
                    },
                    new TutorialStep
                    {
                        Title = "Implement Repository Pattern",
                        Description = "Repositories abstract Addressables behind interfaces",
                        CodeExample = "// Domain Interface (in Domain/Interfaces/)\npublic interface IContentRepository\n{\n    Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;\n    Task<GameObject> InstantiateAsync(string key);\n    Task<long> GetDownloadSizeAsync(string key);\n}\n\n// Infrastructure Implementation (in Infrastructure/Persistence/)\npublic class ContentRepository : IContentRepository\n{\n    private readonly ContentService _contentService;\n    \n    public ContentRepository(ContentService contentService)\n    {\n        _contentService = contentService;\n    }\n    \n    public async Task<T> LoadAssetAsync<T>(string key)\n    {\n        return await _contentService.LoadAsync<T>(key);\n    }\n    \n    // ... other methods\n}",
                        CodeHeight = 240,
                        Tip = "Domain depends on IContentRepository interface, not ContentService directly."
                    },
                    new TutorialStep
                    {
                        Title = "Use ViewModels for Presentation Logic",
                        Description = "ViewModels handle UI logic and call UseCases",
                        CodeExample = "// In _Project/01_Core/Presentation/ViewModels/\npublic class ShopViewModel\n{\n    private readonly PurchaseItemUseCase _purchaseUseCase;\n    private readonly LocalizationService _localization;\n    private readonly IEventBus _eventBus;\n    \n    public async void OnItemClicked(string itemId)\n    {\n        var result = await _purchaseUseCase.ExecuteAsync(itemId);\n        \n        if (result.IsSuccess)\n        {\n            ShowSuccessPopup(result.Value);\n            _eventBus.Publish(new ItemPurchasedEvent(result.Value));\n        }\n        else\n        {\n            string errorMsg = _localization.GetString(\"Errors\", result.Error);\n            ShowErrorPopup(errorMsg);\n        }\n    }\n}",
                        CodeHeight = 200,
                        Tip = "ViewModels are testable and don't contain MonoBehaviour code."
                    },
                    new TutorialStep
                    {
                        Title = "Register Everything in DI Container",
                        Description = "Wire up all layers in ProjectInstaller",
                        CodeExample = "protected override void Configure(IContainerBuilder builder)\n{\n    // Infrastructure\n    builder.Register<ContentService>(Lifetime.Singleton);\n    builder.Register<LocalizationService>(Lifetime.Singleton);\n    \n    // Repositories (interface → implementation)\n    builder.Register<IContentRepository, ContentRepository>(Lifetime.Singleton);\n    builder.Register<IInventoryRepository, InventoryRepository>(Lifetime.Singleton);\n    \n    // Application\n    builder.Register<IEventBus, EventBus>(Lifetime.Singleton);\n    \n    // Domain UseCases\n    builder.Register<PurchaseItemUseCase>(Lifetime.Transient);\n    builder.Register<EquipItemUseCase>(Lifetime.Transient);\n    \n    // Presentation\n    builder.Register<ShopViewModel>(Lifetime.Transient);\n}",
                        CodeHeight = 200,
                        Tip = "UseCases are Transient (new instance per use), Services are Singleton."
                    }
                },

                HowToWorkWith = new List<WorkflowGuide>
                {
                    new WorkflowGuide
                    {
                        Title = "Creating New Features",
                        Description = "Follow Clean Architecture layers for each feature",
                        Steps = new List<string>
                        {
                            "1. Domain: Create entities and UseCases in _Project/02_Features/YourFeature/Domain/",
                            "2. Application: Create feature services if needed",
                            "3. Infrastructure: Implement repositories for data access",
                            "4. Presentation: Create ViewModels and Views",
                            "5. Create FeatureInstaller.cs to register everything",
                            "6. Add feature scope in appropriate scene"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Working with EventBus",
                        Description = "Decouple features with domain events",
                        Steps = new List<string>
                        {
                            "Define event: public class ItemPurchasedEvent { public Item Item; }",
                            "Publish: _eventBus.Publish(new ItemPurchasedEvent(item))",
                            "Subscribe: _eventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased)",
                            "Unsubscribe: _eventBus.Unsubscribe<ItemPurchasedEvent>(OnItemPurchased)",
                            "Events flow through the system without tight coupling"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Testing UseCases",
                        Description = "Write unit tests with mocked repositories",
                        Steps = new List<string>
                        {
                            "Create test class in Tests/Domain/UseCases/",
                            "Mock repositories: var contentMock = new Mock<IContentRepository>()",
                            "Setup mock behavior: contentMock.Setup(c => c.LoadAsync(...)).ReturnsAsync(...)",
                            "Create UseCase with mocks",
                            "Execute and assert results",
                            "No Unity dependencies needed!"
                        }
                    },
                    new WorkflowGuide
                    {
                        Title = "Loading Content Through Repository",
                        Description = "Always use IContentRepository in Domain layer",
                        Steps = new List<string>
                        {
                            "Inject IContentRepository into UseCase/Repository",
                            "Load assets: await _content.LoadAssetAsync<T>(key)",
                            "Check download size: await _content.GetDownloadSizeAsync(key)",
                            "Download if needed: await _content.DownloadAsync(key, progress)",
                            "Repository pattern keeps Domain clean"
                        }
                    }
                },

                CommonTasks = new List<CommonTask>
                {
                    new CommonTask
                    {
                        TaskName = "How to: Create a Battle Feature",
                        Steps = new List<string>
                        {
                            "1. Create folder: _Project/02_Features/Battle/",
                            "2. Add Domain layer: Entities (BattleState, Character), UseCases (StartBattleUseCase)",
                            "3. Add Infrastructure: BattleRepository, BattleContentLoader",
                            "4. Add Presentation: BattleViewModel, BattleView",
                            "5. Create BattleInstaller.cs",
                            "6. Register in scene scope"
                        },
                        Example = "// BattleContentLoader.cs\npublic class BattleContentLoader\n{\n    private readonly IContentRepository _content;\n    \n    public async Task<GameObject> LoadArenaAsync(string arenaId)\n    {\n        string key = $\"Arena_{arenaId}\";\n        \n        // Check if remote\n        var size = await _content.GetDownloadSizeAsync(key);\n        if (size > 0)\n        {\n            await _content.DownloadAsync(key, progress);\n        }\n        \n        return await _content.InstantiateAsync(key);\n    }\n}",
                        ExampleHeight = 200
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Implement Inventory System",
                        Steps = new List<string>
                        {
                            "1. Create IInventoryRepository interface in Domain/Interfaces/",
                            "2. Define methods: GetItemDefinitionAsync, AddItemAsync, RemoveItemAsync",
                            "3. Implement InventoryRepository in Infrastructure/Persistence/",
                            "4. Use IContentRepository to load ItemDefinition ScriptableObjects",
                            "5. Sync with backend via IBackendClient",
                            "6. Create EquipItemUseCase, UnequipItemUseCase"
                        },
                        Example = "public class InventoryRepository : IInventoryRepository\n{\n    private readonly IContentRepository _content;\n    private readonly IBackendClient _backend;\n    \n    public async Task<ItemDefinition> GetItemDefinitionAsync(string itemId)\n    {\n        // Load ScriptableObject via Addressables\n        string key = $\"ItemDef_{itemId}\";\n        return await _content.LoadAssetAsync<ItemDefinition>(key);\n    }\n    \n    public async Task<List<Item>> GetPlayerInventoryAsync()\n    {\n        // Fetch from backend\n        return await _backend.GetInventoryAsync();\n    }\n}",
                        ExampleHeight = 200
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Write Unit Tests for UseCases",
                        Steps = new List<string>
                        {
                            "1. Install NUnit and Moq packages",
                            "2. Create test class in Tests/Domain/UseCases/",
                            "3. Mock all repository interfaces",
                            "4. Setup mock behavior for test scenarios",
                            "5. Execute UseCase and assert results"
                        },
                        Example = "[Test]\npublic async Task PurchaseItem_WithRemoteContent_ShouldCheckDownloadSize()\n{\n    // Arrange\n    var contentMock = new Mock<IContentRepository>();\n    var inventoryMock = new Mock<IInventoryRepository>();\n    var walletMock = new Mock<IWalletRepository>();\n    \n    contentMock.Setup(c => c.GetDownloadSizeAsync(\"Item_sword\"))\n        .ReturnsAsync(5 * 1024 * 1024); // 5MB\n    \n    var useCase = new PurchaseItemUseCase(\n        inventoryMock.Object,\n        walletMock.Object,\n        contentMock.Object\n    );\n    \n    // Act\n    var result = await useCase.ExecuteAsync(\"sword\");\n    \n    // Assert\n    Assert.IsFalse(result.IsSuccess);\n    Assert.AreEqual(\"download_required\", result.Error);\n}",
                        ExampleHeight = 260
                    },
                    new CommonTask
                    {
                        TaskName = "How to: Use EventBus for Feature Communication",
                        Steps = new List<string>
                        {
                            "1. Define event class: public class ItemPurchasedEvent",
                            "2. Publish from UseCase: _eventBus.Publish(new ItemPurchasedEvent(item))",
                            "3. Subscribe in other features: _eventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased)",
                            "4. Handle event and update UI/state",
                            "5. Unsubscribe when done"
                        },
                        Example = "// In ShopViewModel\npublic async void OnPurchaseClicked(string itemId)\n{\n    var result = await _purchaseUseCase.ExecuteAsync(itemId);\n    if (result.IsSuccess)\n    {\n        _eventBus.Publish(new ItemPurchasedEvent(result.Value));\n    }\n}\n\n// In InventoryViewModel\npublic void Initialize()\n{\n    _eventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased);\n}\n\nprivate void OnItemPurchased(ItemPurchasedEvent evt)\n{\n    RefreshInventoryUI();\n}",
                        ExampleHeight = 200
                    }
                },

                NextSteps = "• Create domain entities for your game (Player, Character, Item, etc.)\n" +
                           "• Implement UseCases for core game logic\n" +
                           "• Create repository interfaces and implementations\n" +
                           "• Build feature modules following Clean Architecture\n" +
                           "• Write unit tests for UseCases with mocked repositories\n" +
                           "• Implement EventBus for feature communication\n" +
                           "• Set up backend integration via IBackendClient\n" +
                           "• Create ViewModels for complex UI logic\n" +
                           "• Configure remote Addressables for premium content",

                AdditionalResources = new List<string>
                {
                    "Architecture Guide: Assets/TempalateInstaller/unity-arch-clean.md",
                    "Clean Architecture Book: Robert C. Martin",
                    "VContainer Documentation: https://vcontainer.hadashikick.jp/",
                    "Moq Documentation: https://github.com/moq/moq4",
                    "NUnit Documentation: https://nunit.org/",
                    "Repository Pattern: https://martinfowler.com/eaaCatalog/repository.html",
                    "UseCase Pattern: https://www.plainionist.net/Implementing-Clean-Architecture-UseCases/"
                }
            };
        }
    }

    // Data structures for tutorial content
    public class TemplateTutorialData
    {
        public string Overview;
        public string WhatWasCreated;
        public List<TutorialStep> QuickStartSteps;
        public List<WorkflowGuide> HowToWorkWith;
        public List<CommonTask> CommonTasks;
        public string NextSteps;
        public List<string> AdditionalResources;
    }

    public class TutorialStep
    {
        public string Title;
        public string Description;
        public string CodeExample;
        public int CodeHeight = 80;
        public string Tip;
    }

    public class WorkflowGuide
    {
        public string Title;
        public string Description;
        public List<string> Steps;
    }

    public class CommonTask
    {
        public string TaskName;
        public List<string> Steps;
        public string Example;
        public int ExampleHeight = 100;
    }
}
#endif
