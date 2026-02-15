using System;
using UnityEngine;

namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// Defines a scene to be created as part of template installation
    /// </summary>
    [Serializable]
    public class SceneDefinition
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string scenePath;
        [SerializeField] private bool includeInBuildSettings = true;
        [SerializeField] private int buildIndex = -1;
        [SerializeField] private bool isBootstrapScene = false;

        public string SceneName => sceneName;
        public string ScenePath => scenePath;
        public bool IncludeInBuildSettings => includeInBuildSettings;
        public int BuildIndex => buildIndex;
        public bool IsBootstrapScene => isBootstrapScene;

        public SceneDefinition(string name, string path, bool bootstrap = false, int buildIdx = -1)
        {
            sceneName = name;
            scenePath = path;
            isBootstrapScene = bootstrap;
            buildIndex = buildIdx;
        }
    }
}
