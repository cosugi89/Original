using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public class UserProfileData
    {
        public const int BattleSkillLoadoutSlotCount = 4;

        [Description("プレイヤーを一意に識別するID。将来の連携や移行時に使う。")]
        public string PlayerId { get; set; } = "";

        [Description("プレイヤー表示名。未使用なら空文字のままでよい。")]
        public string PlayerName { get; set; } = "";

        [Description("最後にプレイしたUTC時刻。継続ボーナスや最終ログイン表示に使える。")]
        public string LastPlayedAtUtc { get; set; } = "";

        [Description("通常通貨の所持数。ショップや報酬で使う想定。")]
        public int Gold { get; set; } = 0;

        [Description("プレミアム通貨の所持数。不要なら削除してよい。")]
        public int Gem { get; set; } = 0;

        [Description("プレイヤーレベル。将来の成長要素を入れるなら使う。")]
        public int Level { get; set; } = 1;

        [Description("現在の経験値。レベル制を採用しないなら削除してよい。")]
        public int Experience { get; set; } = 0;

        [Description("現在のアバター見た目。シーンロード後はこの情報を全体で使い回す。")]
        public AvatarAppearanceData Avatar { get; set; } = new();

        [Description("プレイヤーの最大HP。現在の BattleScene では戦闘開始HPとして使う。")]
        public int MaxHp { get; set; } = 450; // TODO: メソッド化して装備や成長要素を反映させる

        [Description("戦闘で使用するスキルロードアウト。装備候補スキル群から選んだ実戦用4枠。")]
        public BattleSkillLoadoutData SkillLoadout { get; set; } = new();

        [Description("スキルスロット順に保持する SkillId。0 なら未設定スロット。")]
        public List<int> SkillSlotSkillIds { get; set; } = new();
    }

    public class BattleSkillLoadoutData
    {
        [Description("戦闘で使用するスキルIDの並び。0 なら未設定スロット。")]
        public List<int> SkillIds { get; set; } = new();
    }

    /// <summary>
    /// デモプレイ用のプロフィール初期値。
    /// UserProfileData を初期化するときの既定値セットとして使う。
    /// </summary>
    public sealed class UserProfileDemoData : UserProfileData
    {
        public UserProfileDemoData()
        {
            MaxHp = 450;

            SkillSlotSkillIds = new List<int>
            {
                2001,
                2002,
                0,
                0,
            };

            SkillLoadout = new BattleSkillLoadoutData
            {
                SkillIds = new List<int>(SkillSlotSkillIds),
            };
        }
    }
}
