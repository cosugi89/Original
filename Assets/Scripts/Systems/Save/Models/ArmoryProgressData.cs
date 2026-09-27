using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public sealed class ArmoryProgressData
    {
        [Description("Armory 商品ごとの購入回数。Resurgence 後も維持する。")]
        public List<ArmoryProductPurchaseData> Purchases { get; set; } = new();
    }

    public sealed class ArmoryProductPurchaseData
    {
        public int ProductId { get; set; }
        public int PurchaseCount { get; set; }
    }
}
