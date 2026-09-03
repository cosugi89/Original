using System;
using Assets.Scripts.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    public sealed class DialogContext
    {
        public DialogContext(
            PreviewStage avatarPreviewRenderer,
            AppearanceController avatarPreviewTemplate = null)
        {
            AvatarPreviewRenderer = avatarPreviewRenderer;
            AvatarPreviewTemplate = avatarPreviewTemplate;
        }

        public PreviewStage AvatarPreviewRenderer { get; }
        public AppearanceController AvatarPreviewTemplate { get; }
    }

    public enum DialogCloseReason
    {
        CloseButton,
        Backdrop,
        Programmatic
    }

    public interface IDialogRequestHandler<in TRequest>
    {
        void Setup(TRequest request, DialogContext context);
    }

    internal interface IDialogHostEntry
    {
        Transform DialogTransform { get; }
        bool BackdropVisible { get; }
        bool BackdropInteractable { get; }
        bool BackdropCloseEnabled { get; }
        void OnBackdropClickedFromHost();
    }

    public abstract class DialogBase<TResult> : MonoBehaviour, IDialogHostEntry
    {
        [Header("Dialog")]
        [SerializeField] private Graphic dialogTitleGraphic;

        protected UniTaskCompletionSource<TResult> _tcs;
        private DialogHost _host;
        private bool _isClosing;
        private bool _backdropVisible = true;
        private bool _backdropInteractable = true;
        private bool _backdropCloseEnabled = true;

        Transform IDialogHostEntry.DialogTransform => transform;
        bool IDialogHostEntry.BackdropVisible => _backdropVisible;
        bool IDialogHostEntry.BackdropInteractable => _backdropInteractable;
        bool IDialogHostEntry.BackdropCloseEnabled => _backdropCloseEnabled;

        public void SetTitle(string title)
        {
            switch (dialogTitleGraphic)
            {
                case TMP_Text tmpText:
                    tmpText.text = title;
                    break;
                case Text text:
                    text.text = title;
                    break;
                default:
                    throw new InvalidOperationException($"{GetType().Name} title reference must be TextMeshProUGUI or Text.");
            }
        }

        public void SetBackdrop(bool visible, bool interactable = true, bool closeEnabled = true)
        {
            _backdropVisible = visible;
            _backdropInteractable = interactable;
            _backdropCloseEnabled = closeEnabled;
            _host?.RefreshBackdropState();
        }

        public void SetBackdropVisible(bool visible)
        {
            _backdropVisible = visible;
            _host?.RefreshBackdropState();
        }

        public void SetBackdropInteractable(bool interactable)
        {
            _backdropInteractable = interactable;
            _host?.RefreshBackdropState();
        }

        public void SetBackdropCloseEnabled(bool closeEnabled)
        {
            _backdropCloseEnabled = closeEnabled;
        }

        public UniTask<TResult> OpenAsync()
        {
            _tcs = new UniTaskCompletionSource<TResult>();
            _isClosing = false;

            gameObject.SetActive(true);
            _host = DialogHost.Resolve();
            _host.Push(this);

            return _tcs.Task;
        }

        public void RequestClose(DialogCloseReason reason)
        {
            if (_isClosing)
                return;

            OnCloseRequested(reason);
        }

        protected virtual void OnCloseRequested(DialogCloseReason reason)
        {
            OnClickClose();
        }

        protected virtual void OnClickClose()
        {
            Close(default);
        }

        void IDialogHostEntry.OnBackdropClickedFromHost()
        {
            if (_backdropCloseEnabled)
            {
                RequestClose(DialogCloseReason.Backdrop);
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

            _host?.Remove(this);

            RunPopupDestroyAsync(animator).Forget();
        }

        private async UniTaskVoid RunPopupDestroyAsync(Animator animator)
        {
            if (animator != null)
            {
                var closeStarted = false;
                for (var i = 0; i < 30; i++)
                {
                    if (animator == null)
                    {
                        break;
                    }

                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Close"))
                    {
                        closeStarted = true;
                        break;
                    }

                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }

                if (closeStarted)
                {
                    await UniTask.WaitUntil(() =>
                        animator == null ||
                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
                }
            }

            if (this != null)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            _host?.Remove(this);
        }
    }
}
