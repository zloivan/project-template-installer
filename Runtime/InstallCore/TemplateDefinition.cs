using System.Collections.Generic;
using UnityEngine;

namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// ScriptableObject defining a complete project template with all its configuration
    /// </summary>
    [CreateAssetMenu(fileName = "NewTemplateDefinition", menuName = "Template Installer/Template Definition", order = 1)]
    public class TemplateDefinition : ScriptableObject
    {
        [Header("Template Info")]
        [SerializeField] private TemplateType templateType;
        [SerializeField] private string templateName;
        [SerializeField, TextArea(3, 6)] private string description;
        [SerializeField] private string recommendedTeamSize = "1-2 developers";
        [SerializeField] private string recommendedTimeframe = "1-3 weeks";

        [Header("Folder Structure")]
        [SerializeField] private List<FolderStructure> folderStructures = new List<FolderStructure>();

        [Header("Scenes")]
        [SerializeField] private List<SceneDefinition> scenes = new List<SceneDefinition>();

        [Header("Addressables Configuration")]
        [SerializeField] private List<AddressableGroupDefinition> addressableGroups = new List<AddressableGroupDefinition>();

        [Header("Localization Configuration")]
        [SerializeField] private List<LocalizationTableDefinition> localizationTables = new List<LocalizationTableDefinition>();

        [Header("Dependencies")]
        [SerializeField] private List<string> requiredPackages = new List<string>
        {
            "com.unity.addressables",
            "com.unity.localization"
        };

        [Header("Code Templates")]
        [SerializeField] private bool includeContentService = true;
        [SerializeField] private bool includeLocalizationService = true;
        [SerializeField] private bool includeStateMachine = true;
        [SerializeField] private bool includeBootstrapInstaller = true;
        [SerializeField] private bool includeRemoteConfig = false;

        // Public accessors
        public TemplateType TemplateType => templateType;
        public string TemplateName => templateName;
        public string Description => description;
        public string RecommendedTeamSize => recommendedTeamSize;
        public string RecommendedTimeframe => recommendedTimeframe;
        public IReadOnlyList<FolderStructure> FolderStructures => folderStructures;
        public IReadOnlyList<SceneDefinition> Scenes => scenes;
        public IReadOnlyList<AddressableGroupDefinition> AddressableGroups => addressableGroups;
        public IReadOnlyList<LocalizationTableDefinition> LocalizationTables => localizationTables;
        public IReadOnlyList<string> RequiredPackages => requiredPackages;
        public bool IncludeContentService => includeContentService;
        public bool IncludeLocalizationService => includeLocalizationService;
        public bool IncludeStateMachine => includeStateMachine;
        public bool IncludeBootstrapInstaller => includeBootstrapInstaller;
        public bool IncludeRemoteConfig => includeRemoteConfig;

        /// <summary>
        /// Validate template definition
        /// </summary>
        public bool Validate(out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(templateName))
            {
                error = "Template name is required";
                return false;
            }

            if (folderStructures.Count == 0)
            {
                error = "At least one folder structure must be defined";
                return false;
            }

            if (scenes.Count == 0)
            {
                error = "At least one scene must be defined";
                return false;
            }

            if (addressableGroups.Count == 0)
            {
                error = "At least one Addressables group must be defined";
                return false;
            }

            return true;
        }
    }
}
