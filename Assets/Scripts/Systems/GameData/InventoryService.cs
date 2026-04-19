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
        private Dictionary<string, EquipmentData> _byId;
        private Dictionary<string, EquipmentData> _byPartIndexKey;
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
                .Where(entry => entry.IsUnlocked && !string.IsNullOrWhiteSpace(entry.EquipmentId))
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

            var ownedIds = new HashSet<string>(
                GetAllEntries()
                    .Where(entry => entry.IsUnlocked && !string.IsNullOrWhiteSpace(entry.EquipmentId))
                    .Select(entry => entry.EquipmentId));

            return definitions
                .Where(data => data != null && ownedIds.Contains(data.EquipmentId))
                .ToArray();
        }

        public bool TryGetDefinition(string equipmentId, out EquipmentData definition)
        {
            return _byId.TryGetValue(equipmentId ?? string.Empty, out definition);
        }

        public bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentData definition)
        {
            return _byPartIndexKey.TryGetValue(EquipmentIdUtility.Build(partType, partsIndex), out definition);
        }

        public bool HasEquipment(string equipmentId)
        {
            return TryGetEntry(equipmentId, out var entry) && entry.IsUnlocked && entry.Quantity > 0;
        }

        public bool TryGetEntry(string equipmentId, out InventoryEntryData entry)
        {
            entry = EnsureInventory().Equipments
                .FirstOrDefault(candidate => candidate != null && candidate.EquipmentId == (equipmentId ?? string.Empty));
            return entry != null;
        }

        public InventoryEntryData GrantEquipment(string equipmentId, int quantity = 1, bool isUnlocked = true)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
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

        public bool RemoveEquipment(string equipmentId, int quantity = 1)
        {
            if (!TryGetEntry(equipmentId, out var entry))
                return false;

            entry.Quantity = Mathf.Max(0, entry.Quantity - Mathf.Max(0, quantity));
            if (entry.Quantity == 0)
                entry.IsUnlocked = false;

            SaveService.MarkDirty();
            return true;
        }

        public void MarkEquipped(string equipmentId)
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
            Session.SaveData.Inventory ??= new InventoryData();
            Session.SaveData.Inventory.Equipments ??= new List<InventoryEntryData>();
            return Session.SaveData.Inventory;
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<string, EquipmentData>();
            _byPartIndexKey = new Dictionary<string, EquipmentData>();
            _byPartType = new Dictionary<PartsType, List<EquipmentData>>();

            foreach (var data in _allEquipmentData)
            {
                if (data == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(data.EquipmentId) && !_byId.TryAdd(data.EquipmentId, data))
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
                pair.Value.Sort((l, r) => l.PartsIndex.CompareTo(r.PartsIndex));
            }
        }

        private void SyncDefaultOwnedEquipments()
        {
            var hasChanges = false;
            foreach (var data in _allEquipmentData)
            {
                if (data == null || !data.IsDefaultOwned || string.IsNullOrWhiteSpace(data.EquipmentId))
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
}
