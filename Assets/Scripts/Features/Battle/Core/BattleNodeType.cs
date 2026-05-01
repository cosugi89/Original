namespace Assets.Scripts.Features.Battle.Core
{
    /// <summary>
    /// Path-Activation System v2 本番盤面で扱うノード種別。
    /// 現在のデモ enum から切り離し、将来の実グリッド実装で共通利用する。
    /// </summary>
    public enum BattleNodeType
    {
        Start,
        Empty,
        Jump,
        Roll,
        Dance,
        Attack,
        HazardNormal,
        HazardSkill,
        Goal,
    }
}
