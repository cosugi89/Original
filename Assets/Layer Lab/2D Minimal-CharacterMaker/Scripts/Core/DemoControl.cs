using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// Main demo controller that initializes and orchestrates all UI panels,
    /// the player, camera, and color systems on scene start.
    /// </summary>
    public class DemoControl : MonoBehaviour
    {
        private const string PathDiscord = "https://discord.gg/qCsVSHHcY7";
        private const string PathFacebook = "https://www.facebook.com/layerlab";
        private const string PathYoutube = "https://www.youtube.com/@LayerlabGameAssets";
        private const string PathAssetStore = "https://assetstore.unity.com/publishers/5232";

        [Header("Core")] [SerializeField] private Player player;
        [SerializeField] private CameraControl cameraControl;

        [Header("UI Panels")] [SerializeField] private AnimationControl animationControl;
        [SerializeField] private PanelPartsControl panelPartsControl;
        [SerializeField] private PanelPartsListControl panelPartsListControl;
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private ColorPresetManager colorPresetManager;
        [SerializeField] private ColorFavoriteManager colorFavoriteManager;

        /// <summary>
        /// The Player instance managed by this demo controller.
        /// </summary>
        public Player Player => player;

        private void Start()
        {
            player.Init();
            if (cameraControl != null) cameraControl.Init(player.transform);
            if (colorPicker != null) colorPicker.Init(player.PartsManager);
            if (colorPresetManager != null) colorPresetManager.Init(player.PartsManager);
            if (colorFavoriteManager != null) colorFavoriteManager.Init();
            if (panelPartsListControl != null) panelPartsListControl.Init(player.PartsManager);
            if (panelPartsControl != null) panelPartsControl.Init(player.PartsManager, panelPartsListControl);
            if (animationControl != null) animationControl.Init(player.PartsManager);
        }

        /// <summary>
        /// Randomizes all character parts and colors (Skin, Hair, Eye), then refreshes the UI.
        /// </summary>
        public void RandomizeCharacter()
        {
            player.PartsManager.RandomizeAll();
            colorPresetManager.SetRandomColor(ColorTargetType.Skin);
            colorPresetManager.SetRandomColor(ColorTargetType.Hair);
            colorPresetManager.SetRandomColor(ColorTargetType.Beard);

            // 현재 선택된 카테고리의 파츠 리스트 리프레시
            if (panelPartsControl != null)
                panelPartsControl.RefreshCurrentSlot();
        }


        public void OnClickDiscord()
        {
            Application.OpenURL(PathDiscord);
        }

        public void OnClickFacebook()
        {
            Application.OpenURL(PathFacebook);
        }

        public void OnClickYoutube()
        {
            Application.OpenURL(PathYoutube);
        }

        public void OnClickAssetstore()
        {
            Application.OpenURL(PathAssetStore);
        }
    }
}