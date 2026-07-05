using System;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// 解決済みターン結果をセッション状態とスキル状態へ反映する。
    /// </summary>
    public sealed class BattleTurnResultApplier
    {
        public void Apply(
            BattleSessionState session,
            BattlePlayerSnapshot playerSnapshot,
            BattleTurnResolutionReport result,
            StageBattlePatternData pattern,
            Action onBattleClear = null)
        {
            if (session == null || result == null)
            {
                return;
            }

            var summary = result.Summary;

            if (summary.EnemyDamageTaken > 0)
            {
                session.ApplyEnemyDamage(summary.EnemyDamageTaken);
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} EnemyDamage={summary.EnemyDamageTaken}");
            }

            if (summary.PlayerDamageTaken > 0)
            {
                session.ApplyPlayerDamage(summary.PlayerDamageTaken);
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} PlayerDamage={summary.PlayerDamageTaken}");
            }

            var selectedSkill = ResolveSelectedSkill(session, playerSnapshot);
            if (selectedSkill != null && summary.SelectedSkillConsumed)
            {
                selectedSkill.Consume();
                session.SelectedSkillSlotIndex = -1;
            }

            if (summary.NextTurnHazardBoostReserved)
            {
                session.ReserveNextTurnHazardBoost();
            }

            if (summary.EnemyDefeated)
            {
                session.BattleEnded = true;
                onBattleClear?.Invoke();
                return;
            }

            if (summary.PlayerDefeated || session.PlayerHp <= 0)
            {
                session.BattleEnded = true;
            }
        }

        private static SkillSlotRuntime ResolveSelectedSkill(
            BattleSessionState session,
            BattlePlayerSnapshot playerSnapshot)
        {
            var skillSlots = playerSnapshot?.SkillSlots;
            var slotIndex = session != null ? session.SelectedSkillSlotIndex : -1;
            if (skillSlots == null || slotIndex < 0 || slotIndex >= skillSlots.Count)
            {
                return null;
            }

            return skillSlots[slotIndex];
        }

        private static string GetPatternLabel(StageBattlePatternData pattern)
        {
            if (pattern == null || string.IsNullOrWhiteSpace(pattern.DebugLabel))
            {
                return "Pattern";
            }

            return pattern.DebugLabel;
        }
    }
}

