using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// 戦闘参加者ごとの stat block を入力ソースから組み立てる。
    /// </summary>
    public sealed class BattleCombatantStatsFactory
    {
        public BattlePlayerStats CreatePlayerStats(BattleStatBlockData statBlock, int maxHp)
        {
            var builder = CreateBuilder(
                statBlock,
                maxHp,
                new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    [BattleDamageValueKeys.NormalAttackMultiplier] = 1f,
                    [BattleDamageValueKeys.DoubleAttackFollowUpMultiplier] = 1.25f,
                    [BattleDamageValueKeys.JumpAttackMultiplier] = 1.5f,
                    [BattleDamageValueKeys.FinalDamageMultiplier] = 1f,
                });

            return builder.Build();
        }

        public BattleCombatantStats CreateCombatantStats(
            BattleStatBlockData statBlock,
            int maxHp = 0,
            IReadOnlyDictionary<string, float> namedDefaults = null)
        {
            var builder = CreateBuilder(
                statBlock,
                maxHp,
                namedDefaults ?? new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    [BattleDamageValueKeys.FinalDamageMultiplier] = 1f,
                });

            return builder.BuildCombatantStats();
        }

        public BattlePlayerStats ApplyModifiers(
            BattlePlayerStats baseStats,
            IReadOnlyList<BattleStatModifierData> modifiers)
        {
            var builder = CreateBuilderFromStats(
                baseStats ?? new BattlePlayerStats(baseAttackPower: 1));
            ApplyModifiers(builder, modifiers);
            return builder.Build();
        }

        public BattleCombatantStats ApplyModifiers(
            BattleCombatantStats baseStats,
            IReadOnlyList<BattleStatModifierData> modifiers)
        {
            var builder = CreateBuilderFromStats(baseStats ?? new BattleCombatantStats());
            ApplyModifiers(builder, modifiers);
            return builder.BuildCombatantStats();
        }

        private static BattlePlayerStatsBuilder CreateBuilder(
            BattleStatBlockData statBlock,
            int maxHp,
            IReadOnlyDictionary<string, float> namedDefaults)
        {
            var resolvedStatBlock = statBlock ?? new BattleStatBlockData();
            var builder = new BattlePlayerStatsBuilder()
                .SetParameter(BattleSkillParameterType.Attack, Math.Max(0, resolvedStatBlock.Attack))
                .SetParameter(BattleSkillParameterType.Hp, Math.Max(0, maxHp))
                .SetParameter(BattleSkillParameterType.Defense, Math.Max(0, resolvedStatBlock.Defense))
                .SetParameter(BattleSkillParameterType.Guard, Math.Max(0, resolvedStatBlock.Guard));

            ApplyNamedDefaults(builder, namedDefaults);
            ApplyModifiers(builder, resolvedStatBlock.Modifiers);
            return builder;
        }

        private static BattlePlayerStatsBuilder CreateBuilderFromStats(BattleCombatantStats stats)
        {
            var builder = new BattlePlayerStatsBuilder();
            if (stats == null)
            {
                return builder;
            }

            foreach (var pair in stats.ParameterValues)
            {
                builder.SetParameter(pair.Key, pair.Value);
            }

            foreach (var pair in stats.NamedValues)
            {
                builder.SetNamedValue(pair.Key, pair.Value);
            }

            return builder;
        }

        private static void ApplyNamedDefaults(
            BattlePlayerStatsBuilder builder,
            IReadOnlyDictionary<string, float> namedDefaults)
        {
            if (builder == null || namedDefaults == null)
            {
                return;
            }

            foreach (var pair in namedDefaults)
            {
                builder.SetNamedValue(pair.Key, pair.Value);
            }
        }

        private static void ApplyModifiers(
            BattlePlayerStatsBuilder builder,
            IReadOnlyList<BattleStatModifierData> modifiers)
        {
            if (builder == null || modifiers == null)
            {
                return;
            }

            for (var i = 0; i < modifiers.Count; i++)
            {
                builder.ApplyModifier(modifiers[i]);
            }
        }
    }
}

