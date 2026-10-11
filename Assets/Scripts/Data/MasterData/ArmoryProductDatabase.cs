using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Armory Product Database", fileName = "ArmoryProductDatabase")]
    public sealed class ArmoryProductDatabase : ScriptableObject
    {
        [SerializeField] private List<ArmoryProductMasterData> products = new();

        public IReadOnlyList<ArmoryProductMasterData> Products => products
            .Where(product => product != null && product.ProductId > 0 && product.IsValidForArmory)
            .OrderBy(product => product.SortOrder)
            .ThenBy(product => product.ProductId)
            .ToArray();
    }
}
