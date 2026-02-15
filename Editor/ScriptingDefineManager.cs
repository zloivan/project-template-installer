#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Automatically manages scripting define symbols based on installed packages
    /// </summary>
    [InitializeOnLoad]
    public static class ScriptingDefineManager
    {
        private const string ADDRESSABLES_DEFINE = "ADDRESSABLES_INSTALLED";
        private const string LOCALIZATION_DEFINE = "LOCALIZATION_INSTALLED";

        private static ListRequest _listRequest;

        static ScriptingDefineManager()
        {
            // Check packages on editor load
            EditorApplication.delayCall += CheckInstalledPackages;
        }

        private static void CheckInstalledPackages()
        {
            _listRequest = Client.List(true);
            EditorApplication.update += OnPackageListCompleted;
        }

        private static void OnPackageListCompleted()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
                return;

            EditorApplication.update -= OnPackageListCompleted;

            if (_listRequest.Status == StatusCode.Success)
            {
                bool hasAddressables = false;
                bool hasLocalization = false;

                foreach (var package in _listRequest.Result)
                {
                    if (package.name == "com.unity.addressables")
                        hasAddressables = true;
                    if (package.name == "com.unity.localization")
                        hasLocalization = true;
                }

                UpdateScriptingDefines(hasAddressables, hasLocalization);
            }

            _listRequest = null;
        }

        private static void UpdateScriptingDefines(bool hasAddressables, bool hasLocalization)
        {
            // Get current build target group
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            // Get current defines
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var definesList = currentDefines.Split(';').ToList();

            bool changed = false;

            // Manage ADDRESSABLES_INSTALLED
            if (hasAddressables && !definesList.Contains(ADDRESSABLES_DEFINE))
            {
                definesList.Add(ADDRESSABLES_DEFINE);
                changed = true;
                Debug.Log($"[ScriptingDefineManager] Added {ADDRESSABLES_DEFINE} define symbol");
            }
            else if (!hasAddressables && definesList.Contains(ADDRESSABLES_DEFINE))
            {
                definesList.Remove(ADDRESSABLES_DEFINE);
                changed = true;
                Debug.Log($"[ScriptingDefineManager] Removed {ADDRESSABLES_DEFINE} define symbol");
            }

            // Manage LOCALIZATION_INSTALLED
            if (hasLocalization && !definesList.Contains(LOCALIZATION_DEFINE))
            {
                definesList.Add(LOCALIZATION_DEFINE);
                changed = true;
                Debug.Log($"[ScriptingDefineManager] Added {LOCALIZATION_DEFINE} define symbol");
            }
            else if (!hasLocalization && definesList.Contains(LOCALIZATION_DEFINE))
            {
                definesList.Remove(LOCALIZATION_DEFINE);
                changed = true;
                Debug.Log($"[ScriptingDefineManager] Removed {LOCALIZATION_DEFINE} define symbol");
            }

            // Apply changes if needed
            if (changed)
            {
                var newDefines = string.Join(";", definesList.Where(d => !string.IsNullOrEmpty(d)));
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
                Debug.Log("[ScriptingDefineManager] Scripting define symbols updated. Unity will recompile.");
            }
        }

        [MenuItem("Tools/Template Installer/Refresh Scripting Defines")]
        private static void ManualRefresh()
        {
            Debug.Log("[ScriptingDefineManager] Manually refreshing scripting defines...");
            CheckInstalledPackages();
        }
    }
}
#endif
