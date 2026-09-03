using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Dialog;
using Cysharp.Threading.Tasks;
using LayerLab.ArtMakerUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusScreen : Screen
    {
        private static readonly StatusEquipmentCategory[] ActiveCategories =
        {
            StatusEquipmentCategory.All,
            StatusEquipmentCategory.Weapon,
            StatusEquipmentCategory.Shield,
            StatusEquipmentCategory.Armor,
            StatusEquipmentCategory.Headgear
        };

        [SerializeField] private StatusEquipmentTabView[] tabViews;
        [SerializeField] private StatusEquipmentListView[] listViews;
        [SerializeField] private StatusEquipmentSlotView[] slotEquipmentViews;
        [SerializeField] private StatusSkillSlotView[] slotSkillViews;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private StatusAvatarPreviewView avatarPreview;

        [Header("Appearance Dialog")]
        [SerializeField] private Button skinButton;
        [SerializeField] private Button deckButton;
        [SerializeField] private PreviewStage avatarPreviewRenderer;

        private AvatarEditorPresenter _presenter;
        private StatusEquipmentCategory _currentCategory = StatusEquipmentCategory.All;
        private bool _initialized;
        private bool _equipmentDetailDialogOpen;
        private bool _skillLoadoutDialogOpen;
        private bool _deckLoadoutDialogOpen;

        public void Init(AppearanceController previewPartsManager, params AppearanceController[] additionalPreviewTargets)
        {
            avatarPreview.Initialize(previewPartsManager, additionalPreviewTargets, avatarPreviewRenderer);
            _presenter = AvatarEditorPresenter.CreateDefault();
            _presenter.BindPreview(previewPartsManager);
            Setup();
            SelectCategory(StatusEquipmentCategory.All);
            _initialized = true;
        }

        private void Setup()
        {
            // Tabs
            for (var i = 0; i < tabViews.Length; i++)
            {
                var tabView = tabViews[i];
                tabView.Bind(IsActiveCategory(tabView.Category), SelectCategory);
            }

            // equipment slots
            for (var i = 0; i < slotEquipmentViews.Length; i++)
            {
                slotEquipmentViews[i].SetVisible(true);
                slotEquipmentViews[i].Bind(OnEquipmentSlotClicked);
            }

            // skill slots
            var battleSkillLoadoutSlotCount = UserProfileData.BattleSkillLoadoutSlotCount;
            if (slotSkillViews.Length < battleSkillLoadoutSlotCount)
            {
                throw new InvalidOperationException($"{nameof(StatusScreen)} requires {battleSkillLoadoutSlotCount} skill slot views.");
            }
            for (var i = 0; i < slotSkillViews.Length; i++)
            {
                var visible = i < battleSkillLoadoutSlotCount;
                slotSkillViews[i].SetVisible(visible);
                if (visible)
                {
                    slotSkillViews[i].Bind(OnSkillSlotClicked);
                }
            }

            // Appearance Dialog
            skinButton.onClick.RemoveListener(OnClickSkinButton);
            skinButton.onClick.AddListener(OnClickSkinButton);
            deckButton.onClick.RemoveListener(OnClickDeckButton);
            deckButton.onClick.AddListener(OnClickDeckButton);
        }

        private void SelectCategory(StatusEquipmentCategory category)
        {
            if (!IsActiveCategory(category))
            {
                category = StatusEquipmentCategory.All;
            }

            _currentCategory = category;
            Refresh();
        }

        private void Refresh()
        {
            // header
            var userDataManager = UserDataManager.EnsureInitialized();
            var profile = userDataManager.UserData.Profile;
            playerLevelText.text = $"Lv.{Mathf.Max(1, profile.Level)}";
            titleText.text = string.IsNullOrWhiteSpace(profile.PlayerName) ? "Name" : profile.PlayerName;

            // tabs
            for (var i = 0; i < tabViews.Length; i++)
            {
                var tabView = tabViews[i];
                tabView.SetSelected(tabView.Category == _currentCategory);
            }

            for (var i = 0; i < listViews.Length; i++)
            {
                var listView = listViews[i];
                listView.gameObject.SetActive(
                    IsActiveCategory(listView.Category) && listView.Category == _currentCategory);
            }

            // equipment slots
            for (var i = 0; i < slotEquipmentViews.Length; i++)
            {
                var section = GetSectionForCategory(slotEquipmentViews[i].Category);
                slotEquipmentViews[i].Refresh(section, slotEquipmentViews[i].Category == _currentCategory);
            }

            // skill slots
            var loadoutService = userDataManager.Decks;
            var candidates = loadoutService.GetCandidateSkillsFromEquippedEquipment();
            var skillMap = BuildSkillMap(candidates);
            var skillSourceCategoryMap = BuildSkillSourceCategoryMap(userDataManager);
            var skillIds = loadoutService.EnsureUsableLoadout();

            for (var i = 0; i < slotSkillViews.Length; i++)
            {
                var visible = i < UserProfileData.BattleSkillLoadoutSlotCount;
                slotSkillViews[i].SetVisible(visible);
                if (!visible)
                {
                    continue;
                }

                var skillId = i < skillIds.Count ? skillIds[i] : 0;
                skillMap.TryGetValue(skillId, out var skill);
                skillSourceCategoryMap.TryGetValue(skillId, out var sourceCategory);
                slotSkillViews[i].Refresh(skill, sourceCategory, false);
            }

            // lists
            var options = BuildOptionsForCategory(_currentCategory);
            for (var i = 0; i < listViews.Length; i++)
            {
                var listView = listViews[i];
                var isCurrentCategory = listView.Category == _currentCategory;
                listView.Refresh(
                    isCurrentCategory ? options : Array.Empty<AvatarPartOptionViewData>(),
                    OnEquipmentClicked,
                    isCurrentCategory && _currentCategory != StatusEquipmentCategory.All
                        ? OnEmptyEquipmentClicked
                        : null);
            }
        }


        private void OnEquipmentClicked(AvatarPartOptionViewData option)
        {
            if (option == null)
            {
                return;
            }

            OpenEquipmentDetailDialogAsync(option, isOpenedFromEquipmentSlot: false).Forget();
        }

        private void OnEquipmentSlotClicked(StatusEquipmentCategory category)
        {
            SelectCategory(category);

            var selectedOption = BuildOptionsForCategory(category)
                .FirstOrDefault(option => option.IsSelected);
            if (selectedOption != null)
            {
                OpenEquipmentDetailDialogAsync(selectedOption, isOpenedFromEquipmentSlot: true).Forget();
            }
        }

        private void OnEmptyEquipmentClicked()
        {
            var categories = ToUiCategories(_currentCategory);
            if (categories.Length != 1)
            {
                return;
            }

            _presenter.ResetCategory(categories[0]);
            _presenter.Commit();
            avatarPreview.ApplySavedAppearance();

            var userDataManager = UserDataManager.EnsureInitialized();
            userDataManager.Decks.EnsureUsableLoadout(persistImmediately: true);
            userDataManager.UserDataStore.SaveSession();
            Refresh();
        }

        private void OnSkillSlotClicked(int slotIndex)
        {
            OpenSkillLoadoutDialogAsync(slotIndex).Forget();
        }

        private async UniTaskVoid OpenEquipmentDetailDialogAsync(
            AvatarPartOptionViewData option,
            bool isOpenedFromEquipmentSlot)
        {
            if (_equipmentDetailDialogOpen)
            {
                return;
            }

            _equipmentDetailDialogOpen = true;
            try
            {
                var result = await DialogService.OpenAsync<
                    DialogEquipmentDetail,
                    DialogEquipmentDetailRequest,
                    DialogEquipmentDetailResult>(
                        new DialogEquipmentDetailRequest(option, isOpenedFromEquipmentSlot),
                        new DialogContext(avatarPreviewRenderer));
                if (result.IsEquipRequested)
                {
                    if (option == null || !_presenter.TrySelectEquipment(option.EquipmentId))
                    {
                        return;
                    }

                    _presenter.Commit();
                    avatarPreview.ApplySavedAppearance();
                    var userDataManager = UserDataManager.EnsureInitialized();
                    userDataManager.Decks.EnsureUsableLoadout(persistImmediately: true);
                    userDataManager.UserDataStore.SaveSession();
                    Refresh();
                }
            }
            finally
            {
                _equipmentDetailDialogOpen = false;
            }
        }

        private async UniTaskVoid OpenSkillLoadoutDialogAsync(int slotIndex)
        {
            if (_skillLoadoutDialogOpen)
            {
                return;
            }

            _skillLoadoutDialogOpen = true;
            try
            {
                var result = await DialogService.OpenAsync<
                    SkillLoadoutDialog,
                    SkillLoadoutDialogRequest,
                    SkillLoadoutDialogResponse>(
                        new SkillLoadoutDialogRequest(slotIndex),
                        new DialogContext(avatarPreviewRenderer));
                if (result.IsSaved)
                {
                    Refresh();
                }
            }
            finally
            {
                _skillLoadoutDialogOpen = false;
            }
        }

        private void OnClickSkinButton()
        {
            OpenAppearanceDialogAsync().Forget();
        }

        private void OnClickDeckButton()
        {
            OpenDeckLoadoutDialogAsync().Forget();
        }

        private async UniTaskVoid OpenDeckLoadoutDialogAsync()
        {
            if (_deckLoadoutDialogOpen)
            {
                return;
            }

            _deckLoadoutDialogOpen = true;
            try
            {
                var result = await DialogService.OpenAsync<
                    DialogDeckLoadout,
                    DialogDeckLoadoutRequest,
                    DialogDeckLoadoutResult>(
                        new DialogDeckLoadoutRequest(),
                        new DialogContext(avatarPreviewRenderer, avatarPreview.PrimaryTarget));
                if (result.IsDeckSelected)
                {
                    avatarPreview.ApplySavedAppearance();
                    Refresh();
                }
            }
            finally
            {
                _deckLoadoutDialogOpen = false;
            }
        }

        private async UniTaskVoid OpenAppearanceDialogAsync()
        {
            avatarPreview.SetPrimaryVisible(false);
            try
            {
                var request = new AvatarDialogRequest(avatarPreview.PrimaryTarget, avatarPreview.GetTargets());
                var result = await DialogService.OpenAsync<AvatarDialog, AvatarDialogRequest, AvatarDialogResult>(
                    request,
                    new DialogContext(avatarPreviewRenderer));
                if (result.IsSaved)
                {
                    _presenter = AvatarEditorPresenter.CreateDefault();
                    _presenter.BindPreview(avatarPreview.PrimaryTarget);
                    avatarPreview.ApplySavedAppearance();
                    Refresh();
                }
            }
            finally
            {
                avatarPreview.SetPrimaryVisible(isActiveAndEnabled);
            }
        }

        private IReadOnlyList<AvatarPartOptionViewData> BuildOptionsForCategory(StatusEquipmentCategory category)
        {
            var result = new List<AvatarPartOptionViewData>();
            foreach (var uiCategory in ToUiCategories(category))
            {
                var section = _presenter.GetSection(uiCategory);
                if (section?.Options != null)
                {
                    result.AddRange(section.Options);
                }
            }

            return result
                .OrderBy(option => CategorySort(option.Category))
                .ThenBy(option => option.SortOrder)
                .ThenBy(option => option.DisplayName)
                .ToArray();
        }

        private AvatarPartSectionViewData GetSectionForCategory(StatusEquipmentCategory category)
        {
            var uiCategories = ToUiCategories(category);
            return uiCategories.Length == 1 ? _presenter.GetSection(uiCategories[0]) : null;
        }

        private static UICategory[] ToUiCategories(StatusEquipmentCategory category)
        {
            return category switch
            {
                StatusEquipmentCategory.All => new[] { UICategory.HandRight, UICategory.HandLeft, UICategory.Chest, UICategory.Helmet },
                StatusEquipmentCategory.Weapon => new[] { UICategory.HandRight },
                StatusEquipmentCategory.Shield => new[] { UICategory.HandLeft },
                StatusEquipmentCategory.Armor => new[] { UICategory.Chest },
                StatusEquipmentCategory.Headgear => new[] { UICategory.Helmet },
                _ => Array.Empty<UICategory>()
            };
        }

        private static Dictionary<int, BattleSkillData> BuildSkillMap(IReadOnlyList<BattleSkillData> skills)
        {
            var result = new Dictionary<int, BattleSkillData>();
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                if (skill != null && skill.SkillId > 0 && !result.ContainsKey(skill.SkillId))
                {
                    result.Add(skill.SkillId, skill);
                }
            }

            return result;
        }

        private static Dictionary<int, StatusEquipmentCategory> BuildSkillSourceCategoryMap(
            UserDataManager userDataManager)
        {
            var result = new Dictionary<int, StatusEquipmentCategory>();
            var equippedParts = userDataManager.Appearance.GetAllPartStates();
            for (var i = 0; i < equippedParts.Count; i++)
            {
                var state = equippedParts[i];
                if (state == null ||
                    state.EquipmentId <= 0 ||
                    !userDataManager.Inventory.TryGetDefinition(state.EquipmentId, out var equipment) ||
                    !TryGetSkillSourceCategory(equipment, out var sourceCategory))
                {
                    continue;
                }

                var skills = equipment.AssignableSkills;
                for (var skillIndex = 0; skillIndex < skills.Count; skillIndex++)
                {
                    var skill = skills[skillIndex];
                    if (skill != null && skill.SkillId > 0)
                    {
                        result.TryAdd(skill.SkillId, sourceCategory);
                    }
                }
            }

            return result;
        }

        private static bool TryGetSkillSourceCategory(
            EquipmentData equipment,
            out StatusEquipmentCategory category)
        {
            category = StatusEquipmentCategory.All;
            if (equipment == null)
            {
                return false;
            }

            category = equipment.ExclusiveGroup switch
            {
                PartsExclusiveGroup.HandRight => StatusEquipmentCategory.Weapon,
                PartsExclusiveGroup.HandLeft => StatusEquipmentCategory.Shield,
                _ => equipment.PartType switch
                {
                    PartsType.Chest => StatusEquipmentCategory.Armor,
                    PartsType.Helmet => StatusEquipmentCategory.Headgear,
                    _ => StatusEquipmentCategory.All
                }
            };
            return category != StatusEquipmentCategory.All;
        }

        private static int CategorySort(UICategory category)
        {
            return category switch
            {
                UICategory.HandRight => 0,
                UICategory.HandLeft => 1,
                UICategory.Chest => 2,
                UICategory.Helmet => 3,
                _ => 99
            };
        }

        private static bool IsActiveCategory(StatusEquipmentCategory category)
        {
            return ActiveCategories.Contains(category);
        }

        private void OnEnable()
        {
            if (_initialized)
            {
                Refresh();
            }
        }

        private void OnDestroy()
        {
            skinButton.onClick.RemoveListener(OnClickSkinButton);
        }
    }
}
