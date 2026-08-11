using System;
using Assets.Scripts.Data.DTO;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// ターンをまたいで有効な一時ステータス補正。
    /// </summary>
    public sealed class BattleTemporaryStatModifier
    {
        public BattleTemporaryStatModifier(
            bool targetsPlayer,
            BattleStatModifierData modifier,
            int remainingTurns)
        {
            TargetsPlayer = targetsPlayer;
            Modifier = modifier;
            RemainingTurns = Math.Max(0, remainingTurns);
        }

        public bool TargetsPlayer { get; }

        public BattleStatModifierData Modifier { get; }

        public int RemainingTurns { get; private set; }

        public bool IsExpired => RemainingTurns <= 0 || Modifier == null;

        public void ConsumeTurn()
        {
            RemainingTurns = Math.Max(0, RemainingTurns - 1);
        }
    }
}
