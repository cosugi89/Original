namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 のデモで解決するマスの種別。
    /// 本番盤面のノード種別を段階的に増やす想定で最小構成のみ列挙する。
    /// </summary>
    public enum BattleDemoNodeType
    {
        /// <summary>スタート地点。実際の効果解決は行わない。</summary>
        Start,
        /// <summary>空マス。回避状態などは引き継ぐ。</summary>
        Empty,
        /// <summary>Jump マス。通常危険を回避でき、次の Attack が Jump Attack になる。</summary>
        Jump,
        /// <summary>Roll マス。通常/スキル危険を1回だけ回避する。</summary>
        Roll,
        /// <summary>Dance マス。デモでは装備依存効果のプレースホルダ。</summary>
        Dance,
        /// <summary>Attack マス。通常攻撃または選択中スキルを解決する。</summary>
        Attack,
        /// <summary>通常種別の危険マス。</summary>
        HazardNormal,
        /// <summary>スキル種別の危険マス。</summary>
        HazardSkill,
        /// <summary>ゴール地点。到達でターン確定。</summary>
        Goal,
    }
}
