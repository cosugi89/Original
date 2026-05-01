using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 盤面 1 マスぶんの状態。
    /// </summary>
    public class BattleCellState
    {
        public BattleGridPosition Position { get; set; }

        public BattleNodeType NodeType { get; set; }

        public int HazardGroupId { get; set; } = -1;

        public bool IsGoal => NodeType == BattleNodeType.Goal;

        public bool IsHazard =>
            NodeType == BattleNodeType.HazardNormal ||
            NodeType == BattleNodeType.HazardSkill;
    }
}
