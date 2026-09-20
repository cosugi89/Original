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
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            var isStart = previousRow == null;
            SetupItem(traitItemCenter, displayRow.CenterTreeNodeId, isStart, tree, previousRow, statesById);
            SetupItem(traitItemLeft, displayRow.LeftTreeNodeId, isStart, tree, previousRow, statesById);
            SetupItem(traitItemRight, displayRow.RightTreeNodeId, isStart, tree, previousRow, statesById);

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
                lineTwoToOne.SetupLeft(IsLinkEnabled(tree, previousRow.LeftTreeNodeId, displayRow.CenterTreeNodeId, statesById));
                lineTwoToOne.SetupRight(IsLinkEnabled(tree, previousRow.RightTreeNodeId, displayRow.CenterTreeNodeId, statesById));
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
            bool isStart,
            AbilityTreeMasterData tree,
            AbilityTreeDisplayRow previousRow,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            var hasNode = !string.IsNullOrWhiteSpace(treeNodeId);
            item.gameObject.SetActive(hasNode);
            if (!hasNode)
            {
                return;
            }

            var state = statesById[treeNodeId];
            item.SetContent(state.Entry.Node.Icon, state.CurrentLevel, state.MaxLevel);
            item.SetAlertVisible(false);
            item.SetUpgradeMarkerVisible(false);
            if (isStart)
            {
                item.ShowFocused();
            }
            else if (state.CurrentLevel > 0)
            {
                item.ShowFocused();
            }
            else if (HasFocusedPreviousItem(tree, previousRow, treeNodeId, statesById))
            {
                item.ShowNormal();
            }
            else
            {
                item.ShowDisabled();
            }
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
                if (IsFocusedOrPurchased(tree, link.FromTreeNodeId, statesById))
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

        private static bool ContainsNodeId(AbilityTreeDisplayRow row, string treeNodeId)
        {
            return row.CenterTreeNodeId == treeNodeId ||
                   row.LeftTreeNodeId == treeNodeId ||
                   row.RightTreeNodeId == treeNodeId;
        }

        private static bool IsFocusedOrPurchased(
            AbilityTreeMasterData tree,
            string treeNodeId,
            IReadOnlyDictionary<string, AbilityTreeNodeState> statesById)
        {
            if (statesById[treeNodeId].CurrentLevel > 0)
            {
                return true;
            }

            for (var i = 0; i < tree.Links.Count; i++)
            {
                var link = tree.Links[i];
                if (link != null && link.LinkType == AbilityTreeLinkType.Prerequisite &&
                    link.ToTreeNodeId == treeNodeId)
                {
                    return false;
                }
            }

            return true;
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
    }
}
