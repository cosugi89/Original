using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// 1ステージぶんの転送用データ。ロジック層が参照するステージ情報。
    /// マスターデータ（StageMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class StageData
    {
        [Description("ステージID")]
        public int StageId { get; init; }

        [Description("ステージ名")]
        public string StageName { get; init; } = "";

        [Description("ステージの説明")]
        public string Description { get; init; } = "";

        [Description("背景画像")]
        public Sprite BackgroundImage { get; init; }

        [Description("プレビュー画像")]
        public Sprite PreviewImage { get; init; }

        [Description("このステージで戦う敵")]
        public StageBattleEnemyData Enemy { get; init; } = new();
    }
}
