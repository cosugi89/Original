using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public class BattleProgressData
    {
        [Description("ステージごとの進捗一覧。")]
        public List<StageProgressData> Stages { get; set; } = new();

        [Description("最後に選択していたステージID。ステージ選択画面の初期選択に使える。")]
        public string LastSelectedStageId { get; set; } = "";
    }

    public class StageProgressData
    {
        [Description("進捗対象のステージID。ステージマスタと対応する。")]
        public string StageId { get; set; } = "";

        [Description("そのステージが挑戦可能な状態かどうか。")]
        public bool IsUnlocked { get; set; } = false;

        [Description("そのステージを一度でもクリアしたかどうか。")]
        public bool IsCleared { get; set; } = false;

        [Description("そのステージをクリアした回数。")]
        public int ClearCount { get; set; } = 0;

        [Description("そのステージの自己ベストスコア。スコア制が無ければ削除してよい。")]
        public int BestScore { get; set; } = 0;

        [Description("そのステージの最高評価。S/A/Bのようなランク制を使うなら利用する。")]
        public string BestRank { get; set; } = "";

        [Description("そのステージの最短クリア時間秒。タイムアタック制が無ければ削除してよい。")]
        public float BestClearTimeSeconds { get; set; } = 0f;

        [Description("最後にクリアしたUTC時刻。履歴が不要なら削除してよい。")]
        public string LastClearedAtUtc { get; set; } = "";
    }
}
