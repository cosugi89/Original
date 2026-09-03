using System;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Core.Const;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusSkillInfoView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedFrame;
        [SerializeField] private GameObject selectionBlocker;
        [SerializeField] private TextMeshProUGUI assignedSlotText;
        [SerializeField] private PartsSkillItem skillItem;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetAction(Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        public void Refresh(
            BattleSkillData skill,
            EquipmentData sourceEquipment,
            bool selected,
            bool blocked,
            int assignedSlotIndex)
        {
            selectedFrame.SetActive(selected);
            selectionBlocker.SetActive(blocked);
            button.interactable = !blocked;
            assignedSlotText.gameObject.SetActive(blocked);
            assignedSlotText.text = $"Assigned to Skill Slot {assignedSlotIndex + 1}";
            skillItem.ShowSkillIconOnly(skill.Icon);
            nameText.text = $"{BuildDisplayName(skill)} · {BuildEquipmentName(sourceEquipment)}";
            descriptionText.text = $"Equipment: {BuildEquipmentName(sourceEquipment)}\n{BuildDescription(skill)}";
        }

        private static string BuildDisplayName(BattleSkillData skill)
        {
            return !string.IsNullOrWhiteSpace(skill.DisplayName)
                ? TextLocalization.Get(skill.DisplayName)
                : $"Skill {skill.SkillId}";
        }

        private static string BuildDescription(BattleSkillData skill)
        {
            return string.IsNullOrWhiteSpace(skill.Description)
                ? "No description."
                : skill.Description;
        }

        private static string BuildEquipmentName(EquipmentData equipment)
        {
            return !string.IsNullOrWhiteSpace(equipment.DisplayName)
                ? TextLocalization.Get(equipment.DisplayName)
                : $"Equipment {equipment.EquipmentId}";
        }
    }
}
