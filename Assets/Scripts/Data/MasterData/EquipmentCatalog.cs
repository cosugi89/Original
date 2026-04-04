using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Equipment Catalog", fileName = "EquipmentCatalog")]
    public class EquipmentCatalog : ScriptableObject
    {
        [field: SerializeField]
        [Description("装備マスタ一覧。ID検索や装備枠ごとの一覧取得に使う。")]
        public List<EquipmentDefinition> Equipments { get; private set; } = new();

        private Dictionary<string, EquipmentDefinition> _byId;
        private Dictionary<string, EquipmentDefinition> _byPartIndexKey;
        private Dictionary<PartsType, List<EquipmentDefinition>> _byPartType;

        private void OnEnable()
        {
            RebuildLookups();
        }

        private void OnValidate()
        {
            RebuildLookups();
        }

        public bool TryGetById(string equipmentId, out EquipmentDefinition definition)
        {
            EnsureLookups();
            return _byId.TryGetValue(equipmentId ?? string.Empty, out definition);
        }

        public bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentDefinition definition)
        {
            EnsureLookups();
            return _byPartIndexKey.TryGetValue(EquipmentIdUtility.Build(partType, partsIndex), out definition);
        }

        public IReadOnlyList<EquipmentDefinition> GetByPartType(PartsType partType)
        {
            EnsureLookups();
            return _byPartType.TryGetValue(partType, out var definitions)
                ? definitions
                : Array.Empty<EquipmentDefinition>();
        }

        public IReadOnlyList<EquipmentDefinition> GetDefaultOwned()
        {
            EnsureLookups();
            return Equipments
                .Where(definition => definition != null && definition.IsDefaultOwned)
                .OrderBy(definition => definition.PartType)
                .ThenBy(definition => definition.PartsIndex)
                .ToArray();
        }

        private void EnsureLookups()
        {
            if (_byId == null || _byPartIndexKey == null || _byPartType == null)
            {
                RebuildLookups();
            }
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<string, EquipmentDefinition>();
            _byPartIndexKey = new Dictionary<string, EquipmentDefinition>();
            _byPartType = new Dictionary<PartsType, List<EquipmentDefinition>>();

            foreach (var definition in Equipments)
            {
                if (definition == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(definition.EquipmentId) && !_byId.TryAdd(definition.EquipmentId, definition))
                {
                    Debug.LogWarning($"Duplicate equipmentId detected in {name}: {definition.EquipmentId}", this);
                }

                var legacyKey = EquipmentIdUtility.Build(definition.PartType, definition.PartsIndex);
                if (definition.PartsIndex >= 0 && !_byPartIndexKey.TryAdd(legacyKey, definition))
                {
                    Debug.LogWarning($"Duplicate partType/index detected in {name}: {legacyKey}", this);
                }

                if (!_byPartType.TryGetValue(definition.PartType, out var list))
                {
                    list = new List<EquipmentDefinition>();
                    _byPartType[definition.PartType] = list;
                }

                list.Add(definition);
            }

            foreach (var pair in _byPartType)
            {
                pair.Value.Sort((left, right) => left.PartsIndex.CompareTo(right.PartsIndex));
            }
        }
    }
}
