using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
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

        public BattleResolvedAction CreateSkillStartedAction(SkillSlotRuntime selectedSkill)
        {
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                animationCue: BattlePlayerAnimationCue.Skill,
                logMessage: $"Skill resolved: {selectedSkill?.DisplayName}",
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                semantics: BattleResolvedActionSemantics.ResolvedAttack | BattleResolvedActionSemantics.ResolvedSkill);
        }

        public BattleResolvedAction CreateSkillEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            int effectIndex)
        {
            if (selectedSkill == null || effect == null)
            {
                return null;
            }

            var success = ResolveSkillEffectSuccess(selectedSkill, effect, effectIndex);
            if (success != null && !success.Succeeded)
            {
                return CreateSkillEffectFailedAction(selectedSkill, success);
            }

            return effect.EffectType switch
            {
                BattleSkillOperationType.Damage => CreateSkillDamageEffectAction(selectedSkill, effect, success),
                BattleSkillOperationType.ChargeGain => CreateSkillChargeGainEffectAction(selectedSkill, effect, success),
                BattleSkillOperationType.Heal => CreateSkillHealEffectAction(selectedSkill, effect, success),
                BattleSkillOperationType.GuardGain => CreateSkillGuardGainEffectAction(selectedSkill, effect, success),
                BattleSkillOperationType.Buff => CreateSkillTemporaryStatModifierEffectAction(
                    selectedSkill,
                    effect,
                    success,
                    targetsPlayer: true),
                BattleSkillOperationType.Debuff => CreateSkillTemporaryStatModifierEffectAction(
                    selectedSkill,
                    effect,
                    success,
                    targetsPlayer: false),
                _ => CreateSkillNoOpEffectAction(selectedSkill, success),
            };
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

        private BattleResolvedAction CreateSkillDamageEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            BattleSkillEffectSuccessResult success)
        {
            return CreateDamageAction(
                BattleResolvedActionType.Skill,
                BattleDamageKind.Skill,
                targetIsEnemy: true,
                selectedSkill: selectedSkill,
                skillEffect: effect,
                skillEffectSuccess: success,
                logWhenZeroBaseDamage: false,
                semantics: BattleResolvedActionSemantics.None,
                defeatLogMessage: "Enemy defeated.");
        }

        private BattleResolvedAction CreateSkillNoOpEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectSuccessResult success)
        {
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                skillEffectSuccess: success);
        }

        private BattleResolvedAction CreateSkillChargeGainEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            BattleSkillEffectSuccessResult success)
        {
            var amount = ResolveSkillEffectAmount(selectedSkill, effect);
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                logMessage: amount > 0 ? $"Skill charge +{amount}." : string.Empty,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                skillEffectSuccess: success,
                skillChargeGain: amount);
        }

        private BattleResolvedAction CreateSkillHealEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            BattleSkillEffectSuccessResult success)
        {
            var amount = ResolveActualPlayerHealing(ResolveSkillEffectAmount(selectedSkill, effect));
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                logMessage: amount > 0 ? $"Player healed +{amount}." : string.Empty,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                skillEffectSuccess: success,
                playerHealing: amount);
        }

        private BattleResolvedAction CreateSkillGuardGainEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            BattleSkillEffectSuccessResult success)
        {
            var amount = ResolveSkillEffectAmount(selectedSkill, effect);
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                logMessage: amount > 0 ? $"Guard +{amount}." : string.Empty,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                skillEffectSuccess: success,
                playerGuardGain: amount);
        }

        private BattleResolvedAction CreateSkillTemporaryStatModifierEffectAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            BattleSkillEffectSuccessResult success,
            bool targetsPlayer)
        {
            var temporaryModifier = CreateTemporaryStatModifier(effect, targetsPlayer);
            var hasModifier = temporaryModifier != null;
            var label = targetsPlayer ? "Buff" : "Debuff";
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                logMessage: hasModifier ? $"{label} reserved for {temporaryModifier.RemainingTurns} turn(s)." : string.Empty,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                skillEffectSuccess: success,
                temporaryStatModifiers: hasModifier
                    ? new[] { temporaryModifier }
                    : Array.Empty<BattleTemporaryStatModifier>());
        }

        private BattleResolvedAction CreateSkillEffectFailedAction(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectSuccessResult success)
        {
            return CreateSimpleAction(
                BattleResolvedActionType.Skill,
                displayName: selectedSkill?.DisplayName ?? string.Empty,
                skillEffectSuccess: success);
        }

        private BattleSkillEffectSuccessResult ResolveSkillEffectSuccess(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect,
            int effectIndex)
        {
            var resolver = _context?.SkillEffectSuccessResolver ?? BattleSkillEffectSuccessResolver.CreatePreview();
            return resolver.Resolve(new BattleSkillEffectSuccessContext
            {
                TurnContext = _context,
                Skill = selectedSkill,
                Effect = effect,
                EffectIndex = effectIndex,
            });
        }

        private BattleResolvedAction CreateDamageAction(
            BattleResolvedActionType actionType,
            BattleDamageKind damageKind,
            bool targetIsEnemy,
            SkillSlotRuntime selectedSkill = null,
            BattleSkillEffectEntryData skillEffect = null,
            BattleSkillEffectSuccessResult skillEffectSuccess = null,
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
                SkillEffect = skillEffect,
                HazardBoosted = hazardBoosted,
            });

            if (skipIfZeroBaseDamage && resolution.BaseDamage <= 0)
            {
                return null;
            }

            var playerGuardAbsorbed = 0;
            resolution = ApplyPlayerGuardToDamage(resolution, targetIsEnemy, out playerGuardAbsorbed);

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
                SkillEffectSuccess = skillEffectSuccess,
                PlayerGuardAbsorbed = playerGuardAbsorbed,
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
            BattleResolvedActionSemantics semantics = BattleResolvedActionSemantics.None,
            BattleSkillEffectSuccessResult skillEffectSuccess = null,
            int playerHealing = 0,
            int playerGuardGain = 0,
            int skillChargeGain = 0,
            IReadOnlyList<BattleTemporaryStatModifier> temporaryStatModifiers = null)
        {
            return new BattleResolvedAction
            {
                ActionType = actionType,
                DisplayName = displayName ?? string.Empty,
                Semantics = semantics,
                SkillEffectSuccess = skillEffectSuccess,
                PlayerHealing = Max(0, playerHealing),
                PlayerGuardGain = Max(0, playerGuardGain),
                SkillChargeGain = Max(0, skillChargeGain),
                TemporaryStatModifiers = temporaryStatModifiers ?? Array.Empty<BattleTemporaryStatModifier>(),
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

        private BattleDamageResolution ApplyPlayerGuardToDamage(
            BattleDamageResolution resolution,
            bool targetIsEnemy,
            out int playerGuardAbsorbed)
        {
            playerGuardAbsorbed = 0;
            if (targetIsEnemy ||
                resolution == null ||
                resolution.Damage <= 0 ||
                _state == null ||
                _state.CurrentPlayerGuard <= 0)
            {
                return resolution;
            }

            playerGuardAbsorbed = Min(_state.CurrentPlayerGuard, resolution.Damage);
            return new BattleDamageResolution
            {
                BaseDamage = resolution.BaseDamage,
                Damage = Max(0, resolution.Damage - playerGuardAbsorbed),
                AppliedPercent = resolution.AppliedPercent,
                Effectiveness = resolution.Effectiveness,
                AttributeSummary = resolution.AttributeSummary,
            };
        }

        private int ResolveSkillEffectAmount(
            SkillSlotRuntime selectedSkill,
            BattleSkillEffectEntryData effect)
        {
            if (effect == null)
            {
                return 0;
            }

            var input = new BattleDamageCalculationInput(_context, selectedSkill);
            var baseValue = effect.ParameterType == BattleSkillParameterType.None
                ? 0
                : input.ResolveParameterValue(effect.ParameterType);
            var resolved = effect.ValueType switch
            {
                BattleSkillValueType.FixedValue => effect.Value,
                BattleSkillValueType.Multiplier => baseValue * effect.Value,
                BattleSkillValueType.Percentage => baseValue * effect.Value,
                _ => 0f,
            };

            return Max(0, RoundToInt(resolved));
        }

        private int ResolveActualPlayerHealing(int requestedHealing)
        {
            if (requestedHealing <= 0)
            {
                return 0;
            }

            var currentHp = _state?.CurrentPlayerHp ?? _context?.CurrentPlayerHp ?? 0;
            var maxHp = _state?.MaxPlayerHp ?? Max(currentHp, _context?.MaxPlayerHp ?? currentHp);
            return Max(0, Min(requestedHealing, maxHp - currentHp));
        }

        private BattleTemporaryStatModifier CreateTemporaryStatModifier(
            BattleSkillEffectEntryData effect,
            bool targetsPlayer)
        {
            if (effect == null ||
                effect.DurationTurns <= 0 ||
                effect.ParameterType == BattleSkillParameterType.None ||
                BattleSkillParameterTypeUtility.IsLegacySpeed(effect.ParameterType))
            {
                return null;
            }

            if (!TryResolveTemporaryStatModifierOperation(
                    effect,
                    targetsPlayer,
                    out var operationType,
                    out var value))
            {
                return null;
            }

            return new BattleTemporaryStatModifier(
                targetsPlayer,
                new BattleStatModifierData
                {
                    TargetType = BattleStatModifierTargetType.Parameter,
                    ParameterType = effect.ParameterType,
                    OperationType = operationType,
                    Value = value,
                },
                effect.DurationTurns);
        }

        private static bool TryResolveTemporaryStatModifierOperation(
            BattleSkillEffectEntryData effect,
            bool targetsPlayer,
            out BattleStatModifierOperationType operationType,
            out float value)
        {
            operationType = BattleStatModifierOperationType.Add;
            value = 0f;

            if (effect == null)
            {
                return false;
            }

            var isBuff = targetsPlayer;
            switch (effect.ValueType)
            {
                case BattleSkillValueType.FixedValue:
                    operationType = BattleStatModifierOperationType.Add;
                    value = isBuff ? Max(0f, effect.Value) : -Max(0f, effect.Value);
                    return Math.Abs(value) > 0.0001f;
                case BattleSkillValueType.Multiplier:
                    operationType = BattleStatModifierOperationType.Multiply;
                    value = Max(0f, effect.Value);
                    return Math.Abs(value - 1f) > 0.0001f;
                case BattleSkillValueType.Percentage:
                    operationType = BattleStatModifierOperationType.Multiply;
                    value = isBuff
                        ? 1f + Max(0f, effect.Value)
                        : Max(0f, 1f - Max(0f, effect.Value));
                    return Math.Abs(value - 1f) > 0.0001f;
                default:
                    return false;
            }
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

        private static float Max(float left, float right)
        {
            return left > right ? left : right;
        }

        private static int Min(int left, int right)
        {
            return left < right ? left : right;
        }

        private static int RoundToInt(float value)
        {
            return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}

