#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Safely removes the installer package after installation is complete
    /// </summary>
    public static class PackageSelfRemover
    {
        private const string PACKAGE_NAME = "com.ikhom.project-template-installer";
        private static RemoveRequest _removeRequest;

        public static void RemovePackage()
        {
            if (EditorUtility.DisplayDialog(
                "Remove Template Installer",
                "This will remove the Template Installer package from your project.\n\n" +
                "Your generated project structure will remain intact.\n\n" +
                "This action cannot be undone. Continue?",
                "Remove",
                "Cancel"))
            {
                Debug.Log("[PackageSelfRemover] Removing installer package...");

                _removeRequest = Client.Remove(PACKAGE_NAME);
                EditorApplication.update += RemovalProgress;
            }
        }

        private static void RemovalProgress()
        {
            if (_removeRequest == null || !_removeRequest.IsCompleted)
                return;

            if (_removeRequest.Status == StatusCode.Success)
            {
                Debug.Log("[PackageSelfRemover] Package removed successfully");
                EditorUtility.DisplayDialog(
                    "Package Removed",
                    "Template Installer package has been removed.\n\nYour project structure remains intact.",
                    "OK"
                );
            }
            else if (_removeRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"[PackageSelfRemover] Failed to remove package: {_removeRequest.Error.message}");
                EditorUtility.DisplayDialog(
                    "Removal Failed",
                    $"Failed to remove package:\n{_removeRequest.Error.message}",
                    "OK"
                );
            }

            EditorApplication.update -= RemovalProgress;
            _removeRequest = null;
        }

        /// <summary>
        /// Clean up any temporary files created by the installer
        /// </summary>
        public static void CleanupTemporaryFiles()
        {
            // Clean up any temp files if needed
            string tempPath = Path.Combine(Application.temporaryCachePath, "TemplateInstaller");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
                Debug.Log("[PackageSelfRemover] Cleaned up temporary files");
            }
        }
    }
}
#endif
