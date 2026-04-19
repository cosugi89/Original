namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 のデモで扱う敵のターン行動種別。
    /// </summary>
    public enum BattleDemoEnemyActionType
    {
        /// <summary>通常攻撃。通常危険を配置する想定。</summary>
        NormalAttack,
        /// <summary>スキル攻撃。スキル危険を配置する想定。</summary>
        Skill,
        /// <summary>Dance。次ターンの危険強化を予約する。</summary>
        Dance,
    }
}
