using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LayerLab.ArtMakerUnity;
using Assets.Scripts.UI.Dialog;

namespace Assets.Scripts.Features.Home
{
    public class HomeUIScript : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Player player;
        [SerializeField] private CameraControl cameraControl;

        [Header("Canvas")]
        [SerializeField] private Transform dialogRoot;
        [SerializeField] private Button avatarButton;
        [SerializeField] private Button ItemButton;

        [Header("Prefabs")]
        [SerializeField] private AvatarDialog avatarDialog;
        [SerializeField] private ItemDialog itemDialog;

        public Player Player => player;

        private void Awake()
        {
            avatarButton.onClick.AddListener(OnClickAvatarButton);
            ItemButton.onClick.AddListener(OnClickItemButton);
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
            bool result = await OpenDialogAsync(avatarDialog, dialogRoot);

            if (result)
            {
                Debug.Log("OK");
            }
        }

        private async UniTaskVoid OpenItemDialogAsync()
        {
            bool result = await OpenDialogAsync(itemDialog, dialogRoot);

            if (result)
            {
                Debug.Log("OK");
            }
        }

        public async UniTask<bool> OpenDialogAsync(DialogBase<bool> avatarDialog, Transform dialogRoot)
        {
            var dialog = Instantiate(avatarDialog, dialogRoot);

            dialog.transform.localScale = Vector3.one;
            dialog.Setup(player);

            return await dialog.OpenAsync();
        }
    }
}