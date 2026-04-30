using UnityEngine;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// バトルステージ1件の転送用データ。ロジック層が参照するステージ選択情報。
    /// マスターデータ（BattleStageMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class BattleStageData
    {
        public int StageId { get; init; }
        public string DisplayName { get; init; } = "";
        public string DescriptionText { get; init; } = "";
        public int SortOrder { get; init; }
        public bool IsInitiallyUnlocked { get; init; }
        public Sprite PreviewImage { get; init; }
    }
}
