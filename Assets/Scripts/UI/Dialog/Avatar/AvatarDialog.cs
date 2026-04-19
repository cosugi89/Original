using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;

namespace Assets.Scripts.UI.Dialog
{
    public readonly struct AvatarDialogRequest
    {
        public AvatarDialogRequest(Player player)
        {
            Player = player;
        }

        public Player Player { get; }
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

    public class AvatarDialog : DialogBase<AvatarDialogResult>, IDialogRequestHandler<AvatarDialogRequest>
    {
        [Header("UI")]
        [SerializeField] private RawImage previewImage;
        [SerializeField] private RenderTexture previewTexture;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;

        [Header("Prefab")]
        [SerializeField] private PartsManager characterPrefab;

        [Header("Scripts")]
        [SerializeField] private PanelPartsControl panelPartsControl;
        [SerializeField] private PanelPartsListControl panelPartsListControl;
        [SerializeField] private ColorSelectScrollUIScript colorPicker;

        private PartsManager dialogPlayer;
        private PreviewHandle dialogPreviewHandle;
        private Player currentPlayer;
        private PreviewStage previewRenderer;
        private AvatarEditorPresenter avatarEditorPresenter;

        public void Setup(AvatarDialogRequest request, DialogContext context)
        {
            currentPlayer = request.Player;
            previewRenderer = context.AvatarPreviewRenderer;

            // PreviewCamera からの出力を反映
            previewImage.texture = previewTexture;

            // Player の生成・反映
            dialogPreviewHandle = previewRenderer.Spawn(characterPrefab);
            dialogPlayer = dialogPreviewHandle?.Get<PartsManager>();
            dialogPlayer.Init();
            avatarEditorPresenter = AvatarEditorPresenter.CreateDefault(currentPlayer?.PartsManager);
            avatarEditorPresenter.BindPreview(dialogPlayer);

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
            AvatarRenderService.EnsureInitialized().ApplyTo(currentPlayer.PartsManager);
            GameSaveService.EnsureInitialized().SaveSession();
            DespawnDialogPreview();

            Close(AvatarDialogResult.Saved);
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
