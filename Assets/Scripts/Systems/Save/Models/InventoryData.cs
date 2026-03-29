using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public class InventoryData
    {
        [Description("所持している装備一覧。装備マスタのIDで参照する。")]
        public List<InventoryEntryData> Equipments { get; set; } = new();
    }

    public class InventoryEntryData
    {
        [Description("所持している装備のID。装備マスタと対応する。")]
        public string EquipmentId { get; set; } = "";

        [Description("所持数。装備品を一度入手で永続解放にするなら1固定でもよい。")]
        public int Quantity { get; set; } = 1;

        [Description("その装備を使用可能として解放済みかどうか。Quantityだけで表現するなら削除してよい。")]
        public bool IsUnlocked { get; set; } = true;

        [Description("その装備を最初に入手したUTC時刻。不要なら削除してよい。")]
        public string ObtainedAtUtc { get; set; } = "";

        [Description("最後に装備したUTC時刻。装備履歴やおすすめ表示に使える。")]
        public string LastEquippedAtUtc { get; set; } = "";
    }
}
