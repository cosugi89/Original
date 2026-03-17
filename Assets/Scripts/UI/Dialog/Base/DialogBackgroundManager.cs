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

        private void Awake()
        {
            CreateBackgroundIfNeeded();
            _backgroundObject.SetActive(false);
        }

        private void CreateBackgroundIfNeeded()
        {
            if (_backgroundObject != null)
                return;

            _backgroundObject = new GameObject("DialogBackground");
            _backgroundObject.transform.SetParent(dialogRoot, false);

            var rectTransform = _backgroundObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            _backgroundImage = _backgroundObject.AddComponent<Image>();
            _backgroundImage.color = backgroundColor;
            _backgroundImage.raycastTarget = true;

            var backgroundView = _backgroundObject.AddComponent<DialogBackgroundView>();
            backgroundView.Initialize(OnClickBackground);
        }

        public void Push(IDialogBackgroundHandler dialog)
        {
            CreateBackgroundIfNeeded();

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

            var topDialog = _dialogs[_dialogs.Count - 1];

            _backgroundObject.SetActive(true);
            _backgroundObject.transform.SetSiblingIndex(topDialog.CachedTransform.GetSiblingIndex());
        }

        private void OnClickBackground()
        {
            if (_dialogs.Count == 0)
                return;

            var topDialog = _dialogs[_dialogs.Count - 1];
            topDialog.OnBackgroundClickedFromManager();
        }
    }
}
