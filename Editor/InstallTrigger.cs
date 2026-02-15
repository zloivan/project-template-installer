#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Automatically opens the installer window on first project load after package installation
    /// </summary>
    [InitializeOnLoad]
    public static class InstallTrigger
    {
        private const string INSTALLER_SHOWN_KEY = "TemplateInstaller_WindowShown";

        static InstallTrigger()
        {
            // Check if window has been shown before
            if (!SessionState.GetBool(INSTALLER_SHOWN_KEY, false))
            {
                // Delay to ensure Unity is fully loaded
                EditorApplication.delayCall += () =>
                {
                    // Check if this is a new/empty project
                    if (ShouldShowInstaller())
                    {
                        TemplateInstallerWindow.ShowWindow();
                        SessionState.SetBool(INSTALLER_SHOWN_KEY, true);
                    }
                };
            }
        }

        private static bool ShouldShowInstaller()
        {
            // Check if _Project folder doesn't exist (indicating new project)
            string projectPath = System.IO.Path.Combine(Application.dataPath, "_Project");
            bool isNewProject = !System.IO.Directory.Exists(projectPath);

            return isNewProject;
        }
    }
}
#endif
