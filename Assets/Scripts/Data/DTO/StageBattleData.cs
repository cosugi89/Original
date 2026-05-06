using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// ステージに紐づくバトル固有データ。
    /// BattleScene がターン盤面や敵情報を引く本番用 DTO。
    /// </summary>
    public class StageBattleData
    {
        public StageBattleBoardData Board { get; init; } = new();

        public StageBattleEnemyData Enemy { get; init; } = new();

        public IReadOnlyList<StageTurnData> TurnDefinitions { get; init; } = Array.Empty<StageTurnData>();
    }

    /// <summary>
    /// ステージ全体で共有する盤面サイズと開始位置。
    /// </summary>
    public class StageBattleBoardData
    {
        public int Width { get; init; } = 5;

        public int Height { get; init; } = 6;

        public StageGridPositionData StartPosition { get; init; } = new() { X = 2, Y = 5 };
    }

    /// <summary>
    /// ステージ戦闘で対面する敵の基本情報。
    /// </summary>
    public class StageBattleEnemyData
    {
        public string Name { get; init; } = "";

        public int MaxHp { get; init; } = 1;
    }

    /// <summary>
    /// 1 ターンぶんの盤面、敵行動、演出用文言。
    /// BattleDemoTurnScript を本番データへ置き換える受け皿。
    /// </summary>
    public class StageTurnData
    {
        public string DebugLabel { get; init; } = "";

        public BattleEnemyActionType EnemyAction { get; init; } = BattleEnemyActionType.NormalAttack;

        public string BoardSummary { get; init; } = "";

        public string ConfirmText { get; init; } = "";

        public string Notes { get; init; } = "";

        public int EnemyActionDamage { get; init; }

        public IReadOnlyList<StageTurnHazardGroupData> HazardGroups { get; init; } = Array.Empty<StageTurnHazardGroupData>();

        public IReadOnlyList<StageTurnCellData> CellPlacements { get; init; } = Array.Empty<StageTurnCellData>();
    }

    /// <summary>
    /// 盤面上の危険グループ定義。
    /// 複数マスの危険が同一攻撃に属する場合は同じ GroupId を参照する。
    /// </summary>
    public class StageTurnHazardGroupData
    {
        public int GroupId { get; init; }

        public int Damage { get; init; }
    }

    /// <summary>
    /// 1 ターン盤面に置くセル 1 個ぶんの定義。
    /// </summary>
    public class StageTurnCellData
    {
        public StageGridPositionData Position { get; init; } = new();

        public BattleNodeType NodeType { get; init; } = BattleNodeType.Empty;

        public int HazardGroupId { get; init; } = -1;
    }

    /// <summary>
    /// StageData 系 DTO で共通利用する整数座標。
    /// </summary>
    public class StageGridPositionData
    {
        public int X { get; init; }

        public int Y { get; init; }
    }
}
