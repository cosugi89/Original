using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Boss Insight Database", fileName = "BossInsightDatabase")]
    public sealed class BossInsightDatabase : ScriptableObject
    {
        [SerializeField] private List<BossInsightMasterData> entries = new();

        private Dictionary<int, BossInsightMasterData> _byStageId;

        public IReadOnlyList<BossInsightMasterData> Entries => entries ??= new List<BossInsightMasterData>();

        public IReadOnlyList<BossInsightMasterData> OrderedEntries => Entries
            .Where(entry => entry != null)
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.StageId)
            .ToArray();

        public bool TryGetByStageId(int stageId, out BossInsightMasterData entry)
        {
            EnsureLookup();
            return _byStageId.TryGetValue(stageId, out entry);
        }

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        private void EnsureLookup()
        {
            if (_byStageId == null)
                RebuildLookup();
        }

        private void RebuildLookup()
        {
            _byStageId = new Dictionary<int, BossInsightMasterData>();
            foreach (var entry in Entries)
            {
                if (entry == null || entry.StageId <= 0)
                    continue;

                if (!_byStageId.TryAdd(entry.StageId, entry))
                    Debug.LogWarning($"Duplicate Boss Insight StageId in {name}: {entry.StageId}", this);
            }
        }
    }
}
