using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Components;
using LayerLab.ArtMakerUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    [Serializable]
    public sealed class AvatarAppearanceCategoryIcon
    {
        [SerializeField] private UICategory category;
        [SerializeField] private Sprite icon;

        public UICategory Category => category;
        public Sprite Icon => icon;
    }

    public readonly struct AvatarDialogRequest
    {
        public AvatarDialogRequest(Player player)
        {
            SourcePartsManager = player != null ? player.AppearanceController : null;
            SaveTargets = null;
        }

        public AvatarDialogRequest(AppearanceController sourcePartsManager, IReadOnlyList<AppearanceController> saveTargets = null)
        {
            SourcePartsManager = sourcePartsManager;
            SaveTargets = saveTargets;
        }

        public AppearanceController SourcePartsManager { get; }
        public IReadOnlyList<AppearanceController> SaveTargets { get; }
    }

    public readonly struct AvatarDialogResult
    {
        private AvatarDialogResult(bool isSaved) => IsSaved = isSaved;

        public bool IsSaved { get; }
        public static AvatarDialogResult Saved => new(true);
        public static AvatarDialogResult Cancelled => new(false);
    }

    [DialogAddress("Dialog/AvatarController")]
    public class AvatarDialog : DialogBase<AvatarDialogResult>, IDialogRequestHandler<AvatarDialogRequest>
    {
        [Header("UI")]
        [SerializeField] private RawImage previewImage;
        [SerializeField] private RenderTexture previewTexture;
        [SerializeField] private GameButton closeButton;
        [SerializeField] private GameButton saveButton;
        [SerializeField] private Transform categoryContent;
        [SerializeField] private AvatarAppearanceSelectionItemView categorySlotTemplate;
        [SerializeField] private Transform partOptionContent;
        [SerializeField] private AvatarAppearanceSelectionItemView partOptionSlotTemplate;
        [SerializeField] private TextMeshProUGUI emptyText;
        [SerializeField] private AvatarAppearanceCategoryIcon[] categoryFallbackIcons;
        [SerializeField] private ColorSelectScrollUIScript colorPicker;

        [Header("Prefab")]
        [SerializeField] private AppearanceController characterPrefab;

        [Header("Preview Placement")]
        [SerializeField] private Vector3 previewLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 previewLocalScale = Vector3.one;

        private AppearanceController dialogPlayer;
        private PreviewHandle dialogPreviewHandle;
        private AppearanceController sourcePartsManager;
        private IReadOnlyList<AppearanceController> saveTargets;
        private PreviewStage previewRenderer;
        private AvatarEditorPresenter avatarEditorPresenter;
        private UICategory selectedCategory;
        private readonly List<AvatarAppearanceSelectionItemView> categorySlots = new();
        private readonly List<AvatarAppearanceSelectionItemView> partOptionSlots = new();

        public void Setup(AvatarDialogRequest request, DialogContext context)
        {
            sourcePartsManager = request.SourcePartsManager;
            saveTargets = request.SaveTargets;
            previewRenderer = context.AvatarPreviewRenderer;
            previewImage.texture = previewTexture;

            var placement = new PreviewPlacement(previewLocalPosition, Quaternion.identity, previewLocalScale);
            dialogPreviewHandle = previewRenderer.Spawn(characterPrefab, placement);
            dialogPlayer = dialogPreviewHandle.Get<AppearanceController>();
            dialogPlayer.Init();

            avatarEditorPresenter = AvatarEditorPresenter.CreateDefault(sourcePartsManager, AvatarEditorMode.AppearanceOnly);
            avatarEditorPresenter.BindPreview(dialogPlayer);

            colorPicker.Init(dialogPlayer, avatarEditorPresenter);
            BindButtons();
            CreateCategorySlots();
        }

        protected override void OnClickClose()
        {
            DespawnDialogPreview();
            Close(AvatarDialogResult.Cancelled);
        }

        private void BindButtons()
        {
            closeButton.onClick.AddListener(OnClickClose);
            saveButton.onClick.AddListener(OnClickSave);

        }

        private void CreateCategorySlots()
        {
            categorySlotTemplate.gameObject.SetActive(false);
            var sections = avatarEditorPresenter.GetSections();
            foreach (var section in sections)
            {
                var category = section.Category;
                var slot = Instantiate(categorySlotTemplate, categoryContent);
                categorySlots.Add(slot);
                slot.Show(ResolveCategoryIcon(section), false, true, () => SelectCategory(category));
            }

            SelectCategory(sections[0].Category);
        }

        private void SelectCategory(UICategory category)
        {
            selectedCategory = category;
            var section = avatarEditorPresenter.GetSection(category);

            var sections = avatarEditorPresenter.GetSections();
            for (var i = 0; i < categorySlots.Count; i++)
            {
                var slotCategory = sections[i].Category;
                categorySlots[i].Show(
                    ResolveCategoryIcon(sections[i]),
                    slotCategory == selectedCategory,
                    true,
                    () => SelectCategory(slotCategory));
            }

            var options = section.Options;
            emptyText.gameObject.SetActive(options.Count == 0);
            if (options.Count == 0)
            {
                emptyText.text = "No options available";
            }

            partOptionSlotTemplate.gameObject.SetActive(false);
            while (partOptionSlots.Count < options.Count)
            {
                partOptionSlots.Add(Instantiate(partOptionSlotTemplate, partOptionContent));
            }

            for (var i = 0; i < partOptionSlots.Count; i++)
            {
                if (i >= options.Count)
                {
                    partOptionSlots[i].gameObject.SetActive(false);
                    continue;
                }
                var option = options[i];
                var optionSlot = partOptionSlots[i];
                optionSlot.Show(option.Icon, option.IsSelected, option.CanSelect, () => SelectOption(option.EquipmentId));
            }

            colorPicker.gameObject.SetActive(section.SupportsColor);
            if (section.SupportsColor)
            {
                colorPicker.SetTarget(section.ColorTarget);
            }
        }

        private void SelectOption(int equipmentId)
        {
            if (avatarEditorPresenter.TrySelectEquipment(equipmentId))
            {
                SelectCategory(selectedCategory);
            }
        }

        private Sprite ResolveCategoryIcon(AvatarPartSectionViewData section)
        {
            if (section.SelectedIcon != null)
            {
                return section.SelectedIcon;
            }

            var optionIcon = section.Options
                .Select(option => option.Icon)
                .FirstOrDefault(icon => icon != null);
            if (optionIcon != null)
            {
                return optionIcon;
            }

            return categoryFallbackIcons
                .Where(candidate => candidate != null && candidate.Category == section.Category)
                .Select(candidate => candidate.Icon)
                .FirstOrDefault(icon => icon != null);
        }

        private void OnClickSave()
        {
            avatarEditorPresenter.Commit();
            ApplyCurrentAppearanceToSaveTargets();
            UserDataManager.EnsureInitialized().UserDataStore.SaveSession();
            DespawnDialogPreview();
            Close(AvatarDialogResult.Saved);
        }

        private void ApplyCurrentAppearanceToSaveTargets()
        {
            var renderService = AppearanceAdapter.EnsureInitialized();
            foreach (var target in ResolveSaveTargets())
            {
                renderService.ApplyTo(target);
            }
        }

        private IReadOnlyList<AppearanceController> ResolveSaveTargets()
        {
            if (saveTargets != null && saveTargets.Count > 0)
            {
                return saveTargets.Where(target => target != null).Distinct().ToArray();
            }

            return sourcePartsManager != null ? new[] { sourcePartsManager } : Array.Empty<AppearanceController>();
        }

        private void DespawnDialogPreview()
        {
            if (previewRenderer != null && dialogPreviewHandle != null)
            {
                previewRenderer.Despawn(dialogPreviewHandle);
            }

            dialogPreviewHandle = null;
            dialogPlayer = null;
        }
    }
}
