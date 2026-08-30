using System;
using Assets.Scripts.Core.Const;
using Assets.Scripts.UI.Dialog;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentListItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private PartsEquipmentItem equipmentItem;
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

            equipmentItem.ShowNormal();
            equipmentItem.Setup(
                option.Category,
                option.Icon != null ? option.Icon : fallbackIcon,
                BuildDisplayName(option),
                false,
                new UIColors.PlumColorSet());
            equipmentItem.SetFocused(option.IsSelected);
            equipmentItem.SetChecked(option.IsSelected);
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
