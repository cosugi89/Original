using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI.Dialog
{
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
        private AvatarDialogResult(bool isSaved)
        {
            IsSaved = isSaved;
        }

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
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;

        [Header("Prefab")]
        [SerializeField] private AppearanceController characterPrefab;

        [Header("Preview Placement")]
        [SerializeField] private Vector3 previewLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 previewLocalScale = Vector3.one;

        [Header("Scripts")]
        [SerializeField] private PanelPartsControl panelPartsControl;
        [SerializeField] private PanelPartsListControl panelPartsListControl;
        [SerializeField] private ColorSelectScrollUIScript colorPicker;

        private AppearanceController dialogPlayer;
        private PreviewHandle dialogPreviewHandle;
        private AppearanceController sourcePartsManager;
        private IReadOnlyList<AppearanceController> saveTargets;
        private PreviewStage previewRenderer;
        private AvatarEditorPresenter avatarEditorPresenter;

        public void Setup(AvatarDialogRequest request, DialogContext context)
        {
            sourcePartsManager = request.SourcePartsManager;
            saveTargets = request.SaveTargets;
            previewRenderer = context.AvatarPreviewRenderer;

            // PreviewCamera からの出力を反映
            previewImage.texture = previewTexture;

            // Player の生成・反映
            var placement = new PreviewPlacement(previewLocalPosition, Quaternion.identity, previewLocalScale);
            dialogPreviewHandle = previewRenderer.Spawn(characterPrefab, placement);
            dialogPlayer = dialogPreviewHandle?.Get<AppearanceController>();
            if (dialogPlayer != null)
            {
                dialogPlayer.Init();
            }

            avatarEditorPresenter = AvatarEditorPresenter.CreateDefault(sourcePartsManager);
            if (dialogPlayer != null)
            {
                avatarEditorPresenter.BindPreview(dialogPlayer);
            }

            colorPicker.Init(dialogPlayer, avatarEditorPresenter);
            panelPartsListControl.Init(dialogPlayer, avatarEditorPresenter);
            panelPartsControl.Init(dialogPlayer, panelPartsListControl, avatarEditorPresenter);
            closeButton.onClick.AddListener(OnClickClose);
            saveButton.onClick.AddListener(OnClickSave);
            panelPartsControl.RefreshSelectionFrame();
            panelPartsListControl.RefreshSelectionFrame();
            RefreshSelectionFramesDeferred().Forget();
        }

        protected override void OnClickClose()
        {
            DespawnDialogPreview();

            Close(AvatarDialogResult.Cancelled);
        }

        private void OnClickSave()
        {
            avatarEditorPresenter?.Commit();
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
                return saveTargets
                    .Where(target => target != null)
                    .Distinct()
                    .ToArray();
            }

            return sourcePartsManager != null
                ? new[] { sourcePartsManager }
                : System.Array.Empty<AppearanceController>();
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

        private async UniTaskVoid RefreshSelectionFramesDeferred()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            if (this == null)
                return;

            panelPartsControl?.RefreshSelectionFrame();
            panelPartsListControl?.RefreshSelectionFrame();
        }
    }
}
