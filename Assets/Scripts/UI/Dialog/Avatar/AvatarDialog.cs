using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.UI.Dialog
{
    public class AvatarDialog : DialogBase<bool>
    {
        [Header("UI")]
        [SerializeField] private RawImage previewImage;
        [SerializeField] private RenderTexture previewTexture;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button saveButton;

        [Header("Prefab")]
        [SerializeField] private PartsManager characterPrefab;

        [Header("Scripts")]
        [SerializeField] private AnimationControl animationControl;
        [SerializeField] private PanelPartsControl panelPartsControl;
        [SerializeField] private PanelPartsListControl panelPartsListControl;
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private ColorPresetManager colorPresetManager;
        [SerializeField] private ColorFavoriteManager colorFavoriteManager;

        private PartsManager dialogPlayer;
        private Player currentPlayer;
        private AvatarPreviewRenderer previewRenderer;

        public override void Setup(Player player, AvatarPreviewRenderer previewRenderer)
        {
            base.Setup();

            this.currentPlayer = player;
            this.previewRenderer = previewRenderer;

            // PreviewCamera からの出力を反映
            previewImage.texture = previewTexture;

            // Player の生成・反映
            dialogPlayer = previewRenderer.SpawnPreview(characterPrefab);
            dialogPlayer.Init();
            dialogPlayer.CopyFrom(currentPlayer.PartsManager);

            colorPicker.Init(dialogPlayer);
            colorPresetManager.Init(dialogPlayer);
            colorFavoriteManager.Init();
            panelPartsListControl.Init(dialogPlayer);
            panelPartsControl.Init(dialogPlayer, panelPartsListControl);
            animationControl.Init(dialogPlayer);
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

            Close(false);
        }

        private void OnClickSave()
        {
            currentPlayer.PartsManager.Init();
            currentPlayer.PartsManager.CopyFrom(dialogPlayer);
            panelPartsControl.SaveCurrentPreset();
            if (previewRenderer != null)
                previewRenderer.ClearPreview();

            Close(true);
        }

        /// <summary>
        /// 参照なし
        /// </summary>
        public void RandomizeCharacter()
        {
            dialogPlayer.RandomizeAll();

            if (colorPresetManager != null)
            {
                colorPresetManager.SetRandomColor(ColorTargetType.Skin);
                colorPresetManager.SetRandomColor(ColorTargetType.Hair);
                colorPresetManager.SetRandomColor(ColorTargetType.Beard);
            }

            // 現在選択されているカテゴリのパーツリストリフレッシュ
            if (panelPartsControl != null)
            {
                panelPartsControl.RefreshCurrentSlot();
            }
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
