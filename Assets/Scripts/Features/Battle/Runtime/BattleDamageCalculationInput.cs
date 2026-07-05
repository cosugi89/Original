using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 単発ダメージ計算に必要な入出力パラメータをまとめる。
    /// </summary>
    public sealed class BattleDamageCalculationInput
    {
        private readonly Dictionary<BattleSkillParameterType, int> _parameterOverrides;
        private readonly Dictionary<string, float> _namedValueOverrides;

        public BattleDamageCalculationInput(
            BattleTurnContext context,
            SkillSlotRuntime selectedSkill = null,
            BattleCombatantStats sourceStats = null,
            BattleCombatantStats targetStats = null,
            int? sourceCurrentHp = null,
            int? targetCurrentHp = null,
            IReadOnlyDictionary<BattleSkillParameterType, int> parameterOverrides = null,
            IReadOnlyDictionary<string, float> namedValueOverrides = null)
        {
            Context = context;
            SelectedSkill = selectedSkill;
            SourceStats = sourceStats ?? context?.PlayerStats ?? context?.EnemyStats;
            TargetStats = targetStats ?? ResolveDefaultTargetStats(context, SourceStats);
            SourceCurrentHp = Math.Max(0, sourceCurrentHp ?? ResolveDefaultSourceCurrentHp(context, SourceStats));
            TargetCurrentHp = Math.Max(0, targetCurrentHp ?? ResolveDefaultTargetCurrentHp(context, TargetStats));
            _parameterOverrides = parameterOverrides != null
                ? new Dictionary<BattleSkillParameterType, int>(parameterOverrides)
                : new Dictionary<BattleSkillParameterType, int>();
            _namedValueOverrides = namedValueOverrides != null
                ? new Dictionary<string, float>(namedValueOverrides, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        }

        public BattleTurnContext Context { get; }

        public SkillSlotRuntime SelectedSkill { get; }

        public BattleCombatantStats SourceStats { get; }

        public BattleCombatantStats TargetStats { get; }

        public int SourceCurrentHp { get; }

        public int TargetCurrentHp { get; }

        public int ResolveParameterValue(BattleSkillParameterType parameterType)
        {
            if (_parameterOverrides.TryGetValue(parameterType, out var overrideValue))
            {
                return Math.Max(0, overrideValue);
            }

            return parameterType switch
            {
                BattleSkillParameterType.Hp => SourceCurrentHp,
                BattleSkillParameterType.Charge => Math.Max(0, SelectedSkill?.CurrentCharge ?? 0),
                BattleSkillParameterType.None => ResolveSourceParameterValue(BattleSkillParameterType.Attack, fallbackValue: 1),
                _ => ResolveSourceParameterValue(parameterType, fallbackValue: parameterType == BattleSkillParameterType.Attack ? 1 : 0),
            };
        }

        public float ResolveNamedValue(string key, float fallbackValue = 0f)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return fallbackValue;
            }

            if (_namedValueOverrides.TryGetValue(key, out var overrideValue))
            {
                return overrideValue;
            }

            return SourceStats?.GetNamedValue(key, fallbackValue) ?? fallbackValue;
        }

        private int ResolveSourceParameterValue(BattleSkillParameterType parameterType, int fallbackValue)
        {
            return SourceStats?.GetParameterValue(parameterType, fallbackValue) ?? fallbackValue;
        }

        private static BattleCombatantStats ResolveDefaultTargetStats(
            BattleTurnContext context,
            BattleCombatantStats sourceStats)
        {
            if (ReferenceEquals(sourceStats, context?.EnemyStats))
            {
                return context?.PlayerStats;
            }

            return context?.EnemyStats;
        }

        private static int ResolveDefaultSourceCurrentHp(
            BattleTurnContext context,
            BattleCombatantStats sourceStats)
        {
            if (ReferenceEquals(sourceStats, context?.EnemyStats))
            {
                return context?.CurrentEnemyHp ?? 0;
            }

            return context?.CurrentPlayerHp ?? 0;
        }

        private static int ResolveDefaultTargetCurrentHp(
            BattleTurnContext context,
            BattleCombatantStats targetStats)
        {
            if (ReferenceEquals(targetStats, context?.EnemyStats))
            {
                return context?.CurrentEnemyHp ?? 0;
            }

            return context?.CurrentPlayerHp ?? 0;
        }
    }
}

