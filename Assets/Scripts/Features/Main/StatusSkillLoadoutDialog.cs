using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public readonly struct StatusSkillLoadoutDialogRequest
    {
        public StatusSkillLoadoutDialogRequest(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public int SlotIndex { get; }
    }

    public readonly struct StatusSkillLoadoutDialogResult
    {
        private StatusSkillLoadoutDialogResult(bool isSaved)
        {
            IsSaved = isSaved;
        }

        public bool IsSaved { get; }

        public static StatusSkillLoadoutDialogResult Saved => new(true);
        public static StatusSkillLoadoutDialogResult Cancelled => new(false);
    }

    [DialogAddress("Dialog/SkillLoadout")]
    public sealed class StatusSkillLoadoutDialog :
        DialogBase<StatusSkillLoadoutDialogResult>,
        IDialogRequestHandler<StatusSkillLoadoutDialogRequest>
    {
        [SerializeField] private TextMeshProUGUI currentSkillNameText;
        [SerializeField] private TextMeshProUGUI currentSkillDescriptionText;
        [SerializeField] private TextMeshProUGUI emptyCandidatesText;
        [SerializeField] private GameButton[] slotButtons = Array.Empty<GameButton>();
        [SerializeField] private GameButton[] candidateButtons = Array.Empty<GameButton>();
        [SerializeField] private GameButton clearButton;
        [SerializeField] private GameButton saveButton;
        [SerializeField] private GameButton closeButton;

        private readonly List<BattleSkillData> _candidates = new();
        private readonly List<int> _draftSkillIds = new();
        private BattleSkillLoadoutService _loadoutService;
        private int _selectedSlotIndex;

        public void Setup(StatusSkillLoadoutDialogRequest request, DialogContext context)
        {
            EnsurePrefabReferences();

            _loadoutService = BattleSkillLoadoutService.EnsureInitialized();
            _selectedSlotIndex = Mathf.Clamp(
                request.SlotIndex,
                0,
                UserProfileData.BattleSkillLoadoutSlotCount - 1);

            BindButtons();
            LoadDraft();
            Refresh();
        }

        protected override void OnClickClose()
        {
            Close(StatusSkillLoadoutDialogResult.Cancelled);
        }

        private void BindButtons()
        {
            for (var i = 0; i < slotButtons.Length; i++)
            {
                var index = i;
                slotButtons[i].SetAction(() => SelectSlot(index));
            }

            for (var i = 0; i < candidateButtons.Length; i++)
            {
                var index = i;
                candidateButtons[i].SetAction(() => SelectCandidate(index));
            }

            clearButton.SetAction(ClearSelectedSlot);
            saveButton.SetAction(Save);
            closeButton.SetAction(OnClickClose);
        }

        private void LoadDraft()
        {
            _candidates.Clear();
            _candidates.AddRange(_loadoutService.GetCandidateSkillsFromEquippedEquipment());

            _draftSkillIds.Clear();
            _draftSkillIds.AddRange(_loadoutService.EnsureUsableLoadout());
            NormalizeDraft();
        }

        private void SelectSlot(int index)
        {
            if (index < 0 || index >= UserProfileData.BattleSkillLoadoutSlotCount)
            {
                return;
            }

            _selectedSlotIndex = index;
            Refresh();
        }

        private void SelectCandidate(int index)
        {
            if (index < 0 || index >= _candidates.Count)
            {
                return;
            }

            NormalizeDraft();
            _draftSkillIds[_selectedSlotIndex] = _candidates[index].SkillId;
            Refresh();
        }

        private void ClearSelectedSlot()
        {
            NormalizeDraft();
            _draftSkillIds[_selectedSlotIndex] = 0;
            Refresh();
        }

        private void Save()
        {
            NormalizeDraft();
            _loadoutService.SaveLoadout(_draftSkillIds);
            Close(StatusSkillLoadoutDialogResult.Saved);
        }

        private void Refresh()
        {
            SetTitle($"Skill Slot {_selectedSlotIndex + 1}");
            RefreshSlotButtons();
            RefreshCandidateButtons();
            RefreshSkillDetail();
        }

        private void RefreshSlotButtons()
        {
            for (var i = 0; i < slotButtons.Length; i++)
            {
                var visible = i < UserProfileData.BattleSkillLoadoutSlotCount;
                slotButtons[i].SetVisible(visible);
                if (!visible)
                {
                    continue;
                }

                var skillId = i < _draftSkillIds.Count ? _draftSkillIds[i] : 0;
                slotButtons[i].SetLabel($"{i + 1}. {ResolveSkillName(skillId)}");
                slotButtons[i].SetSelected(i == _selectedSlotIndex);
            }
        }

        private void RefreshCandidateButtons()
        {
            emptyCandidatesText.gameObject.SetActive(_candidates.Count == 0);
            emptyCandidatesText.text = "No skills available from equipped gear.";

            for (var i = 0; i < candidateButtons.Length; i++)
            {
                var visible = i < _candidates.Count;
                candidateButtons[i].SetVisible(visible);
                if (!visible)
                {
                    continue;
                }

                var skill = _candidates[i];
                var selected = SkillIdAt(_selectedSlotIndex) == skill.SkillId;
                candidateButtons[i].SetLabel(BuildCandidateLabel(skill));
                candidateButtons[i].SetIcon(skill.Icon);
                candidateButtons[i].SetSelected(selected);
                candidateButtons[i].SetBadgeVisible(selected);
            }
        }

        private void RefreshSkillDetail()
        {
            var skillId = SkillIdAt(_selectedSlotIndex);
            var skill = _candidates.FirstOrDefault(candidate => candidate != null && candidate.SkillId == skillId);
            if (skill == null)
            {
                currentSkillNameText.text = $"Slot {_selectedSlotIndex + 1}: Empty";
                currentSkillDescriptionText.text = _candidates.Count == 0
                    ? "Equip gear with assignable skills to set this slot."
                    : "Choose a skill from the list, then save.";
                clearButton.SetInteractable(false);
                saveButton.SetInteractable(true);
                return;
            }

            currentSkillNameText.text = BuildDisplayName(skill);
            currentSkillDescriptionText.text = BuildSkillDetail(skill);
            clearButton.SetInteractable(true);
            saveButton.SetInteractable(true);
        }

        private int SkillIdAt(int index)
        {
            return index >= 0 && index < _draftSkillIds.Count ? _draftSkillIds[index] : 0;
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

        private static string BuildCandidateLabel(BattleSkillData skill)
        {
            return !string.IsNullOrWhiteSpace(skill.DisplayName)
                ? skill.DisplayName
                : $"Skill {skill.SkillId}";
        }

        private static string BuildDisplayName(BattleSkillData skill)
        {
            return !string.IsNullOrWhiteSpace(skill.DisplayName)
                ? skill.DisplayName
                : $"Skill {skill.SkillId}";
        }

        private static string BuildSkillDetail(BattleSkillData skill)
        {
            var description = string.IsNullOrWhiteSpace(skill.Description)
                ? "No description."
                : skill.Description;
            var effect = skill.PrimaryEffect;
            var effectText = effect != null
                ? $"{effect.EffectType} / {effect.TargetType} / {effect.Value}"
                : "-";

            return $"{description}\nCharge: {skill.RequiredCharge}\nEffect: {effectText}";
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

        private void EnsurePrefabReferences()
        {
            if (slotButtons.Length < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                throw new InvalidOperationException($"{nameof(StatusSkillLoadoutDialog)} requires {UserProfileData.BattleSkillLoadoutSlotCount} slot buttons.");
            }

            if (candidateButtons.Length == 0)
            {
                throw new InvalidOperationException($"{nameof(StatusSkillLoadoutDialog)} requires candidate buttons.");
            }
        }
    }
}
