using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// 確定済みパスを通過順に解決する pure C# ロジック。
    /// 第 1 段階では現行デモ相当の最小ルールを本番名へ移す。
    /// </summary>
    public static class BattleTurnResolver
    {
        private readonly struct AttributeDamageResolution
        {
            public AttributeDamageResolution(
                int damage,
                int appliedPercent,
                BattleAttributeEffectiveness effectiveness,
                string summary)
            {
                Damage = damage;
                AppliedPercent = appliedPercent;
                Effectiveness = effectiveness;
                Summary = summary ?? string.Empty;
            }

            public int Damage { get; }
            public int AppliedPercent { get; }
            public BattleAttributeEffectiveness Effectiveness { get; }
            public string Summary { get; }
        }

        public static BattleTurnResolutionReport Resolve(IReadOnlyList<BattleCellState> path, BattleTurnContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var report = new BattleTurnResolutionReport();
            if (path == null || path.Count == 0)
            {
                return report;
            }

            var currentEnemyHp = Max(1, context.CurrentEnemyHp);
            var currentPlayerHp = Max(1, context.CurrentPlayerHp);
            var normalAttackDamage = Max(1, context.NormalAttackDamage);
            var doubleAttackFollowUpDamage = Max(1, context.DoubleAttackFollowUpDamage);
            var jumpAttackDamage = Max(1, context.JumpAttackDamage);
            var enemyActionDamage = Max(0, context.EnemyActionDamage);
            var selectedSkill = context.SelectedSkill;

            var jumpCanEvade = false;
            var jumpAttackPrimed = false;
            var rollCanEvade = false;
            var resolvedHazardGroups = new HashSet<int>();
            var basicAttackCount = 0;

            for (var i = 0; i < path.Count; i++)
            {
                var cell = path[i];
                var nodeType = cell?.NodeType ?? BattleNodeType.Empty;
                report.ResolvedNodeCount++;

                switch (nodeType)
                {
                    case BattleNodeType.Start:
                    case BattleNodeType.Empty:
                        break;

                    case BattleNodeType.Jump:
                        jumpCanEvade = true;
                        jumpAttackPrimed = true;
                        rollCanEvade = false;
                        report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Jump);
                        report.AddLog("Jump prepared.");
                        break;

                    case BattleNodeType.Roll:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = true;
                        report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Roll);
                        report.AddLog("Roll prepared.");
                        break;

                    case BattleNodeType.Dance:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = false;
                        report.ResolvedDanceCount++;
                        report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Dance);
                        report.AddLog("Dance resolved.");
                        break;

                    case BattleNodeType.Attack:
                    {
                        report.ResolvedAttackCount++;
                        report.EnemyDamageTaken += ResolveAttackDamage(
                            context,
                            selectedSkill,
                            normalAttackDamage,
                            doubleAttackFollowUpDamage,
                            jumpAttackDamage,
                            ref jumpAttackPrimed,
                            ref basicAttackCount,
                            report);

                        jumpCanEvade = false;
                        rollCanEvade = false;

                        if (report.EnemyDamageTaken >= currentEnemyHp)
                        {
                            report.EnemyDefeated = true;
                            report.AddLog("Enemy defeated.");
                            FinalizeOutcomeFlags(report, selectedSkill);
                            return report;
                        }
                        break;
                    }

                    case BattleNodeType.HazardNormal:
                    case BattleNodeType.HazardSkill:
                    {
                        report.ResolvedHazardCount++;
                        var hazardGroupKey = ResolveHazardGroupKey(cell, i);

                        if (resolvedHazardGroups.Contains(hazardGroupKey))
                        {
                            report.AddLog($"Hazard group {hazardGroupKey} already resolved.");
                            break;
                        }

                        if (nodeType == BattleNodeType.HazardNormal && jumpCanEvade)
                        {
                            jumpCanEvade = false;
                            resolvedHazardGroups.Add(hazardGroupKey);
                            report.AddLog($"Jump evaded hazard group {hazardGroupKey}.");
                            break;
                        }

                        if (rollCanEvade)
                        {
                            rollCanEvade = false;
                            jumpAttackPrimed = false;
                            resolvedHazardGroups.Add(hazardGroupKey);
                            report.AddLog($"Roll evaded hazard group {hazardGroupKey}.");
                            break;
                        }

                        resolvedHazardGroups.Add(hazardGroupKey);
                        report.TookHit = true;
                        report.StoppedByHazardHit = true;
                        report.PlayerDamageTaken += ResolveHazardDamage(context, report);
                        report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Stun);
                        report.AddLog($"Hazard group {hazardGroupKey} hit the player.");

                        if (report.PlayerDamageTaken >= currentPlayerHp)
                        {
                            report.PlayerDefeated = true;
                            report.AddLog("Player defeated.");
                        }

                        FinalizeOutcomeFlags(report, selectedSkill);
                        return report;
                    }

                    case BattleNodeType.Goal:
                        report.GoalReached = true;
                        report.PathConfirmed = true;
                        report.AddLog("Goal reached.");
                        ResolvePendingEnemyActionIfNeeded(report, context, enemyActionDamage, currentPlayerHp);
                        FinalizeOutcomeFlags(report, selectedSkill);
                        return report;
                }
            }

            ResolvePendingEnemyActionIfNeeded(report, context, enemyActionDamage, currentPlayerHp);
            FinalizeOutcomeFlags(report, selectedSkill);
            return report;
        }

        public static BattleTurnResolutionReport Resolve(IReadOnlyList<BattleNodeType> path, BattleTurnContext context)
        {
            if (path == null || path.Count == 0)
            {
                return new BattleTurnResolutionReport();
            }

            var cells = new List<BattleCellState>(path.Count);
            for (var i = 0; i < path.Count; i++)
            {
                cells.Add(new BattleCellState
                {
                    NodeType = path[i],
                    HazardGroupId = -1,
                });
            }

            return Resolve(cells, context);
        }

        private static int ResolveAttackDamage(
            BattleTurnContext context,
            BattleSkillSlotRuntime selectedSkill,
            int normalAttackDamage,
            int doubleAttackFollowUpDamage,
            int jumpAttackDamage,
            ref bool jumpAttackPrimed,
            ref int basicAttackCount,
            BattleTurnResolutionReport report)
        {
            if (selectedSkill != null)
            {
                report.ResolvedSkillCount++;
                report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Skill);
                var resolution = ResolveAttributeDamage(
                    Max(0, selectedSkill.Damage),
                    selectedSkill.Attribute,
                    context?.EnemyDefenseAttributeModifiers);
                ApplyEnemyDamageAttributeSummary(report, resolution);
                report.AddLog($"Skill resolved: {selectedSkill.DisplayName}{BuildAttributeLogSuffix(resolution)}");
                return resolution.Damage;
            }

            if (jumpAttackPrimed)
            {
                basicAttackCount++;
                jumpAttackPrimed = false;
                report.AddPlayerAnimationCue(BattlePlayerAnimationCue.JumpAttack);
                var resolution = ResolveAttributeDamage(
                    jumpAttackDamage,
                    context?.PlayerAttackAttribute,
                    context?.EnemyDefenseAttributeModifiers);
                ApplyEnemyDamageAttributeSummary(report, resolution);
                report.AddLog($"Jump attack resolved{BuildAttributeLogSuffix(resolution)}.");
                return resolution.Damage;
            }

            basicAttackCount++;
            if (basicAttackCount >= 2)
            {
                report.AddPlayerAnimationCue(BattlePlayerAnimationCue.DoubleAttack);
                var resolution = ResolveAttributeDamage(
                    doubleAttackFollowUpDamage,
                    context?.PlayerAttackAttribute,
                    context?.EnemyDefenseAttributeModifiers);
                ApplyEnemyDamageAttributeSummary(report, resolution);
                report.AddLog($"Double attack follow-up resolved{BuildAttributeLogSuffix(resolution)}.");
                return resolution.Damage;
            }

            report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Attack);
            var attackResolution = ResolveAttributeDamage(
                normalAttackDamage,
                context?.PlayerAttackAttribute,
                context?.EnemyDefenseAttributeModifiers);
            ApplyEnemyDamageAttributeSummary(report, attackResolution);
            report.AddLog($"Normal attack resolved{BuildAttributeLogSuffix(attackResolution)}.");
            return attackResolution.Damage;
        }

        private static void FinalizeOutcomeFlags(BattleTurnResolutionReport report, BattleSkillSlotRuntime selectedSkill)
        {
            report.GoalEffectTriggered = report.GoalReached && !report.TookHit && !report.PlayerDefeated;
            report.SelectedSkillConsumed = report.PathConfirmed && selectedSkill != null;
        }

        private static int ResolveHazardGroupKey(BattleCellState cell, int stepIndex)
        {
            if (cell != null && cell.HazardGroupId >= 0)
            {
                return cell.HazardGroupId;
            }

            return int.MinValue + stepIndex;
        }

        private static int ResolveHazardDamage(BattleTurnContext context, BattleTurnResolutionReport report)
        {
            var hazardDamage = context != null ? Max(0, context.EnemyActionDamage) : 0;
            var boostedDamage = context != null && context.HazardBoosted
                ? RoundToInt(hazardDamage * 1.5f)
                : hazardDamage;
            var resolution = ResolveAttributeDamage(
                boostedDamage,
                context?.EnemyAttackAttribute,
                context?.PlayerDefenseAttributeModifiers);
            ApplyPlayerDamageAttributeSummary(report, resolution);
            return resolution.Damage;
        }

        private static void ResolvePendingEnemyActionIfNeeded(
            BattleTurnResolutionReport report,
            BattleTurnContext context,
            int enemyActionDamage,
            int currentPlayerHp)
        {
            if (report == null ||
                !report.PathConfirmed ||
                report.EnemyDefeated ||
                report.PlayerDefeated ||
                report.ResolvedHazardCount > 0)
            {
                return;
            }

            var resolution = ResolveAttributeDamage(
                enemyActionDamage,
                context?.EnemyAttackAttribute,
                context?.PlayerDefenseAttributeModifiers);

            switch (context != null ? context.EnemyAction : BattleEnemyActionType.NormalAttack)
            {
                case BattleEnemyActionType.Dance:
                    report.AddLog("Enemy dance reserved the next turn hazard boost.");
                    return;

                case BattleEnemyActionType.Skill:
                    if (enemyActionDamage <= 0)
                    {
                        return;
                    }

                    report.ResolvedEnemyActionCount++;
                    report.PlayerDamageTaken += resolution.Damage;
                    ApplyPlayerDamageAttributeSummary(report, resolution);
                    report.AddLog($"Enemy skill resolved{BuildAttributeLogSuffix(resolution)}.");
                    break;

                case BattleEnemyActionType.NormalAttack:
                default:
                    if (enemyActionDamage <= 0)
                    {
                        return;
                    }

                    report.ResolvedEnemyActionCount++;
                    report.PlayerDamageTaken += resolution.Damage;
                    ApplyPlayerDamageAttributeSummary(report, resolution);
                    report.AddLog($"Enemy normal attack resolved{BuildAttributeLogSuffix(resolution)}.");
                    break;
            }

            if (report.PlayerDamageTaken >= currentPlayerHp)
            {
                report.PlayerDefeated = true;
                report.AddLog("Player defeated.");
            }
        }

        private static AttributeDamageResolution ResolveAttributeDamage(
            int baseDamage,
            AttributeData attackAttribute,
            IReadOnlyList<EquipmentAttributeModifierData> defenseModifiers)
        {
            var clampedBaseDamage = Max(0, baseDamage);
            if (clampedBaseDamage <= 0)
            {
                return new AttributeDamageResolution(0, 0, BattleAttributeEffectiveness.None, string.Empty);
            }

            if (attackAttribute == null || attackAttribute.AttributeId <= 0)
            {
                return new AttributeDamageResolution(clampedBaseDamage, 100, BattleAttributeEffectiveness.None, string.Empty);
            }

            var appliedPercent = ResolveAttributePercent(attackAttribute.AttributeId, defenseModifiers);
            var effectiveness = ResolveEffectiveness(appliedPercent);
            var resolvedDamage = RoundToInt(clampedBaseDamage * (appliedPercent / 100f));
            return new AttributeDamageResolution(
                resolvedDamage,
                appliedPercent,
                effectiveness,
                BuildAttributeSummary(attackAttribute.DisplayName, effectiveness));
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
                    return Max(0, modifier.DamagePercent);
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

        private static string BuildAttributeLogSuffix(AttributeDamageResolution resolution)
        {
            if (string.IsNullOrWhiteSpace(resolution.Summary))
            {
                return string.Empty;
            }

            return $" ({resolution.Summary} {resolution.AppliedPercent}%)";
        }

        private static void ApplyEnemyDamageAttributeSummary(
            BattleTurnResolutionReport report,
            AttributeDamageResolution resolution)
        {
            if (report == null || string.IsNullOrWhiteSpace(resolution.Summary))
            {
                return;
            }

            report.EnemyDamageAttributeSummary = resolution.Summary;
            report.EnemyDamageEffectiveness = resolution.Effectiveness;
        }

        private static void ApplyPlayerDamageAttributeSummary(
            BattleTurnResolutionReport report,
            AttributeDamageResolution resolution)
        {
            if (report == null || string.IsNullOrWhiteSpace(resolution.Summary))
            {
                return;
            }

            report.PlayerDamageAttributeSummary = resolution.Summary;
            report.PlayerDamageEffectiveness = resolution.Effectiveness;
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }

        private static int RoundToInt(float value)
        {
            return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}
