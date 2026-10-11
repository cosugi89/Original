using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum AbilityNodeProgressionType
    {
        UnlockOnly = 0,
        Ranked = 1,
    }

    [Serializable]
    public sealed class AbilityNodeRankDefinition
    {
        [SerializeField, Min(0)] private int pointCost;
        [SerializeField] private List<BattleStatModifierDefinitionEntry> battleStatModifiers = new();

        public int PointCost => Mathf.Max(0, pointCost);
        public IReadOnlyList<BattleStatModifierDefinitionEntry> BattleStatModifiers =>
            battleStatModifiers ??= new List<BattleStatModifierDefinitionEntry>();

        public void Normalize()
        {
            pointCost = Mathf.Max(0, pointCost);
            battleStatModifiers ??= new List<BattleStatModifierDefinitionEntry>();
            for (var i = 0; i < battleStatModifiers.Count; i++)
            {
                battleStatModifiers[i]?.Normalize();
            }
        }
    }

    /// <summary>
    /// Node固有の内容だけを保持する。Tree上の配置と接続はAbilityTreeMasterData側で定義する。
    /// </summary>
    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Ability Node", fileName = "AbilityNodeMasterData")]
    public sealed class AbilityNodeMasterData : ValidatedMasterDataAsset
    {
        [SerializeField] private string nodeId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea(2, 4)] private string description = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField] private AbilityNodeProgressionType progressionType = AbilityNodeProgressionType.Ranked;
        [SerializeField] private List<AbilityNodeRankDefinition> ranks = new();

        public string NodeId => nodeId ?? string.Empty;
        public string DisplayName => displayName ?? string.Empty;
        public string Description => description ?? string.Empty;
        public Sprite Icon => icon;
        public AbilityNodeProgressionType ProgressionType => progressionType;
        public IReadOnlyList<AbilityNodeRankDefinition> Ranks => ranks ??= new List<AbilityNodeRankDefinition>();
        public int MaxLevel => Ranks.Count;

        protected override void NormalizeFields()
        {
            nodeId = (nodeId ?? string.Empty).Trim();
            displayName = (displayName ?? string.Empty).Trim();
            description = (description ?? string.Empty).Trim();
            ranks ??= new List<AbilityNodeRankDefinition>();
            for (var i = 0; i < ranks.Count; i++)
            {
                ranks[i]?.Normalize();
            }
        }

        protected override void CollectValidationIssues(List<ValidationIssue> issues)
        {
            AddRequiredStringIssue(issues, NodeId, "missing_node_id", "NodeId が空です。保存で使う不変IDを設定してください。", ValidationSeverity.Error);
            AddRequiredStringIssue(issues, DisplayName, "missing_display_name", "DisplayName が空です。", ValidationSeverity.Warning);
            AddNullReferenceIssue(issues, Icon, "missing_icon", "Icon が未設定です。", ValidationSeverity.Warning);

            if (ProgressionType == AbilityNodeProgressionType.UnlockOnly && Ranks.Count != 1)
            {
                AddIssue(issues, "invalid_unlock_only_rank_count", "UnlockOnly のRankは1件にしてください。", ValidationSeverity.Error);
            }

            if (ProgressionType == AbilityNodeProgressionType.Ranked && Ranks.Count < 2)
            {
                AddIssue(issues, "invalid_ranked_rank_count", "Ranked のRankは2件以上設定してください。", ValidationSeverity.Error);
            }

            for (var i = 0; i < Ranks.Count; i++)
            {
                var rank = Ranks[i];
                if (rank == null)
                {
                    AddIssue(issues, "null_rank", $"Ranks[{i}] が未設定です。", ValidationSeverity.Error);
                    continue;
                }

                for (var modifierIndex = 0; modifierIndex < rank.BattleStatModifiers.Count; modifierIndex++)
                {
                    var modifier = rank.BattleStatModifiers[modifierIndex];
                    if (modifier == null)
                    {
                        AddIssue(issues, "null_rank_modifier", $"Rank {i + 1} に空の能力値補正があります。", ValidationSeverity.Warning);
                    }
                    else if (modifier.TargetType == BattleStatModifierTargetType.NamedValue &&
                             string.IsNullOrWhiteSpace(modifier.NamedValueKey))
                    {
                        AddIssue(issues, "missing_rank_named_value_key", $"Rank {i + 1} は NamedValue対象ですが key が空です。", ValidationSeverity.Warning);
                    }
                }
            }
        }
    }
}
