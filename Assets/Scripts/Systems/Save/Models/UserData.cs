using System.Collections.Generic;
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
        public UserProfileData Profile { get; set; } = new();

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

    public class UserProfileData
    {
        [Description("プレイヤーの識別情報と基本プロフィール。")]
        public UserIdentityData Identity { get; set; } = new();

        [Description("最終プレイ日時などの活動情報。")]
        public UserActivityData Activity { get; set; } = new();

        [Description("通貨などの経済情報。")]
        public UserEconomyData Economy { get; set; } = new();

        [Description("レベルや経験値などの成長情報。")]
        public UserProgressionData Progression { get; set; } = new();

        [Description("現在のアバター見た目。シーンロード後はこの情報を全体で使い回す。")]
        public AvatarAppearanceData Avatar { get; set; } = new();

        [Description("プレイヤーの戦闘向け基本設定。BattleScene で使う基礎パラメータや将来のロードアウト情報を持つ。")]
        public UserBattleProfileData BattleProfile { get; set; } = new();
    }

    public class UserIdentityData
    {
        [Description("プレイヤーを一意に識別するID。将来の連携や移行時に使う。")]
        public string PlayerId { get; set; } = "";

        [Description("プレイヤー表示名。未使用なら空文字のままでよい。")]
        public string PlayerName { get; set; } = "";
    }

    public class UserActivityData
    {
        [Description("最後にプレイしたUTC時刻。継続ボーナスや最終ログイン表示に使える。")]
        public string LastPlayedAtUtc { get; set; } = "";
    }

    public class UserEconomyData
    {
        [Description("通常通貨の所持数。ショップや報酬で使う想定。")]
        public int Gold { get; set; } = 0;

        [Description("プレミアム通貨の所持数。不要なら削除してよい。")]
        public int Gem { get; set; } = 0;
    }

    public class UserProgressionData
    {
        [Description("プレイヤーレベル。将来の成長要素を入れるなら使う。")]
        public int Level { get; set; } = 1;

        [Description("現在の経験値。レベル制を採用しないなら削除してよい。")]
        public int Experience { get; set; } = 0;
    }

    public class UserBattleProfileData
    {
        [Description("プレイヤーの最大HP。現在の BattleScene では戦闘開始HPとして使う。")]
        public int MaxHp { get; set; } = 450;

        [Description("通常 Attack マス解決時のダメージ。")]
        public int NormalAttackDamage { get; set; } = 100;

        [Description("連続 Attack の追撃ダメージ。")]
        public int DoubleAttackFollowUpDamage { get; set; } = 125;

        [Description("Jump 後の Attack に適用するダメージ。")]
        public int JumpAttackDamage { get; set; } = 150;

        [Description("BattleScene で使用するスキルスロット定義。ロード時にランタイムスロットへ変換する。")]
        public List<UserBattleSkillSlotData> SkillSlots { get; set; } = CreateDefaultSkillSlots();

        public static List<UserBattleSkillSlotData> CreateDefaultSkillSlots()
        {
            return new List<UserBattleSkillSlotData>
            {
                new UserBattleSkillSlotData
                {
                    SkillId = 2001,
                    DisplayName = "Wide Blast",
                    Description = "広い範囲に危険を置く純粋攻撃寄り Skill。",
                    IsUnlocked = true,
                    IsConfigured = true,
                    RequiredCharge = 3,
                    StartingCharge = 3,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 140,
                },
                new UserBattleSkillSlotData
                {
                    SkillId = 2002,
                    DisplayName = "Pierce Volley",
                    Description = "単体高火力寄りの Skill。",
                    IsUnlocked = true,
                    IsConfigured = true,
                    RequiredCharge = 5,
                    StartingCharge = 2,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 220,
                },
                new UserBattleSkillSlotData
                {
                    SkillId = 2003,
                    DisplayName = "Locked Slot",
                    Description = "ゲーム進行で解放される想定のロック枠。",
                    IsUnlocked = false,
                    IsConfigured = false,
                    RequiredCharge = 4,
                    StartingCharge = 0,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 0,
                },
                new UserBattleSkillSlotData
                {
                    DisplayName = "Empty Slot",
                    Description = "武器側に Skill が未設定の枠。",
                    IsUnlocked = true,
                    IsConfigured = false,
                    RequiredCharge = 4,
                    StartingCharge = 0,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 0,
                },
            };
        }
    }

    public class UserBattleSkillSlotData
    {
        [Description("参照するスキルマスタID。0 なら旧来の生データ枠として扱う。")]
        public int SkillId { get; set; } = 0;

        [Description("UI 表示用の名称。")]
        public string DisplayName { get; set; } = "Skill";

        [Description("スキルの説明文。")]
        public string Description { get; set; } = string.Empty;

        [Description("プレイヤーがこのスロットを解放済みかどうか。")]
        public bool IsUnlocked { get; set; } = true;

        [Description("武器側に Skill が設定済みかどうか。")]
        public bool IsConfigured { get; set; } = true;

        [Description("READY に必要なチャージ量。")]
        public int RequiredCharge { get; set; } = 3;

        [Description("バトル開始時の初期チャージ量。")]
        public int StartingCharge { get; set; } = 0;

        [Description("ターン開始時に加算されるチャージ量。")]
        public int TurnChargeGain { get; set; } = 1;

        [Description("Attack マス解決ごとに加算されるチャージ量。")]
        public int AttackChargeGain { get; set; } = 1;

        [Description("Skill 発動時に敵へ与えるダメージ。")]
        public int Damage { get; set; } = 0;
    }
}
