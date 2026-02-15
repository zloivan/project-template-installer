#if UNITY_EDITOR && ADDRESSABLES_INSTALLED
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Configures Addressables groups and settings
    /// </summary>
    public class AddressablesConfigurator
    {
        private AddressableAssetSettings _settings;

        public AddressablesConfigurator()
        {
            _settings = AddressableAssetSettingsDefaultObject.Settings;

            // Initialize Addressables if not already done
            if (_settings == null)
            {
                _settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            }
        }

        /// <summary>
        /// Create an Addressables group from definition
        /// </summary>
        public AddressableAssetGroup CreateGroup(AddressableGroupDefinition definition)
        {
            if (_settings == null)
            {
                Debug.LogError("[AddressablesConfigurator] Addressables settings not initialized");
                return null;
            }

            // Check if group already exists
            var existingGroup = _settings.FindGroup(definition.GroupName);
            if (existingGroup != null)
            {
                Debug.Log($"[AddressablesConfigurator] Group '{definition.GroupName}' already exists. Skipping.");
                return existingGroup;
            }

            // Create new group
            var group = _settings.CreateGroup(definition.GroupName, false, false, true, null);

            // Configure BundledAssetGroupSchema
            var bundledSchema = group.AddSchema<BundledAssetGroupSchema>();
            ConfigureBundledSchema(bundledSchema, definition);

            // Configure ContentUpdateGroupSchema
            var contentUpdateSchema = group.AddSchema<ContentUpdateGroupSchema>();
            contentUpdateSchema.StaticContent = definition.IsLocal;

            // Add labels
            if (definition.Labels != null && definition.Labels.Length > 0)
            {
                foreach (var label in definition.Labels)
                {
                    if (!_settings.GetLabels().Contains(label))
                    {
                        _settings.AddLabel(label);
                    }
                }
            }

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AddressablesConfigurator] Created group: {definition.GroupName}");
            return group;
        }

        private void ConfigureBundledSchema(BundledAssetGroupSchema schema, AddressableGroupDefinition definition)
        {
            // Set build and load paths
            if (definition.IsLocal)
            {
                schema.BuildPath.SetVariableByName(_settings, AddressableAssetSettings.kLocalBuildPath);
                schema.LoadPath.SetVariableByName(_settings, AddressableAssetSettings.kLocalLoadPath);
            }
            else
            {
                schema.BuildPath.SetVariableByName(_settings, AddressableAssetSettings.kRemoteBuildPath);
                schema.LoadPath.SetVariableByName(_settings, AddressableAssetSettings.kRemoteLoadPath);
            }

            // Set compression
            schema.Compression = definition.CompressBundles
                ? BundledAssetGroupSchema.BundleCompressionMode.LZ4
                : BundledAssetGroupSchema.BundleCompressionMode.Uncompressed;

            // Additional settings
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.IncludeInBuild = true;
        }

        /// <summary>
        /// Configure global Addressables settings
        /// </summary>
        public void ConfigureGlobalSettings()
        {
            if (_settings == null) return;

            // Enable catalog updates for remote content
            _settings.BuildRemoteCatalog = true;
            _settings.DisableCatalogUpdateOnStartup = false;

            // Set catalog provider
            _settings.OverridePlayerVersion = Application.version;

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();

            Debug.Log("[AddressablesConfigurator] Global settings configured");
        }
    }
}
#elif UNITY_EDITOR
// Stub implementation when Addressables is not installed
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    public class AddressablesConfigurator
    {
        public AddressablesConfigurator()
        {
            Debug.LogWarning("[AddressablesConfigurator] Addressables package not installed. Please install it via Package Manager.");
        }

        public object CreateGroup(AddressableGroupDefinition definition)
        {
            Debug.LogWarning("[AddressablesConfigurator] Addressables package not installed. Skipping group creation.");
            return null;
        }

        public void ConfigureGlobalSettings()
        {
            Debug.LogWarning("[AddressablesConfigurator] Addressables package not installed. Skipping configuration.");
        }
    }
}
#endif
