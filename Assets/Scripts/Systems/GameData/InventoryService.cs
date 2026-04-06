using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        [Description("装備IDと見た目情報を紐付ける装備マスタ。")]
        public EquipmentCatalog EquipmentCatalog { get; private set; }

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
                MasterDataResourceLoader.LoadEquipmentCatalog());
            Instance.Initialize();
            return Instance;
        }

        public InventoryService(GameSaveService saveService, EquipmentCatalog equipmentCatalog)
        {
            SaveService = saveService;
            Session = saveService.Session;
            EquipmentCatalog = equipmentCatalog;
        }

        public void RefreshCatalog(EquipmentCatalog equipmentCatalog)
        {
            EquipmentCatalog = equipmentCatalog;
            Initialize();
        }

        public IReadOnlyList<InventoryEntryData> GetAllEntries()
        {
            return EnsureInventory().Equipments
                .Where(entry => entry != null)
                .ToArray();
        }

        public IReadOnlyList<EquipmentDefinition> GetOwnedDefinitions()
        {
            if (EquipmentCatalog == null)
                return new List<EquipmentDefinition>();

            return GetAllEntries()
                .Where(entry => entry.IsUnlocked && !string.IsNullOrWhiteSpace(entry.EquipmentId))
                .Select(entry => EquipmentCatalog.TryGetById(entry.EquipmentId, out var definition) ? definition : null)
                .Where(definition => definition != null)
                .ToArray();
        }

        public IReadOnlyList<EquipmentDefinition> GetDefinitionsByPartType(PartsType partType, bool ownedOnly = false)
        {
            if (EquipmentCatalog == null)
                return new List<EquipmentDefinition>();

            var definitions = EquipmentCatalog.GetByPartType(partType);
            if (!ownedOnly)
                return definitions;

            var ownedIds = new HashSet<string>(
                GetAllEntries()
                    .Where(entry => entry.IsUnlocked && !string.IsNullOrWhiteSpace(entry.EquipmentId))
                    .Select(entry => entry.EquipmentId));

            return definitions
                .Where(definition => definition != null && ownedIds.Contains(definition.EquipmentId))
                .ToArray();
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

        public bool TryGetDefinition(string equipmentId, out EquipmentDefinition definition)
        {
            definition = null;
            return EquipmentCatalog != null && EquipmentCatalog.TryGetById(equipmentId, out definition);
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
                entry.ObtainedAtUtc = System.DateTime.UtcNow.ToString("O");

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

            entry.LastEquippedAtUtc = System.DateTime.UtcNow.ToString("O");
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

        private void SyncDefaultOwnedEquipments()
        {
            if (EquipmentCatalog == null)
                return;

            var hasChanges = false;
            foreach (var definition in EquipmentCatalog.GetDefaultOwned())
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.EquipmentId))
                    continue;

                if (TryGetEntry(definition.EquipmentId, out var existing))
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
                    EquipmentId = definition.EquipmentId,
                    Quantity = 1,
                    IsUnlocked = true
                });
                hasChanges = true;
            }

            if (hasChanges)
                SaveService.MarkDirty();
        }
    }
}
