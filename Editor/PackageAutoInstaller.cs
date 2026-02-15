#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Automatically installs required packages when the Template Installer is first imported
    /// </summary>
    [InitializeOnLoad]
    public static class PackageAutoInstaller
    {
        private const string PACKAGES_INSTALLED_KEY = "TemplateInstaller_PackagesInstalled";
        private const string DIALOG_SHOWN_KEY = "TemplateInstaller_DialogShown";
        private static AddRequest _addRequest;
        private static int _packageIndex = 0;
        private static ListRequest _listRequest;
        private static bool _isChecking = false;
        private static readonly string[] RequiredPackages =
        {
            "com.unity.addressables",
            "com.unity.localization"
        };

        static PackageAutoInstaller()
        {
            // Check if packages have been installed before
            if (!EditorPrefs.GetBool(PACKAGES_INSTALLED_KEY, false))
            {
                EditorApplication.delayCall += CheckAndInstallPackages;
            }
        }

        private static void CheckAndInstallPackages()
        {
            // Prevent multiple simultaneous checks
            if (_isChecking) return;
            _isChecking = true;

            // Check if packages are already installed
            _listRequest = Client.List(true);
            EditorApplication.update += OnListRequestUpdate;
        }

        private static void OnListRequestUpdate()
        {
            if (_listRequest == null || !_listRequest.IsCompleted) return;

            // Unregister immediately to prevent multiple calls
            EditorApplication.update -= OnListRequestUpdate;

            if (_listRequest.Status == StatusCode.Success)
            {
                bool allInstalled = true;
                foreach (var packageName in RequiredPackages)
                {
                    bool found = false;
                    foreach (var package in _listRequest.Result)
                    {
                        if (package.name == packageName)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        allInstalled = false;
                        break;
                    }
                }

                if (!allInstalled)
                {
                    // Check if we've already shown the dialog this session
                    if (!SessionState.GetBool(DIALOG_SHOWN_KEY, false))
                    {
                        SessionState.SetBool(DIALOG_SHOWN_KEY, true);

                        // Show dialog asking to install packages
                        if (EditorUtility.DisplayDialog(
                            "Template Installer - Required Packages",
                            "The Template Installer requires the following packages:\n\n" +
                            "• Addressables (latest)\n" +
                            "• Localization (latest)\n\n" +
                            "Install them now?",
                            "Install",
                            "Later"))
                        {
                            InstallNextPackage();
                        }
                        else
                        {
                            Debug.LogWarning("[TemplateInstaller] Package installation postponed. You can install them later via Window → Package Manager.");
                        }
                    }
                }
                else
                {
                    // All packages already installed
                    EditorPrefs.SetBool(PACKAGES_INSTALLED_KEY, true);
                    Debug.Log("[TemplateInstaller] All required packages are already installed.");
                }
            }

            _listRequest = null;
            _isChecking = false;
        }

        private static void InstallNextPackage()
        {
            if (_packageIndex >= RequiredPackages.Length)
            {
                // All packages installed
                EditorPrefs.SetBool(PACKAGES_INSTALLED_KEY, true);
                Debug.Log("[TemplateInstaller] All required packages installed successfully!");
                EditorUtility.DisplayDialog(
                    "Packages Installed",
                    "Required packages have been installed.\n\n" +
                    "Unity will now compile. After compilation completes, open:\n" +
                    "Tools → Project Template Installer",
                    "OK");
                return;
            }

            string packageName = RequiredPackages[_packageIndex];
            Debug.Log($"[TemplateInstaller] Installing {packageName}...");

            _addRequest = Client.Add(packageName);
            EditorApplication.update += PackageInstallProgress;
        }

        private static void PackageInstallProgress()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
                return;

            EditorApplication.update -= PackageInstallProgress;

            if (_addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"[TemplateInstaller] Successfully installed: {_addRequest.Result.packageId}");
                _packageIndex++;
                InstallNextPackage();
            }
            else if (_addRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"[TemplateInstaller] Failed to install package: {_addRequest.Error.message}");
                EditorUtility.DisplayDialog(
                    "Package Installation Failed",
                    $"Failed to install {RequiredPackages[_packageIndex]}:\n\n{_addRequest.Error.message}\n\n" +
                    "Please install it manually via Window → Package Manager",
                    "OK");
            }

            _addRequest = null;
        }

        [MenuItem("Tools/Template Installer/Install Required Packages")]
        public static void ManualInstallPackages()
        {
            // Reset flags and trigger installation
            SessionState.SetBool(DIALOG_SHOWN_KEY, false);
            _isChecking = false;
            CheckAndInstallPackages();
        }

        [MenuItem("Tools/Template Installer/Reset Package Installation Flag")]
        private static void ResetInstallationFlag()
        {
            EditorPrefs.DeleteKey(PACKAGES_INSTALLED_KEY);
            SessionState.SetBool(DIALOG_SHOWN_KEY, false);
            Debug.Log("[TemplateInstaller] Package installation flags reset. Restart Unity to trigger auto-installation.");
        }
    }
}
#endif
