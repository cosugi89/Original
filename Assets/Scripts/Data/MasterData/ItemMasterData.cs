using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Item Master Data", fileName = "ItemMasterData")]
    public sealed class ItemMasterData : ScriptableObject
    {
        [SerializeField, Tooltip("InventoryData.ItemStacks から参照する一意の ID。0 は未設定です。")]
        [Description("所持数を保存する ItemStackData と対応する一意の ID。")]
        private int itemId;

        [SerializeField, Tooltip("Items ダイアログに表示する名前です。")]
        [Description("UI表示用のアイテム名。")]
        private string displayName = "";

        [SerializeField, TextArea(2, 4), Tooltip("Items ダイアログで選択時に表示する説明です。")]
        [Description("UI表示用の説明文。")]
        private string description = "";

        [SerializeField, Tooltip("Items ダイアログの一覧に表示するアイコンです。")]
        [Description("一覧用のアイコン。")]
        private Sprite icon;

        [SerializeField, Tooltip("Items ダイアログの表示対象にするかどうかです。")]
        [Description("Items ダイアログに表示する重要所持品かどうか。")]
        private bool isImportantItem = true;

        [SerializeField, Tooltip("所持数が0でも Items ダイアログに常に表示するかどうかです。Gold や Ability Point に使います。")]
        [Description("所持数0でも Items ダイアログに表示するかどうか。")]
        private bool isAlwaysVisibleInInventory;

        [SerializeField, Tooltip("Items ダイアログで所持数を表示するかどうかです。数量を持たない重要品は false にします。")]
        [Description("Items ダイアログで所持数を表示するかどうか。")]
        private bool isQuantityVisibleInInventory = true;

        [SerializeField, Tooltip("Mission 初回クリア報酬で、クリア前は内容を伏せて表示するかどうかです。")]
        private bool isHiddenUntilMissionClear;

        [SerializeField, Tooltip("Items ダイアログ内の並び順です。小さい値ほど先に表示します。")]
        [Description("Items ダイアログ内の表示順。")]
        private int sortOrder;

        public int ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public bool IsImportantItem => isImportantItem;
        public bool IsAlwaysVisibleInInventory => isAlwaysVisibleInInventory;
        public bool IsQuantityVisibleInInventory => isQuantityVisibleInInventory;
        public bool IsHiddenUntilMissionClear => isHiddenUntilMissionClear;
        public int SortOrder => sortOrder;
    }
}
