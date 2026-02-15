#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Generates runtime C# code files from templates with namespace injection
    /// </summary>
    public class RuntimeCodeGenerator
    {
        private readonly TemplateDefinition _template;
        private readonly string _namespace;
        private readonly string _codeOutputPath;

        public RuntimeCodeGenerator(TemplateDefinition template, string rootNamespace)
        {
            _template = template;
            _namespace = rootNamespace;
            _codeOutputPath = Path.Combine(Application.dataPath, "_Project/Core");
        }

        /// <summary>
        /// Generate ContentService.cs
        /// </summary>
        public void GenerateContentService()
        {
            string contentPath = Path.Combine(_codeOutputPath, "Content");
            Directory.CreateDirectory(contentPath);

            string code = CodeTemplates.GetContentServiceTemplate(_namespace);
            WriteCodeFile(contentPath, "ContentService.cs", code);

            string configCode = CodeTemplates.GetContentConfigTemplate(_namespace);
            WriteCodeFile(contentPath, "ContentConfig.cs", configCode);
        }

        /// <summary>
        /// Generate LocalizationService.cs
        /// </summary>
        public void GenerateLocalizationService()
        {
            string localizationPath = Path.Combine(_codeOutputPath, "Localization");
            Directory.CreateDirectory(localizationPath);

            string code = CodeTemplates.GetLocalizationServiceTemplate(_namespace);
            WriteCodeFile(localizationPath, "LocalizationService.cs", code);

            string componentCode = CodeTemplates.GetLocalizedTextComponentTemplate(_namespace);
            WriteCodeFile(localizationPath, "LocalizedText.cs", componentCode);
        }

        /// <summary>
        /// Generate StateMachine classes
        /// </summary>
        public void GenerateStateMachine()
        {
            string stateMachinePath = Path.Combine(_codeOutputPath, "StateMachine");
            Directory.CreateDirectory(stateMachinePath);

            string interfaceCode = CodeTemplates.GetStateInterfaceTemplate(_namespace);
            WriteCodeFile(stateMachinePath, "IState.cs", interfaceCode);

            string stateMachineCode = CodeTemplates.GetStateMachineTemplate(_namespace, _template.TemplateType);
            WriteCodeFile(stateMachinePath, GetStateMachineName() + ".cs", stateMachineCode);

            // Generate states based on template type
            GenerateStatesForTemplate(stateMachinePath);
        }

        /// <summary>
        /// Generate Bootstrap installer
        /// </summary>
        public void GenerateBootstrapInstaller()
        {
            string bootstrapPath = GetBootstrapPath();
            Directory.CreateDirectory(bootstrapPath);

            string installerCode = CodeTemplates.GetBootstrapInstallerTemplate(_namespace, _template.TemplateType);
            WriteCodeFile(bootstrapPath, GetInstallerName() + ".cs", installerCode);

            string entryPointCode = CodeTemplates.GetEntryPointTemplate(_namespace, _template.TemplateType);
            WriteCodeFile(bootstrapPath, GetEntryPointName() + ".cs", entryPointCode);

            string contentBootstrapCode = CodeTemplates.GetContentBootstrapTemplate(_namespace);
            WriteCodeFile(bootstrapPath, "ContentBootstrap.cs", contentBootstrapCode);
        }

        /// <summary>
        /// Generate sample content (optional)
        /// </summary>
        public void GenerateSampleContent()
        {
            // Generate sample level config
            string contentPath = Path.Combine(Application.dataPath, "_Project/Content/Configs");
            Directory.CreateDirectory(contentPath);

            string sampleConfigCode = CodeTemplates.GetSampleLevelConfigTemplate(_namespace);
            WriteCodeFile(contentPath, "LevelConfig.cs", sampleConfigCode);
        }

        private void GenerateStatesForTemplate(string stateMachinePath)
        {
            switch (_template.TemplateType)
            {
                case TemplateType.SingleScene:
                    GenerateSingleSceneStates(stateMachinePath);
                    break;
                case TemplateType.Modular:
                    GenerateModularStates(stateMachinePath);
                    break;
                case TemplateType.CleanArchitecture:
                    GenerateCleanArchitectureStates(stateMachinePath);
                    break;
            }
        }

        private void GenerateSingleSceneStates(string path)
        {
            WriteCodeFile(path, "LoadingState.cs", CodeTemplates.GetLoadingStateTemplate(_namespace, TemplateType.SingleScene));
            WriteCodeFile(path, "GameplayState.cs", CodeTemplates.GetGameplayStateTemplate(_namespace, TemplateType.SingleScene));
            WriteCodeFile(path, "ResultsState.cs", CodeTemplates.GetResultsStateTemplate(_namespace, TemplateType.SingleScene));
        }

        private void GenerateModularStates(string path)
        {
            WriteCodeFile(path, "BootstrapState.cs", CodeTemplates.GetBootstrapStateTemplate(_namespace));
            WriteCodeFile(path, "PersistentState.cs", CodeTemplates.GetPersistentStateTemplate(_namespace));
            WriteCodeFile(path, "ShellState.cs", CodeTemplates.GetShellStateTemplate(_namespace));
            WriteCodeFile(path, "LoadLevelState.cs", CodeTemplates.GetLoadLevelStateTemplate(_namespace));
            WriteCodeFile(path, "GameplayState.cs", CodeTemplates.GetGameplayStateTemplate(_namespace, TemplateType.Modular));
            WriteCodeFile(path, "ResultsState.cs", CodeTemplates.GetResultsStateTemplate(_namespace, TemplateType.Modular));
        }

        private void GenerateCleanArchitectureStates(string path)
        {
            GenerateModularStates(path); // Same states, different infrastructure
        }

        private void WriteCodeFile(string directory, string filename, string content)
        {
            string fullPath = Path.Combine(directory, filename);

            // Don't overwrite if file exists
            if (File.Exists(fullPath))
            {
                Debug.Log($"[RuntimeCodeGenerator] File already exists, skipping: {filename}");
                return;
            }

            File.WriteAllText(fullPath, content);
            Debug.Log($"[RuntimeCodeGenerator] Generated: {fullPath}");
        }

        private string GetBootstrapPath()
        {
            switch (_template.TemplateType)
            {
                case TemplateType.SingleScene:
                    return Path.Combine(Application.dataPath, "_Project/Bootstrap");
                case TemplateType.Modular:
                case TemplateType.CleanArchitecture:
                    return Path.Combine(Application.dataPath, "_Project/00_Bootstrap");
                default:
                    return Path.Combine(Application.dataPath, "_Project/Bootstrap");
            }
        }

        private string GetStateMachineName()
        {
            return _template.TemplateType == TemplateType.SingleScene
                ? "GameStateMachine"
                : "AppStateMachine";
        }

        private string GetInstallerName()
        {
            return _template.TemplateType == TemplateType.SingleScene
                ? "BootstrapInstaller"
                : "ProjectInstaller";
        }

        private string GetEntryPointName()
        {
            return _template.TemplateType == TemplateType.SingleScene
                ? "GameEntryPoint"
                : "AppEntryPoint";
        }
    }
}
#endif
