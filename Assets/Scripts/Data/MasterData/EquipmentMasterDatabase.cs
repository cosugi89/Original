using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Equipment Master Database", fileName = "EquipmentMasterDatabase")]
    public class EquipmentMasterDatabase : ScriptableObject
    {
        public readonly struct ValidationIssue
        {
            public ValidationIssue(string code, string message, EquipmentMasterData definition = null)
            {
                Code = code ?? "";
                Message = message ?? "";
                Definition = definition;
            }

            public string Code { get; }
            public string Message { get; }
            public EquipmentMasterData Definition { get; }
        }

        [SerializeField]
        [Description("装備/appearance マスタ一覧。ID検索や見た目枠ごとの一覧取得に使う。")]
        private List<EquipmentMasterData> equipments = new();

        public List<EquipmentMasterData> Equipments => equipments ??= new List<EquipmentMasterData>();

        private Dictionary<int, EquipmentMasterData> _byId;
        private Dictionary<int, EquipmentMasterData> _byPartIndexKey;
        private Dictionary<PartsType, List<EquipmentMasterData>> _byPartType;
        private List<ValidationIssue> _validationIssues;

        private void OnEnable()
        {
            RebuildLookups();
        }

        private void OnValidate()
        {
            RebuildLookups();
        }

        public bool TryGetById(int equipmentId, out EquipmentMasterData definition)
        {
            EnsureLookups();
            return _byId.TryGetValue(equipmentId, out definition);
        }

        internal bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentMasterData definition)
        {
            EnsureLookups();
            return _byPartIndexKey.TryGetValue(EquipmentIdUtility.Build(partType, partsIndex), out definition);
        }

        public IReadOnlyList<ValidationIssue> GetValidationIssues()
        {
            EnsureLookups();
            return _validationIssues != null
                ? _validationIssues
                : Array.Empty<ValidationIssue>();
        }

        public IReadOnlyList<EquipmentMasterData> GetByPartType(PartsType partType)
        {
            EnsureLookups();
            return _byPartType.TryGetValue(partType, out var definitions)
                ? definitions
                : Array.Empty<EquipmentMasterData>();
        }

        public IReadOnlyList<EquipmentMasterData> GetDefaultOwned()
        {
            EnsureLookups();
            return Equipments
                .Where(definition => definition != null && definition.IsDefaultOwned)
                .OrderBy(definition => definition.PartType)
                .ThenBy(definition => definition.SortOrder > 0 ? definition.SortOrder : definition.PartsIndex)
                .ToArray();
        }

        private void EnsureLookups()
        {
            if (_byId == null || _byPartIndexKey == null || _byPartType == null || _validationIssues == null)
            {
                RebuildLookups();
            }
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<int, EquipmentMasterData>();
            _byPartIndexKey = new Dictionary<int, EquipmentMasterData>();
            _byPartType = new Dictionary<PartsType, List<EquipmentMasterData>>();
            _validationIssues = new List<ValidationIssue>();

            foreach (var definition in Equipments)
            {
                if (definition == null)
                {
                    _validationIssues.Add(new ValidationIssue(
                        "null_definition",
                        $"[{name}] Equipments に null 参照が含まれています。",
                        null));
                    continue;
                }

                if (definition.EquipmentId <= 0)
                {
                    _validationIssues.Add(new ValidationIssue(
                        "missing_equipment_id",
                        $"[{name}] {definition.name} の EquipmentId が未設定です。",
                        definition));
                }

                if (definition.EquipmentId > 0 && !_byId.TryAdd(definition.EquipmentId, definition))
                {
                    _validationIssues.Add(new ValidationIssue(
                        "duplicate_equipment_id",
                        $"[{name}] EquipmentId が重複しています: {definition.EquipmentId}",
                        definition));
                }

                var partIndexKey = EquipmentIdUtility.Build(definition.PartType, definition.PartsIndex);
                if (definition.PartsIndex < 0)
                {
                    _validationIssues.Add(new ValidationIssue(
                        "invalid_parts_index",
                        $"[{name}] {definition.name} の PartsIndex が未設定です。",
                        definition));
                }
                else if (definition.Icon == null)
                {
                    _validationIssues.Add(new ValidationIssue(
                        "missing_icon",
                        $"[{name}] {definition.name} の Icon が未設定です。",
                        definition));
                }

                if (definition.PartsIndex >= 0 && !_byPartIndexKey.TryAdd(partIndexKey, definition))
                {
                    _validationIssues.Add(new ValidationIssue(
                        "duplicate_part_type_index",
                        $"[{name}] PartType/PartsIndex が重複しています: {definition.PartType} / {definition.PartsIndex}",
                        definition));
                }

                if (!_byPartType.TryGetValue(definition.PartType, out var list))
                {
                    list = new List<EquipmentMasterData>();
                    _byPartType[definition.PartType] = list;
                }

                list.Add(definition);
            }

            foreach (var pair in _byPartType)
            {
                pair.Value.Sort((left, right) =>
                {
                    var leftSortOrder = left.SortOrder > 0 ? left.SortOrder : left.PartsIndex;
                    var rightSortOrder = right.SortOrder > 0 ? right.SortOrder : right.PartsIndex;
                    var compare = leftSortOrder.CompareTo(rightSortOrder);
                    if (compare != 0)
                    {
                        return compare;
                    }

                    return left.PartsIndex.CompareTo(right.PartsIndex);
                });
            }
        }
    }
}
