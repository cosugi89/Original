using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    public sealed class DialogContext
    {
        public DialogContext(AvatarPreviewRenderer avatarPreviewRenderer)
        {
            AvatarPreviewRenderer = avatarPreviewRenderer;
        }

        public AvatarPreviewRenderer AvatarPreviewRenderer { get; }
    }

    public interface IDialogRequestHandler<in TRequest>
    {
        void Setup(TRequest request, DialogContext context);
    }

    public abstract class DialogBase<TResult> : MonoBehaviour, IDialogBackgroundHandler
    {
        [Header("Background")]
        [SerializeField] private bool useBackground = true;
        [SerializeField] private bool closeOnBackgroundClick = true;

        protected UniTaskCompletionSource<TResult> _tcs;
        private bool _isClosing;
        private DialogBackgroundManager _backgroundManager;

        public Transform CachedTransform => transform;
        public bool UseBackground => useBackground;
        public bool CloseOnBackgroundClick => closeOnBackgroundClick;

        public void SetBackgroundManager(DialogBackgroundManager backgroundManager)
        {
            _backgroundManager = backgroundManager;
        }

        public UniTask<TResult> OpenAsync()
        {
            _tcs = new UniTaskCompletionSource<TResult>();
            _isClosing = false;

            gameObject.SetActive(true);

            if (useBackground)
            {
                _backgroundManager ??= GetComponentInParent<DialogBackgroundManager>();
                if (_backgroundManager != null)
                {
                    _backgroundManager.Push(this);
                }
            }

            return _tcs.Task;
        }

        protected virtual void OnClickClose()
        {
            Close(default);
        }

        public void OnBackgroundClickedFromManager()
        {
            if (closeOnBackgroundClick)
            {
                OnClickClose();
            }
        }

        protected void Close(TResult result)
        {
            if (_isClosing)
                return;

            _isClosing = true;
            _tcs?.TrySetResult(result);

            var animator = GetComponent<Animator>();
            if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
                animator.Play("Close");

            if (useBackground && _backgroundManager != null)
            {
                _backgroundManager.Remove(this);
            }

            RunPopupDestroyAsync(animator).Forget();
        }

        private async UniTaskVoid RunPopupDestroyAsync(Animator animator)
        {
            if (animator != null)
            {
                await UniTask.WaitUntil(() =>
                    animator.GetCurrentAnimatorStateInfo(0).IsName("Close"));

                await UniTask.WaitUntil(() =>
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            }

            Destroy(gameObject);
        }
    }
}
