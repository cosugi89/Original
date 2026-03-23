using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LayerLab.ArtMakerUnity;
using Assets.Scripts.UI.Dialog;
using Assets.Scripts.Systems.Save;

namespace Assets.Scripts.Features.Home
{
    public class HomeUIScript : MonoBehaviour
    {
        private DialogBackgroundManager _dialogBackgroundManager;

        [Header("Core")]
        [SerializeField] private Player player;
        [SerializeField] private CameraControl cameraControl;
        [SerializeField] private AvatarPreviewRenderer previewRenderer;

        [Header("Canvas")]
        [SerializeField] private Transform modalRoot;
        [SerializeField] private Transform backdropLayer;
        [SerializeField] private Transform dialogLayer;
        [SerializeField] private Button avatarButton;
        [SerializeField] private Button ItemButton;

        [Header("Prefabs")]
        [SerializeField] private AvatarDialog avatarDialog;
        [SerializeField] private ItemDialog itemDialog;

        public Player Player => player;

        private void Awake()
        {
            _dialogBackgroundManager = GetComponent<DialogBackgroundManager>();

            ConfigureModalHierarchy();
            _dialogBackgroundManager?.Configure(backdropLayer);

            avatarButton.onClick.AddListener(OnClickAvatarButton);
            ItemButton.onClick.AddListener(OnClickItemButton);
        }

        private void Start()
        {
            if (player != null)
            {
                player.Init();
                // TODO: Title画面のロード機能と統合する。
                AvatarAppearanceJsonStore.TryApplyTo(player.PartsManager);
            }
        }

        public void OnClickAvatarButton()
        {
            OpenAvatarDialogAsync().Forget();
        }

        public void OnClickItemButton()
        {
            OpenItemDialogAsync().Forget();
        }

        private async UniTaskVoid OpenAvatarDialogAsync()
        {
            bool result = await OpenDialogAsync(avatarDialog);

            if (result)
            {
                Debug.Log("OK");
            }
        }

        private async UniTaskVoid OpenItemDialogAsync()
        {
            bool result = await OpenDialogAsync(itemDialog);

            if (result)
            {
                Debug.Log("OK");
            }
        }

        // TODO: Extract dialog data into a dedicated data class.
        public async UniTask<bool> OpenDialogAsync(DialogBase<bool> dialogPrefab)
        {
            ConfigureModalHierarchy();
            _dialogBackgroundManager?.Configure(backdropLayer);

            var dialogParent = dialogLayer != null ? dialogLayer : (modalRoot != null ? modalRoot : transform);
            var dialog = Instantiate(dialogPrefab, dialogParent);

            if (dialog.transform is RectTransform rectTransform)
            {
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                dialog.transform.localScale = Vector3.one;
                dialog.transform.localRotation = Quaternion.identity;
                dialog.transform.localPosition = Vector3.zero;
            }

            dialog.transform.SetAsLastSibling();
            dialog.SetBackgroundManager(_dialogBackgroundManager);
            dialog.Setup(player, previewRenderer);

            return await dialog.OpenAsync();
        }

        private void ConfigureModalHierarchy()
        {
            StretchToFillParent(modalRoot as RectTransform);

            if (modalRoot != null && backdropLayer != null && backdropLayer.parent != modalRoot)
            {
                backdropLayer.SetParent(modalRoot, false);
            }

            if (modalRoot != null && dialogLayer != null && dialogLayer.parent != modalRoot)
            {
                dialogLayer.SetParent(modalRoot, false);
            }

            StretchToFillParent(backdropLayer as RectTransform);
            StretchToFillParent(dialogLayer as RectTransform);

            if (backdropLayer != null)
            {
                backdropLayer.SetSiblingIndex(0);
            }

            if (dialogLayer != null)
            {
                dialogLayer.SetAsLastSibling();
            }
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
    }
}

