using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum AbilityPrerequisiteMode
    {
        All = 0,
        Any = 1,
    }

    public enum AbilityTreeLinkType
    {
        Prerequisite = 0,
        Exclusive = 1,
    }

    /// <summary>
    /// Tree上へ配置されたNodeの実体。進捗・接続・排他はNode MasterではなくこのIDを単位に扱う。
    /// </summary>
    [Serializable]
    public sealed class AbilityTreeNodeEntry
    {
        [SerializeField] private string treeNodeId = string.Empty;
        [SerializeField] private AbilityNodeMasterData node;
        [SerializeField] private AbilityPrerequisiteMode prerequisiteMode = AbilityPrerequisiteMode.All;

        public string TreeNodeId => treeNodeId ?? string.Empty;
        public AbilityNodeMasterData Node => node;
        public AbilityPrerequisiteMode PrerequisiteMode => prerequisiteMode;

        public void Normalize()
        {
            treeNodeId = (treeNodeId ?? string.Empty).Trim();
        }
    }

    /// <summary>Trait Tree UIの1段分の配置。並び順はTreeの始点から終点までとする。</summary>
    [Serializable]
    public sealed class AbilityTreeDisplayRow
    {
        [SerializeField] private string centerTreeNodeId = string.Empty;
        [SerializeField] private string leftTreeNodeId = string.Empty;
        [SerializeField] private string rightTreeNodeId = string.Empty;

        public string CenterTreeNodeId => centerTreeNodeId ?? string.Empty;
        public string LeftTreeNodeId => leftTreeNodeId ?? string.Empty;
        public string RightTreeNodeId => rightTreeNodeId ?? string.Empty;
        public int NodeCount => !string.IsNullOrWhiteSpace(CenterTreeNodeId)
            ? 1
            : !string.IsNullOrWhiteSpace(LeftTreeNodeId) && !string.IsNullOrWhiteSpace(RightTreeNodeId) ? 2 : 0;

        public void Normalize()
        {
            centerTreeNodeId = (centerTreeNodeId ?? string.Empty).Trim();
            leftTreeNodeId = (leftTreeNodeId ?? string.Empty).Trim();
            rightTreeNodeId = (rightTreeNodeId ?? string.Empty).Trim();
        }
    }

    [Serializable]
    public sealed class AbilityTreeLinkDefinition
    {
        // 旧形式Assetを読み込んだ際に参照を失わないための互換フィールド。新規接続では使用しない。
        [SerializeField] private AbilityNodeMasterData fromNode;
        [SerializeField] private AbilityNodeMasterData toNode;
        [SerializeField] private AbilityTreeLinkType linkType;
        [SerializeField] private string fromTreeNodeId = string.Empty;
        [SerializeField] private string toTreeNodeId = string.Empty;
        [SerializeField, Min(1)] private int requiredLevel = 1;

        public AbilityTreeLinkType LinkType => linkType;
        public string FromTreeNodeId => fromTreeNodeId ?? string.Empty;
        public string ToTreeNodeId => toTreeNodeId ?? string.Empty;
        public int RequiredLevel => Mathf.Max(1, requiredLevel);

        // 旧形式Asset用の参照。新規実装はTreeNodeIdを使用する。
        public AbilityNodeMasterData FromNode => fromNode;

        public AbilityNodeMasterData ToNode => toNode;

        public void Normalize()
        {
            fromTreeNodeId = (fromTreeNodeId ?? string.Empty).Trim();
            toTreeNodeId = (toTreeNodeId ?? string.Empty).Trim();
            requiredLevel = Mathf.Max(1, requiredLevel);
        }
    }

    [CreateAssetMenu(menuName = "Original/Master Data/Ability Tree", fileName = "AbilityTreeMasterData")]
    public sealed class AbilityTreeMasterData : ValidatedMasterDataAsset
    {
        [SerializeField] private string treeId = "trait";
        [SerializeField] private string displayName = "Trait";
        [SerializeField] private List<AbilityTreeNodeEntry> nodeEntries = new();
        [SerializeField] private List<AbilityTreeLinkDefinition> links = new();
        [SerializeField] private List<AbilityTreeDisplayRow> displayRows = new();

        public string TreeId => treeId ?? string.Empty;
        public string DisplayName => displayName ?? string.Empty;
        public IReadOnlyList<AbilityTreeNodeEntry> NodeEntries => nodeEntries ??= new List<AbilityTreeNodeEntry>();
        public IReadOnlyList<AbilityTreeLinkDefinition> Links => links ??= new List<AbilityTreeLinkDefinition>();
        public IReadOnlyList<AbilityTreeDisplayRow> DisplayRows => displayRows ??= new List<AbilityTreeDisplayRow>();

        public AbilityTreeNodeEntry FindNodeEntry(string treeNodeId)
        {
            for (var i = 0; i < NodeEntries.Count; i++)
            {
                var entry = NodeEntries[i];
                if (entry != null && string.Equals(entry.TreeNodeId, treeNodeId, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return null;
        }

        protected override void NormalizeFields()
        {
            treeId = (treeId ?? string.Empty).Trim();
            displayName = (displayName ?? string.Empty).Trim();
            nodeEntries ??= new List<AbilityTreeNodeEntry>();
            links ??= new List<AbilityTreeLinkDefinition>();
            displayRows ??= new List<AbilityTreeDisplayRow>();
            for (var i = 0; i < nodeEntries.Count; i++)
            {
                nodeEntries[i]?.Normalize();
            }

            for (var i = 0; i < links.Count; i++)
            {
                links[i]?.Normalize();
            }

            for (var i = 0; i < displayRows.Count; i++)
            {
                displayRows[i]?.Normalize();
            }
        }

        protected override void CollectValidationIssues(List<ValidationIssue> issues)
        {
            AddRequiredStringIssue(issues, TreeId, "missing_tree_id", "TreeId が空です。", ValidationSeverity.Error);
            if (NodeEntries.Count == 0)
            {
                AddIssue(issues, "empty_tree", "Node Entries が空です。Treeに所属するNodeを設定してください。", ValidationSeverity.Warning);
                return;
            }

            var entriesById = CollectEntries(issues);
            ValidateNodeMasters(issues, entriesById);
            ValidateLinks(issues, entriesById);
            ValidatePrerequisiteCycles(issues, entriesById);
            ValidateDisplayRows(issues, entriesById);
        }

        private void ValidateDisplayRows(
            List<ValidationIssue> issues,
            IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById)
        {
            if (DisplayRows.Count == 0)
            {
                AddIssue(issues, "empty_display_rows", "Display Rows が空です。Trait Tree UI用の配置を設定してください。", ValidationSeverity.Warning);
                return;
            }

            var displayedIds = new HashSet<string>(StringComparer.Ordinal);
            for (var rowIndex = 0; rowIndex < DisplayRows.Count; rowIndex++)
            {
                var row = DisplayRows[rowIndex];
                if (row == null)
                {
                    AddIssue(issues, "null_display_row", $"Display Rows[{rowIndex}] が未設定です。", ValidationSeverity.Error);
                    continue;
                }

                var hasCenter = !string.IsNullOrWhiteSpace(row.CenterTreeNodeId);
                var hasLeft = !string.IsNullOrWhiteSpace(row.LeftTreeNodeId);
                var hasRight = !string.IsNullOrWhiteSpace(row.RightTreeNodeId);
                if (hasCenter == (hasLeft || hasRight) || hasLeft != hasRight)
                {
                    AddIssue(issues, "invalid_display_row_slots", $"Display Rows[{rowIndex}] は Center 1件、または Left / Right 2件だけを設定してください。", ValidationSeverity.Error);
                    continue;
                }

                ValidateDisplayRowNode(issues, entriesById, displayedIds, row.CenterTreeNodeId, rowIndex, "Center");
                ValidateDisplayRowNode(issues, entriesById, displayedIds, row.LeftTreeNodeId, rowIndex, "Left");
                ValidateDisplayRowNode(issues, entriesById, displayedIds, row.RightTreeNodeId, rowIndex, "Right");
            }

            foreach (var treeNodeId in entriesById.Keys)
            {
                if (!displayedIds.Contains(treeNodeId))
                {
                    AddIssue(issues, "missing_display_row_node", $"TreeNodeId {treeNodeId} が Display Rows にありません。", ValidationSeverity.Error);
                }
            }
        }

        private static void ValidateDisplayRowNode(
            List<ValidationIssue> issues,
            IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById,
            HashSet<string> displayedIds,
            string treeNodeId,
            int rowIndex,
            string slotName)
        {
            if (string.IsNullOrWhiteSpace(treeNodeId))
            {
                return;
            }

            if (!entriesById.ContainsKey(treeNodeId))
            {
                AddIssue(issues, "display_row_node_outside_tree", $"Display Rows[{rowIndex}] {slotName} がTree外のTreeNodeIdを参照しています: {treeNodeId}", ValidationSeverity.Error);
            }
            else if (!displayedIds.Add(treeNodeId))
            {
                AddIssue(issues, "duplicate_display_row_node", $"TreeNodeId {treeNodeId} が Display Rows に重複しています。", ValidationSeverity.Error);
            }
        }

        private Dictionary<string, AbilityTreeNodeEntry> CollectEntries(List<ValidationIssue> issues)
        {
            var entriesById = new Dictionary<string, AbilityTreeNodeEntry>(StringComparer.Ordinal);
            for (var i = 0; i < NodeEntries.Count; i++)
            {
                var entry = NodeEntries[i];
                if (entry == null || entry.Node == null)
                {
                    AddIssue(issues, "missing_tree_node", $"Node Entries[{i}] の Nodeが未設定です。", ValidationSeverity.Error);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.TreeNodeId))
                {
                    AddIssue(issues, "missing_tree_node_id", $"Node Entries[{i}] の TreeNodeId が空です。", ValidationSeverity.Error);
                    continue;
                }

                if (!entriesById.TryAdd(entry.TreeNodeId, entry))
                {
                    AddIssue(issues, "duplicate_tree_node_id", $"TreeNodeId が重複しています: {entry.TreeNodeId}", ValidationSeverity.Error);
                }
            }

            return entriesById;
        }

        private static void ValidateNodeMasters(
            List<ValidationIssue> issues,
            IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById)
        {
            foreach (var pair in entriesById)
            {
                foreach (var issue in pair.Value.Node.GetValidationIssues())
                {
                    AddIssue(issues, $"node_{pair.Key}_{issue.Code}", $"TreeNode {pair.Key}: {issue.Message}", issue.Severity);
                }
            }
        }

        private void ValidateLinks(
            List<ValidationIssue> issues,
            IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById)
        {
            var seenLinks = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < Links.Count; i++)
            {
                var link = Links[i];
                if (link == null || string.IsNullOrWhiteSpace(link.FromTreeNodeId) || string.IsNullOrWhiteSpace(link.ToTreeNodeId))
                {
                    AddIssue(issues, "missing_link_node_id", $"Links[{i}] の From Tree Node Id または To Tree Node Id が未設定です。", ValidationSeverity.Error);
                    continue;
                }

                if (link.FromTreeNodeId == link.ToTreeNodeId)
                {
                    AddIssue(issues, "self_link", $"Links[{i}] が同一TreeNodeを接続しています: {link.FromTreeNodeId}", ValidationSeverity.Error);
                    continue;
                }

                if (!entriesById.TryGetValue(link.FromTreeNodeId, out var fromEntry) ||
                    !entriesById.ContainsKey(link.ToTreeNodeId))
                {
                    AddIssue(issues, "link_node_outside_tree", $"Links[{i}] がTreeに所属しないTreeNodeIdを参照しています。", ValidationSeverity.Error);
                    continue;
                }

                if (link.LinkType == AbilityTreeLinkType.Prerequisite && link.RequiredLevel > fromEntry.Node.MaxLevel)
                {
                    AddIssue(issues, "unreachable_prerequisite_level", $"Links[{i}] は {link.FromTreeNodeId} に到達不能なLv.{link.RequiredLevel}を要求しています。", ValidationSeverity.Error);
                }

                if (!seenLinks.Add(BuildLinkKey(link)))
                {
                    AddIssue(issues, "duplicate_link", $"Links[{i}] と同じ接続が重複しています。", ValidationSeverity.Warning);
                }
            }
        }

        private void ValidatePrerequisiteCycles(
            List<ValidationIssue> issues,
            IReadOnlyDictionary<string, AbilityTreeNodeEntry> entriesById)
        {
            var prerequisitesById = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var treeNodeId in entriesById.Keys)
            {
                prerequisitesById.Add(treeNodeId, new List<string>());
            }

            for (var i = 0; i < Links.Count; i++)
            {
                var link = Links[i];
                if (link == null || link.LinkType != AbilityTreeLinkType.Prerequisite ||
                    !prerequisitesById.ContainsKey(link.FromTreeNodeId) || !prerequisitesById.ContainsKey(link.ToTreeNodeId))
                {
                    continue;
                }

                prerequisitesById[link.ToTreeNodeId].Add(link.FromTreeNodeId);
            }

            var states = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var treeNodeId in prerequisitesById.Keys)
            {
                VisitPrerequisites(treeNodeId, prerequisitesById, states, issues);
            }
        }

        private static void VisitPrerequisites(
            string treeNodeId,
            IReadOnlyDictionary<string, List<string>> prerequisitesById,
            Dictionary<string, int> states,
            List<ValidationIssue> issues)
        {
            if (states.TryGetValue(treeNodeId, out var state))
            {
                if (state == 1)
                {
                    AddIssue(issues, "cyclic_prerequisite", $"TreeNode {treeNodeId} を含む前提Nodeの循環があります。", ValidationSeverity.Error);
                }

                return;
            }

            states[treeNodeId] = 1;
            foreach (var prerequisiteId in prerequisitesById[treeNodeId])
            {
                VisitPrerequisites(prerequisiteId, prerequisitesById, states, issues);
            }

            states[treeNodeId] = 2;
        }

        private static string BuildLinkKey(AbilityTreeLinkDefinition link)
        {
            if (link.LinkType == AbilityTreeLinkType.Exclusive &&
                string.CompareOrdinal(link.FromTreeNodeId, link.ToTreeNodeId) > 0)
            {
                return $"{link.LinkType}:{link.ToTreeNodeId}:{link.FromTreeNodeId}";
            }

            return $"{link.LinkType}:{link.FromTreeNodeId}:{link.ToTreeNodeId}";
        }
    }
}
