#if UNITY_EDITOR
using IKhom.TemplateInstaller;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Minimal safe templates for the generator.
    /// Full templates (production) can be added later in separate files.
    /// </summary>
    public static class CodeTemplates
    {
        public static string GetContentServiceTemplate(string ns)
        {
            return "using UnityEngine;\n\nnamespace " + ns + ".Core.Content\n{\n    public class ContentService { }\n}\n";
        }

        public static string GetContentConfigTemplate(string ns)
        {
            return "using UnityEngine;\n\nnamespace " + ns + ".Core.Content\n{\n    public class ContentConfig : ScriptableObject { }\n}\n";
        }

        public static string GetLocalizationServiceTemplate(string ns)
        {
            return "using UnityEngine;\n\nnamespace " + ns + ".Core.Localization\n{\n    public class LocalizationService { }\n}\n";
        }

        public static string GetLocalizedTextComponentTemplate(string ns)
        {
            return "using UnityEngine;\n\nnamespace " + ns + ".Core.Localization\n{\n    public class LocalizedText : MonoBehaviour { }\n}\n";
        }

        public static string GetStateInterfaceTemplate(string ns)
        {
            return "namespace " + ns + ".Core.StateMachine\n{\n    public interface IState { void Enter(); void Exit(); }\n}\n";
        }

        public static string GetStateMachineTemplate(string ns, TemplateType templateType)
        {
            var name = templateType == TemplateType.SingleScene ? "GameStateMachine" : "AppStateMachine";
            return "using System;\nusing System.Collections.Generic;\n\nnamespace " + ns + ".Core.StateMachine\n{\n    public class " + name + " { }\n}\n";
        }

        public static string GetBootstrapInstallerTemplate(string ns, TemplateType templateType) => "using UnityEngine;\nnamespace " + ns + ".Bootstrap { public class BootstrapInstaller : MonoBehaviour { } }\n";
        public static string GetEntryPointTemplate(string ns, TemplateType templateType) => "using UnityEngine;\nnamespace " + ns + ".Bootstrap { public class EntryPoint : MonoBehaviour { } }\n";
        public static string GetContentBootstrapTemplate(string ns) => "using UnityEngine;\nnamespace " + ns + ".Bootstrap { public class ContentBootstrap : MonoBehaviour { } }\n";
        public static string GetLoadingStateTemplate(string ns, TemplateType templateType) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class LoadingState : IState { public void Enter(){} public void Exit(){} } }\n";
        public static string GetSampleLevelConfigTemplate(string ns) => "using UnityEngine;\nnamespace " + ns + ".Content { public class LevelConfig : ScriptableObject { } }\n";

        // Missing methods
        public static string GetGameplayStateTemplate(string ns, TemplateType templateType) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class GameplayState : IState { public void Enter(){} public void Exit(){} } }\n";
        public static string GetResultsStateTemplate(string ns, TemplateType templateType) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class ResultsState : IState { public void Enter(){} public void Exit(){} } }\n";
        public static string GetBootstrapStateTemplate(string ns) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class BootstrapState : IState { public void Enter(){} public void Exit(){} } }\n";
        public static string GetPersistentStateTemplate(string ns) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class PersistentState : IState { public void Enter(){} public void Exit(){} } }\n";
        public static string GetShellStateTemplate(string ns) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class ShellState : IState { public void Enter(){} public void Exit(){} } }\n";
        public static string GetLoadLevelStateTemplate(string ns) => "using UnityEngine;\nnamespace " + ns + ".Core.StateMachine { public class LoadLevelState : IState { public void Enter(){} public void Exit(){} } }\n";
    }
}
#endif
