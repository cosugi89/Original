using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// デッキ一覧内の1行を表示し、選択入力だけをDialogへ返すView。
    /// </summary>
    public sealed class DeckLoadoutItemView : MonoBehaviour
    {
        [SerializeField] private FeedbackButton selectButton;
        [SerializeField] private GameObject selectedFrame; // 選択中のデッキに表示する枠
        [SerializeField] private RawImage avatarPreviewImage;
        [SerializeField] private TextMeshProUGUI totalAttackText;
        [SerializeField] private TextMeshProUGUI totalDefenseText;
        [SerializeField] private PartsSkillItem skill1;
        [SerializeField] private PartsSkillItem skill2;
        [SerializeField] private PartsSkillItem skill3;
        [SerializeField] private PartsSkillItem skill4;

        public void Setup(
            DeckData deck,
            IReadOnlyList<BattleSkillData> candidateSkills,
            bool isSelected,
            Action<int> onClick)
        {
            selectedFrame.SetActive(isSelected);
            selectButton.SetAction(() => onClick?.Invoke(deck.DeckId));

            var skillsById = (candidateSkills ?? Array.Empty<BattleSkillData>())
                .Where(skill => skill != null && skill.SkillId > 0)
                .GroupBy(skill => skill.SkillId)
                .ToDictionary(group => group.Key, group => group.First());
            var skillIds = deck.SkillIds ?? new List<int>();
            SetupSkill(skill1, skillIds, 0, skillsById);
            SetupSkill(skill2, skillIds, 1, skillsById);
            SetupSkill(skill3, skillIds, 2, skillsById);
            SetupSkill(skill4, skillIds, 3, skillsById);

            totalAttackText.text = "--";
            totalDefenseText.text = "--";
            avatarPreviewImage.raycastTarget = false;

            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text.name == "Text_Level")
                    text.gameObject.SetActive(false);
            }
        }

        public void SetAvatarPreview(Texture previewTexture)
        {
            avatarPreviewImage.texture = previewTexture;
            avatarPreviewImage.color = Color.white;
        }

        private static void SetupSkill(
            PartsSkillItem skillItem,
            IReadOnlyList<int> skillIds,
            int slotIndex,
            IReadOnlyDictionary<int, BattleSkillData> skillsById)
        {
            var skillId = slotIndex < skillIds.Count ? skillIds[slotIndex] : 0;
            if (skillId > 0 && skillsById.TryGetValue(skillId, out var skill))
            {
                skillItem.ShowSkillIconOnly(skill.Icon);
                return;
            }

            skillItem.ShowEmptyIconOnly();
        }
    }
}
