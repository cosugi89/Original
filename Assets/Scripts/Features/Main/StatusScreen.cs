using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.GameData;
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

        [SerializeField] private StatusEquipmentTabView[] tabViews = Array.Empty<StatusEquipmentTabView>();
        [SerializeField] private StatusEquipmentListView[] listViews = Array.Empty<StatusEquipmentListView>();
        [SerializeField] private StatusEquipmentSlotView[] slotEquipmentViews = Array.Empty<StatusEquipmentSlotView>();
        [SerializeField] private StatusSkillSlotView[] slotSkillViews = Array.Empty<StatusSkillSlotView>();
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private StatusAvatarPreviewView avatarPreview;

        [Header("Appearance Dialog")]
        [SerializeField] private Button skinButton;
        [SerializeField] private PreviewStage avatarPreviewRenderer;

        private AvatarEditorPresenter _presenter;
        private StatusEquipmentCategory _currentCategory = StatusEquipmentCategory.All;
        private bool _initialized;
        private bool _equipmentDetailDialogOpen;
        private bool _skillLoadoutDialogOpen;

        public void Init(AppearanceController previewPartsManager, params AppearanceController[] additionalPreviewTargets)
        {
            avatarPreview.Initialize(previewPartsManager, additionalPreviewTargets, avatarPreviewRenderer);
            _presenter = AvatarEditorPresenter.CreateDefault();
            _presenter.BindPreview(previewPartsManager);
            BindTabs();
            BindSlots();
            BindSkillSlots();
            BindAppearanceDialog();
            SelectCategory(StatusEquipmentCategory.All);
            _initialized = true;
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

        private void BindTabs()
        {
            for (var i = 0; i < tabViews.Length; i++)
            {
                var tabView = tabViews[i];
                tabView.Bind(IsActiveCategory(tabView.Category), SelectCategory);
            }
        }

        private void BindSlots()
        {
            for (var i = 0; i < slotEquipmentViews.Length; i++)
            {
                slotEquipmentViews[i].SetVisible(true);
                slotEquipmentViews[i].Bind(SelectCategory);
            }
        }

        private void BindSkillSlots()
        {
            if (slotSkillViews.Length < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                throw new InvalidOperationException(
                    $"{nameof(StatusScreen)} requires {UserProfileData.BattleSkillLoadoutSlotCount} skill slot views.");
            }

            for (var i = 0; i < slotSkillViews.Length; i++)
            {
                var visible = i < UserProfileData.BattleSkillLoadoutSlotCount;
                slotSkillViews[i].SetVisible(visible);
                if (visible)
                {
                    slotSkillViews[i].Bind(OnSkillSlotClicked);
                }
            }
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
            RefreshHeader();
            RefreshTabs();
            RefreshSlots();
            RefreshSkillSlots();
            RefreshLists();
        }

        private void RefreshHeader()
        {
            var profile = UserDataManager.EnsureInitialized().UserData.Profile;
            playerLevelText.text = $"Lv.{Mathf.Max(1, profile.Level)}";
            titleText.text = string.IsNullOrWhiteSpace(profile.PlayerName) ? "Name" : profile.PlayerName;
        }

        private void RefreshTabs()
        {
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
        }

        private void RefreshSlots()
        {
            for (var i = 0; i < slotEquipmentViews.Length; i++)
            {
                var section = GetSectionForCategory(slotEquipmentViews[i].Category);
                slotEquipmentViews[i].Refresh(section, slotEquipmentViews[i].Category == _currentCategory);
            }
        }

        private void RefreshSkillSlots()
        {
            var loadoutService = UserDataManager.EnsureInitialized().SkillLoadout;
            var candidates = loadoutService.GetCandidateSkillsFromEquippedEquipment();
            var skillMap = BuildSkillMap(candidates);
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
                slotSkillViews[i].Refresh(skill, false);
            }
        }

        private void RefreshLists()
        {
            var options = BuildOptionsForCategory(_currentCategory);
            for (var i = 0; i < listViews.Length; i++)
            {
                var listView = listViews[i];
                listView.Refresh(
                    listView.Category == _currentCategory ? options : Array.Empty<AvatarPartOptionViewData>(),
                    OnEquipmentClicked);
            }
        }

        private void OnEquipmentClicked(AvatarPartOptionViewData option)
        {
            if (option == null)
            {
                return;
            }

            OpenEquipmentDetailDialogAsync(option).Forget();
        }

        private void OnSkillSlotClicked(int slotIndex)
        {
            OpenSkillLoadoutDialogAsync(slotIndex).Forget();
        }

        private async UniTaskVoid OpenEquipmentDetailDialogAsync(AvatarPartOptionViewData option)
        {
            if (_equipmentDetailDialogOpen)
            {
                return;
            }

            _equipmentDetailDialogOpen = true;
            try
            {
                var result = await DialogService.OpenAsync<
                    StatusEquipmentDetailDialog,
                    StatusEquipmentDetailDialogRequest,
                    StatusEquipmentDetailDialogResult>(
                        new StatusEquipmentDetailDialogRequest(option),
                        new DialogContext(avatarPreviewRenderer));
                if (result.IsEquipRequested)
                {
                    EquipOption(option);
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

        private void EquipOption(AvatarPartOptionViewData option)
        {
            if (option == null || !_presenter.TrySelectEquipment(option.EquipmentId))
            {
                return;
            }

            _presenter.Commit();
            avatarPreview.ApplySavedAppearance();
            var userDataManager = UserDataManager.EnsureInitialized();
            userDataManager.SkillLoadout.EnsureUsableLoadout(persistImmediately: true);
            userDataManager.UserDataStore.SaveSession();
            Refresh();
        }

        private void BindAppearanceDialog()
        {
            skinButton.onClick.RemoveListener(OnClickSkinButton);
            skinButton.onClick.AddListener(OnClickSkinButton);
        }

        private void OnClickSkinButton()
        {
            OpenAppearanceDialogAsync().Forget();
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
                    RebuildPresenterFromSavedAppearance();
                    avatarPreview.ApplySavedAppearance();
                    Refresh();
                }
            }
            finally
            {
                avatarPreview.SetPrimaryVisible(isActiveAndEnabled);
            }
        }

        private void RebuildPresenterFromSavedAppearance()
        {
            _presenter = AvatarEditorPresenter.CreateDefault();
            _presenter.BindPreview(avatarPreview.PrimaryTarget);
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

    }
}
