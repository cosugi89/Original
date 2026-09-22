using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public readonly struct TraitItemDetailDialogRequest
    {
        public TraitItemDetailDialogRequest(
            AbilityTreeNodeState nodeState,
            PartsTraitItem.VisualStyle traitItemVisualStyle)
        {
            NodeState = nodeState;
            TraitItemVisualStyle = traitItemVisualStyle;
        }

        public AbilityTreeNodeState NodeState { get; }
        public PartsTraitItem.VisualStyle TraitItemVisualStyle { get; }
    }

    public readonly struct TraitItemDetailDialogResult
    {
        private TraitItemDetailDialogResult(bool isPurchaseRequested)
        {
            IsPurchaseRequested = isPurchaseRequested;
        }

        public bool IsPurchaseRequested { get; }

        public static TraitItemDetailDialogResult PurchaseRequested => new(true);
        public static TraitItemDetailDialogResult Closed => new(false);
    }

    [DialogAddress("Dialog/TraitItemDetail")]
    public sealed class TraitItemDetailDialog :
        DialogBase<TraitItemDetailDialogResult>,
        IDialogRequestHandler<TraitItemDetailDialogRequest>
    {
        private const float FullLayoutHeight = 625f; // CurrentValue + NextValue + PurchaseButton
        private const float PreviewLayoutHeight = 520f; // CurrentValue + NextValue
        private const float BasicLayoutHeight = 440f; // CurrentValue
        
        [SerializeField] private RectTransform popupPanel;
        [SerializeField] private PartsTraitItem traitItem;
        [SerializeField] private TextMeshProUGUI currentValueText;
        [SerializeField] private GameObject nextValue;
        [SerializeField] private TextMeshProUGUI nextValueText;
        [SerializeField] private GameButton purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseCostText;
        [SerializeField] private GameButton closeButton;
        
        private AbilityTreeNodeState _nodeState;
        private PartsTraitItem.VisualStyle _traitItemVisualStyle;

        public void Setup(TraitItemDetailDialogRequest request, DialogContext context)
        {
            _nodeState = request.NodeState;
            _traitItemVisualStyle = request.TraitItemVisualStyle;
            Refresh();

            purchaseButton.SetAction(RequestPurchase);
            closeButton.SetAction(OnClickClose);
        }

        /// <summary>購入ボタンから呼ぶ。購入自体は呼び出し元へ委譲する。</summary>
        private void RequestPurchase()
        {
            if (_nodeState == null)
            {
                return;
            }

            if (!_nodeState.IsPurchasable)
            {
                if (_nodeState.LockReason == AbilityNodeLockReason.InsufficientPoints)
                {
                    Debug.Log($"[TraitItemDetailDialog] Pointが不足しています。必要: {_nodeState.NextRankPointCost}");
                }

                return;
            }

            Close(TraitItemDetailDialogResult.PurchaseRequested);
        }

        protected override void OnClickClose()
        {
            Close(TraitItemDetailDialogResult.Closed);
        }

        private void Refresh()
        {
            var isMaxLevel = _nodeState.CurrentLevel >= _nodeState.MaxLevel;
            var canShowPurchase = _nodeState.IsPurchasable ||
                _nodeState.LockReason == AbilityNodeLockReason.InsufficientPoints;
            var hasNextRankPreview = !isMaxLevel;
            
            traitItem.Setup(_traitItemVisualStyle);

            currentValueText.text = BuildCurrentValueText(_nodeState);
            nextValueText.text = hasNextRankPreview ? BuildNextValueText(_nodeState) : string.Empty;
            purchaseCostText.text = _nodeState.NextRankPointCost.ToString();
            nextValue.SetActive(hasNextRankPreview);
            
            purchaseButton.SetVisible(canShowPurchase);
            purchaseButton.SetInteractable(canShowPurchase);
            purchaseButton.SetLabel("Purchase");
            
            var size = popupPanel.sizeDelta;
            size.y = isMaxLevel
                ? BasicLayoutHeight
                : canShowPurchase
                    ? FullLayoutHeight
                    : PreviewLayoutHeight;
            popupPanel.sizeDelta = size;
        }

        private static string BuildCurrentValueText(AbilityTreeNodeState state)
        {
            var builder = new StringBuilder();
            for (var rankIndex = 0; rankIndex < state.CurrentLevel; rankIndex++)
            {
                AppendModifierText(builder, state.Entry.Node.Ranks[rankIndex]?.BattleStatModifiers);
            }

            return builder.Length > 0 ? builder.ToString() : "—";
        }

        private static string BuildNextValueText(AbilityTreeNodeState state)
        {
            var builder = new StringBuilder();
            AppendModifierText(builder, state.Entry.Node.Ranks[state.CurrentLevel]?.BattleStatModifiers);
            return builder.Length > 0 ? builder.ToString() : "—";
        }

        private static void AppendModifierText(
            StringBuilder builder,
            IReadOnlyList<BattleStatModifierDefinitionEntry> modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                var target = modifier.TargetType == BattleStatModifierTargetType.NamedValue
                    ? modifier.NamedValueKey
                    : modifier.ParameterType.ToString();
                var value = modifier.OperationType switch
                {
                    BattleStatModifierOperationType.Add => $"{modifier.Value:+0.##;-0.##;0}",
                    BattleStatModifierOperationType.Multiply => $"x{modifier.Value:0.##}",
                    _ => modifier.Value.ToString("0.##")
                };
                builder.Append(target).Append(' ').Append(value);
            }
        }
    }
}
