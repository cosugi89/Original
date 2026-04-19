using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Assets.Scripts.Systems.GameData;
using LayerLab.ArtMakerUnity;
using Assets.Scripts.UI.Dialog;

namespace Assets.Scripts.Features.Home
{
    public class HomeUIScript : MonoBehaviour
    {
        private DialogBackgroundManager _dialogBackgroundManager;

        [Header("Core")]
        [SerializeField] private Player player;
        [SerializeField] private CameraControl cameraControl;
        [SerializeField] private PreviewStage previewRenderer;

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
                var avatarRenderService = AvatarRenderService.EnsureInitialized();
                avatarRenderService.SyncSessionFromRendererIfNeeded(player.PartsManager, saveAfterSync: true);
                avatarRenderService.ApplyTo(player.PartsManager);
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
            var request = new AvatarDialogRequest(player);
            AvatarDialogResult result = await OpenDialogAsync<AvatarDialog, AvatarDialogRequest, AvatarDialogResult>(avatarDialog, request);

            if (result.IsSaved)
            {
                Debug.Log("OK");
            }
        }

        private async UniTaskVoid OpenItemDialogAsync()
        {
            var request = new ItemDialogRequest();
            ItemDialogResult result = await OpenDialogAsync<ItemDialog, ItemDialogRequest, ItemDialogResult>(itemDialog, request);

            if (result.IsConfirmed)
            {
                Debug.Log("OK");
            }
        }

        public async UniTask<TResult> OpenDialogAsync<TDialog, TRequest, TResult>(TDialog dialogPrefab, TRequest request)
            where TDialog : DialogBase<TResult>, IDialogRequestHandler<TRequest>
        {
            ConfigureModalHierarchy();
            _dialogBackgroundManager?.Configure(backdropLayer);

            var dialogParent = dialogLayer != null ? dialogLayer : (modalRoot != null ? modalRoot : transform);
            var dialog = Instantiate(dialogPrefab, dialogParent);
            var dialogContext = new DialogContext(previewRenderer);

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
            dialog.Setup(request, dialogContext);

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

