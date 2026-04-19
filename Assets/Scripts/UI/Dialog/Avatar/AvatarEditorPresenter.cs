using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save.Models;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    public class AvatarEditorPresenter
    {
        private static readonly UICategory[] SectionOrder =
        {
            UICategory.Hair,
            UICategory.Eye,
            UICategory.Beard,
            UICategory.Skin,
            UICategory.Helmet,
            UICategory.Chest,
            UICategory.HandRight,
            UICategory.HandLeft
        };

        private readonly AvatarService _avatarService;
        private readonly InventoryService _inventoryService;
        private readonly AvatarRenderService _avatarRenderService;
        private AvatarAppearanceData _draftAppearance;
        private PartsManager _previewPartsManager;

        public AvatarEditorPresenter(
            AvatarService avatarService,
            InventoryService inventoryService,
            AvatarRenderService avatarRenderService,
            AvatarAppearanceData initialAppearance)
        {
            _avatarService = avatarService;
            _inventoryService = inventoryService;
            _avatarRenderService = avatarRenderService;
            _draftAppearance = CloneAppearance(initialAppearance);
            EnsureDefaultColors(_draftAppearance);
        }

        public static AvatarEditorPresenter CreateDefault(PartsManager sourcePartsManager = null)
        {
            var inventoryService = InventoryService.EnsureInitialized();
            var avatarService = AvatarService.EnsureInitialized();
            var avatarRenderService = AvatarRenderService.EnsureInitialized();

            AvatarAppearanceData initialAppearance = sourcePartsManager != null
                ? PartsManagerAvatarAdapter.CaptureAppearance(sourcePartsManager, inventoryService)
                : avatarService.GetCurrentAppearance();

            return new AvatarEditorPresenter(avatarService, inventoryService, avatarRenderService, initialAppearance);
        }

        public void BindPreview(PartsManager previewPartsManager)
        {
            _previewPartsManager = previewPartsManager;
            ApplyDraftToPreview();
        }

        public IReadOnlyList<AvatarPartSectionViewData> GetSections(bool includeLocked = false)
        {
            return SectionOrder
                .Select(category => GetSection(category, includeLocked))
                .ToArray();
        }

        public AvatarPartSectionViewData GetSection(UICategory category, bool includeLocked = false)
        {
            var partTypes = UICategoryConfig.GetSubTypes(category);
            var options = BuildOptions(category, partTypes, includeLocked);
            var selectedOption = options.FirstOrDefault(option => option.IsSelected);
            var activePartType = selectedOption?.PartType
                ?? GetVisiblePartType(partTypes)
                ?? (partTypes.Length > 0 ? partTypes[0] : default);

            var supportsColor = TryGetColorTarget(category, out var colorTarget);
            var currentColorHtml = supportsColor ? GetColorHtml(colorTarget) : "#FFFFFFFF";

            return new AvatarPartSectionViewData
            {
                Category = category,
                DisplayName = category.ToString(),
                IsGroup = UICategoryConfig.IsGroup(category),
                HasSelection = selectedOption != null,
                ActivePartType = activePartType,
                SelectedEquipmentId = selectedOption?.EquipmentId ?? string.Empty,
                SelectedDisplayName = selectedOption?.DisplayName ?? string.Empty,
                SelectedIcon = selectedOption?.Icon,
                SupportsColor = supportsColor,
                ColorTarget = colorTarget,
                CurrentColorHtml = currentColorHtml,
                Options = options
            };
        }

        public bool TrySelectEquipment(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId) ||
                !_inventoryService.TryGetDefinition(equipmentId, out var definition) ||
                definition == null ||
                !_inventoryService.HasEquipment(equipmentId))
            {
                return false;
            }

            var category = ToCategory(definition.PartType);
            if (UICategoryConfig.IsGroup(category))
            {
                foreach (var partType in UICategoryConfig.GetSubTypes(category))
                {
                    if (partType != definition.PartType)
                    {
                        ResetPart(partType);
                    }
                }
            }

            var state = GetOrCreatePartState(definition.PartType);
            state.EquipmentId = definition.EquipmentId;
            state.IsVisible = true;

            ApplyDraftToPreview();
            return true;
        }

        public void ResetCategory(UICategory category)
        {
            foreach (var partType in UICategoryConfig.GetSubTypes(category))
            {
                ResetPart(partType);
            }

            ApplyDraftToPreview();
        }

        public void SetColor(ColorTargetType target, Color color)
        {
            var normalized = $"#{ColorUtility.ToHtmlStringRGBA(color)}";
            var colorState = GetOrCreateColorState(target);
            if (string.Equals(colorState.HtmlColor, normalized, StringComparison.OrdinalIgnoreCase))
                return;

            colorState.HtmlColor = normalized;
            ApplyDraftToPreview();
        }

        public bool TryGetCurrentColor(ColorTargetType target, out Color color)
        {
            color = Color.white;
            return ColorUtility.TryParseHtmlString(GetColorHtml(target), out color);
        }

        public IReadOnlyList<AvatarColorOptionViewData> GetColorOptions(ColorTargetType target, IReadOnlyList<Color> presetColors)
        {
            var options = new List<AvatarColorOptionViewData>();
            TryGetCurrentColor(target, out var currentColor);
            var currentColor32 = (Color32)currentColor;

            if (presetColors == null)
                return options;

            for (int i = 0; i < presetColors.Count; i++)
            {
                var presetColor = presetColors[i];
                var presetColor32 = (Color32)presetColor;
                options.Add(new AvatarColorOptionViewData
                {
                    Target = target,
                    HtmlColor = $"#{ColorUtility.ToHtmlStringRGBA(presetColor)}",
                    SwatchColor = presetColor,
                    IsSelected = presetColor32.r == currentColor32.r &&
                                 presetColor32.g == currentColor32.g &&
                                 presetColor32.b == currentColor32.b &&
                                 presetColor32.a == currentColor32.a
                });
            }

            return options;
        }

        public void Commit()
        {
            var committedAppearance = CloneAppearance(_draftAppearance);
            _avatarService.SetCurrentAppearance(committedAppearance);

            foreach (var equipmentId in committedAppearance.Parts
                         .Where(state => state != null && !string.IsNullOrWhiteSpace(state.EquipmentId))
                         .Select(state => state.EquipmentId)
                         .Distinct())
            {
                _inventoryService.MarkEquipped(equipmentId);
            }

            if (_previewPartsManager != null)
            {
                _avatarRenderService.ApplyTo(_previewPartsManager);
            }
        }

        public void ApplyDraftTo(PartsManager partsManager)
        {
            if (partsManager == null)
                return;

            EnsurePartsManagerInitialized(partsManager);
            PartsManagerAvatarAdapter.ApplyAppearance(partsManager, _draftAppearance, _inventoryService);
        }

        private IReadOnlyList<AvatarPartOptionViewData> BuildOptions(
            UICategory category,
            IReadOnlyList<PartsType> partTypes,
            bool includeLocked)
        {
            var options = new List<AvatarPartOptionViewData>();

            for (int groupIndex = 0; groupIndex < partTypes.Count; groupIndex++)
            {
                var partType = partTypes[groupIndex];
                var partState = GetPartState(partType);
                var definitions = _inventoryService.GetDefinitionsByPartType(partType, ownedOnly: false);

                foreach (var definition in definitions)
                {
                    if (definition == null || string.IsNullOrWhiteSpace(definition.EquipmentId))
                        continue;

                    var isOwned = _inventoryService.HasEquipment(definition.EquipmentId);
                    if (!includeLocked && !isOwned)
                        continue;

                    options.Add(new AvatarPartOptionViewData
                    {
                        EquipmentId = definition.EquipmentId,
                        DisplayName = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.EquipmentId : definition.DisplayName,
                        Category = category,
                        PartType = definition.PartType,
                        Icon = definition.Icon,
                        IsOwned = isOwned,
                        IsSelected = partState != null &&
                                     partState.IsVisible &&
                                     string.Equals(partState.EquipmentId, definition.EquipmentId, StringComparison.Ordinal),
                        IsVisible = partState?.IsVisible ?? false,
                        CanSelect = isOwned,
                        SortOrder = (groupIndex * 10000) + Mathf.Max(definition.PartsIndex, 0)
                    });
                }
            }

            return options
                .OrderBy(option => option.SortOrder)
                .ThenBy(option => option.DisplayName)
                .ToArray();
        }

        private string GetColorHtml(ColorTargetType target)
        {
            var colorState = _draftAppearance.Colors.FirstOrDefault(candidate => candidate != null && candidate.Target == target);
            return colorState?.HtmlColor ?? "#FFFFFFFF";
        }

        private void ApplyDraftToPreview()
        {
            if (_previewPartsManager == null)
                return;

            ApplyDraftTo(_previewPartsManager);
        }

        private AvatarPartStateData GetPartState(PartsType partType)
        {
            return _draftAppearance.Parts.FirstOrDefault(candidate => candidate != null && candidate.PartType == partType);
        }

        private AvatarPartStateData GetOrCreatePartState(PartsType partType)
        {
            var partState = GetPartState(partType);
            if (partState != null)
                return partState;

            partState = new AvatarPartStateData
            {
                PartType = partType,
                EquipmentId = string.Empty,
                IsVisible = false
            };

            _draftAppearance.Parts.Add(partState);
            return partState;
        }

        private AvatarColorData GetOrCreateColorState(ColorTargetType target)
        {
            var colorState = _draftAppearance.Colors.FirstOrDefault(candidate => candidate != null && candidate.Target == target);
            if (colorState != null)
                return colorState;

            colorState = new AvatarColorData
            {
                Target = target,
                HtmlColor = "#FFFFFFFF"
            };

            _draftAppearance.Colors.Add(colorState);
            return colorState;
        }

        private void ResetPart(PartsType partType)
        {
            var state = GetOrCreatePartState(partType);
            state.EquipmentId = string.Empty;
            state.IsVisible = false;
        }

        private static AvatarAppearanceData CloneAppearance(AvatarAppearanceData source)
        {
            var clone = new AvatarAppearanceData();
            if (source == null)
                return clone;

            clone.Parts = (source.Parts ?? new List<AvatarPartStateData>())
                .Where(state => state != null)
                .Select(state => new AvatarPartStateData
                {
                    PartType = state.PartType,
                    EquipmentId = state.EquipmentId ?? string.Empty,
                    IsVisible = state.IsVisible
                })
                .ToList();

            clone.Colors = (source.Colors ?? new List<AvatarColorData>())
                .Where(colorState => colorState != null)
                .Select(colorState => new AvatarColorData
                {
                    Target = colorState.Target,
                    HtmlColor = string.IsNullOrWhiteSpace(colorState.HtmlColor) ? "#FFFFFFFF" : colorState.HtmlColor
                })
                .ToList();

            return clone;
        }

        private static void EnsureDefaultColors(AvatarAppearanceData appearance)
        {
            appearance.Parts ??= new List<AvatarPartStateData>();
            appearance.Colors ??= new List<AvatarColorData>();

            EnsureColor(appearance, ColorTargetType.Skin);
            EnsureColor(appearance, ColorTargetType.Hair);
            EnsureColor(appearance, ColorTargetType.Eye);
            EnsureColor(appearance, ColorTargetType.Beard);
        }

        private static void EnsureColor(AvatarAppearanceData appearance, ColorTargetType target)
        {
            if (appearance.Colors.Any(candidate => candidate != null && candidate.Target == target))
                return;

            appearance.Colors.Add(new AvatarColorData
            {
                Target = target,
                HtmlColor = "#FFFFFFFF"
            });
        }

        private PartsType? GetVisiblePartType(IEnumerable<PartsType> partTypes)
        {
            foreach (var partType in partTypes)
            {
                var partState = GetPartState(partType);
                if (partState != null &&
                    partState.IsVisible &&
                    !string.IsNullOrWhiteSpace(partState.EquipmentId))
                {
                    return partType;
                }
            }

            return null;
        }

        private static UICategory ToCategory(PartsType partType)
        {
            foreach (var category in SectionOrder)
            {
                if (UICategoryConfig.GetSubTypes(category).Contains(partType))
                    return category;
            }

            return UICategory.Hair;
        }

        private static bool TryGetColorTarget(UICategory category, out ColorTargetType target)
        {
            target = ColorTargetType.Skin;
            var partTypes = UICategoryConfig.GetSubTypes(category);
            if (partTypes.Length != 1)
                return false;

            return partTypes[0] switch
            {
                PartsType.Skin => Assign(ColorTargetType.Skin, out target),
                PartsType.Hair => Assign(ColorTargetType.Hair, out target),
                PartsType.Beard => Assign(ColorTargetType.Beard, out target),
                _ => false
            };
        }

        private static bool Assign(ColorTargetType value, out ColorTargetType target)
        {
            target = value;
            return true;
        }

        private static void EnsurePartsManagerInitialized(PartsManager partsManager)
        {
            if (partsManager.ActiveIndices.Count == 0)
            {
                partsManager.Init();
            }
        }
    }
}
