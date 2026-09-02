using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public class UserProfileData
    {
        public const int BattleSkillLoadoutSlotCount = 4;
        public const int DeckCount = 3;

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

        [Description("髪・目・ひげ・色など、戦闘デッキと独立したアバター見た目。")]
        public AvatarAppearanceData AvatarAppearance { get; set; } = new();

        [Description("プレイヤーの最大HP。現在の BattleScene では戦闘開始HPとして使う。")]
        public int MaxHp { get; set; } = 450; // TODO: メソッド化して装備や成長要素を反映させる

        [Description("現在適用している戦闘デッキID。1から始まる。")]
        public int CurrentDeckId { get; set; } = 1;

        [Description("戦闘用の装備4枠とSkill4枠を保持する固定デッキ一覧。")]
        public List<DeckData> Decks { get; set; } = new();

    }

    public class DeckData
    {
        [Description("デッキを識別する1始まりのID。")]
        public int DeckId { get; set; } = 1;

        [Description("右手装備ID。未装備は0。")]
        public int RightHandEquipmentId { get; set; }

        [Description("左手装備ID。未装備は0。")]
        public int LeftHandEquipmentId { get; set; }

        [Description("ヘルメット装備ID。未装備は0。")]
        public int HelmetEquipmentId { get; set; }

        [Description("胴装備ID。未装備は0。")]
        public int ChestEquipmentId { get; set; }

        [Description("戦闘で使用するSkill IDの並び。0は未設定スロット。")]
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

            AvatarAppearance = new AvatarAppearanceData();
            Decks = new List<DeckData>
            {
                new()
                {
                    DeckId = 1,
                    SkillIds = new List<int> { 2001, 2002, 0, 0 },
                },
                new() { DeckId = 2 },
                new() { DeckId = 3 },
            };
        }
    }
}
