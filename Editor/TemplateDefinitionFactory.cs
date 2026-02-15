#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Factory for creating template definitions programmatically
    /// </summary>
    public static class TemplateDefinitionFactory
    {
        [MenuItem("Tools/Template Installer/Create Template Definitions")]
        public static void CreateAllTemplateDefinitions()
        {
            string templatePath = "Assets/TempalateInstaller/Runtime/InstallData/Templates";

            if (!System.IO.Directory.Exists(templatePath))
            {
                System.IO.Directory.CreateDirectory(templatePath);
            }

            CreateSingleSceneTemplate(templatePath);
            CreateModularTemplate(templatePath);
            CreateCleanArchitectureTemplate(templatePath);

            AssetDatabase.Refresh();
            Debug.Log("[TemplateDefinitionFactory] Created all template definitions");
        }

        public static TemplateDefinition CreateSingleSceneTemplate(string path = null)
        {
            var template = ScriptableObject.CreateInstance<TemplateDefinition>();

            // Use reflection to set private fields (since they're serialized)
            var type = typeof(TemplateDefinition);
            var templateTypeField = type.GetField("templateType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var templateNameField = type.GetField("templateName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var foldersField = type.GetField("folderStructures", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scenesField = type.GetField("scenes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var addressableGroupsField = type.GetField("addressableGroups", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var localizationTablesField = type.GetField("localizationTables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            templateTypeField.SetValue(template, TemplateType.SingleScene);
            templateNameField.SetValue(template, "Single-Scene Prototype");
            descriptionField.SetValue(template, "Hypercasual game template with single scene architecture");

            // Folder structures
            var folders = new List<FolderStructure>
            {
                new FolderStructure("_Project", "Bootstrap", "Core", "Core/Content", "Core/Localization", "Core/StateMachine", "Game", "Game/Levels", "Game/UI", "Scenes", "Content", "Content/Configs")
            };
            foldersField.SetValue(template, folders);

            // Scenes
            var scenes = new List<SceneDefinition>
            {
                new SceneDefinition("Game", "_Project/Scenes", true, 0)
            };
            scenesField.SetValue(template, scenes);

            // Addressable groups
            var groups = new List<AddressableGroupDefinition>
            {
                new AddressableGroupDefinition("00_Static", true, "static"),
                new AddressableGroupDefinition("01_Localization", true, "localization"),
                new AddressableGroupDefinition("02_Levels_Local", true, "level", "local"),
                new AddressableGroupDefinition("04_UI", true, "ui")
            };
            addressableGroupsField.SetValue(template, groups);

            // Localization tables
            var tables = new List<LocalizationTableDefinition>
            {
                new LocalizationTableDefinition("UI", LocalizationTableType.StringTable, "en", "ru"),
                new LocalizationTableDefinition("Gameplay", LocalizationTableType.StringTable, "en", "ru")
            };
            localizationTablesField.SetValue(template, tables);

            if (path != null)
            {
                AssetDatabase.CreateAsset(template, $"{path}/SingleSceneTemplate.asset");
            }

            return template;
        }

        public static TemplateDefinition CreateModularTemplate(string path = null)
        {
            var template = ScriptableObject.CreateInstance<TemplateDefinition>();

            var type = typeof(TemplateDefinition);
            var templateTypeField = type.GetField("templateType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var templateNameField = type.GetField("templateName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var foldersField = type.GetField("folderStructures", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scenesField = type.GetField("scenes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var addressableGroupsField = type.GetField("addressableGroups", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var localizationTablesField = type.GetField("localizationTables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var remoteConfigField = type.GetField("includeRemoteConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            templateTypeField.SetValue(template, TemplateType.Modular);
            templateNameField.SetValue(template, "Multi-Scene Modular");
            descriptionField.SetValue(template, "Hybrid-casual game template with multi-scene modular architecture");
            remoteConfigField.SetValue(template, true);

            // Folder structures
            var folders = new List<FolderStructure>
            {
                new FolderStructure("_Project", "00_Bootstrap", "01_Core", "01_Core/Content", "01_Core/Localization", "01_Core/StateMachine", "02_Features", "02_Features/Meta", "02_Features/Gameplay", "03_Content", "03_Content/Configs", "04_SDK", "05_LiveOps")
            };
            foldersField.SetValue(template, folders);

            // Scenes
            var scenes = new List<SceneDefinition>
            {
                new SceneDefinition("Bootstrap", "_Project/00_Bootstrap", true, 0),
                new SceneDefinition("Persistent", "_Project/01_Core/Scenes", false, 1),
                new SceneDefinition("Shell", "_Project/02_Features/Scenes", false, 2),
                new SceneDefinition("Gameplay", "_Project/02_Features/Scenes", false, 3)
            };
            scenesField.SetValue(template, scenes);

            // Addressable groups
            var groups = new List<AddressableGroupDefinition>
            {
                new AddressableGroupDefinition("00_Static", true, "static"),
                new AddressableGroupDefinition("01_Localization", true, "localization"),
                new AddressableGroupDefinition("02_Levels_Local", true, "level", "local"),
                new AddressableGroupDefinition("03_Levels_Remote", false, "level", "remote"),
                new AddressableGroupDefinition("04_UI", true, "ui"),
                new AddressableGroupDefinition("05_Audio", false, "audio", "remote")
            };
            addressableGroupsField.SetValue(template, groups);

            // Localization tables
            var tables = new List<LocalizationTableDefinition>
            {
                new LocalizationTableDefinition("UI", LocalizationTableType.StringTable, "en", "ru", "zh"),
                new LocalizationTableDefinition("Shop", LocalizationTableType.StringTable, "en", "ru", "zh"),
                new LocalizationTableDefinition("Gameplay", LocalizationTableType.StringTable, "en", "ru", "zh"),
                new LocalizationTableDefinition("UI_Assets", LocalizationTableType.AssetTable, "en", "ru", "zh")
            };
            localizationTablesField.SetValue(template, tables);

            if (path != null)
            {
                AssetDatabase.CreateAsset(template, $"{path}/ModularTemplate.asset");
            }

            return template;
        }

        public static TemplateDefinition CreateCleanArchitectureTemplate(string path = null)
        {
            var template = ScriptableObject.CreateInstance<TemplateDefinition>();

            var type = typeof(TemplateDefinition);
            var templateTypeField = type.GetField("templateType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var templateNameField = type.GetField("templateName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var foldersField = type.GetField("folderStructures", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scenesField = type.GetField("scenes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var addressableGroupsField = type.GetField("addressableGroups", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var localizationTablesField = type.GetField("localizationTables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var remoteConfigField = type.GetField("includeRemoteConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            templateTypeField.SetValue(template, TemplateType.CleanArchitecture);
            templateNameField.SetValue(template, "Clean Architecture");
            descriptionField.SetValue(template, "Midcore game template with Clean Architecture and domain-driven design");
            remoteConfigField.SetValue(template, true);

            // Folder structures
            var folders = new List<FolderStructure>
            {
                new FolderStructure("_Project", "00_Bootstrap", "01_Core", "01_Core/Domain", "01_Core/Domain/Entities", "01_Core/Domain/UseCases", "01_Core/Domain/Interfaces", "01_Core/Application", "01_Core/Application/Services", "01_Core/Application/Events", "01_Core/Infrastructure", "01_Core/Infrastructure/Persistence", "01_Core/Infrastructure/Network", "01_Core/Presentation", "01_Core/Presentation/UI", "01_Core/Presentation/ViewModels", "02_Features", "02_Features/Battle", "02_Features/Inventory", "02_Features/Social", "03_Shared", "04_Content")
            };
            foldersField.SetValue(template, folders);

            // Scenes (same as Modular)
            var scenes = new List<SceneDefinition>
            {
                new SceneDefinition("Bootstrap", "_Project/00_Bootstrap", true, 0),
                new SceneDefinition("Persistent", "_Project/01_Core/Scenes", false, 1),
                new SceneDefinition("Shell", "_Project/02_Features/Scenes", false, 2),
                new SceneDefinition("Gameplay", "_Project/02_Features/Scenes", false, 3)
            };
            scenesField.SetValue(template, scenes);

            // Addressable groups
            var groups = new List<AddressableGroupDefinition>
            {
                new AddressableGroupDefinition("00_Static", true, "static"),
                new AddressableGroupDefinition("01_Localization", true, "localization"),
                new AddressableGroupDefinition("02_Content_Local", true, "content", "local"),
                new AddressableGroupDefinition("03_Content_Remote", false, "content", "remote"),
                new AddressableGroupDefinition("04_UI", true, "ui"),
                new AddressableGroupDefinition("05_Features", false, "feature", "remote")
            };
            addressableGroupsField.SetValue(template, groups);

            // Localization tables
            var tables = new List<LocalizationTableDefinition>
            {
                new LocalizationTableDefinition("UI", LocalizationTableType.StringTable, "en", "ru", "zh", "ja", "ko"),
                new LocalizationTableDefinition("Shop", LocalizationTableType.StringTable, "en", "ru", "zh", "ja", "ko"),
                new LocalizationTableDefinition("Battle", LocalizationTableType.StringTable, "en", "ru", "zh", "ja", "ko"),
                new LocalizationTableDefinition("Inventory", LocalizationTableType.StringTable, "en", "ru", "zh", "ja", "ko"),
                new LocalizationTableDefinition("Errors", LocalizationTableType.StringTable, "en", "ru", "zh", "ja", "ko"),
                new LocalizationTableDefinition("Character_Icons", LocalizationTableType.AssetTable, "en", "ru", "zh", "ja", "ko"),
                new LocalizationTableDefinition("Item_Icons", LocalizationTableType.AssetTable, "en", "ru", "zh", "ja", "ko")
            };
            localizationTablesField.SetValue(template, tables);

            if (path != null)
            {
                AssetDatabase.CreateAsset(template, $"{path}/CleanArchitectureTemplate.asset");
            }

            return template;
        }
    }
}
#endif
