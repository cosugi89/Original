using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 解決済みアクション列を、同時再生や遅延再生を含む演出ビート列へ変換する。
    /// </summary>
    internal static class BattleTurnAnimationPlanner
    {
        private const float EnemyAttackHitFallbackDelaySeconds = 0.45f;
        private const float EnemySkillHitFallbackDelaySeconds = 0.55f;

        public static IReadOnlyList<BattleTurnAnimationBeat> Build(IReadOnlyList<BattleResolvedAction> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                return new List<BattleTurnAnimationBeat>();
            }

            var beats = new List<BattleTurnAnimationBeat>();
            for (var i = 0; i < actions.Count; i++)
            {
                if (TryCreateEvadedHazardBeat(actions, i, out var evadedBeat))
                {
                    beats.Add(evadedBeat);
                    i += 2;
                    continue;
                }

                if (TryCreateHazardHitBeat(actions, i, out var hitBeat))
                {
                    beats.Add(hitBeat);
                    i += 1;
                    continue;
                }

                if (TryAppendSkillBeats(actions, i, beats, out var consumedSkillActionCount))
                {
                    i += consumedSkillActionCount - 1;
                    continue;
                }

                if (TryCreateSingleActionBeat(actions[i], out var singleBeat))
                {
                    beats.Add(singleBeat);
                }
            }

            return beats;
        }

        private static bool TryCreateEvadedHazardBeat(
            IReadOnlyList<BattleResolvedAction> actions,
            int index,
            out BattleTurnAnimationBeat beat)
        {
            beat = default;
            if (!TryGetAction(actions, index, out var evasionAction) ||
                !TryGetAction(actions, index + 1, out var enemyIntentAction) ||
                !TryGetAction(actions, index + 2, out var evadedAction))
            {
                return false;
            }

            if (!IsPreparedEvasionAction(evasionAction) ||
                enemyIntentAction.ActionType != BattleResolvedActionType.EnemyIntent ||
                evadedAction.ActionType != BattleResolvedActionType.HazardEvaded ||
                !evasionAction.AnimationCue.HasValue ||
                !enemyIntentAction.AnimationCue.HasValue)
            {
                return false;
            }

            beat = BattleTurnAnimationBeat.Create(
                new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Player,
                    evasionAction.AnimationCue.Value,
                    BattleTurnAnimationStartTrigger.OnAnimationEvent(
                        BattleTurnAnimationActor.Enemy,
                        BattleAnimationEventKeys.AttackStart,
                        fallbackDelaySeconds: 0f,
                        triggerCue: enemyIntentAction.AnimationCue.Value),
                    returnToIdle: true,
                    resolvedActions: CreateActionList(evasionAction, evadedAction)),
                new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Enemy,
                    enemyIntentAction.AnimationCue.Value,
                    0f,
                    returnToIdle: true,
                    resolvedActions: CreateActionList(enemyIntentAction)));
            return true;
        }

        private static bool TryCreateHazardHitBeat(
            IReadOnlyList<BattleResolvedAction> actions,
            int index,
            out BattleTurnAnimationBeat beat)
        {
            beat = default;
            if (!TryGetAction(actions, index, out var enemyIntentAction) ||
                !TryGetAction(actions, index + 1, out var hazardHitAction))
            {
                return false;
            }

            if (enemyIntentAction.ActionType != BattleResolvedActionType.EnemyIntent ||
                hazardHitAction.ActionType != BattleResolvedActionType.HazardHit ||
                !enemyIntentAction.AnimationCue.HasValue ||
                !hazardHitAction.AnimationCue.HasValue)
            {
                return false;
            }

            beat = BattleTurnAnimationBeat.Create(
                new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Player,
                    hazardHitAction.AnimationCue.Value,
                    BattleTurnAnimationStartTrigger.OnAnimationEvent(
                        BattleTurnAnimationActor.Enemy,
                        BattleAnimationEventKeys.Hit,
                        GetHitFallbackDelaySeconds(enemyIntentAction.AnimationCue.Value),
                        enemyIntentAction.AnimationCue.Value),
                    returnToIdle: true,
                    resolvedActions: CreateActionList(hazardHitAction)),
                new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Enemy,
                    enemyIntentAction.AnimationCue.Value,
                    0f,
                    returnToIdle: true,
                    resolvedActions: CreateActionList(enemyIntentAction)));
            return true;
        }

        private static bool TryAppendSkillBeats(
            IReadOnlyList<BattleResolvedAction> actions,
            int index,
            List<BattleTurnAnimationBeat> beats,
            out int consumedActionCount)
        {
            consumedActionCount = 0;
            if (!TryGetAction(actions, index, out var skillStartAction) ||
                skillStartAction.ActionType != BattleResolvedActionType.Skill ||
                !skillStartAction.AnimationCue.HasValue ||
                skillStartAction.AnimationCue.Value != BattlePlayerAnimationCue.Skill ||
                !skillStartAction.HasSemantic(BattleResolvedActionSemantics.ResolvedSkill))
            {
                return false;
            }

            var effectActions = CollectFollowingSkillEffectActions(actions, index + 1);
            var hasEffectActions = effectActions.Count > 0;
            beats.Add(BattleTurnAnimationBeat.Create(new BattleTurnAnimationEvent(
                ResolveActor(skillStartAction),
                skillStartAction.AnimationCue.Value,
                0f,
                returnToIdle: !hasEffectActions,
                resolvedActions: CreateActionList(skillStartAction),
                resolveTiming: BattleTurnAnimationResolveTiming.OnAnimationStart,
                waitForRecoverEnd: !hasEffectActions)));

            for (var i = 0; i < effectActions.Count; i++)
            {
                var effectAction = effectActions[i];
                var effectAnimation = effectAction.EffectAnimation;
                var effectCue = TryResolveEffectAnimationCue(effectAnimation, out var resolvedCue)
                    ? resolvedCue
                    : (BattlePlayerAnimationCue?)null;
                var startTrigger = i == 0
                    ? BattleTurnAnimationStartTrigger.OnAnimationEvent(
                        BattleTurnAnimationActor.Player,
                        BattleAnimationEventKeys.Hit,
                        ResolveEffectFallbackSeconds(effectAnimation),
                        BattlePlayerAnimationCue.Skill)
                    : BattleTurnAnimationStartTrigger.AfterDelay(0f);

                beats.Add(BattleTurnAnimationBeat.Create(new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Player,
                    effectCue,
                    startTrigger,
                    returnToIdle: i == effectActions.Count - 1,
                    resolvedActions: CreateActionList(effectAction),
                    resolveTiming: ResolveSkillEffectTiming(effectAction),
                    waitForRecoverEnd: effectCue.HasValue,
                    resolveDelaySeconds: ResolveEffectFallbackSeconds(effectAnimation),
                    effectAnimation: effectAnimation)));
            }

            consumedActionCount = 1 + effectActions.Count;
            return true;
        }

        private static IReadOnlyList<BattleResolvedAction> CollectFollowingSkillEffectActions(
            IReadOnlyList<BattleResolvedAction> actions,
            int startIndex)
        {
            var result = new List<BattleResolvedAction>();
            for (var i = startIndex; i < actions.Count; i++)
            {
                var effectAction = actions[i];
                if (effectAction == null ||
                    effectAction.ActionType != BattleResolvedActionType.Skill ||
                    effectAction.AnimationCue.HasValue ||
                    effectAction.HasSemantic(BattleResolvedActionSemantics.ResolvedSkill))
                {
                    break;
                }

                result.Add(effectAction);
            }

            return result;
        }

        private static bool TryCreateSingleActionBeat(
            BattleResolvedAction action,
            out BattleTurnAnimationBeat beat)
        {
            beat = default;
            if (action == null || !action.AnimationCue.HasValue)
            {
                return false;
            }

            beat = BattleTurnAnimationBeat.Create(new BattleTurnAnimationEvent(
                ResolveActor(action),
                action.AnimationCue.Value,
                0f,
                returnToIdle: true,
                resolvedActions: CreateActionList(action),
                resolveTiming: ResolveTiming(action)));
            return true;
        }

        private static BattleTurnAnimationActor ResolveActor(BattleResolvedAction action)
        {
            switch (action.ActionType)
            {
                case BattleResolvedActionType.EnemyAction:
                case BattleResolvedActionType.EnemyIntent:
                case BattleResolvedActionType.HazardBoostReserved:
                    return BattleTurnAnimationActor.Enemy;
                default:
                    return BattleTurnAnimationActor.Player;
            }
        }

        private static float GetHitFallbackDelaySeconds(BattlePlayerAnimationCue enemyCue)
        {
            return enemyCue == BattlePlayerAnimationCue.Skill
                ? EnemySkillHitFallbackDelaySeconds
                : EnemyAttackHitFallbackDelaySeconds;
        }

        private static bool TryGetAction(
            IReadOnlyList<BattleResolvedAction> actions,
            int index,
            out BattleResolvedAction action)
        {
            action = null;
            if (actions == null || index < 0 || index >= actions.Count)
            {
                return false;
            }

            action = actions[index];
            return action != null;
        }

        private static bool IsPreparedEvasionAction(BattleResolvedAction action)
        {
            return action != null &&
                   (action.ActionType == BattleResolvedActionType.JumpPrepared ||
                    action.ActionType == BattleResolvedActionType.RollPrepared);
        }

        private static BattleTurnAnimationResolveTiming ResolveTiming(BattleResolvedAction action)
        {
            if (action == null)
            {
                return BattleTurnAnimationResolveTiming.OnAnimationStart;
            }

            if (action.ActionType == BattleResolvedActionType.HazardHit)
            {
                return BattleTurnAnimationResolveTiming.OnAnimationStart;
            }

            return action.DamageResolution != null || action.PlayerHealing > 0
                ? BattleTurnAnimationResolveTiming.OnHit
                : BattleTurnAnimationResolveTiming.OnAnimationStart;
        }

        private static BattleTurnAnimationResolveTiming ResolveSkillEffectTiming(BattleResolvedAction action)
        {
            if (action == null)
            {
                return BattleTurnAnimationResolveTiming.OnAnimationStart;
            }

            if (action.DamageResolution != null ||
                action.PlayerHealing > 0 ||
                action.PlayerGuardGain > 0 ||
                action.SkillChargeGain > 0 ||
                action.TemporaryStatModifiers?.Count > 0)
            {
                return BattleTurnAnimationResolveTiming.OnHit;
            }

            return BattleTurnAnimationResolveTiming.OnAnimationStart;
        }

        private static float ResolveEffectFallbackSeconds(EffectAnimationData effectAnimation)
        {
            return effectAnimation != null
                ? Math.Max(0f, effectAnimation.WaitSeconds)
                : 0f;
        }

        private static bool TryResolveEffectAnimationCue(
            EffectAnimationData effectAnimation,
            out BattlePlayerAnimationCue cue)
        {
            cue = default;
            var animationName = effectAnimation?.CharacterAnimationName;
            if (string.IsNullOrWhiteSpace(animationName))
            {
                return false;
            }

            var values = (BattlePlayerAnimationCue[])Enum.GetValues(typeof(BattlePlayerAnimationCue));
            for (var i = 0; i < values.Length; i++)
            {
                var candidate = values[i];
                if (string.Equals(candidate.ToString(), animationName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(BattleAnimationCatalog.GetClipName(candidate), animationName, StringComparison.OrdinalIgnoreCase))
                {
                    cue = candidate;
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyList<BattleResolvedAction> CreateActionList(params BattleResolvedAction[] actions)
        {
            if (actions == null || actions.Length == 0)
            {
                return Array.Empty<BattleResolvedAction>();
            }

            var result = new List<BattleResolvedAction>(actions.Length);
            for (var i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                {
                    result.Add(actions[i]);
                }
            }

            return result.Count > 0 ? result : Array.Empty<BattleResolvedAction>();
        }
    }

    internal enum BattleTurnAnimationActor
    {
        Player,
        Enemy,
    }

    internal enum BattleTurnAnimationResolveTiming
    {
        OnAnimationStart,
        OnHit,
    }

    internal readonly struct BattleTurnAnimationEvent
    {
        public BattleTurnAnimationEvent(
            BattleTurnAnimationActor actor,
            BattlePlayerAnimationCue cue,
            float startDelaySeconds,
            bool returnToIdle,
            IReadOnlyList<BattleResolvedAction> resolvedActions = null,
            BattleTurnAnimationResolveTiming resolveTiming = BattleTurnAnimationResolveTiming.OnAnimationStart,
            bool waitForRecoverEnd = true,
            float resolveDelaySeconds = 0f,
            EffectAnimationData effectAnimation = null)
            : this(
                actor,
                cue,
                BattleTurnAnimationStartTrigger.AfterDelay(startDelaySeconds),
                returnToIdle,
                resolvedActions,
                resolveTiming,
                waitForRecoverEnd,
                resolveDelaySeconds,
                effectAnimation)
        {
        }

        public BattleTurnAnimationEvent(
            BattleTurnAnimationActor actor,
            BattlePlayerAnimationCue? cue,
            BattleTurnAnimationStartTrigger startTrigger,
            bool returnToIdle,
            IReadOnlyList<BattleResolvedAction> resolvedActions = null,
            BattleTurnAnimationResolveTiming resolveTiming = BattleTurnAnimationResolveTiming.OnAnimationStart,
            bool waitForRecoverEnd = true,
            float resolveDelaySeconds = 0f,
            EffectAnimationData effectAnimation = null)
        {
            Actor = actor;
            Cue = cue;
            StartTrigger = startTrigger;
            ReturnToIdle = returnToIdle;
            ResolvedActions = resolvedActions ?? Array.Empty<BattleResolvedAction>();
            ResolveTiming = resolveTiming;
            WaitForRecoverEnd = waitForRecoverEnd;
            ResolveDelaySeconds = Math.Max(0f, resolveDelaySeconds);
            EffectAnimation = effectAnimation;
        }

        public BattleTurnAnimationActor Actor { get; }

        public BattlePlayerAnimationCue? Cue { get; }

        public BattleTurnAnimationStartTrigger StartTrigger { get; }

        public bool ReturnToIdle { get; }

        public IReadOnlyList<BattleResolvedAction> ResolvedActions { get; }

        public BattleTurnAnimationResolveTiming ResolveTiming { get; }

        public bool WaitForRecoverEnd { get; }

        public float ResolveDelaySeconds { get; }

        public EffectAnimationData EffectAnimation { get; }
    }

    internal readonly struct BattleTurnAnimationStartTrigger
    {
        private BattleTurnAnimationStartTrigger(
            bool waitsForAnimationEvent,
            BattleTurnAnimationActor triggerActor,
            string eventKey,
            float fallbackDelaySeconds,
            float delaySeconds,
            BattlePlayerAnimationCue? triggerCue)
        {
            WaitsForAnimationEvent = waitsForAnimationEvent;
            TriggerActor = triggerActor;
            EventKey = eventKey;
            FallbackDelaySeconds = fallbackDelaySeconds;
            DelaySeconds = delaySeconds;
            TriggerCue = triggerCue;
        }

        public bool WaitsForAnimationEvent { get; }

        public BattleTurnAnimationActor TriggerActor { get; }

        public string EventKey { get; }

        public float FallbackDelaySeconds { get; }

        public float DelaySeconds { get; }

        public BattlePlayerAnimationCue? TriggerCue { get; }

        public static BattleTurnAnimationStartTrigger AfterDelay(float delaySeconds)
        {
            return new BattleTurnAnimationStartTrigger(
                waitsForAnimationEvent: false,
                triggerActor: BattleTurnAnimationActor.Player,
                eventKey: string.Empty,
                fallbackDelaySeconds: 0f,
                delaySeconds: delaySeconds,
                triggerCue: null);
        }

        public static BattleTurnAnimationStartTrigger OnAnimationEvent(
            BattleTurnAnimationActor triggerActor,
            string eventKey,
            float fallbackDelaySeconds,
            BattlePlayerAnimationCue? triggerCue = null)
        {
            return new BattleTurnAnimationStartTrigger(
                waitsForAnimationEvent: true,
                triggerActor: triggerActor,
                eventKey: eventKey,
                fallbackDelaySeconds: fallbackDelaySeconds,
                delaySeconds: 0f,
                triggerCue: triggerCue);
        }
    }

    internal readonly struct BattleTurnAnimationBeat
    {
        private BattleTurnAnimationBeat(IReadOnlyList<BattleTurnAnimationEvent> events)
        {
            Events = events;
        }

        public IReadOnlyList<BattleTurnAnimationEvent> Events { get; }

        public static BattleTurnAnimationBeat Create(params BattleTurnAnimationEvent[] events)
        {
            return new BattleTurnAnimationBeat(events);
        }
    }
}
