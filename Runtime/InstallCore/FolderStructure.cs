using System;
using System.Collections.Generic;
using UnityEngine;

namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// Defines a folder structure to be created during template installation
    /// </summary>
    [Serializable]
    public class FolderStructure
    {
        [SerializeField] private string rootFolder = "_Project";
        [SerializeField] private List<string> subfolders = new List<string>();

        public string RootFolder => rootFolder;
        public IReadOnlyList<string> Subfolders => subfolders;

        public FolderStructure(string root, params string[] folders)
        {
            rootFolder = root;
            subfolders.AddRange(folders);
        }
    }

    /// <summary>
    /// Collection of folder structures for a template
    /// </summary>
    [Serializable]
    public class FolderStructureCollection
    {
        [SerializeField] private List<FolderStructure> structures = new List<FolderStructure>();

        public IReadOnlyList<FolderStructure> Structures => structures;

        public void Add(FolderStructure structure)
        {
            structures.Add(structure);
        }
    }
}
