using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    public class AvatarDialog : DialogBase<bool>
    {
        [Header("UI")]
        [SerializeField] private Button closeButton;

        [Header("Scripts")]
        [SerializeField] private AnimationControl animationControl;
        [SerializeField] private PanelPartsControl panelPartsControl;
        [SerializeField] private PanelPartsListControl panelPartsListControl;
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private ColorPresetManager colorPresetManager;
        [SerializeField] private ColorFavoriteManager colorFavoriteManager;

        private Player currentPlayer;

        public override void Setup(Player player)
        {
            base.Setup();
            currentPlayer = player;
            currentPlayer.Init();
            //cameraControl.Init(player.transform); // 別場所に移動

            colorPicker.Init(player.PartsManager);
            colorPresetManager.Init(player.PartsManager);
            colorFavoriteManager.Init();
            panelPartsListControl.Init(player.PartsManager);
            panelPartsControl.Init(player.PartsManager, panelPartsListControl);
            animationControl.Init(player.PartsManager);
            closeButton.onClick.AddListener(OnClickClose);
        }

        protected override void OnClickClose()
        {
            Close(false);
        }

        /// <summary>
        /// 参照なし
        /// </summary>
        public void RandomizeCharacter()
        {
            currentPlayer.PartsManager.RandomizeAll();

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
    }
}