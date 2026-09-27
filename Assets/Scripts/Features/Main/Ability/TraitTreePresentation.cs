using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.UI.Components;

namespace Assets.Scripts.Features.Main.Ability
{
    /// <summary>Trait Treeのゲーム状態を、Row UIが直接描画できる状態へ変換する。</summary>
    public sealed class TraitTreePresentation
    {
        private readonly AbilityTreeMasterData _tree;
        private readonly IReadOnlyDictionary<string, AbilityTreeNodeState> _states;
        private readonly IReadOnlyDictionary<string, PartsTraitItem.VisualState> _visualStates;

        public TraitTreePresentation(
            AbilityTreeMasterData tree,
            IReadOnlyDictionary<string, AbilityTreeNodeState> states,
            IReadOnlyDictionary<string, PartsTraitItem.VisualState> visualStates)
        {
            _tree = tree;
            _states = states;
            _visualStates = visualStates;
        }

        public AbilityTreeNodeState GetNodeState(string treeNodeId) => _states[treeNodeId];
        public PartsTraitItem.VisualState GetVisualState(string treeNodeId) => _visualStates[treeNodeId];

        public bool IsLinkEnabled(string fromTreeNodeId, string toTreeNodeId)
        {
            return HasPrerequisiteLink(fromTreeNodeId, toTreeNodeId) &&
                   _states[toTreeNodeId].CurrentLevel > 0;
        }

        public bool IsTwoToOneLineEnabled(string fromTreeNodeId, string toTreeNodeId)
        {
            return _states[fromTreeNodeId].CurrentLevel > 0 && IsLinkEnabled(fromTreeNodeId, toTreeNodeId);
        }

        private bool HasPrerequisiteLink(string fromTreeNodeId, string toTreeNodeId)
        {
            for (var i = 0; i < _tree.Links.Count; i++)
            {
                var link = _tree.Links[i];
                if (link != null && link.LinkType == AbilityTreeLinkType.Prerequisite &&
                    link.FromTreeNodeId == fromTreeNodeId && link.ToTreeNodeId == toTreeNodeId)
                    return true;
            }
            return false;
        }
    }

    public static class TraitTreePresentationBuilder
    {
        public static TraitTreePresentation Build(AbilityTreeMasterData tree, AbilityDerivedData derivedData)
        {
            var states = new Dictionary<string, AbilityTreeNodeState>(StringComparer.Ordinal);
            for (var i = 0; i < derivedData.NodeStates.Count; i++)
            {
                var state = derivedData.NodeStates[i];
                if (state?.Entry != null)
                    states[state.Entry.TreeNodeId] = state;
            }

            var visuals = new Dictionary<string, PartsTraitItem.VisualState>(StringComparer.Ordinal);
            for (var rowIndex = 0; rowIndex < tree.DisplayRows.Count; rowIndex++)
            {
                var row = tree.DisplayRows[rowIndex];
                var previousRow = rowIndex > 0 ? tree.DisplayRows[rowIndex - 1] : null;
                SetVisual(row.CenterTreeNodeId, previousRow, tree, states, visuals);
                SetVisual(row.LeftTreeNodeId, previousRow, tree, states, visuals);
                SetVisual(row.RightTreeNodeId, previousRow, tree, states, visuals);
            }
            return new TraitTreePresentation(tree, states, visuals);
        }

        private static void SetVisual(string id, AbilityTreeDisplayRow previousRow, AbilityTreeMasterData tree,
            IReadOnlyDictionary<string, AbilityTreeNodeState> states,
            IDictionary<string, PartsTraitItem.VisualState> visuals)
        {
            if (string.IsNullOrWhiteSpace(id) || !states.TryGetValue(id, out var state)) return;
            if (state.CurrentLevel > 0) { visuals[id] = PartsTraitItem.VisualState.Focused; return; }
            if (state.IsExclusiveClosed || state.LockReason == AbilityNodeLockReason.ImpossiblePrerequisites ||
                IsPermanentlyClosedByPreviousRow(id, previousRow, tree, states, visuals))
            {
                visuals[id] = PartsTraitItem.VisualState.Disabled;
                return;
            }
            visuals[id] = previousRow == null || HasSatisfiedPreviousPrerequisites(id, previousRow, tree, states)
                ? PartsTraitItem.VisualState.Normal
                : PartsTraitItem.VisualState.Locked;
        }

        private static bool HasSatisfiedPreviousPrerequisites(string id, AbilityTreeDisplayRow row,
            AbilityTreeMasterData tree, IReadOnlyDictionary<string, AbilityTreeNodeState> states)
        {
            var hasPrerequisite = false;
            var any = false;
            var all = true;
            foreach (var link in PrerequisitesFromRow(id, row, tree))
            {
                hasPrerequisite = true;
                var purchased = states[link.FromTreeNodeId].CurrentLevel >= link.RequiredLevel;
                any |= purchased; all &= purchased;
            }
            if (!hasPrerequisite) return false;
            return states[id].Entry.PrerequisiteMode == AbilityPrerequisiteMode.Any ? any : all;
        }

        private static bool IsPermanentlyClosedByPreviousRow(string id, AbilityTreeDisplayRow row,
            AbilityTreeMasterData tree, IReadOnlyDictionary<string, AbilityTreeNodeState> states,
            IDictionary<string, PartsTraitItem.VisualState> visuals)
        {
            if (row == null) return false;
            var any = false; var anyDisabled = false; var allDisabled = true;
            foreach (var link in PrerequisitesFromRow(id, row, tree))
            {
                any = true;
                var disabled = visuals.TryGetValue(link.FromTreeNodeId, out var visual) &&
                               visual == PartsTraitItem.VisualState.Disabled;
                anyDisabled |= disabled;
                allDisabled &= disabled;
            }
            if (!any) return false;
            return states[id].Entry.PrerequisiteMode == AbilityPrerequisiteMode.All ? anyDisabled : allDisabled;
        }

        private static IEnumerable<AbilityTreeLinkDefinition> PrerequisitesFromRow(string id, AbilityTreeDisplayRow row,
            AbilityTreeMasterData tree)
        {
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link != null && link.LinkType == AbilityTreeLinkType.Prerequisite && link.ToTreeNodeId == id &&
                    Contains(row, link.FromTreeNodeId)) yield return link;
            }
        }

        private static bool Contains(AbilityTreeDisplayRow row, string id) => row.CenterTreeNodeId == id || row.LeftTreeNodeId == id || row.RightTreeNodeId == id;
    }
}
