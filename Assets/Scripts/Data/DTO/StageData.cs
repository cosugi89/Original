using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// 1ステージぶんの転送用データ。ロジック層が参照するステージ情報。
    /// マスターデータ（StageMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class StageData
    {
        public int StageId { get; init; }
        public string StageName { get; init; } = "";
        public string Description { get; init; } = "";
        public Sprite BackgroundImage { get; init; }
        public Sprite PreviewImage { get; init; }
        public IReadOnlyList<EnemyData> Enemies { get; init; }
        public StageBattleData Battle { get; init; } = new();

        /// <summary>
        /// ステージ上に登場する敵1体ぶんのデータ。
        /// </summary>
        public class EnemyData
        {
            public string Name { get; init; } = "";
            public int Hp { get; init; }
        }
    }
}
