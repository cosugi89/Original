using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save.Models;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public static class PartsManagerAvatarAdapter
    {
        public static AvatarAppearanceData CaptureAppearance(PartsManager partsManager, InventoryService inventoryService)
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
                    if (inventoryService != null &&
                        inventoryService.TryGetByPartsIndex(partType, activeIndex, out var data) &&
                        data != null &&
                        !string.IsNullOrWhiteSpace(data.EquipmentId))
                    {
                        equipmentId = data.EquipmentId;
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

        public static void ApplyAppearance(PartsManager partsManager, AvatarAppearanceData appearance, InventoryService inventoryService)
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

                ApplyPartState(partsManager, state, inventoryService);
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

        private static void ApplyPartState(PartsManager partsManager, AvatarPartStateData state, InventoryService inventoryService)
        {
            if (state == null)
                return;

            if (!TryResolvePartsIndex(state, inventoryService, out var partsIndex))
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
            InventoryService inventoryService,
            out int partsIndex)
        {
            partsIndex = -1;
            if (state == null || string.IsNullOrWhiteSpace(state.EquipmentId))
                return false;

            if (inventoryService != null &&
                inventoryService.TryGetDefinition(state.EquipmentId, out var data) &&
                data != null &&
                data.PartType == state.PartType &&
                data.PartsIndex >= 0)
            {
                partsIndex = data.PartsIndex;
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
