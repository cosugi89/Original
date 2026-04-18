using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save.Models;
using LegacyAvatarAppearanceData = Assets.Scripts.Data.DTO.AvatarAppearanceData;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    public class LegacyAvatarSaveMigration
    {
        private class LegacySaveData
        {
            public LegacyAvatarAppearanceData appearance = new();
        }

        public bool TryApply(GameSaveData saveData)
        {
            if (saveData == null || !TryLoadLegacyAppearance(out var legacyAppearance))
                return false;

            saveData.Player ??= new PlayerData();
            saveData.Inventory ??= new InventoryData();
            saveData.Player.AvatarAppearance = ConvertAppearance(legacyAppearance);
            MergeEquippedItemsIntoInventory(saveData.Inventory, saveData.Player.AvatarAppearance);
            return true;
        }

        private static bool TryLoadLegacyAppearance(out LegacyAvatarAppearanceData appearanceData)
        {
            appearanceData = null;
            var legacyPath = Path.Combine(Application.persistentDataPath, SaveFileNames.LegacyAvatarAppearanceFileName);
            if (!File.Exists(legacyPath))
                return false;

            try
            {
                var json = File.ReadAllText(legacyPath);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                var saveData = JsonUtility.FromJson<LegacySaveData>(json);
                if (saveData?.appearance == null || IsEmpty(saveData.appearance))
                    return false;

                appearanceData = saveData.appearance;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static AvatarAppearanceData ConvertAppearance(LegacyAvatarAppearanceData legacyAppearance)
        {
            var visibilityMap = BuildVisibilityMap(legacyAppearance);
            var partStatesByType = new Dictionary<PartsType, AvatarPartStateData>();
            var appearance = new AvatarAppearanceData();

            if (legacyAppearance.parts != null)
            {
                foreach (var entry in legacyAppearance.parts)
                {
                    if (entry == null || IsDerivedPartType(entry.type))
                        continue;

                    var isVisible = visibilityMap.TryGetValue(entry.type, out var visible) ? visible : entry.index >= 0;
                    partStatesByType[entry.type] = new AvatarPartStateData
                    {
                        PartType = entry.type,
                        EquipmentId = entry.index >= 0 ? BuildLegacyEquipmentId(entry.type, entry.index) : string.Empty,
                        IsVisible = isVisible
                    };
                }
            }

            foreach (var pair in visibilityMap)
            {
                if (IsDerivedPartType(pair.Key) || partStatesByType.ContainsKey(pair.Key))
                    continue;

                partStatesByType[pair.Key] = new AvatarPartStateData
                {
                    PartType = pair.Key,
                    EquipmentId = string.Empty,
                    IsVisible = pair.Value
                };
            }

            appearance.Parts.AddRange(partStatesByType.Values);

            if (legacyAppearance.colors != null)
            {
                foreach (var entry in legacyAppearance.colors)
                {
                    if (entry == null)
                        continue;

                    appearance.Colors.Add(new AvatarColorData
                    {
                        Target = entry.target,
                        HtmlColor = $"#{ColorUtility.ToHtmlStringRGBA(entry.color)}"
                    });
                }
            }

            return appearance;
        }

        private static Dictionary<PartsType, bool> BuildVisibilityMap(LegacyAvatarAppearanceData legacyAppearance)
        {
            var visibilityMap = new Dictionary<PartsType, bool>();
            if (legacyAppearance.visibility == null)
                return visibilityMap;

            foreach (var entry in legacyAppearance.visibility)
            {
                if (entry == null || IsDerivedPartType(entry.type))
                    continue;

                visibilityMap[entry.type] = entry.visible;
            }

            return visibilityMap;
        }

        private static void MergeEquippedItemsIntoInventory(InventoryData inventory, AvatarAppearanceData appearance)
        {
            if (inventory == null)
                return;

            var existingIds = new HashSet<string>();
            foreach (var entry in inventory.Equipments)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.EquipmentId))
                    existingIds.Add(entry.EquipmentId);
            }

            foreach (var part in appearance.Parts)
            {
                if (part == null || string.IsNullOrWhiteSpace(part.EquipmentId) || !existingIds.Add(part.EquipmentId))
                    continue;

                inventory.Equipments.Add(new InventoryEntryData
                {
                    EquipmentId = part.EquipmentId,
                    Quantity = 1,
                    IsUnlocked = true
                });
            }
        }

        private static bool IsEmpty(LegacyAvatarAppearanceData appearanceData)
        {
            return appearanceData == null ||
                   ((appearanceData.parts == null || appearanceData.parts.Count == 0) &&
                    (appearanceData.colors == null || appearanceData.colors.Count == 0) &&
                    (appearanceData.visibility == null || appearanceData.visibility.Count == 0));
        }

        private static bool IsDerivedPartType(PartsType partType)
        {
            return partType == PartsType.Arrow || partType == PartsType.HelmetHair;
        }

        private static string BuildLegacyEquipmentId(PartsType partType, int index)
        {
            return EquipmentIdUtility.Build(partType, index);
        }
    }
}
