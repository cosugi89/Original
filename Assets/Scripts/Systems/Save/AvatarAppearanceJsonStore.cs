using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// Legacy avatar-only JSON persistence.
    /// New code should use the main game save pipeline instead.
    /// </summary>
    [Obsolete("Legacy avatar-only persistence. Use the main game save pipeline for new code.")]
    public static class AvatarAppearanceJsonStore
    {
        [Serializable]
        private class SaveData
        {
            public AppearanceData appearance = new();
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileNames.LegacyAvatarAppearanceFileName);

        public static void Save(AppearanceData appearanceData)
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

        public static bool TryLoad(out AppearanceData appearanceData)
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

        private static bool IsEmpty(AppearanceData appearanceData)
        {
            return appearanceData == null ||
                   ((appearanceData.parts == null || appearanceData.parts.Count == 0) &&
                    (appearanceData.colors == null || appearanceData.colors.Count == 0) &&
                    (appearanceData.visibility == null || appearanceData.visibility.Count == 0));
        }

        private static AppearanceData CloneAppearanceData(AppearanceData appearanceData)
        {
            if (appearanceData == null)
                return new AppearanceData();

            return new AppearanceData
            {
                parts = CloneParts(appearanceData.parts),
                colors = CloneColors(appearanceData.colors),
                visibility = CloneVisibility(appearanceData.visibility)
            };
        }

        private static List<AppearanceData.PartsEntry> CloneParts(List<AppearanceData.PartsEntry> parts)
        {
            var cloned = new List<AppearanceData.PartsEntry>();
            if (parts == null)
                return cloned;

            foreach (var entry in parts)
            {
                if (entry == null) continue;
                cloned.Add(new AppearanceData.PartsEntry
                {
                    type = entry.type,
                    index = entry.index
                });
            }

            return cloned;
        }

        private static List<AppearanceData.ColorEntry> CloneColors(List<AppearanceData.ColorEntry> colors)
        {
            var cloned = new List<AppearanceData.ColorEntry>();
            if (colors == null)
                return cloned;

            foreach (var entry in colors)
            {
                if (entry == null) continue;
                cloned.Add(new AppearanceData.ColorEntry
                {
                    target = entry.target,
                    color = entry.color
                });
            }

            return cloned;
        }

        private static List<AppearanceData.VisibilityEntry> CloneVisibility(List<AppearanceData.VisibilityEntry> visibility)
        {
            var cloned = new List<AppearanceData.VisibilityEntry>();
            if (visibility == null)
                return cloned;

            foreach (var entry in visibility)
            {
                if (entry == null) continue;
                cloned.Add(new AppearanceData.VisibilityEntry
                {
                    type = entry.type,
                    visible = entry.visible
                });
            }

            return cloned;
        }
    }
}
