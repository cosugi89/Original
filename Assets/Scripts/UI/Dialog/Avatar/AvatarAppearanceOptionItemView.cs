using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    public sealed class AvatarAppearanceOptionItemView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject selectedGraphic;

        private UnityAction _action;

        private void Awake()
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        public void Setup(Sprite sprite, bool isSelected, bool isInteractable, UnityAction onClick)
        {
            gameObject.SetActive(true);
            if (sprite != null)
            {
                icon.sprite = sprite;
            }
            icon.color = Color.white;
            selectedGraphic.SetActive(isSelected);
            button.interactable = isInteractable;
            _action = onClick;
        }

        private void OnClick()
        {
            _action?.Invoke();
        }

        public void SetSelected(bool isSelected)
        {
            selectedGraphic.SetActive(isSelected);
        }
    }
}
