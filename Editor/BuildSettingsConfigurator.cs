#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Configures Unity build settings with created scenes
    /// </summary>
    public class BuildSettingsConfigurator
    {
        /// <summary>
        /// Add scenes to build settings
        /// </summary>
        public void ConfigureScenes(List<string> scenePaths)
        {
            if (scenePaths == null || scenePaths.Count == 0)
            {
                Debug.LogWarning("[BuildSettingsConfigurator] No scenes to configure");
                return;
            }

            // Get current scenes in build settings
            List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();

            // Add new scenes
            foreach (var scenePath in scenePaths)
            {
                // Check if scene already in build settings
                bool exists = buildScenes.Any(s => s.path == scenePath);
                if (!exists)
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"[BuildSettingsConfigurator] Added scene to build settings: {scenePath}");
                }
            }

            // Update build settings
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        /// <summary>
        /// Set player settings for mobile development
        /// </summary>
        public void ConfigurePlayerSettings(string companyName, string productName)
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;

            // Recommended mobile settings
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            Debug.Log("[BuildSettingsConfigurator] Player settings configured");
        }
    }
}
#endif
