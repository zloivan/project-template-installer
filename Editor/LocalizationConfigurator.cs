#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Safe stub configurator for Unity Localization.
    /// This file intentionally avoids direct compile-time references to the Unity Localization
    /// assemblies so the editor build won't fail when the Localization package isn't installed.
    /// If the Localization package is present, a full implementation can be added that uses
    /// the Unity Localization and UnityEditor.Localization APIs.
    /// </summary>
    public class LocalizationConfigurator
    {
        private const string LocalizationPath = "Assets/Localization";

        /// <summary>
        /// Creates localization tables based on the provided definition.
        /// If Unity Localization package is not present, this method will log a warning and skip.
        /// The method signature intentionally uses only project-defined types to avoid
        /// hard dependency on Unity.Localization assemblies.
        /// </summary>
        public void CreateTable(LocalizationTableDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogWarning("[LocalizationConfigurator] Received null definition. Skipping.");
                return;
            }

            // Quick check: does any loaded assembly contain a type from Unity's Localization package?
            var localizationFound = false;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in asm.GetTypes())
                {
                    if (t.Namespace != null && t.Namespace.StartsWith("UnityEngine.Localization"))
                    {
                        localizationFound = true;
                        break;
                    }
                }

                if (localizationFound) break;
            }

            if (!localizationFound)
            {
                Debug.LogWarning("[LocalizationConfigurator] Unity Localization package not detected. Skipping localization setup.");
                return;
            }

            // If we reach here, the Localization package appears to be installed.
            // Full configurator implementation that manipulates LocalizationEditorSettings and
            // localization tables should be added here. We purposely avoid invoking those APIs
            // directly from this file to keep this package safe when the Localization package is absent.
            Debug.Log("[LocalizationConfigurator] Unity Localization package detected. Full configurator not implemented in this build.");

            // Ensure localization folder exists (harmless operation)
            if (!Directory.Exists(LocalizationPath))
            {
                Directory.CreateDirectory(LocalizationPath);
                AssetDatabase.Refresh();
            }

            // Note: real table creation should be implemented in a partial class or separate
            // compilation unit which is compiled only when Localization packages are present.
        }
    }
}
#endif
