using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public static class PartsManagerAvatarAdapter
    {
        public static AvatarAppearanceData CaptureAppearance(PartsManager partsManager, EquipmentDatabase equipmentCatalog)
        {
            var appearance = new AvatarAppearanceData();
            if (partsManager == null)
                return appearance;

            foreach (var partType in partsManager.GetAllPartsTypes())
            {
                if (IsDerivedPart(partType))
                    continue;

                var equipmentId = string.Empty;
                if (partsManager.IsEquipped(partType))
                {
                    var activeIndex = partsManager.GetActiveIndex(partType);
                    if (equipmentCatalog != null &&
                        equipmentCatalog.TryGetByPartsIndex(partType, activeIndex, out var definition) &&
                        definition != null &&
                        !string.IsNullOrWhiteSpace(definition.EquipmentId))
                    {
                        equipmentId = definition.EquipmentId;
                    }
                    else
                    {
                        equipmentId = EquipmentIdUtility.Build(partType, activeIndex);
                    }
                }

                appearance.Parts.Add(new AvatarPartStateData
                {
                    PartType = partType,
                    EquipmentId = equipmentId,
                    IsVisible = partsManager.IsPartsVisible(partType)
                });
            }

            foreach (ColorTargetType target in Enum.GetValues(typeof(ColorTargetType)))
            {
                appearance.Colors.Add(new AvatarColorData
                {
                    Target = target,
                    HtmlColor = $"#{ColorUtility.ToHtmlStringRGBA(partsManager.GetColor(target))}"
                });
            }

            return appearance;
        }

        public static void ApplyAppearance(PartsManager partsManager, AvatarAppearanceData appearance, EquipmentDatabase equipmentCatalog)
        {
            if (partsManager == null || appearance == null)
                return;

            var partStates = (appearance.Parts ?? new List<AvatarPartStateData>())
                .Where(state => state != null)
                .GroupBy(state => state.PartType)
                .ToDictionary(group => group.Key, group => group.Last());

            foreach (var partType in partsManager.GetAllPartsTypes())
            {
                if (IsDerivedPart(partType) || !partStates.TryGetValue(partType, out var state))
                    continue;

                ApplyPartState(partsManager, state, equipmentCatalog);
            }

            foreach (var colorState in appearance.Colors ?? Enumerable.Empty<AvatarColorData>())
            {
                if (colorState == null)
                    continue;

                if (ColorUtility.TryParseHtmlString(colorState.HtmlColor, out var color))
                {
                    partsManager.SetColor(colorState.Target, color);
                }
            }
        }

        private static void ApplyPartState(PartsManager partsManager, AvatarPartStateData state, EquipmentDatabase equipmentCatalog)
        {
            if (state == null)
                return;

            if (!TryResolvePartsIndex(state, equipmentCatalog, out var partsIndex))
            {
                partsManager.UnequipParts(state.PartType);
                partsManager.SetPartsVisible(state.PartType, false);
                return;
            }

            partsManager.EquipParts(state.PartType, partsIndex);
            partsManager.SetPartsVisible(state.PartType, state.IsVisible);
        }

        private static bool TryResolvePartsIndex(
            AvatarPartStateData state,
            EquipmentDatabase equipmentCatalog,
            out int partsIndex)
        {
            partsIndex = -1;
            if (state == null || string.IsNullOrWhiteSpace(state.EquipmentId))
                return false;

            if (equipmentCatalog != null &&
                equipmentCatalog.TryGetById(state.EquipmentId, out var definition) &&
                definition != null &&
                definition.PartType == state.PartType &&
                definition.PartsIndex >= 0)
            {
                partsIndex = definition.PartsIndex;
                return true;
            }

            if (EquipmentIdUtility.TryParse(state.EquipmentId, out var partType, out var parsedIndex) &&
                partType == state.PartType &&
                parsedIndex >= 0)
            {
                partsIndex = parsedIndex;
                return true;
            }

            return false;
        }

        private static bool IsDerivedPart(PartsType partType)
        {
            return partType == PartsType.Arrow || partType == PartsType.HelmetHair;
        }
    }
}
