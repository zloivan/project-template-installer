#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Helper for installing Unity packages
    /// </summary>
    public static class PackageInstallationHelper
    {
        private static AddRequest _currentRequest;

        public static void InstallPackage(string packageIdentifier)
        {
            if (_currentRequest != null && !_currentRequest.IsCompleted)
            {
                Debug.LogWarning($"[PackageInstallationHelper] Package installation already in progress");
                return;
            }

            _currentRequest = Client.Add(packageIdentifier);
            EditorApplication.update += PackageInstallationProgress;
        }

        private static void PackageInstallationProgress()
        {
            if (_currentRequest == null || !_currentRequest.IsCompleted)
                return;

            if (_currentRequest.Status == StatusCode.Success)
            {
                Debug.Log($"[PackageInstallationHelper] Successfully installed: {_currentRequest.Result.packageId}");
            }
            else if (_currentRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"[PackageInstallationHelper] Failed to install package: {_currentRequest.Error.message}");
            }

            EditorApplication.update -= PackageInstallationProgress;
            _currentRequest = null;
        }
    }
}
#endif
