using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// ダメージ計算リクエストを受けて前処理から解決結果生成までを連結する。
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

            var attackAttribute = request.AttackAttribute ?? ResolveAttackAttribute(request);
            var defenseModifiers = ResolveDefenseAttributeModifiers(request);
            if (attackAttribute == null || attackAttribute.AttributeId <= 0)
            {
                return new BattleDamageResolution
                {
                    BaseDamage = baseDamage,
                    Damage = baseDamage,
                    AppliedPercent = 100,
                    Effectiveness = BattleAttributeEffectiveness.None,
                    AttributeSummary = string.Empty,
                };
            }

            var appliedPercent = ResolveAttributePercent(attackAttribute.AttributeId, defenseModifiers);
            var effectiveness = ResolveEffectiveness(appliedPercent);
            var resolvedDamage = RoundToInt(baseDamage * (appliedPercent / 100f));
            return new BattleDamageResolution
            {
                BaseDamage = baseDamage,
                Damage = resolvedDamage,
                AppliedPercent = appliedPercent,
                Effectiveness = effectiveness,
                AttributeSummary = BuildAttributeSummary(attackAttribute.DisplayName, effectiveness),
            };
        }

        private static int ResolveBaseDamage(BattleDamagePipelineRequest request)
        {
            return request.DamageKind switch
            {
                BattleDamageKind.NormalAttack => BattleDamageCalculator.ResolveNormalAttackDamage(request.Context),
                BattleDamageKind.DoubleAttackFollowUp => BattleDamageCalculator.ResolveDoubleAttackFollowUpDamage(request.Context),
                BattleDamageKind.JumpAttack => BattleDamageCalculator.ResolveJumpAttackDamage(request.Context),
                BattleDamageKind.Skill => BattleDamageCalculator.ResolveSkillDamage(request.Context, request.Skill),
                BattleDamageKind.EnemyAction => BattleDamageCalculator.ResolveEnemyActionDamage(
                    request.Context,
                    hazardBoosted: request.HazardBoosted),
                _ => 0,
            };
        }

        private static AttributeData ResolveAttackAttribute(BattleDamagePipelineRequest request)
        {
            return request.DamageKind switch
            {
                BattleDamageKind.NormalAttack => request.Context?.PlayerAttackAttribute,
                BattleDamageKind.DoubleAttackFollowUp => request.Context?.PlayerAttackAttribute,
                BattleDamageKind.JumpAttack => request.Context?.PlayerAttackAttribute,
                BattleDamageKind.Skill => request.Skill?.Attribute,
                BattleDamageKind.EnemyAction => request.Context?.EnemyAttackAttribute,
                _ => null,
            };
        }

        private static IReadOnlyList<EquipmentAttributeModifierData> ResolveDefenseAttributeModifiers(
            BattleDamagePipelineRequest request)
        {
            if (request.DefenseAttributeModifiers != null && request.DefenseAttributeModifiers.Count > 0)
            {
                return request.DefenseAttributeModifiers;
            }

            return request.DamageKind switch
            {
                BattleDamageKind.NormalAttack => request.Context?.EnemyDefenseAttributeModifiers,
                BattleDamageKind.DoubleAttackFollowUp => request.Context?.EnemyDefenseAttributeModifiers,
                BattleDamageKind.JumpAttack => request.Context?.EnemyDefenseAttributeModifiers,
                BattleDamageKind.Skill => request.Context?.EnemyDefenseAttributeModifiers,
                BattleDamageKind.EnemyAction => request.Context?.PlayerDefenseAttributeModifiers,
                _ => Array.Empty<EquipmentAttributeModifierData>(),
            };
        }

        private static int ResolveAttributePercent(
            int attributeId,
            IReadOnlyList<EquipmentAttributeModifierData> defenseModifiers)
        {
            if (attributeId <= 0 || defenseModifiers == null || defenseModifiers.Count == 0)
            {
                return 100;
            }

            for (var i = 0; i < defenseModifiers.Count; i++)
            {
                var modifier = defenseModifiers[i];
                if (modifier?.Attribute != null && modifier.Attribute.AttributeId == attributeId)
                {
                    return Math.Max(0, modifier.DamagePercent);
                }
            }

            return 100;
        }

        private static BattleAttributeEffectiveness ResolveEffectiveness(int appliedPercent)
        {
            if (appliedPercent <= 0)
            {
                return BattleAttributeEffectiveness.Immune;
            }

            if (appliedPercent < 100)
            {
                return BattleAttributeEffectiveness.Resist;
            }

            if (appliedPercent > 100)
            {
                return BattleAttributeEffectiveness.Weak;
            }

            return BattleAttributeEffectiveness.Neutral;
        }

        private static string BuildAttributeSummary(string attributeName, BattleAttributeEffectiveness effectiveness)
        {
            if (string.IsNullOrWhiteSpace(attributeName) ||
                effectiveness == BattleAttributeEffectiveness.None ||
                effectiveness == BattleAttributeEffectiveness.Neutral)
            {
                return string.Empty;
            }

            var suffix = effectiveness switch
            {
                BattleAttributeEffectiveness.Weak => "弱点",
                BattleAttributeEffectiveness.Resist => "耐性",
                BattleAttributeEffectiveness.Immune => "無効",
                _ => "等倍",
            };
            return $"{attributeName}/{suffix}";
        }

        private static int RoundToInt(float value)
        {
            return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}

