using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    /// <summary>
    /// ゲーム内で主軸として扱うユーザーデータ。
    /// </summary>
    public class UserData
    {
        [Description("セーブファイル自体のメタ情報。")]
        public UserMetaData Meta { get; set; } = new();

        [Description("プレイヤー固有の状態。プロフィール、通貨、成長、見た目、戦闘設定をまとめる。")]
        public UserProfileData Profile { get; set; } = new UserProfileDemoData();

        [Description("プレイヤーの所持装備一覧。装備可能判定や一覧表示の基準になる。")]
        public InventoryData Inventory { get; set; } = new();

        [Description("バトルステージの解放・クリア状況。")]
        public BattleProgressData BattleProgress { get; set; } = new();
    }

    public class UserMetaData
    {
        [Description("セーブデータのスキーマバージョン。将来の移行処理の分岐に使う。")]
        public int SaveVersion { get; set; } = 1;

        [Description("このセーブデータを最初に作成したUTC時刻。未使用なら空文字でよい。")]
        public string CreatedAtUtc { get; set; } = "";

        [Description("このセーブデータを最後に更新したUTC時刻。保存のたびに更新する。")]
        public string UpdatedAtUtc { get; set; } = "";
    }
}
