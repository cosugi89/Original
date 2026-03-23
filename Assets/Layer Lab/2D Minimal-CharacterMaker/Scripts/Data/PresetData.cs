using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// ScriptableObject that stores a collection of character preset configurations.
    /// Each preset contains parts, colors, and visibility settings for character customization.
    /// </summary>
    [CreateAssetMenu(fileName = "PresetData", menuName = "LayerLab/ArtMakerUnity/PresetData")]
    public class PresetData : ScriptableObject
    {
        public List<PresetItem> items = new();

        /// <summary>
        /// Represents a single character preset configuration with parts, colors, and visibility data.
        /// </summary>
        [Serializable]
        public class PresetItem
        {
            public List<PartsEntry> parts = new();
            public List<ColorEntry> colors = new();
            public List<VisibilityEntry> visibility = new();
            public bool isEmpty = true;
        }

        /// <summary>
        /// Stores the selected sprite index for a specific parts type.
        /// </summary>
        [Serializable]
        public class PartsEntry
        {
            public PartsType type;
            public int index;
        }

        /// <summary>
        /// Stores the color value for a specific color target type.
        /// </summary>
        [Serializable]
        public class ColorEntry
        {
            public ColorTargetType target;
            public Color color;
        }

        /// <summary>
        /// Stores the visibility state for a specific parts type.
        /// </summary>
        [Serializable]
        public class VisibilityEntry
        {
            public PartsType type;
            public bool visible;
        }

        /// <summary>Total number of preset slots.</summary>
        public int SlotCount => items?.Count ?? 0;

        /// <summary>
        /// Returns the preset item at the given slot index, or null if out of range.
        /// </summary>
        /// <param name="slot">The slot index.</param>
        /// <returns>The preset item, or null if the index is invalid.</returns>
        public PresetItem GetItem(int slot)
        {
            if (items == null || slot < 0 || slot >= items.Count) return null;
            return items[slot];
        }

        /// <summary>
        /// Saves the given preset item to the specified slot, expanding the list if necessary.
        /// </summary>
        /// <param name="slot">The slot index to save to.</param>
        /// <param name="item">The preset item to save.</param>
        public void SaveItem(int slot, PresetItem item)
        {
            if (slot < 0 || item == null) return;

            items ??= new List<PresetItem>();

            while (items.Count <= slot)
                items.Add(new PresetItem());

            item.isEmpty = false;
            items[slot] = item;
        }
    }

    /// <summary>
    /// Persists avatar preset data to a JSON file under persistentDataPath.
    /// </summary>
    public static class AvatarPresetJsonStore
    {
        private const string SaveFileName = "avatar-preset-data.json";

        [Serializable]
        private class SaveData
        {
            public int currentPresetIndex;
            public List<PresetData.PresetItem> items = new();
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static PresetData CreateRuntimePresetData(PresetData fallbackPresetData, out int currentPresetIndex)
        {
            var runtimePresetData = fallbackPresetData != null
                ? UnityEngine.Object.Instantiate(fallbackPresetData)
                : ScriptableObject.CreateInstance<PresetData>();

            runtimePresetData.hideFlags = HideFlags.DontSave;
            currentPresetIndex = 0;

            if (!TryReadSaveData(out var saveData))
                return runtimePresetData;

            runtimePresetData.items = NormalizeItems(saveData.items);
            currentPresetIndex = NormalizeCurrentPresetIndex(saveData.currentPresetIndex, runtimePresetData.items.Count);
            return runtimePresetData;
        }

        public static void Save(PresetData presetData, int currentPresetIndex)
        {
            if (presetData == null) return;

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);

                var saveData = new SaveData
                {
                    currentPresetIndex = Mathf.Max(0, currentPresetIndex),
                    items = CloneItems(presetData.items)
                };

                var json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AvatarPresetJsonStore] Failed to save preset json. Path: {SavePath}\n{ex}");
            }
        }

        public static bool TryLoadCurrentPresetItem(out PresetData.PresetItem item)
        {
            item = null;

            if (!TryReadSaveData(out var saveData))
                return false;

            var items = NormalizeItems(saveData.items);
            int currentPresetIndex = NormalizeCurrentPresetIndex(saveData.currentPresetIndex, items.Count);
            if (currentPresetIndex < 0 || currentPresetIndex >= items.Count)
                return false;

            item = CloneItem(items[currentPresetIndex]);
            return item != null && !item.isEmpty;
        }

        public static bool TryApplyCurrentPreset(PartsManager partsManager)
        {
            if (partsManager == null)
                return false;

            if (!TryLoadCurrentPresetItem(out var item))
                return false;

            partsManager.ApplyPresetItem(item);
            return true;
        }

        private static bool TryReadSaveData(out SaveData saveData)
        {
            saveData = null;
            if (!File.Exists(SavePath))
                return false;

            try
            {
                var json = File.ReadAllText(SavePath);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                saveData = JsonUtility.FromJson<SaveData>(json);
                return saveData != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarPresetJsonStore] Failed to load preset json. Path: {SavePath}\n{ex}");
                return false;
            }
        }

        private static int NormalizeCurrentPresetIndex(int currentPresetIndex, int itemCount)
        {
            if (itemCount <= 0)
                return 0;

            return Mathf.Clamp(currentPresetIndex, 0, itemCount - 1);
        }

        private static List<PresetData.PresetItem> NormalizeItems(List<PresetData.PresetItem> items)
        {
            if (items == null)
                return new List<PresetData.PresetItem>();

            var normalized = new List<PresetData.PresetItem>(items.Count);
            foreach (var item in items)
                normalized.Add(CloneItem(item) ?? new PresetData.PresetItem());

            return normalized;
        }

        private static List<PresetData.PresetItem> CloneItems(List<PresetData.PresetItem> items)
        {
            if (items == null)
                return new List<PresetData.PresetItem>();

            var cloned = new List<PresetData.PresetItem>(items.Count);
            foreach (var item in items)
                cloned.Add(CloneItem(item) ?? new PresetData.PresetItem());

            return cloned;
        }

        private static PresetData.PresetItem CloneItem(PresetData.PresetItem item)
        {
            if (item == null)
                return null;

            var cloned = new PresetData.PresetItem
            {
                isEmpty = item.isEmpty
            };

            if (item.parts != null)
            {
                foreach (var entry in item.parts)
                {
                    if (entry == null) continue;
                    cloned.parts.Add(new PresetData.PartsEntry
                    {
                        type = entry.type,
                        index = entry.index
                    });
                }
            }

            if (item.colors != null)
            {
                foreach (var entry in item.colors)
                {
                    if (entry == null) continue;
                    cloned.colors.Add(new PresetData.ColorEntry
                    {
                        target = entry.target,
                        color = entry.color
                    });
                }
            }

            if (item.visibility != null)
            {
                foreach (var entry in item.visibility)
                {
                    if (entry == null) continue;
                    cloned.visibility.Add(new PresetData.VisibilityEntry
                    {
                        type = entry.type,
                        visible = entry.visible
                    });
                }
            }

            return cloned;
        }
    }
}
