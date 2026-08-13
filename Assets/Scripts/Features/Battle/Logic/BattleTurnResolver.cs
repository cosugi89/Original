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
        /// <summary>
        /// 確定パスを先頭から順に解釈し、攻撃・回避・被弾・Goal 到達を 1 ターン結果へ解決する。
        /// </summary>
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

            var selectedSkill = context.SelectedSkill;
            var traversalState = new BattleTurnTraversalState();
            var resolutionState = new BattleTurnResolutionState(
                Max(1, context.CurrentEnemyHp),
                Max(1, context.CurrentPlayerHp),
                Max(context.CurrentPlayerHp, context.MaxPlayerHp));
            var actionFactory = new BattleTurnResolvedActionFactory(
                context,
                resolutionState);

            for (var i = 0; i < path.Count; i++)
            {
                var cell = path[i];
                var nodeType = cell?.NodeType ?? BattleNodeType.Empty;

                switch (nodeType)
                {
                    case BattleNodeType.Start:
                    case BattleNodeType.Empty:
                        break;

                    case BattleNodeType.Jump:
                        traversalState.PrepareJump();
                        ApplyResolvedAction(report, resolutionState, actionFactory.CreateJumpPreparedAction());
                        break;

                    case BattleNodeType.Roll:
                        traversalState.PrepareRoll();
                        ApplyResolvedAction(report, resolutionState, actionFactory.CreateRollPreparedAction());
                        break;

                    case BattleNodeType.Dance:
                        traversalState.ResolveDance();
                        ApplyResolvedAction(report, resolutionState, actionFactory.CreateDanceAction());
                        break;

                    case BattleNodeType.Attack:
                        if (HandleAttackNode(report, selectedSkill, traversalState, resolutionState, actionFactory))
                        {
                            FinalizeOutcomeFlags(report, selectedSkill, resolutionState, actionFactory);
                            return report;
                        }
                        break;

                    case BattleNodeType.HazardNormal:
                    case BattleNodeType.HazardSkill:
                        if (HandleHazardNode(cell, i, nodeType, report, traversalState, resolutionState, actionFactory))
                        {
                            FinalizeOutcomeFlags(report, selectedSkill, resolutionState, actionFactory);
                            return report;
                        }
                        break;

                    case BattleNodeType.Goal:
                        return CompleteGoalNode(report, selectedSkill, resolutionState, actionFactory);
                }
            }

            FinalizeOutcomeFlags(report, selectedSkill, resolutionState, actionFactory);
            return report;
        }

        /// <summary>
        /// ノード種別列を簡易セル列へ変換してターン解決する。
        /// </summary>
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

        private static bool HandleAttackNode(
            BattleTurnResolutionReport report,
            SkillSlotRuntime selectedSkill,
            BattleTurnTraversalState traversalState,
            BattleTurnResolutionState resolutionState,
            BattleTurnResolvedActionFactory actionFactory)
        {
            var stopped = selectedSkill != null
                ? HandleSkillAttackNode(report, selectedSkill, resolutionState, actionFactory)
                : HandleBasicAttackNode(report, traversalState, resolutionState, actionFactory);

            traversalState.ResolveAttackExecution();
            return stopped;
        }

        private static bool HandleBasicAttackNode(
            BattleTurnResolutionReport report,
            BattleTurnTraversalState traversalState,
            BattleTurnResolutionState resolutionState,
            BattleTurnResolvedActionFactory actionFactory)
        {
            var action = actionFactory.CreateBasicAttackAction(traversalState.ConsumeBasicAttackDamageKind());
            ApplyResolvedAction(report, resolutionState, action);
            return resolutionState != null && resolutionState.EnemyDefeated;
        }

        private static bool HandleSkillAttackNode(
            BattleTurnResolutionReport report,
            SkillSlotRuntime selectedSkill,
            BattleTurnResolutionState resolutionState,
            BattleTurnResolvedActionFactory actionFactory)
        {
            ApplyResolvedAction(report, resolutionState, actionFactory.CreateSkillStartedAction(selectedSkill));

            var effects = selectedSkill?.Effects;
            if (effects == null || effects.Count == 0)
            {
                return IsBattleStopped(resolutionState);
            }

            for (var i = 0; i < effects.Count; i++)
            {
                ApplyResolvedAction(
                    report,
                    resolutionState,
                    actionFactory.CreateSkillEffectAction(selectedSkill, effects[i], i));

                if (IsBattleStopped(resolutionState))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HandleHazardNode(
            BattleCellState cell,
            int stepIndex,
            BattleNodeType nodeType,
            BattleTurnResolutionReport report,
            BattleTurnTraversalState traversalState,
            BattleTurnResolutionState resolutionState,
            BattleTurnResolvedActionFactory actionFactory)
        {
            var hazardGroupKey = ResolveHazardGroupKey(cell, stepIndex);
            if (traversalState.IsHazardResolved(hazardGroupKey))
            {
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardIgnoredAction(hazardGroupKey));
                return false;
            }

            if (traversalState.TryEvadeHazardWithJump(nodeType, hazardGroupKey))
            {
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardEnemyIntentAction(nodeType));
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardEvadedAction("Jump", hazardGroupKey));
                return false;
            }

            if (traversalState.TryEvadeHazardWithRoll(hazardGroupKey))
            {
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardEnemyIntentAction(nodeType));
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardEvadedAction("Roll", hazardGroupKey));
                return false;
            }

            traversalState.MarkHazardResolved(hazardGroupKey);
            ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardEnemyIntentAction(nodeType));
            ApplyResolvedAction(report, resolutionState, actionFactory.CreateHazardHitAction(hazardGroupKey));
            return true;
        }

        private static BattleTurnResolutionReport CompleteGoalNode(
            BattleTurnResolutionReport report,
            SkillSlotRuntime selectedSkill,
            BattleTurnResolutionState resolutionState,
            BattleTurnResolvedActionFactory actionFactory)
        {
            ApplyResolvedAction(report, resolutionState, actionFactory.CreateGoalReachedAction());
            FinalizeOutcomeFlags(report, selectedSkill, resolutionState, actionFactory);
            return report;
        }

        private static void FinalizeOutcomeFlags(
            BattleTurnResolutionReport report,
            SkillSlotRuntime selectedSkill,
            BattleTurnResolutionState resolutionState,
            BattleTurnResolvedActionFactory actionFactory)
        {
            if (report == null || actionFactory == null)
            {
                return;
            }

            if (resolutionState != null &&
                resolutionState.GoalReached &&
                !resolutionState.TookHit &&
                !resolutionState.PlayerDefeated)
            {
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateGoalEffectTriggeredAction());
            }

            if (resolutionState != null &&
                resolutionState.PathConfirmed &&
                selectedSkill != null)
            {
                ApplyResolvedAction(report, resolutionState, actionFactory.CreateSkillCommittedAction(selectedSkill));
            }
        }

        private static int ResolveHazardGroupKey(BattleCellState cell, int stepIndex)
        {
            if (cell != null && cell.HazardGroupId >= 0)
            {
                return cell.HazardGroupId;
            }

            return int.MinValue + stepIndex;
        }

        private static void ApplyResolvedAction(
            BattleTurnResolutionReport report,
            BattleTurnResolutionState resolutionState,
            BattleResolvedAction action)
        {
            if (report == null || action == null)
            {
                return;
            }

            report.ApplyResolvedAction(action);
            resolutionState?.Apply(action);
        }

        private static bool IsBattleStopped(BattleTurnResolutionState resolutionState)
        {
            return resolutionState != null &&
                   (resolutionState.EnemyDefeated || resolutionState.PlayerDefeated);
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }
    }
}
