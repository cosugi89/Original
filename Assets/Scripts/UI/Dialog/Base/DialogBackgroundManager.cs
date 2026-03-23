using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    public class DialogBackgroundManager : MonoBehaviour
    {
        [SerializeField] private Transform dialogRoot;
        [SerializeField] private Color backgroundColor = new Color(10f / 255f, 10f / 255f, 10f / 255f, 0.6f);

        private readonly List<IDialogBackgroundHandler> _dialogs = new();

        private GameObject _backgroundObject;
        private Image _backgroundImage;
        private Transform _backgroundLayer;

        public void Configure(Transform backgroundLayer)
        {
            _backgroundLayer = backgroundLayer;

            if (_backgroundObject == null || _backgroundLayer == null)
                return;

            _backgroundObject.transform.SetParent(_backgroundLayer, false);
            StretchToFillParent(_backgroundObject.GetComponent<RectTransform>());
            _backgroundObject.transform.SetAsLastSibling();
        }

        private void CreateBackgroundIfNeeded()
        {
            if (_backgroundObject != null)
                return;

            var parent = ResolveBackgroundParent();
            if (parent == null)
            {
                Debug.LogWarning("Dialog background parent is not configured.", this);
                return;
            }

            _backgroundObject = new GameObject("DialogBackground");
            _backgroundObject.transform.SetParent(parent, false);

            var rectTransform = _backgroundObject.AddComponent<RectTransform>();
            StretchToFillParent(rectTransform);

            _backgroundImage = _backgroundObject.AddComponent<Image>();
            _backgroundImage.color = backgroundColor;
            _backgroundImage.raycastTarget = true;

            var backgroundView = _backgroundObject.AddComponent<DialogBackgroundView>();
            backgroundView.Initialize(OnClickBackground);
            _backgroundObject.transform.SetAsLastSibling();
            _backgroundObject.SetActive(false);
        }

        public void Push(IDialogBackgroundHandler dialog)
        {
            CreateBackgroundIfNeeded();
            if (_backgroundObject == null)
                return;

            if (_dialogs.Contains(dialog))
                return;

            _dialogs.Add(dialog);
            RefreshBackgroundState();
        }

        public void Remove(IDialogBackgroundHandler dialog)
        {
            if (!_dialogs.Remove(dialog))
                return;

            RefreshBackgroundState();
        }

        private void RefreshBackgroundState()
        {
            if (_dialogs.Count == 0)
            {
                _backgroundObject.SetActive(false);
                return;
            }

            _backgroundObject.SetActive(true);
            _backgroundObject.transform.SetAsLastSibling();
        }

        private void OnClickBackground()
        {
            if (_dialogs.Count == 0)
                return;

            var topDialog = _dialogs[_dialogs.Count - 1];
            topDialog.OnBackgroundClickedFromManager();
        }

        private Transform ResolveBackgroundParent()
        {
            return _backgroundLayer != null ? _backgroundLayer : dialogRoot;
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
