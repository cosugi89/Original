namespace Assets.Scripts.Features.Battle.Core
{
    /// <summary>
    /// パス入力中に発生しうる不正理由。
    /// </summary>
    public enum BattlePathValidationError
    {
        None,
        OutOfBounds,
        MissingCell,
        InvalidStart,
        NotAdjacent,
        Revisit,
        CrossedSegment,
        ExtendedAfterGoal,
        GoalNotReached,
    }
}
