using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    /// <summary>
    /// 現在選択中の戦闘デッキを正本として、装備4枠とSkill4枠を更新する窓口。
    /// </summary>
    public class DeckManager
    {
        public UserDataJsonStore UserDataStore { get; }
        public InventoryManager InventoryManager { get; }

        public DeckManager(UserDataJsonStore userDataStore, InventoryManager inventoryManager)
        {
            UserDataStore = userDataStore;
            InventoryManager = inventoryManager;
            EnsureDecks();
            EnsureDefaultDeckEquipment();
            EnsureUsableLoadout();
        }

        public int CurrentDeckId => EnsureProfile().CurrentDeckId;

        public DeckData GetCurrentDeck()
        {
            return GetDeck(CurrentDeckId);
        }

        public IReadOnlyList<DeckData> GetAllDecks()
        {
            return EnsureProfile().Decks.ToArray();
        }

        public bool TrySelectDeck(int deckId, bool persistImmediately = true)
        {
            if (deckId < 1 || deckId > UserProfileData.DeckCount)
                return false;

            var profile = EnsureProfile();
            var deck = GetDeck(deckId);
            var hasChanges = DeckLoadoutUtility.Normalize(deck, InventoryManager);
            hasChanges |= profile.CurrentDeckId != deckId;
            profile.CurrentDeckId = deckId;

            hasChanges |= EnsureUsableLoadoutInternal(deck);
            if (!hasChanges)
                return true;

            UserDataStore.MarkDirty();
            if (persistImmediately)
                UserDataStore.SaveSession();
            return true;
        }

        public IReadOnlyList<AvatarPartStateData> GetCurrentPartStates()
        {
            var states = new List<AvatarPartStateData>();
            foreach (var equipmentId in DeckLoadoutUtility.GetEquipmentIds(GetCurrentDeck()))
            {
                if (equipmentId <= 0 ||
                    !InventoryManager.TryGetDefinition(equipmentId, out var definition) ||
                    definition == null)
                {
                    continue;
                }

                states.Add(new AvatarPartStateData
                {
                    PartType = definition.PartType,
                    EquipmentId = equipmentId,
                    IsVisible = true,
                });
            }

            return states;
        }

        public bool TryEquip(int equipmentId, bool persistImmediately = true)
        {
            if (equipmentId <= 0 ||
                !InventoryManager.TryGetDefinition(equipmentId, out var definition) ||
                definition == null ||
                !InventoryManager.HasEquipment(equipmentId))
            {
                return false;
            }

            var category = DeckLoadoutUtility.GetCategory(definition);
            if (!category.HasValue)
                return false;

            var deck = GetCurrentDeck();
            if (DeckLoadoutUtility.GetEquipmentId(deck, category.Value) == equipmentId)
                return true;

            DeckLoadoutUtility.TrySetEquipment(deck, definition, equipmentId);

            InventoryManager.MarkEquipped(equipmentId);
            EnsureUsableLoadoutInternal(deck);
            UserDataStore.MarkDirty();
            if (persistImmediately)
                UserDataStore.SaveSession();
            return true;
        }

        public void Unequip(UICategory category, bool persistImmediately = true)
        {
            var deck = GetCurrentDeck();
            if (!DeckLoadoutUtility.TryClearEquipment(deck, category))
                return;

            EnsureUsableLoadoutInternal(deck);
            UserDataStore.MarkDirty();
            if (persistImmediately)
                UserDataStore.SaveSession();
        }

        public void SaveEquipmentFromAppearance(AvatarAppearanceData appearance, bool persistImmediately = false)
        {
            var deck = GetCurrentDeck();
            var hasChanges = false;
            foreach (var category in DeckLoadoutUtility.EquipmentCategories)
            {
                var equipmentId = appearance?.Parts?
                    .Where(state => state != null && state.IsVisible && state.EquipmentId > 0)
                    .Select(state => state.EquipmentId)
                    .FirstOrDefault(id =>
                        InventoryManager.TryGetDefinition(id, out var definition) &&
                        DeckLoadoutUtility.GetCategory(definition) == category) ?? 0;

                hasChanges |= DeckLoadoutUtility.SetEquipmentId(deck, category, equipmentId);
            }

            hasChanges |= EnsureUsableLoadoutInternal(deck);
            if (!hasChanges)
                return;

            UserDataStore.MarkDirty();
            if (persistImmediately)
                UserDataStore.SaveSession();
        }

        public IReadOnlyList<BattleSkillData> GetCandidateSkillsFromEquippedEquipment()
        {
            var result = new List<BattleSkillData>();
            var seenSkillIds = new HashSet<int>();
            foreach (var equipmentId in DeckLoadoutUtility.GetEquipmentIds(GetCurrentDeck()))
            {
                if (equipmentId <= 0 ||
                    !InventoryManager.TryGetDefinition(equipmentId, out var equipment) ||
                    equipment == null)
                {
                    continue;
                }

                foreach (var skill in equipment.AssignableSkills ?? Array.Empty<BattleSkillData>())
                {
                    if (skill == null || skill.SkillId <= 0 || !seenSkillIds.Add(skill.SkillId))
                        continue;

                    result.Add(skill);
                }
            }

            return result;
        }

        public IReadOnlyList<int> GetCurrentSkillIds()
        {
            EnsureUsableLoadout();
            return GetCurrentDeck().SkillIds.ToArray();
        }

        public IReadOnlyList<int> EnsureUsableLoadout(bool persistImmediately = false)
        {
            var deck = GetCurrentDeck();
            var hasChanges = DeckLoadoutUtility.Normalize(deck, InventoryManager);
            hasChanges |= EnsureUsableLoadoutInternal(deck);
            if (hasChanges)
            {
                UserDataStore.MarkDirty();
                if (persistImmediately)
                    UserDataStore.SaveSession();
            }

            return deck.SkillIds.ToArray();
        }

        public void SaveLoadout(IReadOnlyList<int> requestedSkillIds, bool persistImmediately = true)
        {
            var deck = GetCurrentDeck();
            var allowedSkillIds = new HashSet<int>(GetCandidateSkillsFromEquippedEquipment().Select(skill => skill.SkillId));
            deck.SkillIds.Clear();
            for (var i = 0; i < UserProfileData.BattleSkillLoadoutSlotCount; i++)
            {
                var skillId = requestedSkillIds != null && i < requestedSkillIds.Count ? Mathf.Max(0, requestedSkillIds[i]) : 0;
                deck.SkillIds.Add(allowedSkillIds.Contains(skillId) ? skillId : 0);
            }

            UserDataStore.MarkDirty();
            if (persistImmediately)
                UserDataStore.SaveSession();
        }

        private UserProfileData EnsureProfile()
        {
            UserDataStore.Session.UserData.Profile ??= new UserProfileDemoData();
            EnsureDecks();
            return UserDataStore.Session.UserData.Profile;
        }

        private void EnsureDecks()
        {
            var profile = UserDataStore.Session.UserData.Profile ??= new UserProfileDemoData();
            profile.Decks ??= new List<DeckData>();
            profile.CurrentDeckId = Mathf.Clamp(profile.CurrentDeckId, 1, UserProfileData.DeckCount);

            for (var deckId = 1; deckId <= UserProfileData.DeckCount; deckId++)
            {
                if (profile.Decks.Any(deck => deck != null && deck.DeckId == deckId))
                    continue;

                profile.Decks.Add(new DeckData { DeckId = deckId });
            }

            profile.Decks = profile.Decks
                .Where(deck => deck != null && deck.DeckId >= 1 && deck.DeckId <= UserProfileData.DeckCount)
                .GroupBy(deck => deck.DeckId)
                .Select(group => group.First())
                .OrderBy(deck => deck.DeckId)
                .ToList();

            for (var i = 0; i < profile.Decks.Count; i++)
                DeckLoadoutUtility.Normalize(profile.Decks[i], InventoryManager);
        }

        private DeckData GetDeck(int deckId)
        {
            return EnsureProfile().Decks.First(deck => deck.DeckId == deckId);
        }

        private void EnsureDefaultDeckEquipment()
        {
            var deck = GetDeck(1);
            if (DeckLoadoutUtility.GetEquipmentIds(deck).Any(equipmentId => equipmentId > 0))
                return;

            foreach (var equipment in InventoryManager.GetOwnedDefinitions())
            {
                var category = DeckLoadoutUtility.GetCategory(equipment);
                if (!category.HasValue || DeckLoadoutUtility.GetEquipmentId(deck, category.Value) > 0)
                    continue;

                DeckLoadoutUtility.TrySetEquipment(deck, equipment, equipment.EquipmentId);
            }
        }

        private bool EnsureUsableLoadoutInternal(DeckData deck)
        {
            var candidates = new HashSet<int>(GetCandidateSkills(deck).Select(skill => skill.SkillId));
            var hasChanges = false;
            for (var i = 0; i < deck.SkillIds.Count; i++)
            {
                if (deck.SkillIds[i] > 0 && !candidates.Contains(deck.SkillIds[i]))
                {
                    deck.SkillIds[i] = 0;
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        private IEnumerable<BattleSkillData> GetCandidateSkills(DeckData deck)
        {
            var seenSkillIds = new HashSet<int>();
            foreach (var equipmentId in DeckLoadoutUtility.GetEquipmentIds(deck))
            {
                if (!InventoryManager.TryGetDefinition(equipmentId, out var equipment) || equipment == null)
                    continue;

                foreach (var skill in equipment.AssignableSkills ?? Array.Empty<BattleSkillData>())
                {
                    if (skill != null && skill.SkillId > 0 && seenSkillIds.Add(skill.SkillId))
                        yield return skill;
                }
            }
        }
    }

    public static class DeckLoadoutUtility
    {
        public static readonly UICategory[] EquipmentCategories =
        {
            UICategory.HandRight,
            UICategory.HandLeft,
            UICategory.Helmet,
            UICategory.Chest,
        };

        public static bool Normalize(DeckData deck, InventoryManager inventoryManager)
        {
            if (deck == null)
                return false;

            deck.SkillIds ??= new List<int>();
            var hasChanges = false;
            foreach (var category in EquipmentCategories)
            {
                var equipmentId = GetEquipmentId(deck, category);
                if (equipmentId <= 0 ||
                    !inventoryManager.TryGetDefinition(equipmentId, out var equipment) ||
                    equipment == null ||
                    !inventoryManager.HasEquipment(equipmentId) ||
                    GetCategory(equipment) != category)
                {
                    hasChanges |= SetEquipmentId(deck, category, 0);
                }
            }

            while (deck.SkillIds.Count < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                deck.SkillIds.Add(0);
                hasChanges = true;
            }

            while (deck.SkillIds.Count > UserProfileData.BattleSkillLoadoutSlotCount)
            {
                deck.SkillIds.RemoveAt(deck.SkillIds.Count - 1);
                hasChanges = true;
            }

            for (var i = 0; i < deck.SkillIds.Count; i++)
            {
                var normalized = Mathf.Max(0, deck.SkillIds[i]);
                hasChanges |= deck.SkillIds[i] != normalized;
                deck.SkillIds[i] = normalized;
            }

            return hasChanges;
        }

        public static bool TrySetEquipment(DeckData deck, EquipmentData equipment, int equipmentId)
        {
            var category = GetCategory(equipment);
            return category.HasValue && SetEquipmentId(deck, category.Value, equipmentId);
        }

        public static bool TryClearEquipment(DeckData deck, UICategory category)
        {
            return EquipmentCategories.Contains(category) && SetEquipmentId(deck, category, 0);
        }

        public static UICategory? GetCategory(EquipmentData equipment)
        {
            if (equipment == null)
                return null;

            if (equipment.ExclusiveGroup == PartsExclusiveGroup.HandRight)
                return UICategory.HandRight;
            if (equipment.ExclusiveGroup == PartsExclusiveGroup.HandLeft)
                return UICategory.HandLeft;
            if (equipment.PartType == PartsType.Helmet)
                return UICategory.Helmet;
            if (equipment.PartType == PartsType.Chest)
                return UICategory.Chest;
            return null;
        }

        public static IEnumerable<int> GetEquipmentIds(DeckData deck)
        {
            if (deck == null)
                yield break;

            yield return deck.RightHandEquipmentId;
            yield return deck.LeftHandEquipmentId;
            yield return deck.HelmetEquipmentId;
            yield return deck.ChestEquipmentId;
        }

        public static int GetEquipmentId(DeckData deck, UICategory category)
        {
            return category switch
            {
                UICategory.HandRight => deck.RightHandEquipmentId,
                UICategory.HandLeft => deck.LeftHandEquipmentId,
                UICategory.Helmet => deck.HelmetEquipmentId,
                UICategory.Chest => deck.ChestEquipmentId,
                _ => 0,
            };
        }

        public static bool SetEquipmentId(DeckData deck, UICategory category, int equipmentId)
        {
            if (deck == null)
                return false;

            equipmentId = Mathf.Max(0, equipmentId);
            return category switch
            {
                UICategory.HandRight when deck.RightHandEquipmentId != equipmentId => SetRightHand(deck, equipmentId),
                UICategory.HandLeft when deck.LeftHandEquipmentId != equipmentId => SetLeftHand(deck, equipmentId),
                UICategory.Helmet when deck.HelmetEquipmentId != equipmentId => SetHelmet(deck, equipmentId),
                UICategory.Chest when deck.ChestEquipmentId != equipmentId => SetChest(deck, equipmentId),
                _ => false,
            };
        }

        private static bool SetRightHand(DeckData deck, int equipmentId) { deck.RightHandEquipmentId = equipmentId; return true; }
        private static bool SetLeftHand(DeckData deck, int equipmentId) { deck.LeftHandEquipmentId = equipmentId; return true; }
        private static bool SetHelmet(DeckData deck, int equipmentId) { deck.HelmetEquipmentId = equipmentId; return true; }
        private static bool SetChest(DeckData deck, int equipmentId) { deck.ChestEquipmentId = equipmentId; return true; }
    }
}
