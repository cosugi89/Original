using System.Linq;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct StatusEquipmentDetailDialogRequest
    {
        public StatusEquipmentDetailDialogRequest(AvatarPartOptionViewData option)
        {
            Option = option;
        }

        public AvatarPartOptionViewData Option { get; }
    }

    public readonly struct StatusEquipmentDetailDialogResult
    {
        private StatusEquipmentDetailDialogResult(bool isEquipRequested)
        {
            IsEquipRequested = isEquipRequested;
        }

        public bool IsEquipRequested { get; }

        public static StatusEquipmentDetailDialogResult EquipRequested => new(true);
        public static StatusEquipmentDetailDialogResult Cancelled => new(false);
    }

    [DialogAddress("Dialog/StatusEquipmentDetail")]
    public sealed class StatusEquipmentDetailDialog :
        DialogBase<StatusEquipmentDetailDialogResult>,
        IDialogRequestHandler<StatusEquipmentDetailDialogRequest>
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI metaText;
        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private GameButton equipButton;
        [SerializeField] private GameButton closeButton;

        private AvatarPartOptionViewData _option;

        public void Setup(StatusEquipmentDetailDialogRequest request, DialogContext context)
        {
            _option = request.Option;
            Refresh();

            closeButton.onClick.RemoveListener(OnClickClose);
            closeButton.onClick.AddListener(OnClickClose);

            equipButton.onClick.RemoveListener(OnClickEquip);
            equipButton.onClick.AddListener(OnClickEquip);
        }

        protected override void OnClickClose()
        {
            Close(StatusEquipmentDetailDialogResult.Cancelled);
        }

        private void OnClickEquip()
        {
            if (_option == null || !_option.CanSelect || _option.IsSelected)
            {
                return;
            }

            Close(StatusEquipmentDetailDialogResult.EquipRequested);
        }

        private void Refresh()
        {
            if (_option == null)
            {
                return;
            }

            titleText.text = BuildDisplayName(_option);
            metaText.text = $"{_option.PartType}\nID {_option.EquipmentId}";
            detailText.text = BuildDetailText(_option);

            iconImage.sprite = _option.Icon;
            iconImage.enabled = _option.Icon != null;

            var canEquip = _option.CanSelect && !_option.IsSelected;
            equipButton.SetInteractable(canEquip);
            equipButton.SetLabel(!_option.CanSelect ? "Locked" : (_option.IsSelected ? "Equipped" : "Equip"));
        }

        private static string BuildDisplayName(AvatarPartOptionViewData option)
        {
            if (!string.IsNullOrWhiteSpace(option.DisplayName))
            {
                return option.DisplayName;
            }

            return option.EquipmentId > 0 ? $"Equipment {option.EquipmentId}" : option.PartType.ToString();
        }

        private static string BuildDetailText(AvatarPartOptionViewData option)
        {
            var tags = option.CategoryTags != null && option.CategoryTags.Count > 0
                ? string.Join(", ", option.CategoryTags.Where(tag => !string.IsNullOrWhiteSpace(tag)))
                : "-";
            var skills = option.AssignableSkillNames != null && option.AssignableSkillNames.Count > 0
                ? string.Join(", ", option.AssignableSkillNames.Where(skill => !string.IsNullOrWhiteSpace(skill)))
                : "-";
            var danceSkill = string.IsNullOrWhiteSpace(option.DanceSkillName) ? "-" : option.DanceSkillName;
            var state = option.IsSelected ? "Equipped" : "Not Equipped";

            return $"Category: {option.Category}\nState: {state}\nTags: {tags}\nSkills: {skills}\nDance: {danceSkill}";
        }
    }
}
