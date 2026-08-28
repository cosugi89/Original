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
        public static AvatarAppearanceData CaptureAppearance(AppearanceController partsManager, InventoryManager inventoryManager)
        {
            var appearance = new AvatarAppearanceData();
            if (partsManager == null)
                return appearance;

            foreach (var partType in partsManager.GetAllPartsTypes())
            {
                if (IsDerivedPart(partType))
                    continue;

                var equipmentId = 0;
                if (partsManager.IsEquipped(partType))
                {
                    var activeIndex = partsManager.GetActiveIndex(partType);
                    if (inventoryManager != null &&
                        inventoryManager.TryGetByPartsIndex(partType, activeIndex, out var data) &&
                        data != null &&
                        data.EquipmentId > 0)
                    {
                        equipmentId = data.EquipmentId;
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

        public static void ApplyAppearance(AppearanceController partsManager, AvatarAppearanceData appearance, InventoryManager inventoryManager)
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

                ApplyPartState(partsManager, state, inventoryManager);
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

        private static void ApplyPartState(AppearanceController partsManager, AvatarPartStateData state, InventoryManager inventoryManager)
        {
            if (state == null)
                return;

            if (!TryResolvePartsIndex(state, inventoryManager, out var partsIndex))
            {
                if (state.IsVisible)
                {
                    if (state.EquipmentId > 0)
                    {
                        Debug.LogWarning(
                            $"[PartsManagerAvatarAdapter] {state.PartType} equipmentId={state.EquipmentId} を master から解決できませんでした。現在の見た目を維持します。");
                    }

                    partsManager.SetPartsVisible(state.PartType, true);
                    return;
                }

                partsManager.UnequipParts(state.PartType);
                partsManager.SetPartsVisible(state.PartType, false);
                return;
            }

            partsManager.EquipParts(state.PartType, partsIndex);
            partsManager.SetPartsVisible(state.PartType, state.IsVisible);
        }

        private static bool TryResolvePartsIndex(
            AvatarPartStateData state,
            InventoryManager inventoryManager,
            out int partsIndex)
        {
            partsIndex = -1;
            if (state == null || state.EquipmentId <= 0)
                return false;

            if (inventoryManager != null &&
                inventoryManager.TryGetDefinition(state.EquipmentId, out var data) &&
                data != null &&
                data.PartType == state.PartType &&
                data.PartsIndex >= 0)
            {
                partsIndex = data.PartsIndex;
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
