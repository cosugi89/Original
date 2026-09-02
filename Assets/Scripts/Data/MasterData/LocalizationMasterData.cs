using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Localization", fileName = "LocalizationMasterData")]
    public sealed class LocalizationMasterData : ScriptableObject
    {
        [SerializeField] private SystemLanguage language = SystemLanguage.Japanese;
        [SerializeField] private List<LocalizationTextEntry> entries = new();

        public SystemLanguage Language => language;
        public IReadOnlyList<LocalizationTextEntry> Entries => entries ??= new List<LocalizationTextEntry>();
    }

    [Serializable]
    public sealed class LocalizationTextEntry
    {
        [SerializeField] private string key = string.Empty;
        [SerializeField, TextArea(2, 4)] private string value = string.Empty;

        public string Key => key ?? string.Empty;
        public string Value => value ?? string.Empty;
    }
}
