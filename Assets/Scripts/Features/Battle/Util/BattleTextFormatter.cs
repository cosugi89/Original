using System.Collections.Generic;
using Assets.Scripts.Core.Const;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Util
{
    /// <summary>
    /// Battle のプレビュー文言、履歴文言、デバッグ表示を組み立てる。
    /// </summary>
    public static class BattleTextFormatter
    {
        public static string BuildTurnPrompt(
            BattleSessionState session,
            bool preferInteractiveTraceInput,
            StageBattlePatternData pattern)
        {
            if (session != null && session.BattleEnded)
            {
                return BattleUiText.BattleEnded;
            }

            if (!preferInteractiveTraceInput)
            {
                return pattern != null ? pattern.ConfirmText : string.Empty;
            }

            if (pattern != null && !string.IsNullOrWhiteSpace(pattern.Description))
            {
                return $"{pattern.Description} {BattleUiText.DrawFromStartToGoal}";
            }

            return BattleUiText.DrawFromStartToGoal;
        }

        public static string BuildTurnPreviewMessage(BattlePreparedTurn preparedTurn, BattleTurnResolutionReport preview)
        {
            var fallbackMessage = preparedTurn?.Pattern != null
                ? preparedTurn.Pattern.ConfirmText
                : BattleUiText.ExecutableFallback;
            return FormatPreviewSummary(preview?.Summary, fallbackMessage);
        }

        public static string BuildTraceValidationErrorMessage(BattlePathValidationError error)
        {
            return error switch
            {
                BattlePathValidationError.NotAdjacent => BattleUiText.TraceNotAdjacent,
                BattlePathValidationError.Revisit => BattleUiText.TraceRevisit,
                BattlePathValidationError.CrossedSegment => BattleUiText.TraceCrossedSegment,
                BattlePathValidationError.ExtendedAfterGoal => BattleUiText.TraceExtendedAfterGoal,
                BattlePathValidationError.InvalidStart => BattleUiText.TraceInvalidStart,
                _ => BattleUiText.InvalidPathFallback,
            };
        }

        public static string BuildPlayerHistoryEntry(BattleResolvedActionSummary summary)
        {
            var parts = new List<string>();

            if (summary.ResolvedSkillCount > 0 && !string.IsNullOrWhiteSpace(summary.SkillDisplayName))
            {
                parts.Add($"Skill {summary.SkillDisplayName}");
            }
            else if (summary.ResolvedAttackCount > 0)
            {
                parts.Add($"{BattleUiText.AttackActionLabel} x{summary.ResolvedAttackCount}");
            }

            if (summary.GoalReached)
            {
                parts.Add(BattleUiText.GoalLabel);
            }

            if (summary.StoppedByHazardHit)
            {
                parts.Add(BattleUiText.HazardStoppedLabel);
            }

            return parts.Count > 0 ? string.Join(" / ", parts) : BattleUiText.PlayerStandby;
        }

        public static string BuildEnemyHistoryEntry(StageBattlePatternData pattern, BattleResolvedActionSummary summary)
        {
            var actionLabel = pattern?.EnemyAction switch
            {
                BattleEnemyActionType.Dance => "Dance",
                BattleEnemyActionType.Skill => "Skill",
                _ => BattleUiText.AttackActionLabel,
            };

            var parts = new List<string> { actionLabel };
            if (summary.PlayerDamageTaken > 0)
            {
                parts.Add($"-{summary.PlayerDamageTaken}");
            }

            if (summary.NextTurnHazardBoostReserved)
            {
                parts.Add(BattleUiText.HazardBoostLabel);
            }

            return string.Join(" / ", parts);
        }

        public static string BuildPlayerSpeech(BattleResolvedActionSummary summary)
        {
            if (summary.EnemyDefeated)
            {
                return BattleUiText.PlayerVictory;
            }

            if (summary.PlayerDefeated)
            {
                return BattleUiText.PlayerOops;
            }

            if (summary.ResolvedSkillCount > 0 && !string.IsNullOrWhiteSpace(summary.SkillDisplayName))
            {
                return $"{summary.SkillDisplayName}!";
            }

            if (summary.StoppedByHazardHit)
            {
                return BattleUiText.PlayerNarrowEscape;
            }

            if (summary.GoalReached)
            {
                return BattleUiText.PlayerReady;
            }

            return BattleUiText.PlayerKeepGoing;
        }

        public static string BuildEnemySpeech(StageBattlePatternData pattern, BattleResolvedActionSummary summary)
        {
            if (summary.PlayerDefeated)
            {
                return TextLocalization.Get("Cornered you");
            }

            if (summary.EnemyDefeated)
            {
                return "……";
            }

            if (summary.NextTurnHazardBoostReserved)
            {
                return TextLocalization.Get("The next turn will be dangerous");
            }

            if (summary.PlayerDamageTaken > 0)
            {
                return TextLocalization.Get("This is not over yet");
            }

            return pattern?.ConfirmText ?? string.Empty;
        }

        public static string BuildEnemyFlavor(StageBattlePatternData pattern, BattleResolvedActionSummary summary)
        {
            if (summary.EnemyDefeated)
            {
                return TextLocalization.Get("Their fighting spirit has faltered");
            }

            if (summary.PlayerDefeated)
            {
                return TextLocalization.Get("Overwhelmed by pressure");
            }

            if (summary.NextTurnHazardBoostReserved)
            {
                return TextLocalization.Get("Hazards grow stronger after Dance");
            }

            return pattern?.Description ?? string.Empty;
        }

        public static string BuildEnemyDefeatedMessage(string enemyName)
        {
            return string.Format(TextLocalization.Get("{0} defeated"), enemyName);
        }

        public static string BuildBattleTypeValueText(int guardValue)
        {
            return guardValue > 0 ? $"{guardValue} / {guardValue}" : BattleUiText.NoGaugeValue;
        }

        public static string GetPatternLabel(StageBattlePatternData pattern)
        {
            if (pattern == null || string.IsNullOrWhiteSpace(pattern.DebugLabel))
            {
                return BattleUiText.PatternFallback;
            }

            return pattern.DebugLabel;
        }

        public static string FormatPath(IReadOnlyList<BattleGridPosition> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                return BattleUiText.EmptyPath;
            }

            return string.Join(" -> ", positions);
        }

        private static string FormatPreviewSummary(BattleResolvedActionSummary summary, string fallbackMessage)
        {
            var resolvedSummary = summary ?? new BattleResolvedActionSummary();
            var parts = new List<string>();

            if (resolvedSummary.ResolvedSkillCount > 0 && !string.IsNullOrWhiteSpace(resolvedSummary.SkillDisplayName))
            {
                parts.Add($"Skill:{resolvedSummary.SkillDisplayName}");
            }
            else if (resolvedSummary.ResolvedAttackCount > 0)
            {
                parts.Add(string.Format(TextLocalization.Get("Attack x{0}"), resolvedSummary.ResolvedAttackCount));
            }

            if (resolvedSummary.EnemyDamageTaken > 0 || !string.IsNullOrWhiteSpace(resolvedSummary.EnemyDamageAttributeSummary))
            {
                parts.Add(string.IsNullOrWhiteSpace(resolvedSummary.EnemyDamageAttributeSummary)
                    ? $"Enemy -{resolvedSummary.EnemyDamageTaken}"
                    : $"Enemy -{resolvedSummary.EnemyDamageTaken} [{resolvedSummary.EnemyDamageAttributeSummary}]");
            }

            if (resolvedSummary.PlayerHealing > 0)
            {
                parts.Add($"Player +{resolvedSummary.PlayerHealing}");
            }

            if (resolvedSummary.PlayerGuardGained > 0)
            {
                parts.Add($"Guard+{resolvedSummary.PlayerGuardGained}");
            }

            if (resolvedSummary.SkillChargeGained > 0)
            {
                parts.Add($"Charge+{resolvedSummary.SkillChargeGained}");
            }

            if (resolvedSummary.PlayerTemporaryModifierCount > 0)
            {
                parts.Add($"Buff+{resolvedSummary.PlayerTemporaryModifierCount}");
            }

            if (resolvedSummary.EnemyTemporaryModifierCount > 0)
            {
                parts.Add($"Debuff+{resolvedSummary.EnemyTemporaryModifierCount}");
            }

            if (resolvedSummary.ResolvedHazardCount > 0)
            {
                parts.Add(resolvedSummary.PlayerDamageTaken > 0 || !string.IsNullOrWhiteSpace(resolvedSummary.PlayerDamageAttributeSummary)
                    ? string.IsNullOrWhiteSpace(resolvedSummary.PlayerDamageAttributeSummary)
                        ? $"Player -{resolvedSummary.PlayerDamageTaken}"
                        : $"Player -{resolvedSummary.PlayerDamageTaken} [{resolvedSummary.PlayerDamageAttributeSummary}]"
                    : BattleUiText.NoDamageTaken);
            }
            else if (resolvedSummary.ResolvedEnemyActionCount > 0 &&
                     (resolvedSummary.PlayerDamageTaken > 0 || !string.IsNullOrWhiteSpace(resolvedSummary.PlayerDamageAttributeSummary)))
            {
                parts.Add(string.IsNullOrWhiteSpace(resolvedSummary.PlayerDamageAttributeSummary)
                    ? $"Enemy attack -{resolvedSummary.PlayerDamageTaken}"
                    : $"Enemy attack -{resolvedSummary.PlayerDamageTaken} [{resolvedSummary.PlayerDamageAttributeSummary}]");
            }

            if (resolvedSummary.StoppedByHazardHit)
            {
                parts.Add(BattleUiText.HazardStoppedPreviewLabel);
            }

            if (resolvedSummary.GoalReached)
            {
                parts.Add(BattleUiText.GoalReachedLabel);
            }

            if (parts.Count == 0)
            {
                return fallbackMessage;
            }

            return string.Join(" / ", parts);
        }
    }
}
