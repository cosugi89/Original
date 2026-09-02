using Assets.Scripts.Data.DTO;
using Assets.Scripts.Core.Const;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentDetailSkillView : MonoBehaviour
    {
        [SerializeField] private RectTransform skillItemRoot;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void Setup(BattleSkillData skill)
        {
            skillItemRoot.GetComponent<PartsSkillItem>().ShowSkillIconOnly(skill.Icon);
            nameText.text = TextLocalization.Get(skill.DisplayName);
            descriptionText.text = TextLocalization.Get(skill.Description);
        }

        public void ShowLocked()
        {
            skillItemRoot.GetComponent<PartsSkillItem>().ShowLockedIconOnly();
        }
    }
}
