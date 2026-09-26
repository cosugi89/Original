using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Item Master Database", fileName = "ItemMasterDatabase")]
    public sealed class ItemMasterDatabase : ScriptableObject
    {
        [SerializeField, Tooltip("Items ダイアログで解決するアイテム定義一覧です。")]
        private List<ItemMasterData> items = new();

        public IReadOnlyList<ItemMasterData> GetImportantItems()
        {
            return items
                .Where(item => item != null && item.ItemId > 0 && item.IsImportantItem)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.ItemId)
                .ToArray();
        }
    }
}
