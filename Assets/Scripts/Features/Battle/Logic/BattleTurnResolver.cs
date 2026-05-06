using System;
using System.Collections.Generic;
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
                        report.PlayerDamageTaken += ResolveHazardDamage(context, cell?.HazardGroupId ?? -1);
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
                        ResolvePendingEnemyActionIfNeeded(report, context.EnemyAction, enemyActionDamage, currentPlayerHp);
                        FinalizeOutcomeFlags(report, selectedSkill);
                        return report;
                }
            }

            ResolvePendingEnemyActionIfNeeded(report, context.EnemyAction, enemyActionDamage, currentPlayerHp);
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
                report.AddLog($"Skill resolved: {selectedSkill.DisplayName}");
                return Max(0, selectedSkill.Damage);
            }

            if (jumpAttackPrimed)
            {
                basicAttackCount++;
                jumpAttackPrimed = false;
                report.AddPlayerAnimationCue(BattlePlayerAnimationCue.JumpAttack);
                report.AddLog("Jump attack resolved.");
                return jumpAttackDamage;
            }

            basicAttackCount++;
            if (basicAttackCount >= 2)
            {
                report.AddPlayerAnimationCue(BattlePlayerAnimationCue.DoubleAttack);
                report.AddLog("Double attack follow-up resolved.");
                return doubleAttackFollowUpDamage;
            }

            report.AddPlayerAnimationCue(BattlePlayerAnimationCue.Attack);
            report.AddLog("Normal attack resolved.");
            return normalAttackDamage;
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

        private static int ResolveHazardDamage(BattleTurnContext context, int hazardGroupId)
        {
            var hazardDamage = context != null ? Max(0, context.HazardDamage) : 0;
            if (context?.HazardDamageByGroupId != null &&
                hazardGroupId >= 0 &&
                context.HazardDamageByGroupId.TryGetValue(hazardGroupId, out var groupDamage))
            {
                hazardDamage = Max(0, groupDamage);
            }

            return context != null && context.HazardBoosted
                ? RoundToInt(hazardDamage * 1.5f)
                : hazardDamage;
        }

        private static void ResolvePendingEnemyActionIfNeeded(
            BattleTurnResolutionReport report,
            BattleEnemyActionType enemyAction,
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

            switch (enemyAction)
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
                    report.PlayerDamageTaken += enemyActionDamage;
                    report.AddLog("Enemy skill resolved.");
                    break;

                case BattleEnemyActionType.NormalAttack:
                default:
                    if (enemyActionDamage <= 0)
                    {
                        return;
                    }

                    report.ResolvedEnemyActionCount++;
                    report.PlayerDamageTaken += enemyActionDamage;
                    report.AddLog("Enemy normal attack resolved.");
                    break;
            }

            if (report.PlayerDamageTaken >= currentPlayerHp)
            {
                report.PlayerDefeated = true;
                report.AddLog("Player defeated.");
            }
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
