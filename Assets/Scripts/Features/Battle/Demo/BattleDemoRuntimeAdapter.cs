using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// 既存 Demo データを本番名の戦闘コアへ橋渡しするアダプタ。
    /// 第 1 段階では共存を優先し、BattleScene の全面差し替えは次段階へ送る。
    /// </summary>
    public static class BattleDemoRuntimeAdapter
    {
        public static BattleDemoBoardFactory.Layout CreateBoardLayout(BattleDemoTurnScript turnScript, int width = 5, int height = 6)
        {
            return BattleDemoBoardFactory.Create(turnScript, width, height);
        }

        public static BattleEnemyActionType ToRuntime(this BattleDemoEnemyActionType actionType)
        {
            switch (actionType)
            {
                case BattleDemoEnemyActionType.Skill:
                    return BattleEnemyActionType.Skill;
                case BattleDemoEnemyActionType.Dance:
                    return BattleEnemyActionType.Dance;
                default:
                    return BattleEnemyActionType.NormalAttack;
            }
        }

        public static BattleNodeType ToRuntime(this BattleDemoNodeType nodeType)
        {
            switch (nodeType)
            {
                case BattleDemoNodeType.Start:
                    return BattleNodeType.Start;
                case BattleDemoNodeType.Empty:
                    return BattleNodeType.Empty;
                case BattleDemoNodeType.Jump:
                    return BattleNodeType.Jump;
                case BattleDemoNodeType.Roll:
                    return BattleNodeType.Roll;
                case BattleDemoNodeType.Dance:
                    return BattleNodeType.Dance;
                case BattleDemoNodeType.Attack:
                    return BattleNodeType.Attack;
                case BattleDemoNodeType.HazardNormal:
                    return BattleNodeType.HazardNormal;
                case BattleDemoNodeType.HazardSkill:
                    return BattleNodeType.HazardSkill;
                case BattleDemoNodeType.Goal:
                    return BattleNodeType.Goal;
                default:
                    return BattleNodeType.Empty;
            }
        }

        public static List<BattleNodeType> ToRuntimePath(this IReadOnlyList<BattleDemoNodeType> path)
        {
            var result = new List<BattleNodeType>();
            if (path == null)
            {
                return result;
            }

            for (var i = 0; i < path.Count; i++)
            {
                result.Add(path[i].ToRuntime());
            }

            return result;
        }

        public static BattleSkillSlotRuntime ToRuntime(this BattleDemoSkillSlot slot)
        {
            if (slot == null)
            {
                return null;
            }

            var runtime = new BattleSkillSlotRuntime
            {
                DisplayName = slot.DisplayName,
                Description = slot.Description,
                IsUnlocked = slot.IsUnlocked,
                IsConfigured = slot.IsConfigured,
                RequiredCharge = slot.RequiredCharge,
                StartingCharge = slot.CurrentCharge,
                TurnChargeGain = slot.TurnChargeGain,
                AttackChargeGain = slot.AttackChargeGain,
                Damage = slot.Damage,
            };
            runtime.ResetRuntime();
            return runtime;
        }

        public static BattleTurnContext CreateTurnContext(
            BattleDemoTurnScript turnScript,
            bool hazardBoosted,
            BattleDemoSkillSlot selectedSkill,
            int currentEnemyHp,
            int currentPlayerHp,
            int normalAttackDamage,
            int doubleAttackFollowUpDamage,
            int jumpAttackDamage)
        {
            return new BattleTurnContext
            {
                CurrentEnemyHp = currentEnemyHp,
                CurrentPlayerHp = currentPlayerHp,
                NormalAttackDamage = normalAttackDamage,
                DoubleAttackFollowUpDamage = doubleAttackFollowUpDamage,
                JumpAttackDamage = jumpAttackDamage,
                HazardDamage = turnScript != null ? turnScript.HazardDamage : 0,
                EnemyActionDamage = turnScript != null ? turnScript.HazardDamage : 0,
                HazardBoosted = hazardBoosted,
                EnemyAction = turnScript != null
                    ? turnScript.EnemyAction.ToRuntime()
                    : BattleEnemyActionType.NormalAttack,
                SelectedSkill = selectedSkill.ToRuntime(),
            };
        }

        public static BattleTurnResolutionReport ResolveWithRuntime(
            BattleDemoTurnScript turnScript,
            bool hazardBoosted,
            BattleDemoSkillSlot selectedSkill,
            int currentEnemyHp,
            int currentPlayerHp,
            int normalAttackDamage,
            int doubleAttackFollowUpDamage,
            int jumpAttackDamage)
        {
            var layout = CreateBoardLayout(turnScript);
            var tracer = new BattlePathTracer(layout.Board);
            var traceResult = TracePlannedPath(tracer, layout.TracePositions);
            if (traceResult != null)
            {
                return traceResult;
            }

            var context = CreateTurnContext(
                turnScript,
                hazardBoosted,
                selectedSkill,
                currentEnemyHp,
                currentPlayerHp,
                normalAttackDamage,
                doubleAttackFollowUpDamage,
                jumpAttackDamage);

            var path = layout.Board.BuildCellPath(tracer.Draft.Positions);
            var report = BattleTurnResolver.Resolve(path, context);
            report.AddLog($"Runtime path length: {tracer.Draft.Count}");
            return report;
        }

        private static BattleTurnResolutionReport TracePlannedPath(
            BattlePathTracer tracer,
            IReadOnlyList<BattleGridPosition> tracePositions)
        {
            if (tracePositions == null || tracePositions.Count == 0)
            {
                return new BattleTurnResolutionReport();
            }

            for (var i = 0; i < tracePositions.Count; i++)
            {
                var result = tracer.TryTrace(tracePositions[i]);
                if (!result.IsAccepted)
                {
                    return CreateTraceFailureReport(
                        $"Trace failed at step {i} ({tracePositions[i]}): {result.ValidationError}");
                }
            }

            var releaseResult = tracer.TryRelease();
            if (releaseResult.Status != BattlePathTraceStatus.Confirmed)
            {
                return CreateTraceFailureReport($"Trace release failed: {releaseResult.ValidationError}");
            }

            return null;
        }

        private static BattleTurnResolutionReport CreateTraceFailureReport(string message)
        {
            var report = new BattleTurnResolutionReport();
            report.AddLog(message);
            return report;
        }
    }
}
