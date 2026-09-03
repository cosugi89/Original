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
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }

        public void SetSelected(bool isSelected)
        {
            selectedGraphic.SetActive(isSelected);
        }
    }
}
