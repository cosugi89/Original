using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Core;

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
        private Player currentPlayer;
        private AvatarPreviewRenderer previewRenderer;

        public void Setup(AvatarDialogRequest request, DialogContext context)
        {
            currentPlayer = request.Player;
            previewRenderer = context.AvatarPreviewRenderer;

            // PreviewCamera からの出力を反映
            previewImage.texture = previewTexture;

            // Player の生成・反映
            dialogPlayer = previewRenderer.SpawnPreview(characterPrefab);
            dialogPlayer.Init();
            dialogPlayer.CopyFrom(currentPlayer.PartsManager);

            colorPicker.Init(dialogPlayer);
            panelPartsListControl.Init(dialogPlayer);
            panelPartsControl.Init(dialogPlayer, panelPartsListControl);
            closeButton.onClick.AddListener(OnClickClose);
            saveButton.onClick.AddListener(OnClickSave);
            panelPartsControl.RefreshSelectionFrame();
            panelPartsListControl.RefreshSelectionFrame();
            RefreshSelectionFramesDeferred().Forget();
        }

        protected override void OnClickClose()
        {
            if (previewRenderer != null)
                previewRenderer.ClearPreview();

            Close(AvatarDialogResult.Cancelled);
        }

        private void OnClickSave()
        {
            currentPlayer.PartsManager.Init();
            currentPlayer.PartsManager.CopyFrom(dialogPlayer);
            panelPartsControl.SaveCurrentAppearance();
            if (previewRenderer != null)
                previewRenderer.ClearPreview();

            Close(AvatarDialogResult.Saved);
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
