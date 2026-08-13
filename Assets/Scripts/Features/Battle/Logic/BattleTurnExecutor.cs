using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// プレビュー計算と本実行の入口としてターン解決フローを呼び分ける。
    /// </summary>
    public sealed class BattleTurnExecutor
    {
        public BattleTurnResolutionReport Preview(
            BattlePreparedTurn preparedTurn,
            IReadOnlyList<BattleGridPosition> positions,
            BattleSessionState session,
            BattlePlayerSnapshot playerSnapshot,
            StageBattleEnemyData enemyData,
            SkillSlotRuntime selectedSkill,
            bool hazardBoosted)
        {
            return Resolve(
                preparedTurn,
                positions,
                session,
                playerSnapshot,
                enemyData,
                selectedSkill,
                hazardBoosted,
                beginTurn: false,
                pathLabel: "Preview");
        }

        public BattleTurnResolutionReport ExecuteInteractive(
            BattlePreparedTurn preparedTurn,
            IReadOnlyList<BattleGridPosition> confirmedPath,
            BattleSessionState session,
            BattlePlayerSnapshot playerSnapshot,
            StageBattleEnemyData enemyData,
            SkillSlotRuntime selectedSkill)
        {
            if (preparedTurn?.Board == null ||
                confirmedPath == null ||
                confirmedPath.Count == 0 ||
                session == null ||
                playerSnapshot == null)
            {
                return null;
            }

            return Resolve(
                preparedTurn,
                confirmedPath,
                session,
                playerSnapshot,
                enemyData,
                selectedSkill,
                session.ConsumeHazardBoostFlag(),
                beginTurn: true,
                pathLabel: "Interactive");
        }

        public BattleTurnResolutionReport ExecuteFallback(
            BattlePreparedTurn preparedTurn,
            BattleSessionState session,
            BattlePlayerSnapshot playerSnapshot,
            StageBattleEnemyData enemyData,
            SkillSlotRuntime selectedSkill)
        {
            if (preparedTurn?.Board == null ||
                preparedTurn.FallbackTracePositions == null ||
                preparedTurn.FallbackTracePositions.Count == 0 ||
                session == null ||
                playerSnapshot == null)
            {
                return null;
            }

            return Resolve(
                preparedTurn,
                preparedTurn.FallbackTracePositions,
                session,
                playerSnapshot,
                enemyData,
                selectedSkill,
                session.ConsumeHazardBoostFlag(),
                beginTurn: true,
                pathLabel: "Fallback");
        }

        private static BattleTurnResolutionReport Resolve(
            BattlePreparedTurn preparedTurn,
            IReadOnlyList<BattleGridPosition> positions,
            BattleSessionState session,
            BattlePlayerSnapshot playerSnapshot,
            StageBattleEnemyData enemyData,
            SkillSlotRuntime selectedSkill,
            bool hazardBoosted,
            bool beginTurn,
            string pathLabel)
        {
            if (preparedTurn?.Board == null ||
                positions == null ||
                positions.Count == 0 ||
                session == null ||
                playerSnapshot == null)
            {
                return null;
            }

            SkillSlotRuntime selectedSkillForContext;
            if (beginTurn)
            {
                BeginTurn(session, playerSnapshot);
                selectedSkillForContext = selectedSkill?.Clone();
            }
            else
            {
                selectedSkillForContext = selectedSkill?.Clone();
                selectedSkillForContext?.GainTurnCharge();
            }

            var context = BattleStageRuntimeAdapter.CreateTurnContext(
                preparedTurn.Pattern,
                enemyData?.Stats,
                hazardBoosted,
                selectedSkillForContext,
                session.EnemyHp,
                session.PlayerHp,
                playerSnapshot.Stats,
                playerSnapshot.MaxHp,
                session.TemporaryStatModifiers);
            context.SkillEffectSuccessResolver = beginTurn
                ? BattleSkillEffectSuccessResolver.CreateRuntime()
                : BattleSkillEffectSuccessResolver.CreatePreview();

            var executedCellPath = preparedTurn.Board.BuildCellPath(positions);
            var report = BattleTurnResolver.Resolve(executedCellPath, context);
            report.AddSystemLog($"{pathLabel} path length: {positions.Count}");
            return report;
        }

        private static void BeginTurn(BattleSessionState session, BattlePlayerSnapshot playerSnapshot)
        {
            session.IsPathInputLocked = true;

            var skillSlots = playerSnapshot.SkillSlots;
            for (var i = 0; i < skillSlots.Count; i++)
            {
                skillSlots[i]?.GainTurnCharge();
            }
        }
    }
}

