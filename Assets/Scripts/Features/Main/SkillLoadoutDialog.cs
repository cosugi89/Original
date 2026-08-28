using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct SkillLoadoutDialogRequest
    {
        public SkillLoadoutDialogRequest(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public int SlotIndex { get; }
    }

    public readonly struct SkillLoadoutDialogResponse
    {
        private SkillLoadoutDialogResponse(bool isSaved)
        {
            IsSaved = isSaved;
        }

        public bool IsSaved { get; }

        public static SkillLoadoutDialogResponse Saved => new(true);
        public static SkillLoadoutDialogResponse Cancelled => new(false);
    }

    [DialogAddress("Dialog/SkillLoadout")]
    public sealed class SkillLoadoutDialog :
        DialogBase<SkillLoadoutDialogResponse>,
        IDialogRequestHandler<SkillLoadoutDialogRequest>
    {
        [SerializeField] private SkillFrameView[] selectedSkillViews = Array.Empty<SkillFrameView>();
        [SerializeField] private StatusSkillInfoView candidateViewPrefab;
        [SerializeField] private Transform candidateContent;
        [SerializeField] private TextMeshProUGUI emptyCandidatesText;
        [SerializeField] private GameButton clearButton;
        [SerializeField] private GameButton saveButton;
        [SerializeField] private GameButton closeButton;

        private readonly List<BattleSkillData> _candidates = new();
        private readonly List<StatusSkillInfoView> _candidateViews = new();
        private readonly List<int> _draftSkillIds = new();
        private SkillLoadoutManager _loadoutService;
        private int _selectedSlotIndex;
        private bool _hasSavedChanges;

        public void Setup(SkillLoadoutDialogRequest request, DialogContext context)
        {
            EnsurePrefabReferences();

            _loadoutService = UserDataManager.EnsureInitialized().SkillLoadout;
            _selectedSlotIndex = Mathf.Clamp(
                request.SlotIndex,
                0,
                UserProfileData.BattleSkillLoadoutSlotCount - 1);
            _hasSavedChanges = false;

            BindButtons();
            LoadDraft();
            CreateCandidateViews();
            Refresh();
        }

        protected override void OnClickClose()
        {
            Close(_hasSavedChanges
                ? SkillLoadoutDialogResponse.Saved
                : SkillLoadoutDialogResponse.Cancelled);
        }

        private void BindButtons()
        {
            for (var i = 0; i < selectedSkillViews.Length; i++)
            {
                var index = i;
                selectedSkillViews[i].SetAction(() => SelectSlot(index));
            }

            clearButton.SetAction(ClearSelectedSlot);
            saveButton.SetVisible(false);
            closeButton.SetAction(OnClickClose);
        }

        private void LoadDraft()
        {
            _candidates.Clear();
            _candidates.AddRange(_loadoutService.GetCandidateSkillsFromEquippedEquipment());

            _draftSkillIds.Clear();
            _draftSkillIds.AddRange(_loadoutService.EnsureUsableLoadout(persistImmediately: true));
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
            var skillId = _candidates[index].SkillId;
            if (_draftSkillIds[_selectedSlotIndex] == skillId)
            {
                Refresh();
                return;
            }

            _draftSkillIds[_selectedSlotIndex] = skillId;
            SaveDraft();
            Refresh();
        }

        private void ClearSelectedSlot()
        {
            NormalizeDraft();
            if (_draftSkillIds[_selectedSlotIndex] == 0)
            {
                Refresh();
                return;
            }

            _draftSkillIds[_selectedSlotIndex] = 0;
            SaveDraft();
            Refresh();
        }

        private void SaveDraft()
        {
            NormalizeDraft();
            _loadoutService.SaveLoadout(_draftSkillIds);
            _hasSavedChanges = true;
        }

        private void Refresh()
        {
            SetTitle($"Skill Slot {_selectedSlotIndex + 1}");
            RefreshSlotButtons();
            RefreshCandidateButtons();
            RefreshSelectedSkill();
        }

        private void RefreshSlotButtons()
        {
            for (var i = 0; i < selectedSkillViews.Length; i++)
            {
                var visible = i < UserProfileData.BattleSkillLoadoutSlotCount;
                selectedSkillViews[i].SetVisible(visible);
                if (!visible)
                {
                    continue;
                }

                var skillId = i < _draftSkillIds.Count ? _draftSkillIds[i] : 0;
                selectedSkillViews[i].Refresh(ResolveSkill(skillId), i == _selectedSlotIndex);
            }
        }

        private void RefreshCandidateButtons()
        {
            emptyCandidatesText.gameObject.SetActive(_candidates.Count == 0);
            emptyCandidatesText.text = "No skills available from equipped gear.";

            for (var i = 0; i < _candidateViews.Count; i++)
            {
                var skill = _candidates[i];
                var selected = SkillIdAt(_selectedSlotIndex) == skill.SkillId;
                _candidateViews[i].Refresh(skill, selected);
            }
        }

        private void RefreshSelectedSkill()
        {
            var skillId = SkillIdAt(_selectedSlotIndex);
            var skill = ResolveSkill(skillId);
            clearButton.SetInteractable(skill != null);
        }

        private int SkillIdAt(int index)
        {
            return index >= 0 && index < _draftSkillIds.Count ? _draftSkillIds[index] : 0;
        }

        private BattleSkillData ResolveSkill(int skillId)
        {
            if (skillId <= 0)
            {
                return null;
            }

            return _candidates.FirstOrDefault(skill => skill != null && skill.SkillId == skillId);
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
            if (selectedSkillViews.Length < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                throw new InvalidOperationException($"{nameof(SkillLoadoutDialog)} requires {UserProfileData.BattleSkillLoadoutSlotCount} selected skill views.");
            }
        }

        private void CreateCandidateViews()
        {
            for (var i = 0; i < _candidates.Count; i++)
            {
                var index = i;
                var candidateView = Instantiate(candidateViewPrefab, candidateContent);
                candidateView.SetAction(() => SelectCandidate(index));
                candidateView.SetVisible(true);
                _candidateViews.Add(candidateView);
            }
        }

        [Serializable]
        private sealed class SkillFrameView
        {
            [SerializeField] private GameObject root;
            [SerializeField] private Button button;
            [SerializeField] private Image iconImage;
            [SerializeField] private GameObject normalRoot;
            [SerializeField] private GameObject emptyRoot;
            [SerializeField] private GameObject selectedRoot;

            public void SetVisible(bool visible)
            {
                root.SetActive(visible);
            }

            public void SetAction(Action action)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => action());
            }

            public void Refresh(BattleSkillData skill, bool selected)
            {
                var hasSkill = skill != null;
                normalRoot.SetActive(hasSkill);
                emptyRoot.SetActive(!hasSkill);
                selectedRoot.SetActive(selected);
                iconImage.sprite = hasSkill ? skill.Icon : null;
                iconImage.enabled = hasSkill && skill.Icon != null;
                iconImage.preserveAspect = true;
            }
        }

    }
}
