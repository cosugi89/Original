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
        public static BattleTurnResolutionReport Resolve(IReadOnlyList<BattleNodeType> path, BattleTurnContext context)
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
            var hazardDamage = Max(0, context.HazardDamage);
            var selectedSkill = context.SelectedSkill;

            var jumpCanEvade = false;
            var jumpAttackPrimed = false;
            var rollCanEvade = false;
            var hazardGroupResolved = false;
            var basicAttackCount = 0;

            for (var i = 0; i < path.Count; i++)
            {
                var nodeType = path[i];
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
                        report.AddLog("Jump prepared.");
                        break;

                    case BattleNodeType.Roll:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = true;
                        report.AddLog("Roll prepared.");
                        break;

                    case BattleNodeType.Dance:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = false;
                        report.ResolvedDanceCount++;
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

                        if (hazardGroupResolved)
                        {
                            report.AddLog("Hazard group already resolved.");
                            break;
                        }

                        if (nodeType == BattleNodeType.HazardNormal && jumpCanEvade)
                        {
                            jumpCanEvade = false;
                            hazardGroupResolved = true;
                            report.AddLog("Jump evaded a normal hazard.");
                            break;
                        }

                        if (rollCanEvade)
                        {
                            rollCanEvade = false;
                            jumpAttackPrimed = false;
                            hazardGroupResolved = true;
                            report.AddLog("Roll evaded a hazard.");
                            break;
                        }

                        hazardGroupResolved = true;
                        report.TookHit = true;
                        report.StoppedByHazardHit = true;
                        report.PlayerDamageTaken += context.HazardBoosted
                            ? RoundToInt(hazardDamage * 1.5f)
                            : hazardDamage;
                        report.AddLog("Hazard hit the player.");

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
                        FinalizeOutcomeFlags(report, selectedSkill);
                        return report;
                }
            }

            FinalizeOutcomeFlags(report, selectedSkill);
            return report;
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
                report.AddLog($"Skill resolved: {selectedSkill.DisplayName}");
                return Max(0, selectedSkill.Damage);
            }

            if (jumpAttackPrimed)
            {
                basicAttackCount++;
                jumpAttackPrimed = false;
                report.AddLog("Jump attack resolved.");
                return jumpAttackDamage;
            }

            basicAttackCount++;
            if (basicAttackCount >= 2)
            {
                report.AddLog("Double attack follow-up resolved.");
                return doubleAttackFollowUpDamage;
            }

            report.AddLog("Normal attack resolved.");
            return normalAttackDamage;
        }

        private static void FinalizeOutcomeFlags(BattleTurnResolutionReport report, BattleSkillSlotRuntime selectedSkill)
        {
            report.GoalEffectTriggered = report.GoalReached && !report.TookHit && !report.PlayerDefeated;
            report.SelectedSkillConsumed = report.PathConfirmed && selectedSkill != null;
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
