using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// ユーザープロフィールと装備からプレイヤーステータスを組み立てる。
    /// </summary>
    public sealed class BattlePlayerStatsBuilder
    {
        private readonly Dictionary<BattleSkillParameterType, int> _parameterValues = new();
        private readonly Dictionary<string, float> _namedValues = new(StringComparer.OrdinalIgnoreCase);

        public BattlePlayerStatsBuilder SetParameter(BattleSkillParameterType parameterType, int value)
        {
            if (ShouldIgnoreParameter(parameterType))
            {
                return this;
            }

            _parameterValues[parameterType] = Math.Max(0, value);
            return this;
        }

        public BattlePlayerStatsBuilder AddParameter(BattleSkillParameterType parameterType, int delta)
        {
            if (ShouldIgnoreParameter(parameterType))
            {
                return this;
            }

            var current = GetParameter(parameterType, 0);
            _parameterValues[parameterType] = Math.Max(0, current + delta);
            return this;
        }

        public BattlePlayerStatsBuilder MultiplyParameter(BattleSkillParameterType parameterType, float multiplier)
        {
            if (ShouldIgnoreParameter(parameterType))
            {
                return this;
            }

            var current = GetParameter(parameterType, 0);
            _parameterValues[parameterType] = Mathf.Max(0, Mathf.RoundToInt(current * multiplier));
            return this;
        }

        public BattlePlayerStatsBuilder SetNamedValue(string key, float value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return this;
            }

            _namedValues[key] = value;
            return this;
        }

        public BattlePlayerStatsBuilder MultiplyNamedValue(string key, float multiplier, float defaultBaseValue = 1f)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return this;
            }

            var current = GetNamedValue(key, defaultBaseValue);
            _namedValues[key] = current * multiplier;
            return this;
        }

        public BattlePlayerStatsBuilder ApplyModifier(BattleStatModifierData modifier)
        {
            if (modifier == null)
            {
                return this;
            }

            if (modifier.TargetType == BattleStatModifierTargetType.NamedValue)
            {
                return ApplyNamedValueModifier(modifier);
            }

            return ApplyParameterModifier(modifier);
        }

        public int GetParameter(BattleSkillParameterType parameterType, int fallbackValue)
        {
            if (ShouldIgnoreParameter(parameterType))
            {
                return Math.Max(0, fallbackValue);
            }

            return _parameterValues.TryGetValue(parameterType, out var value)
                ? Math.Max(0, value)
                : Math.Max(0, fallbackValue);
        }

        public float GetNamedValue(string key, float fallbackValue)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return fallbackValue;
            }

            return _namedValues.TryGetValue(key, out var value)
                ? value
                : fallbackValue;
        }

        public BattlePlayerStats Build()
        {
            var baseAttackPower = GetParameter(BattleSkillParameterType.Attack, 1);
            return new BattlePlayerStats(
                baseAttackPower,
                normalAttackMultiplier: GetNamedValue(BattleDamageValueKeys.NormalAttackMultiplier, 1f),
                doubleAttackFollowUpMultiplier: GetNamedValue(BattleDamageValueKeys.DoubleAttackFollowUpMultiplier, 1.25f),
                jumpAttackMultiplier: GetNamedValue(BattleDamageValueKeys.JumpAttackMultiplier, 1.5f),
                parameterValues: _parameterValues,
                namedValues: _namedValues);
        }

        public BattleCombatantStats BuildCombatantStats()
        {
            var parameterValues = new Dictionary<BattleSkillParameterType, int>(_parameterValues);
            var namedValues = new Dictionary<string, float>(_namedValues, StringComparer.OrdinalIgnoreCase);

            if (!parameterValues.ContainsKey(BattleSkillParameterType.Attack))
            {
                parameterValues[BattleSkillParameterType.Attack] = 0;
            }

            if (!namedValues.ContainsKey(BattleDamageValueKeys.FinalDamageMultiplier))
            {
                namedValues[BattleDamageValueKeys.FinalDamageMultiplier] = 1f;
            }

            return new BattleCombatantStats(parameterValues, namedValues);
        }

        private BattlePlayerStatsBuilder ApplyNamedValueModifier(BattleStatModifierData modifier)
        {
            var key = modifier.NamedValueKey?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                return this;
            }

            return modifier.OperationType switch
            {
                BattleStatModifierOperationType.Set => SetNamedValue(key, modifier.Value),
                BattleStatModifierOperationType.Multiply => MultiplyNamedValue(key, modifier.Value),
                _ => SetNamedValue(key, GetNamedValue(key, 0f) + modifier.Value),
            };
        }

        private BattlePlayerStatsBuilder ApplyParameterModifier(BattleStatModifierData modifier)
        {
            if (BattleSkillParameterTypeUtility.IsLegacySpeed(modifier.ParameterType))
            {
                return this;
            }

            return modifier.OperationType switch
            {
                BattleStatModifierOperationType.Set => SetParameter(
                    modifier.ParameterType,
                    Mathf.RoundToInt(modifier.Value)),
                BattleStatModifierOperationType.Multiply => MultiplyParameter(
                    modifier.ParameterType,
                    modifier.Value),
                _ => AddParameter(
                    modifier.ParameterType,
                    Mathf.RoundToInt(modifier.Value)),
            };
        }

        private static bool ShouldIgnoreParameter(BattleSkillParameterType parameterType)
        {
            return BattleSkillParameterTypeUtility.IsLegacySpeed(parameterType);
        }
    }
}

