using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// 1 つの盤面パターンが持つ盤面サイズと開始位置。
    /// 将来、敵行動に応じてサイズや開始位置が変わる余地を残す。
    /// </summary>
    public class StageBattleBoardData
    {
        public int Width { get; init; } = 5;

        public int Height { get; init; } = 6;

        public StageGridPositionData StartPosition { get; init; } = new() { X = 2, Y = 5 };
    }

    /// <summary>
    /// ステージ戦闘で対面する敵の基本情報。
    /// Appearance は現在の敵装備・見た目定義として扱い、
    /// Patterns はこの敵が使用可能な固定盤面候補を持つ。
    /// </summary>
    public class StageBattleEnemyData
    {
        public string Name { get; init; } = "";

        public int MaxHp { get; init; } = 1;

        public int Damage { get; init; } = 80;

        public AttributeData AttackAttribute { get; init; }

        public IReadOnlyList<EquipmentAttributeModifierData> DefenseAttributeModifiers { get; init; } =
            Array.Empty<EquipmentAttributeModifierData>();

        public AppearanceData Appearance { get; init; } = new();

        public IReadOnlyList<StageBattlePatternData> Patterns { get; init; } = Array.Empty<StageBattlePatternData>();
    }

    /// <summary>
    /// 敵行動にひもづく固定盤面パターン 1 件ぶん。
    /// 敵行動ごとに複数用意し、実行時はこの候補群からランダムに選ぶ。
    /// </summary>
    public class StageBattlePatternData
    {
        public StageBattleBoardData Board { get; init; } = new();

        public string DebugLabel { get; init; } = "";

        public BattleEnemyActionType EnemyAction { get; init; } = BattleEnemyActionType.NormalAttack;

        public string Description { get; init; } = "";

        public string ConfirmText { get; init; } = "";

        public float EnemyActionDamageMultiplier { get; init; } = 1f;

        public IReadOnlyList<StageTurnCellData> CellPlacements { get; init; } = Array.Empty<StageTurnCellData>();
    }

    /// <summary>
    /// 1 ターン盤面に置くセル 1 個ぶんの定義。
    /// </summary>
    public class StageTurnCellData
    {
        public StageGridPositionData Position { get; init; } = new();

        public BattleNodeType NodeType { get; init; } = BattleNodeType.Empty;
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
