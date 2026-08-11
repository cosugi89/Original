using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// ダメージ計算後の値を保持する。
    /// AttributeSummary は旧属性表示との互換受け口で、新仕様では空文字のまま扱う。
    /// </summary>
    public sealed class BattleDamageResolution
    {
        public static BattleDamageResolution Zero { get; } = new();

        public int BaseDamage { get; init; }

        public int Damage { get; init; }

        public int AppliedPercent { get; init; }

        public BattleAttributeEffectiveness Effectiveness { get; init; } = BattleAttributeEffectiveness.None;

        public string AttributeSummary { get; init; } = string.Empty;

        public string LogSuffix =>
            string.IsNullOrWhiteSpace(AttributeSummary)
                ? string.Empty
                : $" ({AttributeSummary} {AppliedPercent}%)";
    }
}

