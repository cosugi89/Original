using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public class AvatarService
    {
        public static AvatarService Instance { get; private set; }

        [Description("現在の共有セッション。アバター見た目の読み書き先。")]
        public GameSession Session { get; }

        [Description("保存のdirty管理と永続化を担当する保存サービス。")]
        public GameSaveService SaveService { get; }

        [Description("装備の所持判定と装備履歴更新を担当するインベントリサービス。")]
        public InventoryService InventoryService { get; }

        [Description("装備IDと装備枠の対応を解決する装備マスタ。")]
        public EquipmentCatalog EquipmentCatalog { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInitialized();
        }

        public static AvatarService EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            var inventoryService = InventoryService.EnsureInitialized();
            Instance = new AvatarService(
                GameSaveService.EnsureInitialized(),
                inventoryService,
                inventoryService.EquipmentCatalog);
            Instance.Initialize();
            return Instance;
        }

        public AvatarService(GameSaveService saveService, InventoryService inventoryService, EquipmentCatalog equipmentCatalog)
        {
            SaveService = saveService;
            Session = saveService.Session;
            InventoryService = inventoryService;
            EquipmentCatalog = equipmentCatalog;
        }

        public void RefreshCatalog(EquipmentCatalog equipmentCatalog)
        {
            EquipmentCatalog = equipmentCatalog;
        }

        public AvatarAppearanceData GetCurrentAppearance()
        {
            return EnsureAppearance();
        }

        public void SetCurrentAppearance(AvatarAppearanceData appearance, bool markDirty = true)
        {
            Session.SaveData.Player ??= new PlayerData();

            var normalized = appearance ?? new AvatarAppearanceData();
            normalized.Parts ??= new List<AvatarPartStateData>();
            normalized.Colors ??= new List<AvatarColorData>();

            Session.SaveData.Player.AvatarAppearance = normalized;
            EnsureDefaultColor(normalized, ColorTargetType.Skin, markDirty);
            EnsureDefaultColor(normalized, ColorTargetType.Hair, markDirty);
            EnsureDefaultColor(normalized, ColorTargetType.Eye, markDirty);
            EnsureDefaultColor(normalized, ColorTargetType.Beard, markDirty);

            if (markDirty)
                SaveService.MarkDirty();
        }

        public IReadOnlyList<AvatarPartStateData> GetAllPartStates()
        {
            return EnsureAppearance().Parts
                .Where(part => part != null)
                .OrderBy(part => part.PartType)
                .ToArray();
        }

        public AvatarPartStateData GetOrCreatePartState(PartsType partType)
        {
            var appearance = EnsureAppearance();
            var partState = appearance.Parts.FirstOrDefault(candidate => candidate != null && candidate.PartType == partType);
            if (partState != null)
                return partState;

            partState = new AvatarPartStateData
            {
                PartType = partType,
                EquipmentId = string.Empty,
                IsVisible = true
            };
            appearance.Parts.Add(partState);
            SaveService.MarkDirty();
            return partState;
        }

        public bool TryEquip(string equipmentId, bool isVisible = true)
        {
            if (string.IsNullOrWhiteSpace(equipmentId) ||
                EquipmentCatalog == null ||
                !EquipmentCatalog.TryGetById(equipmentId, out var definition) ||
                !InventoryService.HasEquipment(equipmentId))
            {
                return false;
            }

            return TryEquip(definition.PartType, equipmentId, isVisible);
        }

        public bool TryEquip(PartsType partType, string equipmentId, bool isVisible = true)
        {
            if (string.IsNullOrWhiteSpace(equipmentId) ||
                EquipmentCatalog == null ||
                !EquipmentCatalog.TryGetById(equipmentId, out var definition) ||
                definition.PartType != partType ||
                !InventoryService.HasEquipment(equipmentId))
            {
                return false;
            }

            var state = GetOrCreatePartState(partType);
            if (state.EquipmentId == equipmentId && state.IsVisible == isVisible)
                return true;

            state.EquipmentId = equipmentId;
            state.IsVisible = isVisible;
            InventoryService.MarkEquipped(equipmentId);
            SaveService.MarkDirty();
            return true;
        }

        public void Unequip(PartsType partType)
        {
            var state = GetOrCreatePartState(partType);
            if (string.IsNullOrEmpty(state.EquipmentId) && !state.IsVisible)
                return;

            state.EquipmentId = string.Empty;
            state.IsVisible = false;
            SaveService.MarkDirty();
        }

        public void SetVisibility(PartsType partType, bool isVisible)
        {
            var state = GetOrCreatePartState(partType);
            if (state.IsVisible == isVisible)
                return;

            state.IsVisible = isVisible;
            SaveService.MarkDirty();
        }

        public bool IsEquipped(string equipmentId)
        {
            return EnsureAppearance().Parts.Any(part => part != null && part.EquipmentId == equipmentId);
        }

        public bool TryGetEquippedEquipmentId(PartsType partType, out string equipmentId)
        {
            var partState = EnsureAppearance().Parts.FirstOrDefault(candidate => candidate != null && candidate.PartType == partType);
            equipmentId = partState?.EquipmentId ?? string.Empty;
            return !string.IsNullOrWhiteSpace(equipmentId);
        }

        public IReadOnlyList<EquipmentDefinition> GetSelectableEquipments(PartsType partType, bool ownedOnly = true)
        {
            return InventoryService.GetDefinitionsByPartType(partType, ownedOnly);
        }

        public string GetColorHtml(ColorTargetType target)
        {
            var color = EnsureAppearance().Colors.FirstOrDefault(candidate => candidate != null && candidate.Target == target);
            return color?.HtmlColor ?? "#FFFFFFFF";
        }

        public bool TryGetColor(ColorTargetType target, out Color color)
        {
            color = Color.white;
            return ColorUtility.TryParseHtmlString(GetColorHtml(target), out color);
        }

        public void SetColor(ColorTargetType target, string htmlColor)
        {
            var normalized = NormalizeHtmlColor(htmlColor);
            var appearance = EnsureAppearance();
            var colorState = appearance.Colors.FirstOrDefault(candidate => candidate != null && candidate.Target == target);
            if (colorState == null)
            {
                colorState = new AvatarColorData
                {
                    Target = target,
                    HtmlColor = normalized
                };
                appearance.Colors.Add(colorState);
                SaveService.MarkDirty();
                return;
            }

            if (string.Equals(colorState.HtmlColor, normalized, StringComparison.OrdinalIgnoreCase))
                return;

            colorState.HtmlColor = normalized;
            SaveService.MarkDirty();
        }

        private void Initialize()
        {
            var appearance = EnsureAppearance();
            EnsureDefaultColor(appearance, ColorTargetType.Skin, true);
            EnsureDefaultColor(appearance, ColorTargetType.Hair, true);
            EnsureDefaultColor(appearance, ColorTargetType.Eye, true);
            EnsureDefaultColor(appearance, ColorTargetType.Beard, true);
        }

        private AvatarAppearanceData EnsureAppearance()
        {
            Session.SaveData.Player ??= new PlayerData();
            Session.SaveData.Player.AvatarAppearance ??= new AvatarAppearanceData();
            Session.SaveData.Player.AvatarAppearance.Parts ??= new List<AvatarPartStateData>();
            Session.SaveData.Player.AvatarAppearance.Colors ??= new List<AvatarColorData>();
            return Session.SaveData.Player.AvatarAppearance;
        }

        private void EnsureDefaultColor(AvatarAppearanceData appearance, ColorTargetType target, bool markDirty)
        {
            if (appearance.Colors.Any(candidate => candidate != null && candidate.Target == target))
                return;

            appearance.Colors.Add(new AvatarColorData
            {
                Target = target,
                HtmlColor = "#FFFFFFFF"
            });

            if (markDirty)
                SaveService.MarkDirty();
        }

        private static string NormalizeHtmlColor(string htmlColor)
        {
            if (ColorUtility.TryParseHtmlString(htmlColor, out var parsedColor))
                return $"#{ColorUtility.ToHtmlStringRGBA(parsedColor)}";

            return "#FFFFFFFF";
        }
    }
}
