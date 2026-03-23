using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// Persists the player's current avatar appearance to JSON.
    /// </summary>
    public static class AvatarAppearanceJsonStore
    {
        private const string SaveFileName = "avatar-appearance-data.json";

        [Serializable]
        private class SaveData
        {
            public AvatarAppearanceData appearance = new();
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

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

            if (!TryReadCurrentSaveData(out var currentSaveData))
                return false;

            appearanceData = CloneAppearanceData(currentSaveData.appearance);
            return !IsEmpty(appearanceData);
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
