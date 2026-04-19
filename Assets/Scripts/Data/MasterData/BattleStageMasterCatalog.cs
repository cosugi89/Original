using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Stage Master Catalog", fileName = "BattleStageMasterCatalog")]
    public class BattleStageMasterCatalog : ScriptableObject
    {
        [field: SerializeField]
        [Description("ステージマスタ一覧。ID検索や選択画面の一覧生成に使う。")]
        public List<BattleStageMasterData> Stages { get; private set; } = new();

        private Dictionary<string, BattleStageMasterData> _byId;

        private void OnEnable()
        {
            RebuildLookups();
        }

        private void OnValidate()
        {
            RebuildLookups();
        }

        public bool TryGetById(string stageId, out BattleStageMasterData definition)
        {
            EnsureLookups();
            return _byId.TryGetValue(stageId ?? string.Empty, out definition);
        }

        public IReadOnlyList<BattleStageMasterData> GetAllOrdered()
        {
            EnsureLookups();
            return Stages
                .Where(definition => definition != null)
                .OrderBy(definition => definition.SortOrder)
                .ThenBy(definition => definition.StageId)
                .ToArray();
        }

        public IReadOnlyList<BattleStageMasterData> GetInitiallyUnlocked()
        {
            EnsureLookups();
            return Stages
                .Where(definition => definition != null && definition.IsInitiallyUnlocked)
                .OrderBy(definition => definition.SortOrder)
                .ThenBy(definition => definition.StageId)
                .ToArray();
        }

        private void EnsureLookups()
        {
            if (_byId == null)
            {
                RebuildLookups();
            }
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<string, BattleStageMasterData>();
            foreach (var definition in Stages)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.StageId))
                    continue;

                if (!_byId.TryAdd(definition.StageId, definition))
                {
                    Debug.LogWarning($"Duplicate stageId detected in {name}: {definition.StageId}", this);
                }
            }
        }
    }
}
