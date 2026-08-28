using System;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private StatusEquipmentCategory category;
        [SerializeField] private GameObject frameRoot;
        [SerializeField] private GameObject normalState;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject focusState;
        [SerializeField] private GameObject lockState;
        [SerializeField] private GameObject addOneState;
        [SerializeField] private GameObject addTwoState;
        [SerializeField] private GameObject typeIconRoot;
        [SerializeField] private Sprite fallbackIcon;

        private Action<StatusEquipmentCategory> _onClicked;

        public StatusEquipmentCategory Category => category;

        public void Bind(Action<StatusEquipmentCategory> onClicked)
        {
            _onClicked = onClicked;
        }

        public void Refresh(AvatarPartSectionViewData section, bool selected)
        {
            var hasSelection = section != null && section.HasSelection;

            frameRoot.SetActive(true);
            normalState.SetActive(true);
            typeIconRoot.SetActive(true);

            var icon = hasSelection && section.SelectedIcon != null ? section.SelectedIcon : fallbackIcon;
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = icon;
            itemImage.enabled = icon != null;
            itemImage.color = Color.white;
            itemImage.preserveAspect = true;

            label.gameObject.SetActive(true);
            label.text = BuildDisplayName(section, hasSelection, category);

            focusState.SetActive(selected);
            lockState.SetActive(false);
            addOneState.SetActive(!hasSelection);
            addTwoState.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(category);
        }

        private static string BuildDisplayName(
            AvatarPartSectionViewData section,
            bool hasSelection,
            StatusEquipmentCategory category)
        {
            if (!hasSelection)
            {
                return BuildEmptyLabel(category);
            }

            if (!string.IsNullOrWhiteSpace(section.SelectedDisplayName))
            {
                return section.SelectedDisplayName;
            }

            return section.SelectedEquipmentId > 0 ? $"Equipment {section.SelectedEquipmentId}" : BuildEmptyLabel(category);
        }

        private static string BuildEmptyLabel(StatusEquipmentCategory category)
        {
            return category switch
            {
                StatusEquipmentCategory.Weapon => "Weapon",
                StatusEquipmentCategory.Shield => "Shield",
                StatusEquipmentCategory.Armor => "Armor",
                StatusEquipmentCategory.Headgear => "Head",
                _ => string.Empty
            };
        }
    }
}
