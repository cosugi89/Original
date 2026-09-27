using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.GameData
{
    public sealed class ArmoryManager
    {
        private readonly UserDataJsonStore _userDataStore;
        private readonly InventoryManager _inventory;
        private readonly ProgressManager _progress;
        private readonly IReadOnlyList<ArmoryProductMasterData> _products;

        public ArmoryManager(UserDataJsonStore userDataStore, InventoryManager inventory, ProgressManager progress, ArmoryProductDatabase database)
        {
            _userDataStore = userDataStore;
            _inventory = inventory;
            _progress = progress;
            _products = database != null ? database.Products : System.Array.Empty<ArmoryProductMasterData>();
        }

        public IReadOnlyList<ArmoryProductMasterData> GetUnlockedProducts() => _products
            .Where(product => product.RequiredClearedStageIds.All(_progress.IsCleared))
            .ToArray();

        public int GetPurchaseCount(int productId) => EnsureProgress().Purchases
            .FirstOrDefault(purchase => purchase.ProductId == productId)?.PurchaseCount ?? 0;

        public int GetRemainingPurchaseCount(ArmoryProductMasterData product) => product.PurchaseLimit <= 0
            ? -1
            : System.Math.Max(0, product.PurchaseLimit - GetPurchaseCount(product.ProductId));

        public bool HasEquipment(int equipmentId) => _inventory.HasEquipment(equipmentId);

        public bool TryPurchase(int productId)
        {
            var product = GetUnlockedProducts().FirstOrDefault(candidate => candidate.ProductId == productId);
            if (product == null || GetRemainingPurchaseCount(product) == 0 ||
                (product.ProductType == ArmoryProductType.Equipment && _inventory.HasEquipment(product.Equipment.EquipmentId)))
                return false;
            if (!_inventory.TryConsumeItem(InventoryItemIds.Gold, product.GoldPrice))
                return false;

            if (product.ProductType == ArmoryProductType.Equipment)
            {
                _inventory.GrantEquipment(product.Equipment.EquipmentId, product.Quantity);
            }
            else
            {
                _inventory.AddItem(product.Item.ItemId, product.Quantity);
            }
            var purchase = EnsureProgress().Purchases.FirstOrDefault(candidate => candidate.ProductId == productId);
            if (purchase == null)
            {
                purchase = new ArmoryProductPurchaseData { ProductId = productId };
                EnsureProgress().Purchases.Add(purchase);
            }
            purchase.PurchaseCount++;
            _userDataStore.MarkDirty();
            _userDataStore.SaveSession();
            return true;
        }

        private ArmoryProgressData EnsureProgress()
        {
            _userDataStore.Session.UserData.Armory ??= new ArmoryProgressData();
            _userDataStore.Session.UserData.Armory.Purchases ??= new List<ArmoryProductPurchaseData>();
            return _userDataStore.Session.UserData.Armory;
        }
    }
}
