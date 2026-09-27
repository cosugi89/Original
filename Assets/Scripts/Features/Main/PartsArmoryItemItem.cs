using System;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class PartsArmoryItemItem : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI grantedQuantityText;
        [SerializeField] private TextMeshProUGUI remainingPurchaseCountText;
        [SerializeField] private GameButton purchaseButton;

        public void Setup(ArmoryProductMasterData product, int remainingPurchaseCount, bool isSoldOut, Action onClickPurchase)
        {
            iconImage.sprite = product.Item.Icon;
            iconImage.enabled = product.Item.Icon != null;
            nameText.SetText(product.Item.DisplayName);
            grantedQuantityText.SetText($"x{product.Quantity}");
            var showRemaining = product.PurchaseLimit > 0;
            remainingPurchaseCountText.gameObject.SetActive(showRemaining);
            remainingPurchaseCountText.SetText(showRemaining ? remainingPurchaseCount.ToString() : string.Empty);
            purchaseButton.SetLabel(product.GoldPrice.ToString());
            purchaseButton.SetInteractable(!isSoldOut);
            purchaseButton.SetAction(onClickPurchase);
        }
    }
}
