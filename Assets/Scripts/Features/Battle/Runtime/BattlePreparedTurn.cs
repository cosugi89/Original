using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// ターン開始時点で確定した敵パターンと盤面情報を持つ。
    /// </summary>
    public sealed class BattlePreparedTurn
    {
        public BattlePreparedTurn(
            StageBattlePatternData pattern,
            BattleBoardState board,
            IReadOnlyList<BattleGridPosition> fallbackTracePositions)
        {
            Pattern = pattern;
            Board = board;
            FallbackTracePositions = fallbackTracePositions ?? Array.Empty<BattleGridPosition>();
        }

        public StageBattlePatternData Pattern { get; }

        public BattleBoardState Board { get; }

        public IReadOnlyList<BattleGridPosition> FallbackTracePositions { get; }
    }
}

