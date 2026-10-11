using System.Collections.Generic;
using Assets.Scripts.Core.Appearance;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum ArmoryProductType
    {
        Equipment,
        Item
    }

    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Armory Product", fileName = "ArmoryProductMasterData")]
    public sealed class ArmoryProductMasterData : ScriptableObject
    {
        [SerializeField] private int productId;
        [SerializeField] private ArmoryProductType productType;
        [SerializeField] private EquipmentMasterData equipment;
        [SerializeField] private ItemMasterData item;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField, Min(0)] private int purchaseLimit = 1;
        [SerializeField, Min(1)] private int goldPrice = 1;
        [SerializeField] private List<int> requiredClearedStageIds = new();
        [SerializeField] private int sortOrder;

        public int ProductId => productId;
        public ArmoryProductType ProductType => productType;
        public EquipmentMasterData Equipment => equipment;
        public ItemMasterData Item => item;
        public int Quantity => quantity;
        public int PurchaseLimit => purchaseLimit;
        public int GoldPrice => goldPrice;
        public IReadOnlyList<int> RequiredClearedStageIds => requiredClearedStageIds;
        public int SortOrder => sortOrder;
        public bool IsValidForArmory => ProductType == ArmoryProductType.Item
            ? Item != null
            : Equipment != null && IsArmoryEquipmentPartType(Equipment.PartType);

        public static bool IsArmoryEquipmentPartType(PartsType partType)
        {
            return partType is
                PartsType.Helmet or
                PartsType.Chest or
                PartsType.Sword or
                PartsType.Axe or
                PartsType.Bow or
                PartsType.Wand or
                PartsType.Staff or
                PartsType.Spear or
                PartsType.Blunt or
                PartsType.Crossbow or
                PartsType.Shield or
                PartsType.SubItem;
        }

        private void OnValidate()
        {
            if (ProductType == ArmoryProductType.Equipment && Equipment != null &&
                !IsArmoryEquipmentPartType(Equipment.PartType))
            {
                Debug.LogWarning(
                    $"[ArmoryProductMasterData] {name} cannot sell cosmetic part {Equipment.PartType}. " +
                    "Only combat equipment can be registered as an Armory product.",
                    this);
            }
        }
    }
}
