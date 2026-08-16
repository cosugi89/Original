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
        private const string StatusPreviewImageName = "CharacterPreviewRawImage";

        private static readonly StatusEquipmentCategory[] ActiveCategories =
        {
            StatusEquipmentCategory.All,
            StatusEquipmentCategory.Weapon,
            StatusEquipmentCategory.Shield,
            StatusEquipmentCategory.Armor,
            StatusEquipmentCategory.Headgear
        };

        [SerializeField] private StatusEquipmentTabView[] tabViews = Array.Empty<StatusEquipmentTabView>();
        [SerializeField] private GameObject[] tabScreens = Array.Empty<GameObject>();
        [SerializeField] private StatusEquipmentListView[] listViews = Array.Empty<StatusEquipmentListView>();
        [SerializeField] private StatusEquipmentSlotView[] slotEquipmentViews = Array.Empty<StatusEquipmentSlotView>();
        [SerializeField] private StatusSkillSlotView[] slotSkillViews = Array.Empty<StatusSkillSlotView>();
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private RawImage statusPreviewImage;

        [Header("Appearance Dialog")]
        [SerializeField] private Button skinButton;
        [SerializeField] private PreviewStage avatarPreviewRenderer;

        private AvatarEditorPresenter _presenter;
        private readonly List<PartsManager> _previewTargets = new();
        private GameObject _primaryPreviewRoot;
        private StatusEquipmentCategory _currentCategory = StatusEquipmentCategory.All;
        private bool _initialized;
        private bool _equipmentDetailDialogOpen;
        private bool _skillLoadoutDialogOpen;

        public void Init(PartsManager previewPartsManager, params PartsManager[] additionalPreviewTargets)
        {
            SetPreviewTargets(previewPartsManager, additionalPreviewTargets);
            _presenter = AvatarEditorPresenter.CreateDefault(previewPartsManager);
            _presenter.BindPreview(previewPartsManager);
            ApplyCurrentAppearanceToPreviewTargets();
            BindTabs();
            BindSlots();
            BindSkillSlots();
            BindAppearanceDialog();
            ConfigureStatusPreviewImage();
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
            if (skinButton != null)
            {
                skinButton.onClick.RemoveListener(OnClickSkinButton);
            }
        }

        private void BindTabs()
        {
            for (var i = 0; i < tabViews.Length; i++)
            {
                var category = IndexToCategory(i);
                var enabled = IsActiveCategory(category);
                if (tabViews[i] != null)
                {
                    tabViews[i].Bind(category, enabled, SelectCategory);
                }

                if (tabScreens != null && i < tabScreens.Length && tabScreens[i] != null)
                {
                    tabScreens[i].SetActive(enabled && category == _currentCategory);
                }
            }
        }

        private void BindSlots()
        {
            var categories = new[]
            {
                StatusEquipmentCategory.Weapon,
                StatusEquipmentCategory.Shield,
                StatusEquipmentCategory.Armor,
                StatusEquipmentCategory.Headgear
            };

            for (var i = 0; i < slotEquipmentViews.Length; i++)
            {
                if (slotEquipmentViews[i] == null)
                {
                    continue;
                }

                if (i < categories.Length)
                {
                    slotEquipmentViews[i].SetVisible(true);
                    slotEquipmentViews[i].BindCategory(categories[i], SelectCategory);
                }
                else
                {
                    slotEquipmentViews[i].SetVisible(false);
                }
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
                    slotSkillViews[i].BindSlot(i, OnSkillSlotClicked);
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
            if (_presenter == null)
            {
                return;
            }

            RefreshHeader();
            RefreshTabs();
            RefreshSlots();
            RefreshSkillSlots();
            RefreshLists();
        }

        private void RefreshHeader()
        {
            var profile = GameSaveService.EnsureInitialized().Session.UserData.Profile;
            if (playerLevelText != null)
            {
                playerLevelText.text = $"Lv.{Mathf.Max(1, profile?.Level ?? 1)}";
            }

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(profile?.PlayerName) ? "Name" : profile.PlayerName;
            }
        }

        private void RefreshTabs()
        {
            for (var i = 0; i < tabViews.Length; i++)
            {
                var category = IndexToCategory(i);
                if (tabViews[i] != null)
                {
                    tabViews[i].SetSelected(category == _currentCategory);
                }

                if (tabScreens != null && i < tabScreens.Length && tabScreens[i] != null)
                {
                    tabScreens[i].SetActive(IsActiveCategory(category) && category == _currentCategory);
                }
            }
        }

        private void RefreshSlots()
        {
            for (var i = 0; i < slotEquipmentViews.Length; i++)
            {
                if (slotEquipmentViews[i] == null || !slotEquipmentViews[i].gameObject.activeSelf)
                {
                    continue;
                }

                var section = GetSectionForCategory(slotEquipmentViews[i].Category);
                slotEquipmentViews[i].Refresh(section, slotEquipmentViews[i].Category == _currentCategory);
            }
        }

        private void RefreshSkillSlots()
        {
            var loadoutService = BattleSkillLoadoutService.EnsureInitialized();
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
                var category = IndexToCategory(i);
                if (listViews[i] != null)
                {
                    listViews[i].Refresh(
                        IsActiveCategory(category) && category == _currentCategory ? options : Array.Empty<AvatarPartOptionViewData>(),
                        OnEquipmentClicked);
                }
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
                    StatusSkillLoadoutDialog,
                    StatusSkillLoadoutDialogRequest,
                    StatusSkillLoadoutDialogResult>(
                        new StatusSkillLoadoutDialogRequest(slotIndex),
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
            ApplyCurrentAppearanceToPreviewTargets();
            BattleSkillLoadoutService.EnsureInitialized().EnsureUsableLoadout(persistImmediately: true);
            GameSaveService.EnsureInitialized().SaveSession();
            Refresh();
        }

        private void BindAppearanceDialog()
        {
            ResolveReferences();
            EnsureSkinButton();

            if (skinButton == null)
            {
                Debug.LogWarning("StatusScreen: Button_Skin is not assigned.", this);
                return;
            }

            skinButton.onClick.RemoveListener(OnClickSkinButton);
            skinButton.onClick.AddListener(OnClickSkinButton);
        }

        private void OnClickSkinButton()
        {
            OpenAppearanceDialogAsync().Forget();
        }

        private async UniTaskVoid OpenAppearanceDialogAsync()
        {
            if (avatarPreviewRenderer == null)
            {
                Debug.LogWarning("StatusScreen: Avatar PreviewStage is not assigned.", this);
                return;
            }

            SetPrimaryPreviewVisible(false);
            try
            {
                var request = new AvatarDialogRequest(ResolveDialogSource(), _previewTargets.ToArray());
                var result = await DialogService.OpenAsync<AvatarDialog, AvatarDialogRequest, AvatarDialogResult>(
                    request,
                    new DialogContext(avatarPreviewRenderer));
                if (result.IsSaved)
                {
                    RebuildPresenterFromSavedAppearance();
                    ApplyCurrentAppearanceToPreviewTargets();
                    Refresh();
                }
            }
            finally
            {
                SetPrimaryPreviewVisible(isActiveAndEnabled);
            }
        }

        private PartsManager ResolveDialogSource()
        {
            return _previewTargets.Count > 0 ? _previewTargets[0] : null;
        }

        private void RebuildPresenterFromSavedAppearance()
        {
            var primaryPreview = ResolveDialogSource();
            _presenter = AvatarEditorPresenter.CreateDefault();
            _presenter.BindPreview(primaryPreview);
        }

        private void SetPreviewTargets(PartsManager primary, IReadOnlyList<PartsManager> additionalTargets)
        {
            _previewTargets.Clear();
            _primaryPreviewRoot = ResolvePreviewRoot(primary);
            AddPreviewTarget(primary);
            if (additionalTargets == null)
            {
                return;
            }

            for (var i = 0; i < additionalTargets.Count; i++)
            {
                AddPreviewTarget(additionalTargets[i]);
            }
        }

        private void SetPrimaryPreviewVisible(bool visible)
        {
            if (_primaryPreviewRoot != null && _primaryPreviewRoot.activeSelf != visible)
            {
                _primaryPreviewRoot.SetActive(visible);
            }

            SetStatusPreviewImageVisible(visible);
        }

        private void SetStatusPreviewImageVisible(bool visible)
        {
            if (statusPreviewImage != null && statusPreviewImage.gameObject.activeSelf != visible)
            {
                statusPreviewImage.gameObject.SetActive(visible);
            }
        }

        private static GameObject ResolvePreviewRoot(PartsManager partsManager)
        {
            if (partsManager == null)
            {
                return null;
            }

            if (partsManager.transform.parent != null &&
                IsPreviewRoot(partsManager.transform.parent))
            {
                return partsManager.transform.parent.gameObject;
            }

            return partsManager.gameObject;
        }

        private static bool IsPreviewRoot(Transform transform)
        {
            return transform.name == "GameObject" ||
                   transform.name == "StatusCharacterPreviewRoot";
        }

        private void AddPreviewTarget(PartsManager target)
        {
            if (target == null || _previewTargets.Contains(target))
            {
                return;
            }

            _previewTargets.Add(target);
        }

        private void ApplyCurrentAppearanceToPreviewTargets()
        {
            var renderService = AvatarRenderService.EnsureInitialized();
            for (var i = 0; i < _previewTargets.Count; i++)
            {
                renderService.ApplyTo(_previewTargets[i]);
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

        private static StatusEquipmentCategory IndexToCategory(int index)
        {
            return index switch
            {
                0 => StatusEquipmentCategory.All,
                1 => StatusEquipmentCategory.Weapon,
                2 => StatusEquipmentCategory.Shield,
                3 => StatusEquipmentCategory.Armor,
                4 => StatusEquipmentCategory.Headgear,
                5 => StatusEquipmentCategory.Item,
                _ => StatusEquipmentCategory.Item
            };
        }

        private static bool IsActiveCategory(StatusEquipmentCategory category)
        {
            return ActiveCategories.Contains(category);
        }

        private void Reset()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (tabViews == null || tabViews.Length == 0)
            {
                tabViews = transform.Find("Middle2/StatusTabs")?.GetComponentsInChildren<StatusEquipmentTabView>(true)
                           ?? Array.Empty<StatusEquipmentTabView>();
            }

            if (tabScreens == null || tabScreens.Length == 0)
            {
                var screensRoot = transform.Find("Middle2/StatusScreens");
                if (screensRoot != null)
                {
                    tabScreens = new GameObject[screensRoot.childCount];
                    for (var i = 0; i < screensRoot.childCount; i++)
                    {
                        tabScreens[i] = screensRoot.GetChild(i).gameObject;
                    }
                }
            }

            if (listViews == null || listViews.Length == 0)
            {
                listViews = transform.Find("Middle2/StatusScreens")?.GetComponentsInChildren<StatusEquipmentListView>(true)
                            ?? Array.Empty<StatusEquipmentListView>();
            }

            if (slotEquipmentViews == null || slotEquipmentViews.Length == 0)
            {
                slotEquipmentViews = transform.Find("Middle/Group_Slot")?.GetComponentsInChildren<StatusEquipmentSlotView>(true)
                            .Where(view => !view.name.StartsWith("SlotSkill", StringComparison.Ordinal))
                            .ToArray()
                            ?? Array.Empty<StatusEquipmentSlotView>();
            }

            if (slotSkillViews == null || slotSkillViews.Length == 0)
            {
                slotSkillViews = transform.Find("Middle/Group_Slot")?.GetComponentsInChildren<StatusSkillSlotView>(true)
                            ?? Array.Empty<StatusSkillSlotView>();
            }

            if (playerLevelText == null)
            {
                playerLevelText = transform.Find("Middle/Text_Level")?.GetComponent<TextMeshProUGUI>();
            }

            if (titleText == null)
            {
                titleText = transform.Find("Top/Title_LineDeco_02_s/Group/Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            }

            if (skinButton == null)
            {
                skinButton = FindChildByName(transform, "Button_Skin")?.GetComponentInChildren<Button>(true);
            }

            if (avatarPreviewRenderer == null)
            {
                avatarPreviewRenderer = FindFirstObjectByType<PreviewStage>(FindObjectsInactive.Include);
            }

            if (statusPreviewImage == null)
            {
                statusPreviewImage = FindChildByName(transform, StatusPreviewImageName)?.GetComponent<RawImage>();
            }
        }

        private void ConfigureStatusPreviewImage()
        {
            var texture = ResolvePreviewTexture();
            if (texture == null)
            {
                return;
            }

            var previewRoot = FindChildByName(transform, "Character");
            if (previewRoot == null)
            {
                return;
            }

            var placeholderImage = previewRoot.GetComponent<Image>();
            if (placeholderImage != null)
            {
                placeholderImage.enabled = false;
            }

            statusPreviewImage = ResolveOrCreateStatusPreviewRawImage(previewRoot);

            statusPreviewImage.texture = texture;
            statusPreviewImage.color = Color.white;
            statusPreviewImage.raycastTarget = false;
        }

        private RawImage ResolveOrCreateStatusPreviewRawImage(Transform previewRoot)
        {
            var existing = FindChildByName(previewRoot, StatusPreviewImageName);
            if (existing != null)
            {
                var existingRawImage = existing.GetComponent<RawImage>();
                if (existingRawImage != null)
                {
                    return existingRawImage;
                }
            }

            var gameObject = new GameObject(StatusPreviewImageName, typeof(RectTransform));
            gameObject.layer = previewRoot.gameObject.layer;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(previewRoot, false);
            StretchToFillParent(rectTransform);
            return gameObject.AddComponent<RawImage>();
        }

        private Texture ResolvePreviewTexture()
        {
            if (avatarPreviewRenderer != null)
            {
                var previewCamera = avatarPreviewRenderer.GetComponentInChildren<Camera>(true);
                if (previewCamera != null && previewCamera.targetTexture != null)
                {
                    return previewCamera.targetTexture;
                }
            }

            var rawImages = transform.root.GetComponentsInChildren<RawImage>(true);
            for (var i = 0; i < rawImages.Length; i++)
            {
                if (rawImages[i] != null && rawImages[i].texture != null)
                {
                    return rawImages[i].texture;
                }
            }

            return null;
        }

        private void EnsureSkinButton()
        {
            if (skinButton != null)
            {
                return;
            }

            var skinButtonRoot = FindChildByName(transform, "Button_Skin");
            if (skinButtonRoot == null)
            {
                return;
            }

            skinButton = skinButtonRoot.GetComponent<Button>();
            if (skinButton == null)
            {
                Debug.LogError("StatusScreen: Button_Skin is missing a Button component.", skinButtonRoot);
                return;
            }

            skinButton.targetGraphic ??= skinButtonRoot.GetComponentInChildren<Graphic>(true);
        }

        private static void StretchToFillParent(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            foreach (Transform child in root)
            {
                var result = FindChildByName(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
