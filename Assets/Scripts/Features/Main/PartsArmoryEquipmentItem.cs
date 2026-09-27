using System;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class PartsArmoryEquipmentItem : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI tagsText;
        [SerializeField] private Image currencyImage;
        [SerializeField] private TextMeshProUGUI purchaseText;
        [SerializeField] private FeedbackButton button;

        public void Setup(ArmoryProductMasterData product, bool isSoldOut, Action onClickPurchase)
        {
            var equipment = product.Equipment;
            iconImage.sprite = equipment.Icon;
            iconImage.enabled = equipment.Icon != null;
            nameText.SetText(equipment.DisplayName);
            tagsText.SetText(string.Join(" / ", equipment.CategoryTags.Where(tag => !string.IsNullOrWhiteSpace(tag))));
            currencyImage.gameObject.SetActive(!isSoldOut);
            purchaseText.SetText(!isSoldOut ? product.GoldPrice.ToString() : "Sold Out");
            button.SetAction(onClickPurchase);
        }
    }
}
