#if UNITY_EDITOR
using IKhom.TemplateInstaller;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Production-ready templates with VContainer integration
    /// </summary>
    public static class CodeTemplates
    {
        public static string GetContentServiceTemplate(string ns)
        {
            return @"using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace " + ns + @".Core.Content
{
    /// <summary>
    /// Service for loading content via Addressables
    /// </summary>
    public class ContentService
    {
        public async Task<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($""[ContentService] Failed to load asset: {key}"");
            return null;
        }

        public async Task<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(key, parent);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($""[ContentService] Failed to instantiate: {key}"");
            return null;
        }

        public void Release<T>(T obj) where T : UnityEngine.Object
        {
            Addressables.Release(obj);
        }
    }
}
";
        }

        public static string GetContentConfigTemplate(string ns)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.Content
{
    [CreateAssetMenu(fileName = ""ContentConfig"", menuName = """ + ns + @"/Content Config"")]
    public class ContentConfig : ScriptableObject
    {
        [Header(""Asset References"")]
        public string[] preloadAssets;
    }
}
";
        }

        public static string GetLocalizationServiceTemplate(string ns)
        {
            return @"using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace " + ns + @".Core.Localization
{
    /// <summary>
    /// Service for managing localization
    /// </summary>
    public class LocalizationService
    {
        public async Task InitializeAsync()
        {
            await LocalizationSettings.InitializationOperation.Task;
            Debug.Log(""[LocalizationService] Initialized"");
        }

        public async Task SetLocaleAsync(string localeCode)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                await Task.Yield();
                Debug.Log($""[LocalizationService] Locale changed to: {localeCode}"");
            }
            else
            {
                Debug.LogWarning($""[LocalizationService] Locale not found: {localeCode}"");
            }
        }

        public string GetString(string tableName, string key)
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(key);
                return entry?.GetLocalizedString() ?? $""[MISSING: {key}]"";
            }
            return $""[MISSING TABLE: {tableName}]"";
        }
    }
}
";
        }

        public static string GetLocalizedTextComponentTemplate(string ns)
        {
            return @"using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace " + ns + @".Core.Localization
{
    /// <summary>
    /// Component for localizing UI text
    /// </summary>
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string tableName = ""UI"";
        [SerializeField] private string key;

        private Text _text;
        private TextMeshProUGUI _tmpText;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _tmpText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateText();
        }

        public void UpdateText()
        {
            // This is a placeholder - inject LocalizationService via VContainer
            var localizedString = $""[{tableName}.{key}]"";

            if (_text != null)
                _text.text = localizedString;
            if (_tmpText != null)
                _tmpText.text = localizedString;
        }
    }
}
";
        }

        public static string GetStateInterfaceTemplate(string ns)
        {
            return @"namespace " + ns + @".Core.StateMachine
{
    /// <summary>
    /// Base interface for all states
    /// </summary>
    public interface IState
    {
        void Enter();
        void Exit();
        void Update();
    }
}
";
        }

        public static string GetStateMachineTemplate(string ns, TemplateType templateType)
        {
            var name = templateType == TemplateType.SingleScene ? "GameStateMachine" : "AppStateMachine";
            return @"using System;
using System.Collections.Generic;
using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    /// <summary>
    /// State machine for managing application/game states
    /// </summary>
    public class " + name + @"
    {
        private IState _currentState;
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        public void RegisterState<T>(T state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        public void ChangeState<T>() where T : IState
        {
            if (!_states.TryGetValue(typeof(T), out var newState))
            {
                Debug.LogError($""[" + name + @"] State not registered: {typeof(T).Name}"");
                return;
            }

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();

            Debug.Log($""[" + name + @"] Changed to state: {typeof(T).Name}"");
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}
";
        }

        public static string GetBootstrapInstallerTemplate(string ns, TemplateType templateType)
        {
            var installerName = templateType == TemplateType.SingleScene ? "BootstrapInstaller" : "ProjectInstaller";
            return @"using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace " + ns + @".Bootstrap
{
    /// <summary>
    /// VContainer installer for core services
    /// </summary>
    public class " + installerName + @" : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Register core services
            builder.Register<Core.Content.ContentService>(Lifetime.Singleton);
            builder.Register<Core.Localization.LocalizationService>(Lifetime.Singleton);

            // Register state machine
            builder.Register<Core.StateMachine." + (templateType == TemplateType.SingleScene ? "GameStateMachine" : "AppStateMachine") + @">(Lifetime.Singleton);

            Debug.Log(""[" + installerName + @"] Services registered"");
        }
    }
}
";
        }

        public static string GetEntryPointTemplate(string ns, TemplateType templateType)
        {
            var entryPointName = templateType == TemplateType.SingleScene ? "GameEntryPoint" : "AppEntryPoint";
            var stateMachineName = templateType == TemplateType.SingleScene ? "GameStateMachine" : "AppStateMachine";

            return @"using VContainer;
using UnityEngine;

namespace " + ns + @".Bootstrap
{
    /// <summary>
    /// Application entry point
    /// </summary>
    public class " + entryPointName + @" : MonoBehaviour
    {
        [Inject] private Core.StateMachine." + stateMachineName + @" _stateMachine;
        [Inject] private Core.Content.ContentService _contentService;
        [Inject] private Core.Localization.LocalizationService _localizationService;

        private async void Start()
        {
            Debug.Log(""[" + entryPointName + @"] Starting..."");

            // Initialize localization
            await _localizationService.InitializeAsync();

            // Start state machine
            // _stateMachine.Start();

            Debug.Log(""[" + entryPointName + @"] Ready"");
        }
    }
}
";
        }

        public static string GetContentBootstrapTemplate(string ns)
        {
            return @"using VContainer;
using UnityEngine;

namespace " + ns + @".Bootstrap
{
    /// <summary>
    /// Handles content preloading
    /// </summary>
    public class ContentBootstrap : MonoBehaviour
    {
        [Inject] private Core.Content.ContentService _contentService;

        private async void Start()
        {
            Debug.Log(""[ContentBootstrap] Preloading content..."");

            // Add your preload logic here

            Debug.Log(""[ContentBootstrap] Content preloaded"");
        }
    }
}
";
        }
        public static string GetLoadingStateTemplate(string ns, TemplateType templateType)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class LoadingState : IState
    {
        public void Enter()
        {
            Debug.Log(""[LoadingState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[LoadingState] Exit"");
        }
    }
}
";
        }

        public static string GetGameplayStateTemplate(string ns, TemplateType templateType)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class GameplayState : IState
    {
        public void Enter()
        {
            Debug.Log(""[GameplayState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[GameplayState] Exit"");
        }
    }
}
";
        }

        public static string GetResultsStateTemplate(string ns, TemplateType templateType)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class ResultsState : IState
    {
        public void Enter()
        {
            Debug.Log(""[ResultsState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[ResultsState] Exit"");
        }
    }
}
";
        }

        public static string GetBootstrapStateTemplate(string ns)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class BootstrapState : IState
    {
        public void Enter()
        {
            Debug.Log(""[BootstrapState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[BootstrapState] Exit"");
        }
    }
}
";
        }

        public static string GetPersistentStateTemplate(string ns)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class PersistentState : IState
    {
        public void Enter()
        {
            Debug.Log(""[PersistentState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[PersistentState] Exit"");
        }
    }
}
";
        }

        public static string GetShellStateTemplate(string ns)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class ShellState : IState
    {
        public void Enter()
        {
            Debug.Log(""[ShellState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[ShellState] Exit"");
        }
    }
}
";
        }

        public static string GetLoadLevelStateTemplate(string ns)
        {
            return @"using UnityEngine;

namespace " + ns + @".Core.StateMachine
{
    public class LoadLevelState : IState
    {
        public void Enter()
        {
            Debug.Log(""[LoadLevelState] Enter"");
        }

        public void Update() { }

        public void Exit()
        {
            Debug.Log(""[LoadLevelState] Exit"");
        }
    }
}
";
        }

        public static string GetSampleLevelConfigTemplate(string ns)
        {
            return @"using UnityEngine;

namespace " + ns + @".Content
{
    [CreateAssetMenu(fileName = ""LevelConfig"", menuName = """ + ns + @"/Level Config"")]
    public class LevelConfig : ScriptableObject
    {
        [Header(""Level Settings"")]
        public string levelName;
        public int levelIndex;
        public string addressableKey;
    }
}
";
        }
    }
}
#endif
