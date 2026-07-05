using Assets.Scripts.Data.MasterData;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// ダメージ式の係数・加算値・参照キー群を表す。
    /// </summary>
    public sealed class BattleDamageFormula
    {
        public BattleSkillParameterType ParameterType { get; init; } = BattleSkillParameterType.Attack;

        public BattleSkillValueType ValueType { get; init; } = BattleSkillValueType.Multiplier;

        public float Value { get; init; } = 1f;

        public int FlatBonus { get; init; } = 0;

        public float FinalMultiplier { get; init; } = 1f;

        public string SourceScalarKey { get; init; } = string.Empty;

        public float SourceScalarFallback { get; init; } = 1f;

        public int MinimumDamage { get; init; } = 0;
    }
}

