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
            ApplyResolvedActionsToSession(session, result, pattern);

            var selectedSkill = ResolveSelectedSkill(session, playerSnapshot);
            if (selectedSkill != null && summary.SelectedSkillConsumed)
            {
                selectedSkill.Consume();
                session.SelectedSkillSlotIndex = -1;
            }

            if (selectedSkill != null && summary.SkillChargeGained > 0)
            {
                selectedSkill.GainCharge(summary.SkillChargeGained);
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} SkillChargeGain={summary.SkillChargeGained}");
            }

            if (summary.NextTurnHazardBoostReserved)
            {
                session.ReserveNextTurnHazardBoost();
            }

            session.ConsumeTemporaryStatModifierTurn();
            if (!summary.EnemyDefeated && !summary.PlayerDefeated && session.PlayerHp > 0)
            {
                ApplyTemporaryStatModifiers(session, summary, pattern);
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

        private static void ApplyTemporaryStatModifiers(
            BattleSessionState session,
            BattleResolvedActionSummary summary,
            StageBattlePatternData pattern)
        {
            var modifiers = summary?.TemporaryStatModifiers;
            if (session == null || modifiers == null || modifiers.Count == 0)
            {
                return;
            }

            var addedCount = 0;
            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier == null || modifier.IsExpired)
                {
                    continue;
                }

                session.AddTemporaryStatModifier(modifier);
                addedCount++;
            }

            if (addedCount > 0)
            {
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} TemporaryStatModifiers={addedCount}");
            }
        }

        private static void ApplyResolvedActionsToSession(
            BattleSessionState session,
            BattleTurnResolutionReport result,
            StageBattlePatternData pattern)
        {
            var actions = result?.ResolvedActions;
            if (actions == null || actions.Count == 0)
            {
                return;
            }

            var enemyDamageTaken = 0;
            var playerDamageTaken = 0;
            var playerHealing = 0;
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action == null)
                {
                    continue;
                }

                var resolution = action.DamageResolution;
                if (resolution != null && resolution.Damage > 0)
                {
                    if (action.TargetsEnemy)
                    {
                        enemyDamageTaken += resolution.Damage;
                        session.ApplyEnemyDamage(resolution.Damage);
                    }
                    else
                    {
                        playerDamageTaken += resolution.Damage;
                        session.ApplyPlayerDamage(resolution.Damage);
                    }
                }

                if (action.PlayerHealing > 0)
                {
                    playerHealing += action.PlayerHealing;
                    session.ApplyPlayerHealing(action.PlayerHealing);
                }
            }

            var patternLabel = GetPatternLabel(pattern);
            if (enemyDamageTaken > 0)
            {
                Debug.Log($"[Battle] {patternLabel} EnemyDamage={enemyDamageTaken}");
            }

            if (playerDamageTaken > 0)
            {
                Debug.Log($"[Battle] {patternLabel} PlayerDamage={playerDamageTaken}");
            }

            if (playerHealing > 0)
            {
                Debug.Log($"[Battle] {patternLabel} PlayerHealing={playerHealing}");
            }
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

