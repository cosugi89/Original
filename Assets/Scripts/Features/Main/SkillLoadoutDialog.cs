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
        [SerializeField] private PartsSkillItem[] selectedSkillItems = Array.Empty<PartsSkillItem>();
        [SerializeField] private Button[] selectedSkillButtons = Array.Empty<Button>();
        [SerializeField] private StatusSkillInfoView candidateItemTemplate;
        [SerializeField] private Transform candidateContent;
        [SerializeField] private TextMeshProUGUI emptyCandidatesText;
        [SerializeField] private GameButton closeButton;

        private readonly List<CandidateSkill> _candidates = new();
        private readonly List<StatusSkillInfoView> _candidateViews = new();
        private readonly List<int> _draftSkillIds = new();
        private DeckManager _loadoutService;
        private int _selectedSlotIndex;
        private bool _hasSavedChanges;

        public void Setup(SkillLoadoutDialogRequest request, DialogContext context)
        {
            EnsurePrefabReferences();

            _loadoutService = UserDataManager.EnsureInitialized().Decks;
            _selectedSlotIndex = Mathf.Clamp(
                request.SlotIndex,
                0,
                UserProfileData.BattleSkillLoadoutSlotCount - 1);
            _hasSavedChanges = false;

            BindButtons();
            LoadDraft();
            candidateItemTemplate.SetVisible(false);
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
            for (var i = 0; i < selectedSkillButtons.Length; i++)
            {
                var index = i;
                selectedSkillButtons[i].onClick.RemoveAllListeners();
                selectedSkillButtons[i].onClick.AddListener(() => SelectSlot(index));
            }

            closeButton.SetAction(OnClickClose);
        }

        private void LoadDraft()
        {
            _candidates.Clear();
            _candidates.AddRange(BuildCandidateSources());

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
            _loadoutService.SaveLoadout(_draftSkillIds);
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
            for (var i = 0; i < selectedSkillItems.Length; i++)
            {
                var candidate = ResolveSkill(SkillIdAt(i));
                if (candidate == null)
                {
                    selectedSkillItems[i].ShowEmptyIconOnly();
                    continue;
                }

                selectedSkillItems[i].ShowSkillIconOnly(ToSkillItemType(candidate.Category), candidate.Skill.Icon);
                selectedSkillItems[i].SetFocused(i == _selectedSlotIndex);
            }
        }

        private void RefreshCandidateButtons()
        {
            emptyCandidatesText.gameObject.SetActive(_candidates.Count == 0);
            emptyCandidatesText.text = "No skills available from equipped gear.";

            for (var i = 0; i < _candidateViews.Count; i++)
            {
                var candidate = _candidates[i];
                var selected = SkillIdAt(_selectedSlotIndex) == candidate.Skill.SkillId;
                var assignedSlotIndex = FindAssignedSlot(candidate.Skill.SkillId);
                _candidateViews[i].Refresh(
                    candidate.Skill,
                    candidate.Equipment,
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

        private CandidateSkill ResolveSkill(int skillId)
        {
            if (skillId <= 0)
            {
                return null;
            }

            return _candidates.FirstOrDefault(candidate => candidate.Skill.SkillId == skillId);
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
            if (selectedSkillItems.Length < UserProfileData.BattleSkillLoadoutSlotCount ||
                selectedSkillButtons.Length < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                throw new InvalidOperationException($"{nameof(SkillLoadoutDialog)} requires {UserProfileData.BattleSkillLoadoutSlotCount} selected skill items and buttons.");
            }
        }

        private void CreateCandidateViews()
        {
            for (var i = 0; i < _candidates.Count; i++)
            {
                var index = i;
                var candidateView = Instantiate(candidateItemTemplate, candidateContent);
                candidateView.SetAction(() => SelectCandidate(index));
                candidateView.SetVisible(true);
                _candidateViews.Add(candidateView);
            }
        }

        private IEnumerable<CandidateSkill> BuildCandidateSources()
        {
            var seenSkillIds = new HashSet<int>();
            var deck = _loadoutService.GetCurrentDeck();
            foreach (var equipmentId in DeckLoadoutUtility.GetEquipmentIds(deck))
            {
                if (equipmentId <= 0 ||
                    !_loadoutService.InventoryManager.TryGetDefinition(equipmentId, out var equipment) ||
                    equipment == null)
                {
                    continue;
                }

                var category = DeckLoadoutUtility.GetCategory(equipment);
                if (!category.HasValue)
                {
                    continue;
                }

                foreach (var skill in equipment.AssignableSkills)
                {
                    if (skill == null || skill.SkillId <= 0 || !seenSkillIds.Add(skill.SkillId))
                    {
                        continue;
                    }

                    yield return new CandidateSkill(skill, equipment, category.Value);
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

        private sealed class CandidateSkill
        {
            public CandidateSkill(BattleSkillData skill, EquipmentData equipment, UICategory category)
            {
                Skill = skill;
                Equipment = equipment;
                Category = category;
            }

            public BattleSkillData Skill { get; }
            public EquipmentData Equipment { get; }
            public UICategory Category { get; }
        }

    }
}
