namespace Assets.Scripts.Features.Battle.Core
{
    /// <summary>
    /// 敵ターンの行動カテゴリ。
    /// デモ専用 enum から切り離し、盤面生成や敵 AI の共通キーとして使う。
    /// </summary>
    public enum BattleEnemyActionType
    {
        NormalAttack,
        Skill,
        Dance,
    }
}
