using System;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// ダメージ計算リクエストを受けて解決結果を生成する。
    /// 旧属性相性は採用しないため、ここでは属性補正を適用しない。
    /// </summary>
    public static class BattleDamagePipeline
    {
        public static BattleDamageResolution Resolve(BattleDamagePipelineRequest request)
        {
            if (request == null)
            {
                return BattleDamageResolution.Zero;
            }

            var baseDamage = Math.Max(0, request.BaseDamageOverride ?? ResolveBaseDamage(request));
            if (baseDamage <= 0)
            {
                return new BattleDamageResolution
                {
                    BaseDamage = 0,
                    Damage = 0,
                    AppliedPercent = 0,
                    Effectiveness = BattleAttributeEffectiveness.None,
                    AttributeSummary = string.Empty,
                };
            }

            return new BattleDamageResolution
            {
                BaseDamage = baseDamage,
                Damage = baseDamage,
                AppliedPercent = 100,
                Effectiveness = BattleAttributeEffectiveness.None,
                AttributeSummary = string.Empty,
            };
        }

        private static int ResolveBaseDamage(BattleDamagePipelineRequest request)
        {
            return request.DamageKind switch
            {
                BattleDamageKind.NormalAttack => BattleDamageCalculator.ResolveNormalAttackDamage(request.Context),
                BattleDamageKind.DoubleAttackFollowUp => BattleDamageCalculator.ResolveDoubleAttackFollowUpDamage(request.Context),
                BattleDamageKind.JumpAttack => BattleDamageCalculator.ResolveJumpAttackDamage(request.Context),
                BattleDamageKind.Skill => BattleDamageCalculator.ResolveSkillDamage(
                    request.Context,
                    request.Skill,
                    request.SkillEffect),
                BattleDamageKind.EnemyAction => BattleDamageCalculator.ResolveEnemyActionDamage(
                    request.Context,
                    hazardBoosted: request.HazardBoosted),
                _ => 0,
            };
        }
    }
}

