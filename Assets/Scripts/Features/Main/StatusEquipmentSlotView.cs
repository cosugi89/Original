using System;
using Assets.Scripts.Core.Const;
using Assets.Scripts.UI.Dialog;
using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private StatusEquipmentCategory category;
        [SerializeField] private PartsEquipmentItem equipmentItem;
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
            if (!hasSelection)
            {
                equipmentItem.ShowEmpty();
                return;
            }

            equipmentItem.ShowNormal();
            equipmentItem.Setup(
                ToUiCategory(category),
                section.SelectedIcon != null ? section.SelectedIcon : fallbackIcon,
                BuildDisplayName(section, category),
                false,
                new UIColors.PlumColorSet());
            equipmentItem.SetFocused(selected);
            equipmentItem.SetChecked(false);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(category);
        }

        private static UICategory ToUiCategory(StatusEquipmentCategory category)
        {
            return category switch
            {
                StatusEquipmentCategory.Weapon => UICategory.HandRight,
                StatusEquipmentCategory.Shield => UICategory.HandLeft,
                StatusEquipmentCategory.Armor => UICategory.Chest,
                StatusEquipmentCategory.Headgear => UICategory.Helmet,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported equipment slot category.")
            };
        }

        private static string BuildDisplayName(
            AvatarPartSectionViewData section,
            StatusEquipmentCategory category)
        {
            if (!string.IsNullOrWhiteSpace(section.SelectedDisplayName))
            {
                return section.SelectedDisplayName;
            }

            return section.SelectedEquipmentId > 0 ? $"Equipment {section.SelectedEquipmentId}" : category.ToString();
        }
    }
}
