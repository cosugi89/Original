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
        [SerializeField] private SkillFrame skillFrame;
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

        public void Refresh(BattleSkillData skill, bool selected)
        {
            selectedFrame.SetActive(selected);
            skillFrame.ShowNormal(skill.Icon);
            nameText.text = BuildDisplayName(skill);
            descriptionText.text = BuildDescription(skill);
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
    }
}
