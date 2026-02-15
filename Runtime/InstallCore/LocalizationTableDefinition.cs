using System;
using UnityEngine;

namespace IKhom.TemplateInstaller
{
    /// <summary>
    /// Type of localization table
    /// </summary>
    public enum LocalizationTableType
    {
        StringTable,
        AssetTable
    }

    /// <summary>
    /// Defines a localization table to be created
    /// </summary>
    [Serializable]
    public class LocalizationTableDefinition
    {
        [SerializeField] private string tableName;
        [SerializeField] private LocalizationTableType tableType = LocalizationTableType.StringTable;
        [SerializeField] private string[] locales = { "en", "ru" };
        [SerializeField] private bool makeAddressable = true;
        [SerializeField] private string addressableGroup = "01_Localization";

        public string TableName => tableName;
        public LocalizationTableType TableType => tableType;
        public string[] Locales => locales;
        public bool MakeAddressable => makeAddressable;
        public string AddressableGroup => addressableGroup;

        public LocalizationTableDefinition(string name, LocalizationTableType type, params string[] supportedLocales)
        {
            tableName = name;
            tableType = type;
            locales = supportedLocales.Length > 0 ? supportedLocales : new[] { "en", "ru" };
        }
    }
}
