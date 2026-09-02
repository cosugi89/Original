using System;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Core.Const;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusSkillSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private PartsSkillItem skillItem;

        private Action<int> _onClicked;
        public int SlotIndex => slotIndex;

        public void Bind(Action<int> onClicked)
        {
            _onClicked = onClicked;
        }

        public void Refresh(
            BattleSkillData skill,
            StatusEquipmentCategory sourceCategory,
            bool selected)
        {
            var hasSelection = skill != null && skill.SkillId > 0;

            var type = ToSkillItemType(sourceCategory);
            if (hasSelection)
            {
                skillItem.ShowSkill(type, skill.Icon, BuildDisplayName(skill));
            }
            else
            {
                skillItem.ShowEmpty("Empty");
            }

            skillItem.SetFocused(selected);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(slotIndex);
        }

        private static SkillItemType ToSkillItemType(StatusEquipmentCategory category)
        {
            return category switch
            {
                StatusEquipmentCategory.Weapon => SkillItemType.Weapon,
                StatusEquipmentCategory.Shield => SkillItemType.Shield,
                StatusEquipmentCategory.Armor => SkillItemType.Armor,
                StatusEquipmentCategory.Headgear => SkillItemType.Headgear,
                _ => SkillItemType.Weapon
            };
        }

        private static string BuildDisplayName(BattleSkillData skill)
        {
            if (!string.IsNullOrWhiteSpace(skill.DisplayName))
            {
                return TextLocalization.Get(skill.DisplayName);
            }

            return skill.SkillId > 0 ? $"Skill {skill.SkillId}" : "Empty";
        }
    }
}
