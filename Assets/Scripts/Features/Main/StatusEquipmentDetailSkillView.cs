using Assets.Scripts.Data.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentDetailSkillView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void Setup(BattleSkillData skill)
        {
            iconImage.sprite = skill.Icon;
            iconImage.enabled = skill.Icon != null;
            nameText.text = skill.DisplayName ?? string.Empty;
            descriptionText.text = skill.Description ?? string.Empty;
        }
    }
}
