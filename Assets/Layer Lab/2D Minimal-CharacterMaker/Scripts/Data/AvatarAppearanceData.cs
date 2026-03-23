using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// Serializable avatar appearance data.
    /// </summary>
    [Serializable]
    public class AvatarAppearanceData
    {
        public List<PartsEntry> parts = new();
        public List<ColorEntry> colors = new();
        public List<VisibilityEntry> visibility = new();

        [Serializable]
        public class PartsEntry
        {
            public PartsType type;
            public int index;
        }

        [Serializable]
        public class ColorEntry
        {
            public ColorTargetType target;
            public Color color;
        }

        [Serializable]
        public class VisibilityEntry
        {
            public PartsType type;
            public bool visible;
        }
    }

    /// <summary>
    /// Persists the player's current avatar appearance to JSON.
    /// </summary>
    public static class AvatarAppearanceJsonStore
    {
        private const string SaveFileName = "avatar-appearance-data.json";
        private const string LegacySaveFileName = "avatar-preset-data.json";

        [Serializable]
        private class SaveData
        {
            public AvatarAppearanceData appearance = new();
        }

        [Serializable]
        private class LegacySaveData
        {
            public int currentPresetIndex = 0;
            public List<LegacyAppearanceItem> items = new();
        }

        [Serializable]
        private class LegacyAppearanceItem
        {
            public List<AvatarAppearanceData.PartsEntry> parts = new();
            public List<AvatarAppearanceData.ColorEntry> colors = new();
            public List<AvatarAppearanceData.VisibilityEntry> visibility = new();
            public bool isEmpty = true;
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private static string LegacySavePath => Path.Combine(Application.persistentDataPath, LegacySaveFileName);

        public static void Save(AvatarAppearanceData appearanceData)
        {
            if (appearanceData == null) return;

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);

                var saveData = new SaveData
                {
                    appearance = CloneAppearanceData(appearanceData)
                };

                var json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AvatarAppearanceJsonStore] Failed to save avatar appearance. Path: {SavePath}\n{ex}");
            }
        }

        public static bool TryLoad(out AvatarAppearanceData appearanceData)
        {
            appearanceData = null;

            if (TryReadCurrentSaveData(out var currentSaveData))
            {
                appearanceData = CloneAppearanceData(currentSaveData.appearance);
                return !IsEmpty(appearanceData);
            }

            if (TryReadLegacySaveData(out var legacyAppearanceData))
            {
                appearanceData = CloneAppearanceData(legacyAppearanceData);
                return !IsEmpty(appearanceData);
            }

            return false;
        }

        public static bool TryApplyTo(PartsManager partsManager)
        {
            if (partsManager == null)
                return false;

            if (!TryLoad(out var appearanceData))
                return false;

            partsManager.ApplyAppearanceData(appearanceData);
            return true;
        }

        private static bool TryReadCurrentSaveData(out SaveData saveData)
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
                return saveData != null && saveData.appearance != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarAppearanceJsonStore] Failed to load avatar appearance. Path: {SavePath}\n{ex}");
                return false;
            }
        }

        private static bool TryReadLegacySaveData(out AvatarAppearanceData appearanceData)
        {
            appearanceData = null;
            if (!File.Exists(LegacySavePath))
                return false;

            try
            {
                var json = File.ReadAllText(LegacySavePath);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                var saveData = JsonUtility.FromJson<LegacySaveData>(json);
                if (saveData?.items == null || saveData.items.Count == 0)
                    return false;

                int index = Mathf.Clamp(saveData.currentPresetIndex, 0, saveData.items.Count - 1);
                var item = saveData.items[index];
                if (item == null || item.isEmpty)
                    return false;

                appearanceData = new AvatarAppearanceData
                {
                    parts = CloneParts(item.parts),
                    colors = CloneColors(item.colors),
                    visibility = CloneVisibility(item.visibility)
                };
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AvatarAppearanceJsonStore] Failed to load legacy avatar appearance. Path: {LegacySavePath}\n{ex}");
                return false;
            }
        }

        private static bool IsEmpty(AvatarAppearanceData appearanceData)
        {
            return appearanceData == null ||
                   ((appearanceData.parts == null || appearanceData.parts.Count == 0) &&
                    (appearanceData.colors == null || appearanceData.colors.Count == 0) &&
                    (appearanceData.visibility == null || appearanceData.visibility.Count == 0));
        }

        private static AvatarAppearanceData CloneAppearanceData(AvatarAppearanceData appearanceData)
        {
            if (appearanceData == null)
                return new AvatarAppearanceData();

            return new AvatarAppearanceData
            {
                parts = CloneParts(appearanceData.parts),
                colors = CloneColors(appearanceData.colors),
                visibility = CloneVisibility(appearanceData.visibility)
            };
        }

        private static List<AvatarAppearanceData.PartsEntry> CloneParts(List<AvatarAppearanceData.PartsEntry> parts)
        {
            var cloned = new List<AvatarAppearanceData.PartsEntry>();
            if (parts == null)
                return cloned;

            foreach (var entry in parts)
            {
                if (entry == null) continue;
                cloned.Add(new AvatarAppearanceData.PartsEntry
                {
                    type = entry.type,
                    index = entry.index
                });
            }

            return cloned;
        }

        private static List<AvatarAppearanceData.ColorEntry> CloneColors(List<AvatarAppearanceData.ColorEntry> colors)
        {
            var cloned = new List<AvatarAppearanceData.ColorEntry>();
            if (colors == null)
                return cloned;

            foreach (var entry in colors)
            {
                if (entry == null) continue;
                cloned.Add(new AvatarAppearanceData.ColorEntry
                {
                    target = entry.target,
                    color = entry.color
                });
            }

            return cloned;
        }

        private static List<AvatarAppearanceData.VisibilityEntry> CloneVisibility(List<AvatarAppearanceData.VisibilityEntry> visibility)
        {
            var cloned = new List<AvatarAppearanceData.VisibilityEntry>();
            if (visibility == null)
                return cloned;

            foreach (var entry in visibility)
            {
                if (entry == null) continue;
                cloned.Add(new AvatarAppearanceData.VisibilityEntry
                {
                    type = entry.type,
                    visible = entry.visible
                });
            }

            return cloned;
        }
    }
}
