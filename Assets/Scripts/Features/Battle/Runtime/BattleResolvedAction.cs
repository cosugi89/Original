using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    [Flags]
    public enum BattleResolvedActionSemantics
    {
        None = 0,
        ResolvedAttack = 1 << 0,
        ResolvedSkill = 1 << 1,
        ResolvedHazard = 1 << 2,
        ResolvedDance = 1 << 3,
        ResolvedEnemyAction = 1 << 4,
        GoalReached = 1 << 5,
        PathConfirmed = 1 << 6,
        TookHit = 1 << 7,
        StoppedByHazardHit = 1 << 8,
        EnemyDefeated = 1 << 9,
        PlayerDefeated = 1 << 10,
        GoalEffectTriggered = 1 << 11,
        SelectedSkillConsumed = 1 << 12,
        NextTurnHazardBoostReserved = 1 << 13,
    }

    /// <summary>
    /// 1 件の解決済み行動とそのダメージ結果を表す。
    /// </summary>

    public sealed class BattleResolvedAction
    {
        public BattleResolvedActionType ActionType { get; init; } = BattleResolvedActionType.None;

        public BattleDamageKind? DamageKind { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public BattleDamageResolution DamageResolution { get; init; }

        public bool TargetsEnemy { get; init; }

        public BattleResolvedActionSemantics Semantics { get; init; } = BattleResolvedActionSemantics.None;

        public BattleSkillEffectSuccessResult SkillEffectSuccess { get; init; }

        public BattleSkillEffectEntryData SkillEffect { get; init; }

        public int SkillEffectIndex { get; init; } = -1;

        public EffectAnimationData EffectAnimation { get; init; }

        public int PlayerHealing { get; init; }

        public int PlayerGuardGain { get; init; }

        public int PlayerGuardAbsorbed { get; init; }

        public int SkillChargeGain { get; init; }

        public IReadOnlyList<BattleTemporaryStatModifier> TemporaryStatModifiers { get; init; } =
            Array.Empty<BattleTemporaryStatModifier>();

        public BattlePlayerAnimationCue? AnimationCue { get; init; }

        public IReadOnlyList<string> LogMessages { get; init; } = Array.Empty<string>();

        public bool HasSemantic(BattleResolvedActionSemantics semantic)
        {
            return (Semantics & semantic) == semantic;
        }
    }
}
