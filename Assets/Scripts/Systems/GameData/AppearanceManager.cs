using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public class AppearanceManager
    {
        [Description("現在の共有セッション。アバター見た目の読み書き先。")]
        public UserDataManager Session { get; }

        [Description("保存のdirty管理と永続化を担当する保存サービス。")]
        public UserDataJsonStore UserDataStore { get; }

        [Description("装備の所持判定・定義検索・装備履歴更新を担当するインベントリサービス。")]
        public InventoryManager InventoryManager { get; }

        [Description("戦闘用装備を保持する現在デッキの窓口。")]
        public DeckManager DeckManager { get; }

        public AppearanceManager(
            UserDataJsonStore userDataStore,
            InventoryManager inventoryManager,
            DeckManager deckManager)
        {
            UserDataStore = userDataStore;
            Session = userDataStore.Session;
            InventoryManager = inventoryManager;
            DeckManager = deckManager;
            Initialize();
        }

        public AvatarAppearanceData GetCurrentAppearance()
        {
            return GetAppearanceForDeck(DeckManager.GetCurrentDeck());
        }

        /// <summary>
        /// 共有する髪・肌・目などの見た目と、指定デッキの装備4枠を合成した表示用外見を返す。
        /// 現在デッキや保存データは変更しない。
        /// </summary>
        public AvatarAppearanceData GetAppearanceForDeck(DeckData deck)
        {
            var storedAppearance = EnsureAppearance();
            return new AvatarAppearanceData
            {
                Parts = storedAppearance.Parts
                    .Where(part => part != null && IsCosmeticPart(part.PartType))
                    .Select(ClonePartState)
                    .Concat(DeckManager.GetPartStates(deck))
                    .ToList(),
                Colors = storedAppearance.Colors
                    .Where(color => color != null)
                    .Select(color => new AvatarColorData
                    {
                        Target = color.Target,
                        HtmlColor = color.HtmlColor,
                    })
                    .ToList(),
            };
        }

        public void SetAppearanceWithDeckEquipment(AvatarAppearanceData appearance, bool markDirty = true)
        {
            Session.UserData.Profile ??= new UserProfileDemoData();

            var normalized = appearance ?? new AvatarAppearanceData();
            normalized.Parts ??= new List<AvatarPartStateData>();
            normalized.Colors ??= new List<AvatarColorData>();

            DeckManager.SaveEquipmentFromAppearance(normalized);
            SetCosmeticAppearance(normalized, markDirty);
        }

        public void SetCosmeticAppearance(AvatarAppearanceData appearance, bool markDirty = true)
        {
            Session.UserData.Profile ??= new UserProfileDemoData();

            var normalized = appearance ?? new AvatarAppearanceData();
            normalized.Parts ??= new List<AvatarPartStateData>();
            normalized.Colors ??= new List<AvatarColorData>();

            var cosmeticAppearance = new AvatarAppearanceData
            {
                Parts = normalized.Parts
                    .Where(part => part != null && IsCosmeticPart(part.PartType))
                    .Select(ClonePartState)
                    .ToList(),
                Colors = normalized.Colors,
            };

            Session.UserData.Profile.AvatarAppearance = cosmeticAppearance;
            EnsureDefaultColor(cosmeticAppearance, ColorTargetType.Skin, markDirty);
            EnsureDefaultColor(cosmeticAppearance, ColorTargetType.Hair, markDirty);
            EnsureDefaultColor(cosmeticAppearance, ColorTargetType.Eye, markDirty);
            EnsureDefaultColor(cosmeticAppearance, ColorTargetType.Beard, markDirty);

            if (markDirty)
                UserDataStore.MarkDirty();
        }

        public IReadOnlyList<AvatarPartStateData> GetAllPartStates()
        {
            return GetCurrentAppearance().Parts
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
                EquipmentId = 0,
                IsVisible = true
            };
            appearance.Parts.Add(partState);
            UserDataStore.MarkDirty();
            return partState;
        }

        public bool TryEquip(int equipmentId, bool isVisible = true)
        {
            if (equipmentId <= 0 ||
                !InventoryManager.TryGetDefinition(equipmentId, out var definition) ||
                !InventoryManager.HasEquipment(equipmentId))
            {
                return false;
            }

            if (DeckLoadoutUtility.GetCategory(definition).HasValue)
                return isVisible && DeckManager.TryEquip(equipmentId);

            return TryEquip(definition.PartType, equipmentId, isVisible);
        }

        public bool TryEquip(PartsType partType, int equipmentId, bool isVisible = true)
        {
            if (equipmentId <= 0 ||
                !InventoryManager.TryGetDefinition(equipmentId, out var definition) ||
                definition.PartType != partType ||
                !InventoryManager.HasEquipment(equipmentId))
            {
                return false;
            }

            if (DeckLoadoutUtility.GetCategory(definition).HasValue)
            {
                if (!isVisible)
                {
                    DeckManager.Unequip(DeckLoadoutUtility.GetCategory(definition).Value);
                    return true;
                }

                return DeckManager.TryEquip(equipmentId);
            }

            var state = GetOrCreatePartState(partType);
            if (state.EquipmentId == equipmentId && state.IsVisible == isVisible)
                return true;

            state.EquipmentId = equipmentId;
            state.IsVisible = isVisible;
            InventoryManager.MarkEquipped(equipmentId);
            UserDataStore.MarkDirty();
            return true;
        }

        public void Unequip(PartsType partType)
        {
            if (TryGetDeckCategory(partType, out var category))
            {
                DeckManager.Unequip(category);
                return;
            }

            var state = GetOrCreatePartState(partType);
            if (state.EquipmentId <= 0 && !state.IsVisible)
                return;

            state.EquipmentId = 0;
            state.IsVisible = false;
            UserDataStore.MarkDirty();
        }

        public void SetVisibility(PartsType partType, bool isVisible)
        {
            if (TryGetDeckCategory(partType, out var category))
            {
                if (!isVisible)
                    DeckManager.Unequip(category);
                return;
            }

            var state = GetOrCreatePartState(partType);
            if (state.IsVisible == isVisible)
                return;

            state.IsVisible = isVisible;
            UserDataStore.MarkDirty();
        }

        public bool IsEquipped(int equipmentId)
        {
            return equipmentId > 0 && GetAllPartStates().Any(part => part.EquipmentId == equipmentId);
        }

        public bool TryGetEquippedEquipmentId(PartsType partType, out int equipmentId)
        {
            var partState = GetAllPartStates().FirstOrDefault(candidate => candidate != null && candidate.PartType == partType);
            equipmentId = partState?.EquipmentId ?? 0;
            return equipmentId > 0;
        }

        public IReadOnlyList<EquipmentData> GetSelectableEquipments(PartsType partType, bool ownedOnly = true)
        {
            return InventoryManager.GetDefinitionsByPartType(partType, ownedOnly);
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
                UserDataStore.MarkDirty();
                return;
            }

            if (string.Equals(colorState.HtmlColor, normalized, StringComparison.OrdinalIgnoreCase))
                return;

            colorState.HtmlColor = normalized;
            UserDataStore.MarkDirty();
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
            Session.UserData.Profile ??= new UserProfileDemoData();
            Session.UserData.Profile.AvatarAppearance ??= new AvatarAppearanceData();
            Session.UserData.Profile.AvatarAppearance.Parts ??= new List<AvatarPartStateData>();
            Session.UserData.Profile.AvatarAppearance.Colors ??= new List<AvatarColorData>();
            return Session.UserData.Profile.AvatarAppearance;
        }

        private static bool IsCosmeticPart(PartsType partType)
        {
            return partType == PartsType.Hair ||
                   partType == PartsType.Eye ||
                   partType == PartsType.Beard ||
                   partType == PartsType.Skin;
        }

        private bool TryGetDeckCategory(PartsType partType, out UICategory category)
        {
            category = default;
            if (partType == PartsType.Helmet)
            {
                category = UICategory.Helmet;
                return true;
            }

            if (partType == PartsType.Chest)
            {
                category = UICategory.Chest;
                return true;
            }

            var definitions = InventoryManager.GetDefinitionsByPartType(partType);
            if (definitions.Count == 0)
                return false;

            var resolvedCategory = DeckLoadoutUtility.GetCategory(definitions[0]);
            if (!resolvedCategory.HasValue)
                return false;

            category = resolvedCategory.Value;
            return true;
        }

        private static AvatarPartStateData ClonePartState(AvatarPartStateData part)
        {
            return new AvatarPartStateData
            {
                PartType = part.PartType,
                EquipmentId = part.EquipmentId,
                IsVisible = part.IsVisible,
            };
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
                UserDataStore.MarkDirty();
        }

        private static string NormalizeHtmlColor(string htmlColor)
        {
            if (ColorUtility.TryParseHtmlString(htmlColor, out var parsedColor))
                return $"#{ColorUtility.ToHtmlStringRGBA(parsedColor)}";

            return "#FFFFFFFF";
        }
    }
}
