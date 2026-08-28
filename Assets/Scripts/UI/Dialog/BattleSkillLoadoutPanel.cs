using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Dialog
{
    /// <summary>
    /// 装備中の候補スキルから、戦闘用4枠ロードアウトを選ぶための最小パネル。
    /// </summary>
    public sealed class BattleSkillLoadoutPanel : MonoBehaviour
    {
        [SerializeField] private Button[] slotButtons = Array.Empty<Button>();
        [SerializeField] private Text[] slotLabels = Array.Empty<Text>();
        [SerializeField] private Button[] candidateButtons = Array.Empty<Button>();
        [SerializeField] private Text[] candidateLabels = Array.Empty<Text>();
        [SerializeField] private Text skillDetailText;
        [SerializeField] private Button clearSlotButton;
        [SerializeField] private Button saveButton;

        private readonly List<BattleSkillData> _candidates = new();
        private readonly List<int> _draftSkillIds = new();
        private SkillLoadoutManager _loadoutService;
        private int _selectedSlotIndex;

        private void Awake()
        {
            _loadoutService = UserDataManager.EnsureInitialized().SkillLoadout;
            BindButtons();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            _loadoutService ??= UserDataManager.EnsureInitialized().SkillLoadout;
            _candidates.Clear();
            _candidates.AddRange(_loadoutService.GetCandidateSkillsFromEquippedEquipment());

            _draftSkillIds.Clear();
            _draftSkillIds.AddRange(_loadoutService.EnsureUsableLoadout(persistImmediately: true));
            NormalizeDraft();
            _selectedSlotIndex = Mathf.Clamp(
                _selectedSlotIndex,
                0,
                UserProfileData.BattleSkillLoadoutSlotCount - 1);

            RefreshSlotLabels();
            RefreshCandidateLabels();
            RefreshSkillDetail();
        }

        private void BindButtons()
        {
            for (var i = 0; i < slotButtons.Length; i++)
            {
                var index = i;
                if (slotButtons[i] != null)
                {
                    slotButtons[i].onClick.AddListener(() => SelectSlot(index));
                }
            }

            for (var i = 0; i < candidateButtons.Length; i++)
            {
                var index = i;
                if (candidateButtons[i] != null)
                {
                    candidateButtons[i].onClick.AddListener(() => SelectCandidate(index));
                }
            }

            if (clearSlotButton != null)
            {
                clearSlotButton.onClick.AddListener(ClearSelectedSlot);
            }

            if (saveButton != null)
            {
                saveButton.onClick.AddListener(Save);
            }
        }

        private void SelectSlot(int index)
        {
            if (index < 0 || index >= UserProfileData.BattleSkillLoadoutSlotCount)
            {
                return;
            }

            _selectedSlotIndex = index;
            RefreshSlotLabels();
            RefreshSkillDetail();
        }

        private void SelectCandidate(int index)
        {
            if (index < 0 || index >= _candidates.Count)
            {
                return;
            }

            NormalizeDraft();
            _draftSkillIds[_selectedSlotIndex] = _candidates[index].SkillId;
            RefreshSlotLabels();
            RefreshSkillDetail();
        }

        private void ClearSelectedSlot()
        {
            NormalizeDraft();
            _draftSkillIds[_selectedSlotIndex] = 0;
            RefreshSlotLabels();
            RefreshSkillDetail();
        }

        private void Save()
        {
            NormalizeDraft();
            _loadoutService.SaveLoadout(_draftSkillIds);
            Refresh();
        }

        private void RefreshSlotLabels()
        {
            for (var i = 0; i < slotLabels.Length; i++)
            {
                if (slotLabels[i] == null)
                {
                    continue;
                }

                var skillId = i < _draftSkillIds.Count ? _draftSkillIds[i] : 0;
                slotLabels[i].text = $"{i + 1}. {ResolveSkillName(skillId)}";
            }
        }

        private void RefreshCandidateLabels()
        {
            for (var i = 0; i < candidateLabels.Length; i++)
            {
                if (candidateLabels[i] == null)
                {
                    continue;
                }

                candidateLabels[i].text = i < _candidates.Count
                    ? _candidates[i].DisplayName
                    : string.Empty;

                if (i < candidateButtons.Length && candidateButtons[i] != null)
                {
                    candidateButtons[i].gameObject.SetActive(i < _candidates.Count);
                }
            }
        }

        private void RefreshSkillDetail()
        {
            if (skillDetailText == null)
            {
                return;
            }

            var skillId = _selectedSlotIndex < _draftSkillIds.Count
                ? _draftSkillIds[_selectedSlotIndex]
                : 0;
            var skill = _candidates.FirstOrDefault(candidate => candidate != null && candidate.SkillId == skillId);
            skillDetailText.text = skill != null
                ? $"{skill.DisplayName}\n{skill.Description}"
                : string.Empty;
        }

        private string ResolveSkillName(int skillId)
        {
            if (skillId <= 0)
            {
                return "Empty";
            }

            return _candidates.FirstOrDefault(skill => skill != null && skill.SkillId == skillId)?.DisplayName
                   ?? $"Unknown {skillId}";
        }

        private void NormalizeDraft()
        {
            while (_draftSkillIds.Count < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                _draftSkillIds.Add(0);
            }

            while (_draftSkillIds.Count > UserProfileData.BattleSkillLoadoutSlotCount)
            {
                _draftSkillIds.RemoveAt(_draftSkillIds.Count - 1);
            }

            var candidateSkillIds = new HashSet<int>(
                _candidates
                    .Where(candidate => candidate != null && candidate.SkillId > 0)
                    .Select(candidate => candidate.SkillId));

            for (var i = 0; i < _draftSkillIds.Count; i++)
            {
                var skillId = Mathf.Max(0, _draftSkillIds[i]);
                _draftSkillIds[i] = skillId > 0 && candidateSkillIds.Contains(skillId) ? skillId : 0;
            }
        }
    }
}
