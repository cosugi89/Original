using System.Collections.Generic;
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
                    returnToIdle: true),
                new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Enemy,
                    enemyIntentAction.AnimationCue.Value,
                    0f,
                    returnToIdle: true));
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
                    returnToIdle: true),
                new BattleTurnAnimationEvent(
                    BattleTurnAnimationActor.Enemy,
                    enemyIntentAction.AnimationCue.Value,
                    0f,
                    returnToIdle: true));
            return true;
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
                returnToIdle: true));
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
    }

    internal enum BattleTurnAnimationActor
    {
        Player,
        Enemy,
    }

    internal readonly struct BattleTurnAnimationEvent
    {
        public BattleTurnAnimationEvent(
            BattleTurnAnimationActor actor,
            BattlePlayerAnimationCue cue,
            float startDelaySeconds,
            bool returnToIdle)
            : this(
                actor,
                cue,
                BattleTurnAnimationStartTrigger.AfterDelay(startDelaySeconds),
                returnToIdle)
        {
        }

        public BattleTurnAnimationEvent(
            BattleTurnAnimationActor actor,
            BattlePlayerAnimationCue cue,
            BattleTurnAnimationStartTrigger startTrigger,
            bool returnToIdle)
        {
            Actor = actor;
            Cue = cue;
            StartTrigger = startTrigger;
            ReturnToIdle = returnToIdle;
        }

        public BattleTurnAnimationActor Actor { get; }

        public BattlePlayerAnimationCue Cue { get; }

        public BattleTurnAnimationStartTrigger StartTrigger { get; }

        public bool ReturnToIdle { get; }
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
