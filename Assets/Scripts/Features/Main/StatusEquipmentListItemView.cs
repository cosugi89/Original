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
            ResolveReferences();
            _option = option;
            _onClicked = onClicked;
            gameObject.SetActive(option != null);

            if (option == null)
            {
                return;
            }

            SetActive(frameRoot, true);
            SetActive(normalState, true);
            SetActive(typeAreaRoot, true);

            if (itemImage != null)
            {
                var icon = option.Icon != null ? option.Icon : fallbackIcon;
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = icon;
                itemImage.enabled = icon != null;
                itemImage.color = Color.white;
                itemImage.preserveAspect = true;
            }

            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.text = BuildDisplayName(option);
            }

            SetActive(focusState, option.IsSelected);
            SetActive(checkState, option.IsSelected);
            SetActive(lockState, !option.CanSelect);
            SetActive(disableState, !option.CanSelect);
            SetActive(addOneState, false);
            SetActive(addTwoState, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_option == null)
            {
                return;
            }

            _onClicked?.Invoke(_option);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
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

        private void Reset()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            frameRoot ??= transform.Find("ItemFrame_01")?.gameObject;
            normalState ??= transform.Find("ItemFrame_01/NormalArea")?.gameObject;
            itemImage ??= transform.Find("ItemFrame_01/Item")?.GetComponent<Image>();
            label ??= transform.Find("Text_Level")?.GetComponent<TextMeshProUGUI>();
            focusState ??= transform.Find("ItemFrame_01/Focus")?.gameObject;
            checkState ??= transform.Find("Check")?.gameObject;
            lockState ??= transform.Find("ItemFrame_01/Lock")?.gameObject;
            disableState ??= transform.Find("ItemFrame_01/Disable")?.gameObject;
            addOneState ??= transform.Find("ItemFrame_01/Add_1")?.gameObject;
            addTwoState ??= transform.Find("ItemFrame_01/Add_2")?.gameObject;
            typeAreaRoot ??= transform.Find("TypeArea")?.gameObject;
            CaptureFallbackIcon();
        }

        private void CaptureFallbackIcon()
        {
            if (fallbackIcon == null && itemImage != null && itemImage.sprite != null)
            {
                fallbackIcon = itemImage.sprite;
            }
        }
    }
}
