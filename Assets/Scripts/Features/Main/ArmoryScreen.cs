using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Dialog;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public sealed class ArmoryScreen : MonoBehaviour
    {
        [SerializeField] private Transform itemContent;
        [SerializeField] private Transform equipmentContent;
        [SerializeField] private PartsArmoryItemItem itemProductPrefab;
        [SerializeField] private PartsArmoryEquipmentItem equipmentProductPrefab;

        private readonly List<GameObject> _productViews = new();

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            foreach (var productView in _productViews)
                Destroy(productView);
            _productViews.Clear();

            var armory = UserDataManager.EnsureInitialized().Armory;
            var products = armory.GetUnlockedProducts();

            foreach (var product in products.Where(product => product.ProductType == ArmoryProductType.Item))
            {
                var remainingCount = armory.GetRemainingPurchaseCount(product);
                var isSoldOut = remainingCount == 0;
                var productView = Instantiate(itemProductPrefab, itemContent);
                productView.Setup(product, remainingCount, isSoldOut, () => OnClickPurchase(product));
                _productViews.Add(productView.gameObject);
            }

            foreach (var product in products
                         .Where(product => product.ProductType == ArmoryProductType.Equipment)
                         .OrderBy(product => IsSoldOut(product, armory))
                         .ThenBy(product => product.SortOrder)
                         .ThenBy(product => product.ProductId))
            {
                var isSoldOut = IsSoldOut(product, armory);
                if (isSoldOut)
                {
                    var remainingPurchaseCount = armory.GetRemainingPurchaseCount(product);
                    var isAlreadyOwned = armory.HasEquipment(product.Equipment.EquipmentId);
                    Debug.LogWarning(
                        $"[Armory] Equipment product is disabled: ProductId={product.ProductId}, " +
                        $"EquipmentId={product.Equipment.EquipmentId}, " +
                        $"RemainingPurchaseCount={remainingPurchaseCount}, " +
                        $"IsAlreadyOwned={isAlreadyOwned}.",
                        this);
                }

                var productView = Instantiate(equipmentProductPrefab, equipmentContent);
                productView.Setup(product, isSoldOut, () => OnClickPurchase(product));
                _productViews.Add(productView.gameObject);
            }
        }

        private void OnClickPurchase(ArmoryProductMasterData product) => OpenPurchaseDialogAsync(product).Forget();

        private async UniTaskVoid OpenPurchaseDialogAsync(ArmoryProductMasterData product)
        {
            var armory = UserDataManager.EnsureInitialized().Armory;
            var canPurchase = CanPurchase(product, armory);
            var isPurchaseConfirmed = product.ProductType switch
            {
                ArmoryProductType.Item => await OpenItemPurchaseDialogAsync(product, canPurchase),
                ArmoryProductType.Equipment => await OpenEquipmentPurchaseDialogAsync(product, canPurchase),
                _ => false,
            };

            if (isPurchaseConfirmed && armory.TryPurchase(product.ProductId))
            {
                Refresh();
            }
        }

        private static async UniTask<bool> OpenItemPurchaseDialogAsync(ArmoryProductMasterData product, bool canPurchase)
        {
            var result = await DialogService.OpenAsync<
                DialogCommonConfirmWithIcon,
                RequestDialogCommonConfirmWithIcon,
                ResultDialogCommonConfirmWithIcon>(
                new RequestDialogCommonConfirmWithIcon(
                    product.Item.DisplayName,
                    product.Quantity,
                    product.Item.Description,
                    product.Item.Icon,
                    canPurchase),
                new DialogContext(null));
            return result.IsConfirmed;
        }

        private static async UniTask<bool> OpenEquipmentPurchaseDialogAsync(ArmoryProductMasterData product, bool canPurchase)
        {
            var result = await DialogService.OpenAsync<
                DialogEquipmentDetail,
                RequestDialogEquipmentDetail,
                ResultDialogEquipmentDetail>(
                RequestDialogEquipmentDetail.ForPurchase(product.Equipment, product.GoldPrice, canPurchase),
                new DialogContext(null));
            return result.IsPurchaseRequested;
        }

        private static bool CanPurchase(ArmoryProductMasterData product, ArmoryManager armory)
        {
            if (product == null || armory.GetRemainingPurchaseCount(product) == 0)
            {
                return false;
            }

            if (product.ProductType == ArmoryProductType.Equipment &&
                armory.HasEquipment(product.Equipment.EquipmentId))
            {
                return false;
            }

            return UserDataManager.EnsureInitialized().Inventory.GetItemCount(InventoryItemIds.Gold) >= product.GoldPrice;
        }

        private static bool IsSoldOut(ArmoryProductMasterData product, ArmoryManager armory)
        {
            return armory.GetRemainingPurchaseCount(product) == 0 || armory.HasEquipment(product.Equipment.EquipmentId);
        }
    }
}
