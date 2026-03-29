using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public class GameSaveData
    {
        [Description("セーブデータのスキーマバージョン。将来の移行処理の分岐に使う。")]
        public int SaveVersion { get; set; } = 1;

        [Description("このセーブデータを最初に作成したUTC時刻。未使用なら空文字でよい。")]
        public string CreatedAtUtc { get; set; } = "";

        [Description("このセーブデータを最後に更新したUTC時刻。保存のたびに更新する。")]
        public string UpdatedAtUtc { get; set; } = "";

        [Description("プレイヤー固有の状態。見た目、プロフィール、通貨などをまとめる。")]
        public PlayerData Player { get; set; } = new();

        [Description("プレイヤーの所持装備一覧。装備可能判定や一覧表示の基準になる。")]
        public InventoryData Inventory { get; set; } = new();

        [Description("バトルステージの解放・クリア状況。")]
        public BattleProgressData BattleProgress { get; set; } = new();
    }

    public class PlayerData
    {
        [Description("プレイヤーを一意に識別するID。将来の連携や移行時に使える。")]
        public string PlayerId { get; set; } = "";

        [Description("プレイヤー表示名。未使用なら空文字のままでよい。")]
        public string PlayerName { get; set; } = "";

        [Description("通常通貨の所持数。ショップや報酬で使う想定。")]
        public int Gold { get; set; } = 0;

        [Description("プレミアム通貨の所持数。不要なら削除してよい。")]
        public int Gem { get; set; } = 0;

        [Description("プレイヤーレベル。将来の成長要素を入れるなら使う。")]
        public int Level { get; set; } = 1;

        [Description("現在の経験値。レベル制を採用しないなら削除してよい。")]
        public int Experience { get; set; } = 0;

        [Description("最後にプレイしたUTC時刻。継続ボーナスや最終ログイン表示に使える。")]
        public string LastPlayedAtUtc { get; set; } = "";

        [Description("現在のアバター見た目。シーンロード後はこの情報を全体で使い回す。")]
        public AvatarAppearanceData AvatarAppearance { get; set; } = new();
    }
}
