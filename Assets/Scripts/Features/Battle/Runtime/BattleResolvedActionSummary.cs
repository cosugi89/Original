using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// ターン全体の集計結果と表示用要約を保持する。
    /// </summary>
    public sealed class BattleResolvedActionSummary
    {
        public static BattleResolvedActionSummary Build(IReadOnlyList<BattleResolvedAction> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                return new BattleResolvedActionSummary();
            }

            var enemyDamageTaken = 0;
            var playerDamageTaken = 0;
            var resolvedAttackCount = 0;
            var resolvedSkillCount = 0;
            var resolvedHazardCount = 0;
            var resolvedDanceCount = 0;
            var resolvedEnemyActionCount = 0;
            var skillDisplayName = string.Empty;
            var enemyDamageAttributeSummary = string.Empty;
            var playerDamageAttributeSummary = string.Empty;
            var pathConfirmed = false;
            var goalReached = false;
            var tookHit = false;
            var stoppedByHazardHit = false;
            var enemyDefeated = false;
            var playerDefeated = false;
            var goalEffectTriggered = false;
            var selectedSkillConsumed = false;
            var nextTurnHazardBoostReserved = false;
            var logEntries = new List<string>();
            var animationSequence = new List<BattlePlayerAnimationCue>();

            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action == null)
                {
                    continue;
                }

                if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedAttack))
                {
                    resolvedAttackCount++;
                }

                if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedSkill))
                {
                    resolvedSkillCount++;
                    if (string.IsNullOrWhiteSpace(skillDisplayName) &&
                        !string.IsNullOrWhiteSpace(action.DisplayName))
                    {
                        skillDisplayName = action.DisplayName;
                    }
                }

                if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedHazard))
                {
                    resolvedHazardCount++;
                }

                if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedDance))
                {
                    resolvedDanceCount++;
                }

                if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedEnemyAction))
                {
                    resolvedEnemyActionCount++;
                }

                if (action.AnimationCue.HasValue)
                {
                    animationSequence.Add(action.AnimationCue.Value);
                }

                var actionLogMessages = action.LogMessages;
                if (actionLogMessages != null)
                {
                    for (var logIndex = 0; logIndex < actionLogMessages.Count; logIndex++)
                    {
                        var logMessage = actionLogMessages[logIndex];
                        if (!string.IsNullOrWhiteSpace(logMessage))
                        {
                            logEntries.Add(logMessage);
                        }
                    }
                }

                var resolution = action.DamageResolution;
                if (resolution != null)
                {
                    if (action.TargetsEnemy)
                    {
                        enemyDamageTaken += resolution.Damage;
                        if (!string.IsNullOrWhiteSpace(resolution.AttributeSummary))
                        {
                            enemyDamageAttributeSummary = resolution.AttributeSummary;
                        }
                    }
                    else
                    {
                        playerDamageTaken += resolution.Damage;
                        if (!string.IsNullOrWhiteSpace(resolution.AttributeSummary))
                        {
                            playerDamageAttributeSummary = resolution.AttributeSummary;
                        }
                    }
                }

                pathConfirmed |= action.HasSemantic(BattleResolvedActionSemantics.PathConfirmed);
                goalReached |= action.HasSemantic(BattleResolvedActionSemantics.GoalReached);
                tookHit |= action.HasSemantic(BattleResolvedActionSemantics.TookHit);
                stoppedByHazardHit |= action.HasSemantic(BattleResolvedActionSemantics.StoppedByHazardHit);
                enemyDefeated |= action.HasSemantic(BattleResolvedActionSemantics.EnemyDefeated);
                playerDefeated |= action.HasSemantic(BattleResolvedActionSemantics.PlayerDefeated);
                goalEffectTriggered |= action.HasSemantic(BattleResolvedActionSemantics.GoalEffectTriggered);
                selectedSkillConsumed |= action.HasSemantic(BattleResolvedActionSemantics.SelectedSkillConsumed);
                nextTurnHazardBoostReserved |= action.HasSemantic(BattleResolvedActionSemantics.NextTurnHazardBoostReserved);
            }

            if (playerDefeated)
            {
                animationSequence.Add(BattlePlayerAnimationCue.Defeat);
            }
            else if (enemyDefeated)
            {
                animationSequence.Add(BattlePlayerAnimationCue.Victory);
            }

            return new BattleResolvedActionSummary
            {
                PathConfirmed = pathConfirmed,
                EnemyDamageTaken = enemyDamageTaken,
                PlayerDamageTaken = playerDamageTaken,
                ResolvedAttackCount = resolvedAttackCount,
                ResolvedSkillCount = resolvedSkillCount,
                ResolvedHazardCount = resolvedHazardCount,
                ResolvedDanceCount = resolvedDanceCount,
                ResolvedEnemyActionCount = resolvedEnemyActionCount,
                SkillDisplayName = skillDisplayName,
                EnemyDamageAttributeSummary = enemyDamageAttributeSummary,
                PlayerDamageAttributeSummary = playerDamageAttributeSummary,
                GoalReached = goalReached,
                TookHit = tookHit,
                StoppedByHazardHit = stoppedByHazardHit,
                EnemyDefeated = enemyDefeated,
                PlayerDefeated = playerDefeated,
                GoalEffectTriggered = goalEffectTriggered,
                SelectedSkillConsumed = selectedSkillConsumed,
                NextTurnHazardBoostReserved = nextTurnHazardBoostReserved,
                LogEntries = logEntries.ToArray(),
                AnimationSequence = animationSequence.ToArray(),
            };
        }

        public bool PathConfirmed { get; init; }

        public int EnemyDamageTaken { get; init; }

        public int PlayerDamageTaken { get; init; }

        public int ResolvedAttackCount { get; init; }

        public int ResolvedSkillCount { get; init; }

        public int ResolvedHazardCount { get; init; }

        public int ResolvedDanceCount { get; init; }

        public int ResolvedEnemyActionCount { get; init; }

        public string SkillDisplayName { get; init; } = string.Empty;

        public string EnemyDamageAttributeSummary { get; init; } = string.Empty;

        public string PlayerDamageAttributeSummary { get; init; } = string.Empty;

        public bool GoalReached { get; init; }

        public bool TookHit { get; init; }

        public bool StoppedByHazardHit { get; init; }

        public bool EnemyDefeated { get; init; }

        public bool PlayerDefeated { get; init; }

        public bool GoalEffectTriggered { get; init; }

        public bool SelectedSkillConsumed { get; init; }

        public bool NextTurnHazardBoostReserved { get; init; }

        public IReadOnlyList<string> LogEntries { get; init; } = Array.Empty<string>();

        public IReadOnlyList<BattlePlayerAnimationCue> AnimationSequence { get; init; } =
            Array.Empty<BattlePlayerAnimationCue>();
    }
}

