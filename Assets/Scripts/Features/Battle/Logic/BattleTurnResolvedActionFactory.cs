using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// トラバーサル結果から BattleResolvedAction を生成する。
    /// </summary>
    internal sealed class BattleTurnResolvedActionFactory
    {
        private readonly BattleTurnContext _context;
        private readonly BattleTurnResolutionState _state;

        public BattleTurnResolvedActionFactory(
            BattleTurnContext context,
            BattleTurnResolutionState state)
        {
            _context = context;
            _state = state;
        }

        public BattleResolvedAction CreateJumpPreparedAction()
        {
            return CreateSimpleAction(
                BattleResolvedActionType.JumpPrepared,
                animationCue: BattlePlayerAnimationCue.Jump,
                logMessage: "Jump prepared.");
        }

        public BattleResolvedAction CreateRollPreparedAction()
        {
            return CreateSimpleAction(
                BattleResolvedActionType.RollPrepared,
                animationCue: BattlePlayerAnimationCue.Roll,
                logMessage: "Roll prepared.");
        }

        public BattleResolvedAction CreateDanceAction()
        {
            return CreateSimpleAction(
                BattleResolvedActionType.Dance,
                animationCue: BattlePlayerAnimationCue.Dance,
                logMessage: "Dance resolved.",
                semantics: BattleResolvedActionSemantics.ResolvedDance);
        }

        public BattleResolvedAction CreateGoalReachedAction()
        {
            return CreateSimpleAction(
                BattleResolvedActionType.GoalReached,
                logMessage: "Goal reached.",
                semantics: BattleResolvedActionSemantics.GoalReached | BattleResolvedActionSemantics.PathConfirmed);
        }

        public BattleResolvedAction CreateGoalEffectTriggeredAction()
        {
            return CreateSimpleAction(
                BattleResolvedActionType.GoalEffectTriggered,
                semantics: BattleResolvedActionSemantics.GoalEffectTriggered);
        }

        public BattleResolvedAction CreateSkillCommittedAction(SkillSlotRuntime selectedSkill)
        {
            return CreateSimpleAction(
                BattleResolvedActionType.SkillCommitted,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                semantics: BattleResolvedActionSemantics.SelectedSkillConsumed);
        }

        public BattleResolvedAction CreateHazardIgnoredAction(int hazardGroupKey)
        {
            return CreateSimpleAction(
                BattleResolvedActionType.HazardIgnored,
                logMessage: $"Hazard group {hazardGroupKey} already resolved.",
                semantics: BattleResolvedActionSemantics.ResolvedHazard);
        }

        public BattleResolvedAction CreateHazardEvadedAction(string actorName, int hazardGroupKey)
        {
            return CreateSimpleAction(
                BattleResolvedActionType.HazardEvaded,
                logMessage: $"{actorName} evaded hazard group {hazardGroupKey}.",
                semantics: BattleResolvedActionSemantics.ResolvedHazard);
        }

        public BattleResolvedAction CreateHazardHitAction(int hazardGroupKey)
        {
            return CreateDamageAction(
                BattleResolvedActionType.HazardHit,
                BattleDamageKind.EnemyAction,
                targetIsEnemy: false,
                hazardBoosted: _context?.HazardBoosted ?? false,
                animationCue: BattlePlayerAnimationCue.Stun,
                logMessage: $"Hazard group {hazardGroupKey} hit the player.",
                semantics: BattleResolvedActionSemantics.ResolvedHazard |
                           BattleResolvedActionSemantics.TookHit |
                           BattleResolvedActionSemantics.StoppedByHazardHit,
                defeatLogMessage: "Player defeated.");
        }

        public BattleResolvedAction CreateSkillAction(SkillSlotRuntime selectedSkill)
        {
            return CreatePlayerDamageAction(
                BattleResolvedActionType.Skill,
                BattleDamageKind.Skill,
                BattlePlayerAnimationCue.Skill,
                $"Skill resolved: {selectedSkill?.DisplayName}",
                selectedSkill: selectedSkill,
                appendPeriod: false,
                semantics: BattleResolvedActionSemantics.ResolvedAttack | BattleResolvedActionSemantics.ResolvedSkill);
        }

        public BattleResolvedAction CreateBasicAttackAction(BattleDamageKind damageKind)
        {
            return damageKind switch
            {
                BattleDamageKind.JumpAttack => CreatePlayerDamageAction(
                    BattleResolvedActionType.PlayerAttack,
                    BattleDamageKind.JumpAttack,
                    BattlePlayerAnimationCue.JumpAttack,
                    "Jump attack resolved"),
                BattleDamageKind.DoubleAttackFollowUp => CreatePlayerDamageAction(
                    BattleResolvedActionType.PlayerAttack,
                    BattleDamageKind.DoubleAttackFollowUp,
                    BattlePlayerAnimationCue.DoubleAttack,
                    "Double attack follow-up resolved"),
                _ => CreatePlayerDamageAction(
                    BattleResolvedActionType.PlayerAttack,
                    BattleDamageKind.NormalAttack,
                    BattlePlayerAnimationCue.Attack,
                    "Normal attack resolved"),
            };
        }

        public BattleResolvedAction CreateEnemyAction(bool isSkill)
        {
            return CreateDamageAction(
                BattleResolvedActionType.EnemyAction,
                BattleDamageKind.EnemyAction,
                targetIsEnemy: false,
                logMessage: isSkill ? "Enemy skill resolved" : "Enemy normal attack resolved",
                logWhenZeroBaseDamage: false,
                skipIfZeroBaseDamage: true,
                semantics: BattleResolvedActionSemantics.ResolvedEnemyAction,
                defeatLogMessage: "Player defeated.");
        }

        public BattleResolvedAction CreateHazardBoostReservedAction()
        {
            return CreateSimpleAction(
                BattleResolvedActionType.HazardBoostReserved,
                logMessage: "Enemy dance reserved the next turn hazard boost.",
                semantics: BattleResolvedActionSemantics.NextTurnHazardBoostReserved);
        }

        private BattleResolvedAction CreatePlayerDamageAction(
            BattleResolvedActionType actionType,
            BattleDamageKind damageKind,
            BattlePlayerAnimationCue animationCue,
            string logMessage,
            SkillSlotRuntime selectedSkill = null,
            bool appendPeriod = true,
            BattleResolvedActionSemantics semantics = BattleResolvedActionSemantics.ResolvedAttack)
        {
            return CreateDamageAction(
                actionType,
                damageKind,
                targetIsEnemy: true,
                selectedSkill: selectedSkill,
                animationCue: animationCue,
                logMessage: logMessage,
                appendPeriod: appendPeriod,
                semantics: semantics,
                defeatLogMessage: "Enemy defeated.");
        }

        private BattleResolvedAction CreateDamageAction(
            BattleResolvedActionType actionType,
            BattleDamageKind damageKind,
            bool targetIsEnemy,
            SkillSlotRuntime selectedSkill = null,
            bool hazardBoosted = false,
            BattlePlayerAnimationCue? animationCue = null,
            string logMessage = "",
            bool appendPeriod = true,
            bool logWhenZeroBaseDamage = true,
            bool skipIfZeroBaseDamage = false,
            BattleResolvedActionSemantics semantics = BattleResolvedActionSemantics.None,
            string defeatLogMessage = "")
        {
            var resolution = BattleDamagePipeline.Resolve(new BattleDamagePipelineRequest
            {
                Context = _context,
                DamageKind = damageKind,
                Skill = selectedSkill,
                HazardBoosted = hazardBoosted,
            });

            if (skipIfZeroBaseDamage && resolution.BaseDamage <= 0)
            {
                return null;
            }

            var marksDefeated = IsTargetDefeatedAfterResolution(
                resolution,
                targetIsEnemy);
            var resolvedSemantics = semantics;
            if (marksDefeated)
            {
                resolvedSemantics |= targetIsEnemy
                    ? BattleResolvedActionSemantics.EnemyDefeated
                    : BattleResolvedActionSemantics.PlayerDefeated;
            }

            return new BattleResolvedAction
            {
                ActionType = actionType,
                DamageKind = damageKind,
                DisplayName = selectedSkill?.DisplayName ?? string.Empty,
                DamageResolution = resolution,
                TargetsEnemy = targetIsEnemy,
                Semantics = resolvedSemantics,
                AnimationCue = animationCue,
                LogMessages = BuildDamageActionLogMessages(
                    resolution,
                    logMessage,
                    appendPeriod,
                    logWhenZeroBaseDamage,
                    marksDefeated ? defeatLogMessage : string.Empty),
            };
        }

        private static BattleResolvedAction CreateSimpleAction(
            BattleResolvedActionType actionType,
            BattlePlayerAnimationCue? animationCue = null,
            string logMessage = "",
            string displayName = "",
            BattleResolvedActionSemantics semantics = BattleResolvedActionSemantics.None)
        {
            return new BattleResolvedAction
            {
                ActionType = actionType,
                DisplayName = displayName ?? string.Empty,
                Semantics = semantics,
                AnimationCue = animationCue,
                LogMessages = string.IsNullOrWhiteSpace(logMessage)
                    ? Array.Empty<string>()
                    : new[] { logMessage },
            };
        }

        private bool IsTargetDefeatedAfterResolution(
            BattleDamageResolution resolution,
            bool targetIsEnemy)
        {
            if (_state == null || resolution == null)
            {
                return false;
            }

            var targetCurrentHp = targetIsEnemy ? _state.CurrentEnemyHp : _state.CurrentPlayerHp;
            return targetCurrentHp > 0 && resolution.Damage >= Max(1, targetCurrentHp);
        }

        private static IReadOnlyList<string> BuildDamageActionLogMessages(
            BattleDamageResolution resolution,
            string logMessage,
            bool appendPeriod,
            bool logWhenZeroBaseDamage,
            string defeatLogMessage)
        {
            var result = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(logMessage) &&
                (logWhenZeroBaseDamage || resolution?.BaseDamage > 0))
            {
                result.Add($"{logMessage}{resolution?.LogSuffix}{(appendPeriod ? "." : string.Empty)}");
            }

            if (!string.IsNullOrWhiteSpace(defeatLogMessage))
            {
                result.Add(defeatLogMessage);
            }

            return result.Count > 0 ? result.ToArray() : Array.Empty<string>();
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }
    }
}

