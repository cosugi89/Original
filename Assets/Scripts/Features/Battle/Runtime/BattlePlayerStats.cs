using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Const;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// プレイヤー固有の戦闘ステータスを表す。
    /// </summary>
    public sealed class BattlePlayerStats : BattleCombatantStats
    {
        public BattlePlayerStats(
            int baseAttackPower,
            float normalAttackMultiplier = 1f,
            float doubleAttackFollowUpMultiplier = 1.25f,
            float jumpAttackMultiplier = 1.5f,
            IReadOnlyDictionary<BattleSkillParameterType, int> parameterValues = null,
            IReadOnlyDictionary<string, float> namedValues = null)
            : base(parameterValues, namedValues)
        {
            SetParameterValue(BattleSkillParameterType.Attack, Math.Max(1, baseAttackPower));
            SetNamedValue(BattleDamageValueKeys.NormalAttackMultiplier, Math.Max(0f, normalAttackMultiplier));
            SetNamedValue(BattleDamageValueKeys.DoubleAttackFollowUpMultiplier, Math.Max(0f, doubleAttackFollowUpMultiplier));
            SetNamedValue(BattleDamageValueKeys.JumpAttackMultiplier, Math.Max(0f, jumpAttackMultiplier));
            SetNamedValue(
                BattleDamageValueKeys.FinalDamageMultiplier,
                GetNamedValue(BattleDamageValueKeys.FinalDamageMultiplier, fallbackValue: 1f));
        }

        public int BaseAttackPower => GetParameterValue(BattleSkillParameterType.Attack, fallbackValue: 1);

        public float NormalAttackMultiplier => GetNamedValue(BattleDamageValueKeys.NormalAttackMultiplier, fallbackValue: 1f);

        public float DoubleAttackFollowUpMultiplier =>
            GetNamedValue(BattleDamageValueKeys.DoubleAttackFollowUpMultiplier, fallbackValue: 1.25f);

        public float JumpAttackMultiplier => GetNamedValue(BattleDamageValueKeys.JumpAttackMultiplier, fallbackValue: 1.5f);
    }
}

