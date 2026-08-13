using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// ダメージ式と補正係数を適用して最終ダメージを算出する。
    /// </summary>
    public static class BattleDamageCalculator
    {
        public static int ResolveDamage(BattleDamageCalculationInput input, BattleDamageFormula formula)
        {
            if (formula == null)
            {
                return 0;
            }

            var baseValue = input?.ResolveParameterValue(formula.ParameterType) ?? 0;
            var coreDamage = formula.ValueType switch
            {
                BattleSkillValueType.Multiplier => baseValue * formula.Value,
                BattleSkillValueType.Percentage => baseValue * formula.Value,
                BattleSkillValueType.FixedValue => formula.Value,
                BattleSkillValueType.None => 0f,
                _ => 0f,
            };

            var sourceScalar = input?.ResolveNamedValue(
                formula.SourceScalarKey,
                formula.SourceScalarFallback) ?? formula.SourceScalarFallback;
            var finalMultiplier = formula.FinalMultiplier;
            var resolvedDamage = (coreDamage * sourceScalar * finalMultiplier) + formula.FlatBonus;
            return Math.Max(formula.MinimumDamage, Math.Max(0, RoundToInt(resolvedDamage)));
        }

        public static int ResolveNormalAttackDamage(BattleTurnContext context)
        {
            var input = new BattleDamageCalculationInput(context);
            return ResolveDamage(
                input,
                new BattleDamageFormula
                {
                    ParameterType = BattleSkillParameterType.Attack,
                    ValueType = BattleSkillValueType.Multiplier,
                    Value = 1f,
                    SourceScalarKey = BattleDamageValueKeys.NormalAttackMultiplier,
                    SourceScalarFallback = 1f,
                    FinalMultiplier = ResolveGlobalDamageMultiplier(input),
                    MinimumDamage = 1,
                });
        }

        public static int ResolveDoubleAttackFollowUpDamage(BattleTurnContext context)
        {
            var input = new BattleDamageCalculationInput(context);
            return ResolveDamage(
                input,
                new BattleDamageFormula
                {
                    ParameterType = BattleSkillParameterType.Attack,
                    ValueType = BattleSkillValueType.Multiplier,
                    Value = 1f,
                    SourceScalarKey = BattleDamageValueKeys.DoubleAttackFollowUpMultiplier,
                    SourceScalarFallback = 1.25f,
                    FinalMultiplier = ResolveGlobalDamageMultiplier(input),
                    MinimumDamage = 1,
                });
        }

        public static int ResolveJumpAttackDamage(BattleTurnContext context)
        {
            var input = new BattleDamageCalculationInput(context);
            return ResolveDamage(
                input,
                new BattleDamageFormula
                {
                    ParameterType = BattleSkillParameterType.Attack,
                    ValueType = BattleSkillValueType.Multiplier,
                    Value = 1f,
                    SourceScalarKey = BattleDamageValueKeys.JumpAttackMultiplier,
                    SourceScalarFallback = 1.5f,
                    FinalMultiplier = ResolveGlobalDamageMultiplier(input),
                    MinimumDamage = 1,
                });
        }

        public static int ResolveSkillDamage(BattleTurnContext context, SkillSlotRuntime selectedSkill)
        {
            return ResolveSkillDamage(context, selectedSkill, ResolvePrimaryDamageEffect(selectedSkill));
        }

        public static int ResolveSkillDamage(
            BattleTurnContext context,
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect)
        {
            var input = new BattleDamageCalculationInput(context, selectedSkill);
            if (selectedSkill == null || effect == null || effect.EffectType != BattleSkillOperationType.Damage)
            {
                return 0;
            }

            if (effect.DamageFormula != null && effect.DamageFormula.IsEnabled)
            {
                return ResolveDamage(
                    input,
                    BuildFormula(
                        effect.DamageFormula,
                        inheritedFinalMultiplier: ResolveGlobalDamageMultiplier(input)),
                    minimumDamageFallback: 0);
            }

            return ResolveDamage(
                input,
                new BattleDamageFormula
                {
                    ParameterType = effect.ParameterType == BattleSkillParameterType.None
                        ? BattleSkillParameterType.Attack
                        : effect.ParameterType,
                    ValueType = effect.ValueType,
                    Value = effect.Value,
                    FinalMultiplier = ResolveGlobalDamageMultiplier(input),
                    MinimumDamage = 0,
                });
        }

        public static int ResolveEnemyActionDamage(BattleTurnContext context, bool hazardBoosted = false)
        {
            if (context == null)
            {
                return 0;
            }

            if (context.EnemyActionDamageFormula != null && context.EnemyActionDamageFormula.IsEnabled)
            {
                var customInput = new BattleDamageCalculationInput(
                    context,
                    sourceStats: context.EnemyStats,
                    targetStats: context.PlayerStats,
                    sourceCurrentHp: context.CurrentEnemyHp,
                    targetCurrentHp: context.CurrentPlayerHp);

                return ResolveDamage(
                    customInput,
                    BuildFormula(
                        context.EnemyActionDamageFormula,
                        inheritedFinalMultiplier: ResolveGlobalDamageMultiplier(customInput) * (hazardBoosted ? 1.5f : 1f),
                        defaultSourceScalarKey: BattleDamageValueKeys.EnemyActionMultiplier,
                        defaultSourceScalarFallback: Math.Max(0f, context.EnemyActionMultiplier)),
                    minimumDamageFallback: 0);
            }

            var input = BuildEnemyActionInput(context);
            return ResolveDamage(
                input,
                new BattleDamageFormula
                {
                    ParameterType = BattleSkillParameterType.Attack,
                    ValueType = BattleSkillValueType.Multiplier,
                    Value = 1f,
                    SourceScalarKey = BattleDamageValueKeys.EnemyActionMultiplier,
                    SourceScalarFallback = 1f,
                    FinalMultiplier = ResolveGlobalDamageMultiplier(input) * (hazardBoosted ? 1.5f : 1f),
                    MinimumDamage = 0,
                });
        }

        private static BattleSkillEffectEntryData ResolvePrimaryDamageEffect(SkillSlotRuntime selectedSkill)
        {
            var effects = selectedSkill?.Effects;
            if (effects == null || effects.Count == 0)
            {
                return null;
            }

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.EffectType == BattleSkillOperationType.Damage)
                {
                    return effect;
                }
            }

            return null;
        }

        private static float ResolveGlobalDamageMultiplier(BattleDamageCalculationInput input)
        {
            return input?.SourceStats?.GetNamedValue(BattleDamageValueKeys.FinalDamageMultiplier, 1f) ?? 1f;
        }

        private static BattleDamageCalculationInput BuildEnemyActionInput(BattleTurnContext context)
        {
            return new BattleDamageCalculationInput(
                context,
                sourceStats: context?.EnemyStats,
                targetStats: context?.PlayerStats,
                sourceCurrentHp: context?.CurrentEnemyHp,
                targetCurrentHp: context?.CurrentPlayerHp,
                namedValueOverrides: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    [BattleDamageValueKeys.EnemyActionMultiplier] = Math.Max(0f, context?.EnemyActionMultiplier ?? 0f),
                });
        }

        private static int ResolveDamage(
            BattleDamageCalculationInput input,
            BattleDamageFormula formula,
            int minimumDamageFallback)
        {
            if (formula == null)
            {
                return minimumDamageFallback;
            }

            var normalizedFormula = new BattleDamageFormula
            {
                ParameterType = formula.ParameterType,
                ValueType = formula.ValueType,
                Value = formula.Value,
                FlatBonus = formula.FlatBonus,
                FinalMultiplier = formula.FinalMultiplier,
                SourceScalarKey = formula.SourceScalarKey,
                SourceScalarFallback = formula.SourceScalarFallback,
                MinimumDamage = Math.Max(minimumDamageFallback, formula.MinimumDamage),
            };

            return ResolveDamage(input, normalizedFormula);
        }

        private static BattleDamageFormula BuildFormula(
            BattleDamageFormulaData formulaData,
            float inheritedFinalMultiplier,
            string defaultSourceScalarKey = "",
            float defaultSourceScalarFallback = 1f)
        {
            if (formulaData == null)
            {
                return null;
            }

            var sourceScalarKey = string.IsNullOrWhiteSpace(formulaData.SourceScalarKey)
                ? defaultSourceScalarKey ?? string.Empty
                : formulaData.SourceScalarKey;
            var sourceScalarFallback = string.IsNullOrWhiteSpace(formulaData.SourceScalarKey)
                ? defaultSourceScalarFallback
                : formulaData.SourceScalarFallback;

            return new BattleDamageFormula
            {
                ParameterType = formulaData.ParameterType == BattleSkillParameterType.None
                    ? BattleSkillParameterType.Attack
                    : formulaData.ParameterType,
                ValueType = formulaData.ValueType,
                Value = formulaData.Value,
                FlatBonus = Math.Max(0, formulaData.FlatBonus),
                FinalMultiplier = Math.Max(0f, formulaData.FinalMultiplier) * Math.Max(0f, inheritedFinalMultiplier),
                SourceScalarKey = sourceScalarKey,
                SourceScalarFallback = sourceScalarFallback,
                MinimumDamage = Math.Max(0, formulaData.MinimumDamage),
            };
        }

        private static int RoundToInt(float value)
        {
            return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}

