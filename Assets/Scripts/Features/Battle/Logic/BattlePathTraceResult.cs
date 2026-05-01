using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// 1 回のトレース入力に対する結果。
    /// </summary>
    public class BattlePathTraceResult
    {
        public BattlePathTraceStatus Status { get; set; }

        public BattlePathValidationError ValidationError { get; set; }

        public bool GoalReachedNow { get; set; }

        public bool CanConfirm { get; set; }

        public int PathLength { get; set; }

        public bool IsAccepted =>
            Status == BattlePathTraceStatus.Started ||
            Status == BattlePathTraceStatus.Appended ||
            Status == BattlePathTraceStatus.Backtracked ||
            Status == BattlePathTraceStatus.Confirmed;
    }

    public enum BattlePathTraceStatus
    {
        None,
        Started,
        Appended,
        Backtracked,
        Ignored,
        Invalid,
        ReleasedWithoutGoal,
        Confirmed,
    }
}
