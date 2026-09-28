using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using LayerLab.ArtMakerUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct RequestDialogSkillLoadout
    {
        public RequestDialogSkillLoadout(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public int SlotIndex { get; }
    }

    public readonly struct ResultDialogSkillLoadout
    {
        private ResultDialogSkillLoadout(bool isSaved)
        {
            IsSaved = isSaved;
        }

        public bool IsSaved { get; }

        public static ResultDialogSkillLoadout Saved => new(true);
        public static ResultDialogSkillLoadout Cancelled => new(false);
    }

    [DialogAddress("Dialog/SkillLoadout")]
    public sealed class DialogSkillLoadout :
        DialogBase<ResultDialogSkillLoadout>,
        IDialogRequestHandler<RequestDialogSkillLoadout>
    {
        [FormerlySerializedAs("selectedSkillItems")]
        [SerializeField] private PartsSkillItem[] selectedSkillSlotItems = Array.Empty<PartsSkillItem>();
        [FormerlySerializedAs("selectedSkillButtons")]
        [SerializeField] private Button[] selectedSkillSlotButtons = Array.Empty<Button>();
        [FormerlySerializedAs("candidateItemTemplate")]
        [SerializeField] private SkillLoadoutCandidateItemView candidateSkillItemTemplate;
        [FormerlySerializedAs("candidateContent")]
        [SerializeField] private Transform candidateSkillContent;
        [SerializeField] private TextMeshProUGUI emptyCandidatesText;
        [SerializeField] private GameButton closeButton;

        private readonly List<DeckSkillCandidate> _candidates = new();
        private readonly List<SkillLoadoutCandidateItemView> _candidateSkillItemViews = new();
        private readonly List<int> _draftSkillIds = new();
        private DeckManager _deckManager;
        private UnityAction[] _selectedSkillSlotActions;
        private int _selectedSlotIndex;
        private bool _hasSavedChanges;

        public void Setup(RequestDialogSkillLoadout request, DialogContext context)
        {
            EnsurePrefabReferences();

            _deckManager = UserDataManager.EnsureInitialized().Decks;
            _selectedSlotIndex = Mathf.Clamp(
                request.SlotIndex,
                0,
                UserProfileData.BattleSkillLoadoutSlotCount - 1);
            _hasSavedChanges = false;

            BindButtons();
            LoadDraft();
            candidateSkillItemTemplate.SetVisible(false);
            CreateCandidateViews();
            Refresh();
        }

        protected override void OnClickClose()
        {
            Close(_hasSavedChanges
                ? ResultDialogSkillLoadout.Saved
                : ResultDialogSkillLoadout.Cancelled);
        }

        private void BindButtons()
        {
            if (_selectedSkillSlotActions == null || _selectedSkillSlotActions.Length != selectedSkillSlotButtons.Length)
            {
                _selectedSkillSlotActions = new UnityAction[selectedSkillSlotButtons.Length];
            }

            for (var i = 0; i < selectedSkillSlotButtons.Length; i++)
            {
                var index = i;
                var button = selectedSkillSlotButtons[index];
                if (_selectedSkillSlotActions[index] != null)
                {
                    button.onClick.RemoveListener(_selectedSkillSlotActions[index]);
                }

                _selectedSkillSlotActions[index] = () => SelectSlot(index);
                button.onClick.AddListener(_selectedSkillSlotActions[index]);
            }

            closeButton.SetAction(OnClickClose);
        }

        private void LoadDraft()
        {
            _candidates.Clear();
            _candidates.AddRange(_deckManager.GetCurrentSkillCandidates());

            _draftSkillIds.Clear();
            _draftSkillIds.AddRange(_deckManager.EnsureUsableLoadout(persistImmediately: true));
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
            var skillId = _candidates[index].Skill.SkillId;
            if (_draftSkillIds[_selectedSlotIndex] == skillId)
            {
                Refresh();
                return;
            }

            _draftSkillIds[_selectedSlotIndex] = skillId;
            SaveDraft();
            Refresh();
        }

        private void SaveDraft()
        {
            NormalizeDraft();
            _deckManager.SaveLoadout(_draftSkillIds);
            _hasSavedChanges = true;
        }

        private void Refresh()
        {
            SetTitle($"Skill Slot {_selectedSlotIndex + 1}");
            RefreshSelectedSkillItems();
            RefreshCandidateButtons();
        }

        private void RefreshSelectedSkillItems()
        {
            for (var i = 0; i < selectedSkillSlotItems.Length; i++)
            {
                var candidate = ResolveSkill(SkillIdAt(i));
                if (candidate == null)
                {
                    selectedSkillSlotItems[i].ShowEmptyIconOnly();
                    continue;
                }

                selectedSkillSlotItems[i].ShowSkillIconOnly(
                    ToSkillItemType(candidate.Value.SourceCategory),
                    candidate.Value.Skill.Icon);
                selectedSkillSlotItems[i].SetFocused(i == _selectedSlotIndex);
            }
        }

        private void RefreshCandidateButtons()
        {
            emptyCandidatesText.gameObject.SetActive(_candidates.Count == 0);
            emptyCandidatesText.text = "No skills available from equipped gear.";

            for (var i = 0; i < _candidateSkillItemViews.Count; i++)
            {
                if (i >= _candidates.Count)
                {
                    _candidateSkillItemViews[i].SetVisible(false);
                    continue;
                }

                var candidate = _candidates[i];
                var selected = SkillIdAt(_selectedSlotIndex) == candidate.Skill.SkillId;
                var assignedSlotIndex = FindAssignedSlot(candidate.Skill.SkillId);
                _candidateSkillItemViews[i].Refresh(
                    candidate.Skill,
                    candidate.SourceEquipment,
                    selected,
                    assignedSlotIndex >= 0,
                    assignedSlotIndex);
            }
        }

        private int SkillIdAt(int index)
        {
            return index >= 0 && index < _draftSkillIds.Count ? _draftSkillIds[index] : 0;
        }

        private int FindAssignedSlot(int skillId)
        {
            for (var i = 0; i < _draftSkillIds.Count; i++)
            {
                if (i != _selectedSlotIndex && _draftSkillIds[i] == skillId)
                {
                    return i;
                }
            }

            return -1;
        }

        private DeckSkillCandidate? ResolveSkill(int skillId)
        {
            if (skillId <= 0)
            {
                return null;
            }

            foreach (var candidate in _candidates)
            {
                if (candidate.Skill.SkillId == skillId)
                {
                    return candidate;
                }
            }

            return null;
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
                    .Where(candidate => candidate.Skill.SkillId > 0)
                    .Select(candidate => candidate.Skill.SkillId));

            var assignedSkillIds = new HashSet<int>();
            for (var i = 0; i < _draftSkillIds.Count; i++)
            {
                var skillId = Mathf.Max(0, _draftSkillIds[i]);
                _draftSkillIds[i] = skillId > 0 &&
                                    candidateSkillIds.Contains(skillId) &&
                                    assignedSkillIds.Add(skillId)
                    ? skillId
                    : 0;
            }
        }

        private void EnsurePrefabReferences()
        {
            if (selectedSkillSlotItems.Length < UserProfileData.BattleSkillLoadoutSlotCount ||
                selectedSkillSlotButtons.Length < UserProfileData.BattleSkillLoadoutSlotCount ||
                selectedSkillSlotItems.Take(UserProfileData.BattleSkillLoadoutSlotCount).Any(item => item == null) ||
                selectedSkillSlotButtons.Take(UserProfileData.BattleSkillLoadoutSlotCount).Any(button => button == null) ||
                candidateSkillItemTemplate == null ||
                candidateSkillContent == null ||
                emptyCandidatesText == null ||
                closeButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DialogSkillLoadout)} requires {UserProfileData.BattleSkillLoadoutSlotCount} selected skill items and buttons, " +
                    "candidateSkillItemTemplate, candidateSkillContent, emptyCandidatesText, and closeButton references.");
            }
        }

        private void CreateCandidateViews()
        {
            while (_candidateSkillItemViews.Count < _candidates.Count)
            {
                var candidateView = Instantiate(candidateSkillItemTemplate, candidateSkillContent);
                _candidateSkillItemViews.Add(candidateView);
            }

            for (var i = 0; i < _candidateSkillItemViews.Count; i++)
            {
                var index = i;
                var isVisible = index < _candidates.Count;
                var candidateView = _candidateSkillItemViews[index];
                candidateView.SetVisible(isVisible);
                if (isVisible)
                {
                    candidateView.SetAction(() => SelectCandidate(index));
                }
            }
        }

        private static SkillItemType ToSkillItemType(UICategory category)
        {
            return category switch
            {
                UICategory.HandRight => SkillItemType.Weapon,
                UICategory.HandLeft => SkillItemType.Shield,
                UICategory.Chest => SkillItemType.Armor,
                UICategory.Helmet => SkillItemType.Headgear,
                _ => throw new InvalidOperationException($"Unsupported equipped skill category: {category}"),
            };
        }

        protected override void OnDestroy()
        {
            if (_selectedSkillSlotActions != null)
            {
                var count = Math.Min(selectedSkillSlotButtons.Length, _selectedSkillSlotActions.Length);
                for (var i = 0; i < count; i++)
                {
                    if (selectedSkillSlotButtons[i] != null && _selectedSkillSlotActions[i] != null)
                    {
                        selectedSkillSlotButtons[i].onClick.RemoveListener(_selectedSkillSlotActions[i]);
                    }
                }
            }

            base.OnDestroy();
        }

    }
}
