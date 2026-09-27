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
    public class InventoryManager
    {
        [Description("現在の共有セッション。所持装備データの読み書き先。")]
        public UserDataManager Session { get; }

        [Description("保存のdirty管理と永続化を担当する保存サービス。")]
        public UserDataJsonStore UserDataStore { get; }

        private IReadOnlyList<EquipmentData> _allEquipmentData;
        private Dictionary<int, EquipmentData> _byId;
        private Dictionary<int, EquipmentData> _byPartIndexKey;
        private Dictionary<PartsType, List<EquipmentData>> _byPartType;
        private readonly Action _onItemStacksChanged;

        public InventoryManager(
            UserDataJsonStore userDataStore,
            IReadOnlyList<EquipmentData> equipmentData,
            Action onItemStacksChanged = null)
        {
            UserDataStore = userDataStore;
            Session = userDataStore.Session;
            _onItemStacksChanged = onItemStacksChanged;
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
            return EnsureInventory().Equipments.Where(entry => entry != null).ToArray();
        }

        public int GetItemCount(int itemId)
        {
            if (itemId <= 0)
                return 0;

            return EnsureInventory().ItemStacks
                .FirstOrDefault(stack => stack != null && stack.ItemId == itemId)?.Quantity ?? 0;
        }

        public int AddItem(int itemId, int quantity)
        {
            if (itemId <= 0 || quantity <= 0)
                return 0;

            var inventory = EnsureInventory();
            var stack = inventory.ItemStacks.FirstOrDefault(candidate => candidate != null && candidate.ItemId == itemId);
            if (stack == null)
            {
                stack = new ItemStackData { ItemId = itemId };
                inventory.ItemStacks.Add(stack);
            }

            var addedQuantity = Mathf.Min(quantity, int.MaxValue - stack.Quantity);
            if (addedQuantity == 0)
                return 0;

            stack.Quantity += addedQuantity;
            UserDataStore.MarkDirty();
            _onItemStacksChanged?.Invoke();
            return addedQuantity;
        }

        /// <summary>指定アイテムの所持数を直接設定する。0を指定した場合はスタックを削除する。</summary>
        public bool SetItemCount(int itemId, int quantity)
        {
            if (itemId <= 0)
                return false;

            var inventory = EnsureInventory();
            var normalizedQuantity = Mathf.Max(0, quantity);
            var stack = inventory.ItemStacks.FirstOrDefault(candidate => candidate != null && candidate.ItemId == itemId);
            if (stack == null)
            {
                if (normalizedQuantity == 0)
                    return false;

                inventory.ItemStacks.Add(new ItemStackData { ItemId = itemId, Quantity = normalizedQuantity });
            }
            else if (normalizedQuantity == 0)
            {
                inventory.ItemStacks.Remove(stack);
            }
            else
            {
                if (stack.Quantity == normalizedQuantity)
                    return false;

                stack.Quantity = normalizedQuantity;
            }

            UserDataStore.MarkDirty();
            _onItemStacksChanged?.Invoke();
            return true;
        }

        public bool TryConsumeItem(int itemId, int quantity)
        {
            if (itemId <= 0 || quantity <= 0)
                return false;

            var stack = EnsureInventory().ItemStacks
                .FirstOrDefault(candidate => candidate != null && candidate.ItemId == itemId);
            if (stack == null || stack.Quantity < quantity)
                return false;

            stack.Quantity -= quantity;
            UserDataStore.MarkDirty();
            _onItemStacksChanged?.Invoke();
            return true;
        }

        public IReadOnlyList<EquipmentData> GetOwnedDefinitions()
        {
            return GetAllEntries()
                .Where(entry => entry.IsUnlocked && entry.EquipmentId > 0)
                .Select(entry => _byId.TryGetValue(entry.EquipmentId, out var data) ? data : null)
                .Where(data => data != null)
                .ToArray();
        }

        public IReadOnlyList<EquipmentData> GetAllEquipmentDefinitions()
        {
            return _allEquipmentData
                .Where(data => data != null && data.EquipmentId > 0)
                .OrderBy(data => data.PartType)
                .ThenBy(data => data.SortOrder)
                .ThenBy(data => data.EquipmentId)
                .ToArray();
        }

        public IReadOnlyList<EquipmentData> GetDefinitionsByPartType(PartsType partType, bool ownedOnly = false)
        {
            if (!_byPartType.TryGetValue(partType, out var definitions))
                return Array.Empty<EquipmentData>();
            if (!ownedOnly)
                return definitions;

            var ownedIds = new HashSet<int>(GetAllEntries().Where(entry => entry.IsUnlocked && entry.EquipmentId > 0).Select(entry => entry.EquipmentId));
            return definitions.Where(data => data != null && ownedIds.Contains(data.EquipmentId)).ToArray();
        }

        public bool TryGetDefinition(int equipmentId, out EquipmentData definition) => _byId.TryGetValue(equipmentId, out definition);

        public bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentData definition)
        {
            return _byPartIndexKey.TryGetValue(EquipmentIdUtility.Build(partType, partsIndex), out definition);
        }

        public bool HasEquipment(int equipmentId) => TryGetEntry(equipmentId, out var entry) && entry.IsUnlocked && entry.Quantity > 0;

        public bool TryGetEntry(int equipmentId, out InventoryEntryData entry)
        {
            entry = EnsureInventory().Equipments.FirstOrDefault(candidate => candidate != null && candidate.EquipmentId == equipmentId);
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
                entry = new InventoryEntryData { EquipmentId = equipmentId };
                inventory.Equipments.Add(entry);
            }

            entry.Quantity = Mathf.Max(0, entry.Quantity + quantity);
            entry.IsUnlocked = entry.IsUnlocked || isUnlocked || entry.Quantity > 0;
            if (string.IsNullOrWhiteSpace(entry.ObtainedAtUtc))
                entry.ObtainedAtUtc = DateTime.UtcNow.ToString("O");
            UserDataStore.MarkDirty();
            return entry;
        }

        public bool RemoveEquipment(int equipmentId, int quantity = 1)
        {
            if (!TryGetEntry(equipmentId, out var entry))
                return false;
            entry.Quantity = Mathf.Max(0, entry.Quantity - Mathf.Max(0, quantity));
            if (entry.Quantity == 0)
                entry.IsUnlocked = false;
            UserDataStore.MarkDirty();
            return true;
        }

        public bool SetEquipmentOwned(int equipmentId, bool isOwned)
        {
            if (equipmentId <= 0 || !_byId.ContainsKey(equipmentId))
            {
                return false;
            }

            var inventory = EnsureInventory();
            var entry = inventory.Equipments.FirstOrDefault(candidate =>
                candidate != null && candidate.EquipmentId == equipmentId);

            if (isOwned)
            {
                if (entry != null && entry.IsUnlocked && entry.Quantity > 0)
                {
                    return false;
                }

                if (entry == null)
                {
                    entry = new InventoryEntryData { EquipmentId = equipmentId };
                    inventory.Equipments.Add(entry);
                }

                entry.IsUnlocked = true;
                entry.Quantity = Mathf.Max(1, entry.Quantity);
                if (string.IsNullOrWhiteSpace(entry.ObtainedAtUtc))
                {
                    entry.ObtainedAtUtc = DateTime.UtcNow.ToString("O");
                }
            }
            else
            {
                if (entry == null)
                {
                    return false;
                }

                inventory.Equipments.Remove(entry);
            }

            UserDataStore.MarkDirty();
            return true;
        }

        public int ResetEquipmentOwnership()
        {
            var inventory = EnsureInventory();
            var removedCount = inventory.Equipments.Count(entry => entry != null && entry.IsUnlocked && entry.Quantity > 0);
            if (inventory.Equipments.Count == 0)
            {
                return 0;
            }

            inventory.Equipments.Clear();
            UserDataStore.MarkDirty();
            return removedCount;
        }

        public void MarkEquipped(int equipmentId)
        {
            if (!TryGetEntry(equipmentId, out var entry))
                return;
            entry.LastEquippedAtUtc = DateTime.UtcNow.ToString("O");
            UserDataStore.MarkDirty();
        }

        private void Initialize()
        {
            EnsureInventory();
            NormalizeItemStacks();
            SyncDefaultOwnedEquipments();
        }

        private InventoryData EnsureInventory()
        {
            Session.UserData.Inventory ??= new InventoryData();
            Session.UserData.Inventory.Equipments ??= new List<InventoryEntryData>();
            Session.UserData.Inventory.ItemStacks ??= new List<ItemStackData>();
            return Session.UserData.Inventory;
        }

        private void NormalizeItemStacks()
        {
            var stacks = EnsureInventory().ItemStacks;
            var quantitiesById = new Dictionary<int, int>();
            foreach (var stack in stacks)
            {
                if (stack == null || stack.ItemId <= 0 || stack.Quantity <= 0)
                    continue;

                quantitiesById.TryGetValue(stack.ItemId, out var quantity);
                quantitiesById[stack.ItemId] = (int)Math.Min(int.MaxValue, (long)quantity + stack.Quantity);
            }

            var normalized = quantitiesById
                .OrderBy(pair => pair.Key)
                .Select(pair => new ItemStackData { ItemId = pair.Key, Quantity = pair.Value })
                .ToList();
            if (stacks.Count == normalized.Count && stacks.Zip(normalized, (left, right) => left.ItemId == right.ItemId && left.Quantity == right.Quantity).All(isSame => isSame))
                return;

            stacks.Clear();
            stacks.AddRange(normalized);
            UserDataStore.MarkDirty();
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
                    Debug.LogWarning($"[InventoryManager] Duplicate equipmentId: {data.EquipmentId}");
                if (data.PartsIndex >= 0)
                    _byPartIndexKey.TryAdd(EquipmentIdUtility.Build(data.PartType, data.PartsIndex), data);
                if (!_byPartType.TryGetValue(data.PartType, out var list))
                {
                    list = new List<EquipmentData>();
                    _byPartType[data.PartType] = list;
                }
                list.Add(data);
            }
            foreach (var pair in _byPartType)
                pair.Value.Sort((left, right) => left.SortOrder != right.SortOrder ? left.SortOrder.CompareTo(right.SortOrder) : left.PartsIndex.CompareTo(right.PartsIndex));
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
                EnsureInventory().Equipments.Add(new InventoryEntryData { EquipmentId = data.EquipmentId, Quantity = 1, IsUnlocked = true });
                hasChanges = true;
            }
            if (hasChanges)
                UserDataStore.MarkDirty();
        }
    }
}
