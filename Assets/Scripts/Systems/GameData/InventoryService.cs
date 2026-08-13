using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public class InventoryService
    {
        public static InventoryService Instance { get; private set; }

        [Description("現在の共有セッション。所持装備データの読み書き先。")]
        public GameSession Session { get; }

        [Description("保存のdirty管理と永続化を担当する保存サービス。")]
        public GameSaveService SaveService { get; }

        private IReadOnlyList<EquipmentData> _allEquipmentData;
        private Dictionary<int, EquipmentData> _byId;
        private Dictionary<int, EquipmentData> _byPartIndexKey;
        private Dictionary<PartsType, List<EquipmentData>> _byPartType;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInitialized();
        }

        public static InventoryService EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            Instance = new InventoryService(
                GameSaveService.EnsureInitialized(),
                MasterDataResourceLoader.LoadEquipmentData());
            return Instance;
        }

        public InventoryService(GameSaveService saveService, IReadOnlyList<EquipmentData> equipmentData)
        {
            SaveService = saveService;
            Session = saveService.Session;
            RefreshDefinitions(equipmentData);
        }

        public void RefreshDefinitions(IReadOnlyList<EquipmentData> equipmentData)
        {
            _allEquipmentData = equipmentData ?? Array.Empty<EquipmentData>();
            RebuildLookups();
            Initialize();
        }

        public IReadOnlyList<InventoryEntryData> GetAllEntries()
        {
            return EnsureInventory().Equipments
                .Where(entry => entry != null)
                .ToArray();
        }

        public IReadOnlyList<EquipmentData> GetOwnedDefinitions()
        {
            return GetAllEntries()
                .Where(entry => entry.IsUnlocked && entry.EquipmentId > 0)
                .Select(entry => _byId.TryGetValue(entry.EquipmentId, out var data) ? data : null)
                .Where(data => data != null)
                .ToArray();
        }

        public IReadOnlyList<EquipmentData> GetDefinitionsByPartType(PartsType partType, bool ownedOnly = false)
        {
            if (!_byPartType.TryGetValue(partType, out var definitions))
                return Array.Empty<EquipmentData>();

            if (!ownedOnly)
                return definitions;

            var ownedIds = new HashSet<int>(
                GetAllEntries()
                    .Where(entry => entry.IsUnlocked && entry.EquipmentId > 0)
                    .Select(entry => entry.EquipmentId));

            return definitions
                .Where(data => data != null && ownedIds.Contains(data.EquipmentId))
                .ToArray();
        }

        public bool TryGetDefinition(int equipmentId, out EquipmentData definition)
        {
            return _byId.TryGetValue(equipmentId, out definition);
        }

        internal bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentData definition)
        {
            return _byPartIndexKey.TryGetValue(EquipmentIdUtility.Build(partType, partsIndex), out definition);
        }

        public bool HasEquipment(int equipmentId)
        {
            return TryGetEntry(equipmentId, out var entry) && entry.IsUnlocked && entry.Quantity > 0;
        }

        public bool TryGetEntry(int equipmentId, out InventoryEntryData entry)
        {
            entry = EnsureInventory().Equipments
                .FirstOrDefault(candidate => candidate != null && candidate.EquipmentId == equipmentId);
            return entry != null;
        }

        public InventoryEntryData GrantEquipment(int equipmentId, int quantity = 1, bool isUnlocked = true)
        {
            if (equipmentId <= 0)
                return null;

            var inventory = EnsureInventory();
            var entry = inventory.Equipments.FirstOrDefault(candidate => candidate != null && candidate.EquipmentId == equipmentId);
            if (entry == null)
            {
                entry = new InventoryEntryData
                {
                    EquipmentId = equipmentId,
                    Quantity = 0,
                    IsUnlocked = false
                };
                inventory.Equipments.Add(entry);
            }

            entry.Quantity = Mathf.Max(0, entry.Quantity + quantity);
            entry.IsUnlocked = entry.IsUnlocked || isUnlocked || entry.Quantity > 0;
            if (string.IsNullOrWhiteSpace(entry.ObtainedAtUtc))
                entry.ObtainedAtUtc = DateTime.UtcNow.ToString("O");

            SaveService.MarkDirty();
            return entry;
        }

        public bool RemoveEquipment(int equipmentId, int quantity = 1)
        {
            if (!TryGetEntry(equipmentId, out var entry))
                return false;

            entry.Quantity = Mathf.Max(0, entry.Quantity - Mathf.Max(0, quantity));
            if (entry.Quantity == 0)
                entry.IsUnlocked = false;

            SaveService.MarkDirty();
            return true;
        }

        public void MarkEquipped(int equipmentId)
        {
            if (!TryGetEntry(equipmentId, out var entry))
                return;

            entry.LastEquippedAtUtc = DateTime.UtcNow.ToString("O");
            SaveService.MarkDirty();
        }

        private void Initialize()
        {
            EnsureInventory();
            SyncDefaultOwnedEquipments();
        }

        private InventoryData EnsureInventory()
        {
            Session.UserData.Inventory ??= new InventoryData();
            Session.UserData.Inventory.Equipments ??= new List<InventoryEntryData>();
            return Session.UserData.Inventory;
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<int, EquipmentData>();
            _byPartIndexKey = new Dictionary<int, EquipmentData>();
            _byPartType = new Dictionary<PartsType, List<EquipmentData>>();

            foreach (var data in _allEquipmentData)
            {
                if (data == null)
                    continue;

                if (data.EquipmentId > 0 && !_byId.TryAdd(data.EquipmentId, data))
                {
                    Debug.LogWarning($"[InventoryService] Duplicate equipmentId: {data.EquipmentId}");
                }

                var legacyKey = EquipmentIdUtility.Build(data.PartType, data.PartsIndex);
                if (data.PartsIndex >= 0)
                    _byPartIndexKey.TryAdd(legacyKey, data);

                if (!_byPartType.TryGetValue(data.PartType, out var list))
                {
                    list = new List<EquipmentData>();
                    _byPartType[data.PartType] = list;
                }
                list.Add(data);
            }

            foreach (var pair in _byPartType)
            {
                pair.Value.Sort((l, r) =>
                {
                    var compare = l.SortOrder.CompareTo(r.SortOrder);
                    if (compare != 0)
                    {
                        return compare;
                    }

                    return l.PartsIndex.CompareTo(r.PartsIndex);
                });
            }
        }

        private void SyncDefaultOwnedEquipments()
        {
            var hasChanges = false;
            foreach (var data in _allEquipmentData)
            {
                if (data == null || !data.IsDefaultOwned || data.EquipmentId <= 0)
                    continue;

                if (TryGetEntry(data.EquipmentId, out var existing))
                {
                    if (!existing.IsUnlocked || existing.Quantity <= 0)
                    {
                        existing.IsUnlocked = true;
                        existing.Quantity = Mathf.Max(1, existing.Quantity);
                        hasChanges = true;
                    }
                    continue;
                }

                EnsureInventory().Equipments.Add(new InventoryEntryData
                {
                    EquipmentId = data.EquipmentId,
                    Quantity = 1,
                    IsUnlocked = true,
                });
                hasChanges = true;
            }

            if (hasChanges)
                SaveService.MarkDirty();
        }
    }

    public class BattleSkillLoadoutService
    {
        public static BattleSkillLoadoutService Instance { get; private set; }

        public GameSaveService SaveService { get; }
        public AvatarService AvatarService { get; }
        public InventoryService InventoryService { get; }

        public static BattleSkillLoadoutService EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            Instance = new BattleSkillLoadoutService(
                GameSaveService.EnsureInitialized(),
                AvatarService.EnsureInitialized(),
                InventoryService.EnsureInitialized());
            return Instance;
        }

        public BattleSkillLoadoutService(
            GameSaveService saveService,
            AvatarService avatarService,
            InventoryService inventoryService)
        {
            SaveService = saveService;
            AvatarService = avatarService;
            InventoryService = inventoryService;
        }

        public IReadOnlyList<BattleSkillData> GetCandidateSkillsFromEquippedEquipment()
        {
            var result = new List<BattleSkillData>();
            var seenSkillIds = new HashSet<int>();
            var equippedParts = AvatarService.GetAllPartStates();
            for (var i = 0; i < equippedParts.Count; i++)
            {
                var state = equippedParts[i];
                if (state == null ||
                    state.EquipmentId <= 0 ||
                    !InventoryService.TryGetDefinition(state.EquipmentId, out var equipment) ||
                    equipment == null)
                {
                    continue;
                }

                AddSkills(equipment.AssignableSkills, result, seenSkillIds);
            }

            return result;
        }

        public IReadOnlyList<int> GetCurrentSkillIds()
        {
            var loadout = EnsureLoadout();
            return loadout.SkillIds.ToArray();
        }

        public IReadOnlyList<int> EnsureUsableLoadout(bool persistImmediately = false)
        {
            var loadout = EnsureLoadout();
            NormalizeLoadout(loadout.SkillIds);
            var candidateSkillIds = new HashSet<int>(
                GetCandidateSkillsFromEquippedEquipment()
                    .Where(skill => skill != null && skill.SkillId > 0)
                    .Select(skill => skill.SkillId));
            var hasChanges = RemoveUnavailableSkills(loadout.SkillIds, candidateSkillIds);

            if (loadout.SkillIds.Any(skillId => skillId > 0))
            {
                MirrorLegacySkillSlots(loadout.SkillIds);
                if (hasChanges)
                {
                    SaveService.MarkDirty();
                    if (persistImmediately)
                    {
                        SaveService.SaveSession();
                    }
                }

                return loadout.SkillIds.ToArray();
            }

            var defaultSkillIds = BuildDefaultSkillIdsFromCandidates();
            if (defaultSkillIds.Count == 0)
            {
                MirrorLegacySkillSlots(loadout.SkillIds);
                if (hasChanges)
                {
                    SaveService.MarkDirty();
                    if (persistImmediately)
                    {
                        SaveService.SaveSession();
                    }
                }

                return loadout.SkillIds.ToArray();
            }

            for (var i = 0; i < loadout.SkillIds.Count; i++)
            {
                loadout.SkillIds[i] = i < defaultSkillIds.Count ? defaultSkillIds[i] : 0;
            }

            MirrorLegacySkillSlots(loadout.SkillIds);
            SaveService.MarkDirty();
            if (persistImmediately)
            {
                SaveService.SaveSession();
            }

            return loadout.SkillIds.ToArray();
        }

        public void SaveLoadout(IReadOnlyList<int> requestedSkillIds, bool persistImmediately = true)
        {
            var loadout = EnsureLoadout();
            var allowedSkillIds = new HashSet<int>(
                GetCandidateSkillsFromEquippedEquipment()
                    .Where(skill => skill != null && skill.SkillId > 0)
                    .Select(skill => skill.SkillId));

            loadout.SkillIds.Clear();
            for (var i = 0; i < UserProfileData.BattleSkillLoadoutSlotCount; i++)
            {
                var skillId = requestedSkillIds != null && i < requestedSkillIds.Count
                    ? Mathf.Max(0, requestedSkillIds[i])
                    : 0;
                loadout.SkillIds.Add(skillId > 0 && allowedSkillIds.Contains(skillId) ? skillId : 0);
            }

            MirrorLegacySkillSlots(loadout.SkillIds);
            SaveService.MarkDirty();
            if (persistImmediately)
            {
                SaveService.SaveSession();
            }
        }

        private BattleSkillLoadoutData EnsureLoadout()
        {
            SaveService.Session.UserData.Profile ??= new UserProfileDemoData();
            var profile = SaveService.Session.UserData.Profile;
            profile.SkillLoadout ??= new BattleSkillLoadoutData();
            profile.SkillLoadout.SkillIds ??= new List<int>();
            if (profile.SkillLoadout.SkillIds.Count == 0)
            {
                if (profile.SkillSlotSkillIds != null)
                {
                    profile.SkillLoadout.SkillIds.AddRange(profile.SkillSlotSkillIds);
                }
            }

            NormalizeLoadout(profile.SkillLoadout.SkillIds);
            MirrorLegacySkillSlots(profile.SkillLoadout.SkillIds);
            return profile.SkillLoadout;
        }

        private IReadOnlyList<int> BuildDefaultSkillIdsFromCandidates()
        {
            return GetCandidateSkillsFromEquippedEquipment()
                .Where(skill => skill != null && skill.SkillId > 0)
                .Select(skill => skill.SkillId)
                .Take(UserProfileData.BattleSkillLoadoutSlotCount)
                .ToArray();
        }

        private void MirrorLegacySkillSlots(IReadOnlyList<int> skillIds)
        {
            SaveService.Session.UserData.Profile.SkillSlotSkillIds =
                skillIds != null
                    ? skillIds.Select(skillId => Mathf.Max(0, skillId)).ToList()
                    : new List<int>();
        }

        private static bool RemoveUnavailableSkills(
            List<int> skillIds,
            HashSet<int> candidateSkillIds)
        {
            if (skillIds == null || candidateSkillIds == null)
            {
                return false;
            }

            var hasChanges = false;
            for (var i = 0; i < skillIds.Count; i++)
            {
                if (skillIds[i] <= 0 || candidateSkillIds.Contains(skillIds[i]))
                {
                    continue;
                }

                skillIds[i] = 0;
                hasChanges = true;
            }

            return hasChanges;
        }

        private static void NormalizeLoadout(List<int> skillIds)
        {
            if (skillIds == null)
            {
                return;
            }

            while (skillIds.Count < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                skillIds.Add(0);
            }

            while (skillIds.Count > UserProfileData.BattleSkillLoadoutSlotCount)
            {
                skillIds.RemoveAt(skillIds.Count - 1);
            }

            for (var i = 0; i < skillIds.Count; i++)
            {
                skillIds[i] = Mathf.Max(0, skillIds[i]);
            }
        }

        private static void AddSkills(
            IReadOnlyList<BattleSkillData> source,
            List<BattleSkillData> destination,
            HashSet<int> seenSkillIds)
        {
            if (source == null || destination == null || seenSkillIds == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                var skill = source[i];
                if (skill == null || skill.SkillId <= 0 || !seenSkillIds.Add(skill.SkillId))
                {
                    continue;
                }

                destination.Add(skill);
            }
        }
    }
}
