using System;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentListItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject frameRoot;
        [SerializeField] private GameObject normalState;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject focusState;
        [SerializeField] private GameObject checkState;
        [SerializeField] private GameObject lockState;
        [SerializeField] private GameObject disableState;
        [SerializeField] private GameObject addOneState;
        [SerializeField] private GameObject addTwoState;
        [SerializeField] private GameObject typeAreaRoot;
        [SerializeField] private Sprite fallbackIcon;

        private AvatarPartOptionViewData _option;
        private Action<AvatarPartOptionViewData> _onClicked;

        public void Bind(AvatarPartOptionViewData option, Action<AvatarPartOptionViewData> onClicked)
        {
            _option = option;
            _onClicked = onClicked;
            gameObject.SetActive(option != null);

            if (option == null)
            {
                return;
            }

            frameRoot.SetActive(true);
            normalState.SetActive(true);
            typeAreaRoot.SetActive(true);

            var icon = option.Icon != null ? option.Icon : fallbackIcon;
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = icon;
            itemImage.enabled = icon != null;
            itemImage.color = Color.white;
            itemImage.preserveAspect = true;

            label.gameObject.SetActive(true);
            label.text = BuildDisplayName(option);

            focusState.SetActive(option.IsSelected);
            checkState.SetActive(option.IsSelected);
            lockState.SetActive(!option.CanSelect);
            disableState.SetActive(!option.CanSelect);
            addOneState.SetActive(false);
            addTwoState.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_option != null)
            {
                _onClicked?.Invoke(_option);
            }
        }

        private static string BuildDisplayName(AvatarPartOptionViewData option)
        {
            if (!string.IsNullOrWhiteSpace(option.DisplayName))
            {
                return option.DisplayName;
            }

            return option.EquipmentId > 0 ? $"Equipment {option.EquipmentId}" : option.PartType.ToString();
        }
    }
}
