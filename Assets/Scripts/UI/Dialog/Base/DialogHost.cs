using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    public sealed class DialogHost : MonoBehaviour
    {
        private const string CanvasName = "DialogCanvas";
        private const string BackdropName = "Backdrop";
        private const string DialogParentName = "DialogParent";
        private const int SortingOrder = 30000;

        private static readonly Color BackdropColor = new Color(10f / 255f, 10f / 255f, 10f / 255f, 0.6f);
        private static DialogHost _instance;

        private readonly List<IDialogHostEntry> _dialogs = new();
        private GameObject _backdropObject;
        private Image _backdropImage;
        private RectTransform _dialogParent;

        public Transform DialogParent => _dialogParent;

        public static DialogHost Resolve()
        {
            if (_instance != null)
            {
                return _instance;
            }

            var existing = FindFirstObjectByType<DialogHost>(FindObjectsInactive.Include);
            if (existing != null)
            {
                _instance = existing;
                _instance.EnsureHierarchy();
                return _instance;
            }

            var layer = LayerMask.NameToLayer("UI");
            if (layer < 0)
            {
                layer = 0;
            }

            var gameObject = new GameObject(
                CanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(DialogHost));
            gameObject.layer = layer;

            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            _instance = gameObject.GetComponent<DialogHost>();
            _instance.EnsureHierarchy();
            return _instance;
        }

        private void Awake()
        {
            _instance = this;
            EnsureHierarchy();
        }

        internal void Push(IDialogHostEntry dialog)
        {
            if (!_dialogs.Contains(dialog))
            {
                _dialogs.Add(dialog);
            }

            dialog.DialogTransform.SetAsLastSibling();
            RefreshBackdropState();
        }

        internal void Remove(IDialogHostEntry dialog)
        {
            if (_dialogs.Remove(dialog))
            {
                RefreshBackdropState();
            }
        }

        internal void RefreshBackdropState()
        {
            EnsureHierarchy();

            var visible = false;
            for (var i = 0; i < _dialogs.Count; i++)
            {
                var dialog = _dialogs[i];
                visible |= dialog.BackdropVisible;
            }

            var interactable = false;
            for (var i = _dialogs.Count - 1; i >= 0; i--)
            {
                var dialog = _dialogs[i];
                if (!dialog.BackdropVisible)
                {
                    continue;
                }

                interactable = dialog.BackdropInteractable;
                break;
            }

            _backdropObject.SetActive(visible);
            _backdropImage.raycastTarget = visible && interactable;
            _backdropObject.transform.SetSiblingIndex(0);
            _dialogParent.SetAsLastSibling();
        }

        internal void OnBackdropClicked()
        {
            for (var i = _dialogs.Count - 1; i >= 0; i--)
            {
                var dialog = _dialogs[i];
                if (!dialog.BackdropVisible || !dialog.BackdropInteractable)
                {
                    continue;
                }

                if (dialog.BackdropCloseEnabled)
                {
                    dialog.OnBackdropClickedFromHost();
                }

                return;
            }
        }

        private void EnsureHierarchy()
        {
            if (_backdropObject == null)
            {
                var backdropTransform = transform.Find(BackdropName) as RectTransform;
                if (backdropTransform == null)
                {
                    var backdrop = new GameObject(BackdropName, typeof(RectTransform), typeof(Image), typeof(DialogBackdropView));
                    backdrop.layer = gameObject.layer;
                    backdropTransform = backdrop.GetComponent<RectTransform>();
                    backdropTransform.SetParent(transform, false);
                    StretchToFillParent(backdropTransform);
                }

                _backdropObject = backdropTransform.gameObject;
                _backdropImage = _backdropObject.GetComponent<Image>();
                _backdropImage.color = BackdropColor;
                _backdropImage.raycastTarget = true;
                _backdropObject.GetComponent<DialogBackdropView>().Initialize(this);
                _backdropObject.SetActive(false);
            }

            if (_dialogParent == null)
            {
                _dialogParent = transform.Find(DialogParentName) as RectTransform;
                if (_dialogParent == null)
                {
                    var parent = new GameObject(DialogParentName, typeof(RectTransform));
                    parent.layer = gameObject.layer;
                    _dialogParent = parent.GetComponent<RectTransform>();
                    _dialogParent.SetParent(transform, false);
                    StretchToFillParent(_dialogParent);
                }
            }

            _backdropObject.transform.SetSiblingIndex(0);
            _dialogParent.SetAsLastSibling();
        }

        private static void StretchToFillParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }
    }
}
