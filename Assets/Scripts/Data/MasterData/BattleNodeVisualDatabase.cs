using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Node Visual Database", fileName = "BattleNodeVisualDatabase")]
    public class BattleNodeVisualDatabase : ScriptableObject
    {
        public const string DefaultResourcePath = "MasterData/BattleNodeVisualDatabase";

        [SerializeField]
        private BattleNodeVisualEntry fallbackEntry = new();

        [SerializeField]
        private List<BattleNodeVisualEntry> entries = new();

        private Dictionary<BattleNodeType, BattleNodeVisualEntry> _byNodeType;

        public BattleNodeVisualEntry FallbackEntry => fallbackEntry ??= new BattleNodeVisualEntry();

        public IReadOnlyList<BattleNodeVisualEntry> Entries => entries ??= new List<BattleNodeVisualEntry>();

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        public BattleNodeVisualEntry GetEntry(BattleNodeType nodeType)
        {
            EnsureLookup();
            return _byNodeType.TryGetValue(nodeType, out var entry) && entry != null
                ? entry
                : FallbackEntry;
        }

        private void EnsureLookup()
        {
            if (_byNodeType == null)
            {
                RebuildLookup();
            }
        }

        private void RebuildLookup()
        {
            _byNodeType = new Dictionary<BattleNodeType, BattleNodeVisualEntry>();
            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                _byNodeType[entry.NodeType] = entry;
            }
        }
    }

    [System.Serializable]
    public class BattleNodeVisualEntry
    {
        [SerializeField]
        private BattleNodeType nodeType = BattleNodeType.Empty;

        [SerializeField]
        private Sprite icon;

        [SerializeField]
        private Color baseColor = new(0.88f, 0.84f, 0.8f, 0.92f);

        [SerializeField]
        private Color accentColor = new(0.98f, 0.95f, 0.9f, 1f);

        [SerializeField]
        private Color iconColor = Color.white;

        public BattleNodeType NodeType => nodeType;

        public Sprite Icon => icon;

        public Color BaseColor => baseColor;

        public Color AccentColor => accentColor;

        public Color IconColor => iconColor;
    }
}
