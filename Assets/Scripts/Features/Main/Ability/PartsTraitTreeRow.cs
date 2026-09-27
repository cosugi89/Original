using System;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Main.Ability;
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
            AbilityTreeDisplayRow displayRow,
            AbilityTreeDisplayRow previousRow,
            TraitTreePresentation presentation,
            Action<string, PartsTraitItem.VisualStyle> onItemClicked)
        {
            SetupItem(traitItemCenter, displayRow.CenterTreeNodeId, presentation, onItemClicked);
            SetupItem(traitItemLeft, displayRow.LeftTreeNodeId, presentation, onItemClicked);
            SetupItem(traitItemRight, displayRow.RightTreeNodeId, presentation, onItemClicked);

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
                lineOneToOne.SetupCenter(presentation.IsLinkEnabled(previousRow.CenterTreeNodeId, displayRow.CenterTreeNodeId));
            }
            else if (previousRow.NodeCount == 1)
            {
                lineOneToTwo.gameObject.SetActive(true);
                lineOneToTwo.SetupLeft(presentation.IsLinkEnabled(previousRow.CenterTreeNodeId, displayRow.LeftTreeNodeId));
                lineOneToTwo.SetupRight(presentation.IsLinkEnabled(previousRow.CenterTreeNodeId, displayRow.RightTreeNodeId));
            }
            else if (displayRow.NodeCount == 1)
            {
                lineTwoToOne.gameObject.SetActive(true);
                lineTwoToOne.SetupLeft(presentation.IsTwoToOneLineEnabled(previousRow.LeftTreeNodeId, displayRow.CenterTreeNodeId));
                lineTwoToOne.SetupRight(presentation.IsTwoToOneLineEnabled(previousRow.RightTreeNodeId, displayRow.CenterTreeNodeId));
            }
            else
            {
                lineTwoToTwo.gameObject.SetActive(true);
                lineTwoToTwo.SetupLeft(presentation.IsLinkEnabled(previousRow.LeftTreeNodeId, displayRow.LeftTreeNodeId));
                lineTwoToTwo.SetupRight(presentation.IsLinkEnabled(previousRow.RightTreeNodeId, displayRow.RightTreeNodeId));
            }
        }

        private static void SetupItem(
            PartsTraitItem item,
            string treeNodeId,
            TraitTreePresentation presentation,
            Action<string, PartsTraitItem.VisualStyle> onItemClicked)
        {
            var hasNode = !string.IsNullOrWhiteSpace(treeNodeId);
            item.gameObject.SetActive(hasNode);
            if (!hasNode)
            {
                return;
            }

            var state = presentation.GetNodeState(treeNodeId);
            var visualState = presentation.GetVisualState(treeNodeId);
            var visualStyle = new PartsTraitItem.VisualStyle(
                state.Entry.Node.Icon,
                state.CurrentLevel,
                state.MaxLevel,
                visualState);
            item.Setup(visualStyle);
            item.SetClickAction(() => onItemClicked(treeNodeId, visualStyle));
        }

    }
}
