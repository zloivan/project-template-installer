#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Validates package installations
    /// </summary>
    public static class PackageValidator
    {
        public static bool IsPackageInstalled(string packageName)
        {
            ListRequest listRequest = Client.List(true);
            while (!listRequest.IsCompleted) { }

            if (listRequest.Status == StatusCode.Success)
            {
                foreach (var package in listRequest.Result)
                {
                    if (package.name == packageName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif
