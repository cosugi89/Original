using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using UnityEngine;

namespace Assets.Scripts.UI.Components
{
    public class PartsTraitTreeRow : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private PartsTraitItem traitItemCenter;
        [SerializeField] private PartsTraitItem traitItemRight;
        [SerializeField] private PartsTraitItem traitItemLeft;
        [SerializeField] private PartsTraitLine lineOneToOne; // 1 → 1
        [SerializeField] private PartsTraitLine lineOneToTwo; // 1 → 2
        [SerializeField] private PartsTraitLine lineTwoToTwo; // 2 → 2
        [SerializeField] private PartsTraitLine lineTwoToOne; // 2 → 1

        public void Setup(
            AbilityTreeMasterData tree,
            AbilityTreeDisplayRow displayRow,
            AbilityTreeDisplayRow previousRow,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById,
            Action<string, PartsTraitItem.VisualStyle> onItemClicked)
        {
            SetupItem(traitItemCenter, displayRow.CenterTreeNodeId, tree, previousRow, statesById, onItemClicked);
            SetupItem(traitItemLeft, displayRow.LeftTreeNodeId, tree, previousRow, statesById, onItemClicked);
            SetupItem(traitItemRight, displayRow.RightTreeNodeId, tree, previousRow, statesById, onItemClicked);

            lineOneToOne.gameObject.SetActive(false);
            lineOneToTwo.gameObject.SetActive(false);
            lineTwoToOne.gameObject.SetActive(false);
            lineTwoToTwo.gameObject.SetActive(false);
            if (previousRow == null)
            {
                return;
            }

            if (previousRow.NodeCount == 1 && displayRow.NodeCount == 1)
            {
                lineOneToOne.gameObject.SetActive(true);
                lineOneToOne.SetupCenter(IsLinkEnabled(tree, previousRow.CenterTreeNodeId, displayRow.CenterTreeNodeId, statesById));
            }
            else if (previousRow.NodeCount == 1)
            {
                lineOneToTwo.gameObject.SetActive(true);
                lineOneToTwo.SetupLeft(IsLinkEnabled(tree, previousRow.CenterTreeNodeId, displayRow.LeftTreeNodeId, statesById));
                lineOneToTwo.SetupRight(IsLinkEnabled(tree, previousRow.CenterTreeNodeId, displayRow.RightTreeNodeId, statesById));
            }
            else if (displayRow.NodeCount == 1)
            {
                lineTwoToOne.gameObject.SetActive(true);
                lineTwoToOne.SetupLeft(IsTwoToOneLineEnabled(tree, previousRow.LeftTreeNodeId, displayRow.CenterTreeNodeId, statesById));
                lineTwoToOne.SetupRight(IsTwoToOneLineEnabled(tree, previousRow.RightTreeNodeId, displayRow.CenterTreeNodeId, statesById));
            }
            else
            {
                lineTwoToTwo.gameObject.SetActive(true);
                lineTwoToTwo.SetupLeft(IsLinkEnabled(tree, previousRow.LeftTreeNodeId, displayRow.LeftTreeNodeId, statesById));
                lineTwoToTwo.SetupRight(IsLinkEnabled(tree, previousRow.RightTreeNodeId, displayRow.RightTreeNodeId, statesById));
            }
        }

        private static void SetupItem(
            PartsTraitItem item,
            string treeNodeId,
            AbilityTreeMasterData tree,
            AbilityTreeDisplayRow previousRow,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById,
            Action<string, PartsTraitItem.VisualStyle> onItemClicked)
        {
            var hasNode = !string.IsNullOrWhiteSpace(treeNodeId);
            item.gameObject.SetActive(hasNode);
            if (!hasNode)
            {
                return;
            }

            var state = statesById[treeNodeId];
            var visualState = state.CurrentLevel > 0
                ? PartsTraitItem.VisualState.Focused
                : state.IsExclusiveClosed || state.LockReason == AbilityNodeLockReason.ImpossiblePrerequisites
                    ? PartsTraitItem.VisualState.Disabled
                : IsDisabledByPreviousRow(tree, previousRow, treeNodeId, statesById)
                    ? PartsTraitItem.VisualState.Disabled
                : previousRow == null || HasFocusedPreviousItem(tree, previousRow, treeNodeId, statesById)
                    ? PartsTraitItem.VisualState.Normal
                    : PartsTraitItem.VisualState.Locked;
            var visualStyle = new PartsTraitItem.VisualStyle(
                state.Entry.Node.Icon,
                state.CurrentLevel,
                state.MaxLevel,
                visualState);
            item.Setup(visualStyle);
            item.SetClickAction(() => onItemClicked(treeNodeId, visualStyle));
        }

        private static bool HasFocusedPreviousItem(
            AbilityTreeMasterData tree,
            AbilityTreeDisplayRow previousRow,
            string treeNodeId,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            if (previousRow == null)
            {
                return false;
            }

            var hasPrerequisite = false;
            var hasFocusedPrerequisite = false;
            var hasUnfocusedPrerequisite = false;
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link == null || link.LinkType != AbilityTreeLinkType.Prerequisite ||
                    link.ToTreeNodeId != treeNodeId || !ContainsNodeId(previousRow, link.FromTreeNodeId))
                {
                    continue;
                }

                hasPrerequisite = true;
                if (IsPurchased(link.FromTreeNodeId, statesById))
                {
                    hasFocusedPrerequisite = true;
                }
                else
                {
                    hasUnfocusedPrerequisite = true;
                }
            }

            if (!hasPrerequisite)
            {
                return false;
            }

            var prerequisiteMode = statesById[treeNodeId].Entry.PrerequisiteMode;
            return prerequisiteMode == AbilityPrerequisiteMode.Any
                ? hasFocusedPrerequisite
                : !hasUnfocusedPrerequisite;
        }

        /// <summary>
        /// 2→2のように同じ経路を継続するNodeは、直前の前提が排他などで恒久的に
        /// Disabledなら、前提不足のLockedではなく同じくDisabledとして扱う。
        /// Anyでは他の直前前提が残っていれば継続できるため、すべてDisabledの場合だけ伝播する。
        /// </summary>
        private static bool IsDisabledByPreviousRow(
            AbilityTreeMasterData tree,
            AbilityTreeDisplayRow previousRow,
            string treeNodeId,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            if (previousRow == null)
            {
                return false;
            }

            var hasPrerequisite = false;
            var hasDisabledPrerequisite = false;
            var hasAvailablePrerequisite = false;
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link == null || link.LinkType != AbilityTreeLinkType.Prerequisite ||
                    link.ToTreeNodeId != treeNodeId || !ContainsNodeId(previousRow, link.FromTreeNodeId))
                {
                    continue;
                }

                hasPrerequisite = true;
                var previousState = statesById[link.FromTreeNodeId];
                var isDisabled = previousState.IsExclusiveClosed ||
                    previousState.LockReason == AbilityNodeLockReason.ImpossiblePrerequisites;
                hasDisabledPrerequisite |= isDisabled;
                hasAvailablePrerequisite |= !isDisabled;
            }

            if (!hasPrerequisite)
            {
                return false;
            }

            return statesById[treeNodeId].Entry.PrerequisiteMode == AbilityPrerequisiteMode.All
                ? hasDisabledPrerequisite
                : !hasAvailablePrerequisite;
        }

        private static bool ContainsNodeId(AbilityTreeDisplayRow row, string treeNodeId)
        {
            return row.CenterTreeNodeId == treeNodeId ||
                   row.LeftTreeNodeId == treeNodeId ||
                   row.RightTreeNodeId == treeNodeId;
        }

        private static bool IsPurchased(
            string treeNodeId,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            return statesById[treeNodeId].CurrentLevel > 0;
        }

        private static bool IsLinkEnabled(
            AbilityTreeMasterData tree,
            string fromTreeNodeId,
            string toTreeNodeId,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link == null || link.LinkType != AbilityTreeLinkType.Prerequisite ||
                    link.FromTreeNodeId != fromTreeNodeId || link.ToTreeNodeId != toTreeNodeId)
                {
                    continue;
                }

                return statesById[toTreeNodeId].CurrentLevel > 0;
            }

            return false;
        }

        private static bool IsTwoToOneLineEnabled(
            AbilityTreeMasterData tree,
            string fromTreeNodeId,
            string toTreeNodeId,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            return statesById[fromTreeNodeId].CurrentLevel > 0 &&
                   IsLinkEnabled(tree, fromTreeNodeId, toTreeNodeId, statesById);
        }
    }
}
