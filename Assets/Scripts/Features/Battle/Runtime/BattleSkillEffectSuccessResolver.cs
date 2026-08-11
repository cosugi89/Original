using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Runtime
{
    public delegate float BattleSkillEffectSuccessRateModifier(
        BattleSkillEffectSuccessContext context,
        float currentRate);

    /// <summary>
    /// スキル効果 1 件の成功率補正で参照する情報をまとめる。
    /// </summary>
    public sealed class BattleSkillEffectSuccessContext
    {
        public BattleTurnContext TurnContext { get; init; }

        public SkillSlotRuntime Skill { get; init; }

        public BattleSkillEffectEntryData Effect { get; init; }

        public int EffectIndex { get; init; }
    }

    /// <summary>
    /// スキル効果 1 件の成否判定結果。
    /// </summary>
    public sealed class BattleSkillEffectSuccessResult
    {
        public BattleSkillEffectSuccessResult(
            bool succeeded,
            float successRate,
            float roll)
        {
            Succeeded = succeeded;
            SuccessRate = Mathf.Clamp01(successRate);
            Roll = Mathf.Clamp01(roll);
        }

        public bool Succeeded { get; }

        public float SuccessRate { get; }

        public float Roll { get; }
    }

    /// <summary>
    /// SuccessRate に補正を適用し、preview と実行時の成否判定を切り替える。
    /// </summary>
    public sealed class BattleSkillEffectSuccessResolver
    {
        private static readonly IReadOnlyList<BattleSkillEffectSuccessRateModifier> EmptyModifiers =
            Array.Empty<BattleSkillEffectSuccessRateModifier>();

        private readonly Func<float> _rollProvider;
        private readonly IReadOnlyList<BattleSkillEffectSuccessRateModifier> _rateModifiers;
        private readonly bool _assumeSuccess;

        private BattleSkillEffectSuccessResolver(
            Func<float> rollProvider,
            IReadOnlyList<BattleSkillEffectSuccessRateModifier> rateModifiers,
            bool assumeSuccess)
        {
            _rollProvider = rollProvider;
            _rateModifiers = rateModifiers ?? EmptyModifiers;
            _assumeSuccess = assumeSuccess;
        }

        public static BattleSkillEffectSuccessResolver CreatePreview(
            IEnumerable<BattleSkillEffectSuccessRateModifier> rateModifiers = null)
        {
            return new BattleSkillEffectSuccessResolver(
                null,
                BuildModifierList(rateModifiers),
                true);
        }

        public static BattleSkillEffectSuccessResolver CreateRuntime(
            Func<float> rollProvider = null,
            IEnumerable<BattleSkillEffectSuccessRateModifier> rateModifiers = null)
        {
            return new BattleSkillEffectSuccessResolver(
                rollProvider ?? (() => UnityEngine.Random.value),
                BuildModifierList(rateModifiers),
                assumeSuccess: false);
        }

        public BattleSkillEffectSuccessResult Resolve(BattleSkillEffectSuccessContext context)
        {
            var successRate = Mathf.Clamp01(context?.Effect?.SuccessRate ?? 1f);
            for (var i = 0; i < _rateModifiers.Count; i++)
            {
                var modifier = _rateModifiers[i];
                if (modifier == null)
                {
                    continue;
                }

                successRate = Mathf.Clamp01(modifier(context, successRate));
            }

            if (_assumeSuccess)
            {
                return new BattleSkillEffectSuccessResult(
                    successRate > 0f,
                    successRate,
                    0f);
            }

            if (successRate >= 1f)
            {
                return new BattleSkillEffectSuccessResult(true, successRate, 0f);
            }

            if (successRate <= 0f)
            {
                return new BattleSkillEffectSuccessResult(false, successRate, 1f);
            }

            var roll = Mathf.Clamp01(_rollProvider?.Invoke() ?? UnityEngine.Random.value);
            return new BattleSkillEffectSuccessResult(
                roll < successRate,
                successRate,
                roll);
        }

        private static IReadOnlyList<BattleSkillEffectSuccessRateModifier> BuildModifierList(
            IEnumerable<BattleSkillEffectSuccessRateModifier> modifiers)
        {
            if (modifiers == null)
            {
                return EmptyModifiers;
            }

            var result = new List<BattleSkillEffectSuccessRateModifier>();
            foreach (var modifier in modifiers)
            {
                if (modifier != null)
                {
                    result.Add(modifier);
                }
            }

            return result.Count > 0 ? result.ToArray() : EmptyModifiers;
        }
    }
}
