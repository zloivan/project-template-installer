using System;
using UnityEngine;

namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// Defines an Addressable group to be created
    /// </summary>
    [Serializable]
    public class AddressableGroupDefinition
    {
        [SerializeField] private string groupName;
        [SerializeField] private bool isLocal = true;
        [SerializeField] private string buildPath = "ServerData/[BuildTarget]";
        [SerializeField] private string loadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]";
        [SerializeField] private bool compressBundles = true;
        [SerializeField] private string[] labels;

        public string GroupName => groupName;
        public bool IsLocal => isLocal;
        public string BuildPath => buildPath;
        public string LoadPath => loadPath;
        public bool CompressBundles => compressBundles;
        public string[] Labels => labels;

        public AddressableGroupDefinition(string name, bool local = true, params string[] groupLabels)
        {
            groupName = name;
            isLocal = local;
            labels = groupLabels;

            if (!local)
            {
                buildPath = "ServerData/[BuildTarget]";
                loadPath = "https://cdn.yourgame.com/[BuildTarget]";
            }
        }
    }
}
