using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// StageData.Enemy.Patterns の定義を戦闘コアで扱う盤面と解決コンテキストへ変換する。
    /// </summary>
    public static class BattleStageRuntimeAdapter
    {
        private static readonly BattleCombatantStatsFactory CombatantStatsFactory = new();

        /// <summary>
        /// 生成した盤面とフォールバック用パスを一組で保持する。
        /// </summary>

        public sealed class Layout
        {
            public Layout(BattleBoardState board, IReadOnlyList<BattleGridPosition> fallbackTracePositions)
            {
                Board = board;
                FallbackTracePositions = fallbackTracePositions ?? Array.Empty<BattleGridPosition>();
            }

            public BattleBoardState Board { get; }

            public IReadOnlyList<BattleGridPosition> FallbackTracePositions { get; }
        }

        /// <summary>
        /// Pattern master の定義から、1 ターンぶんの論理盤面と fallback ルートを構築する。
        /// </summary>
        public static Layout CreateBoardLayout(StageBattlePatternData patternData)
        {
            var width = Math.Max(1, patternData?.Board?.Width ?? 7);
            var height = Math.Max(1, patternData?.Board?.Height ?? 8);
            var startPosition = ResolveStartPosition(patternData, width, height);

            var board = new BattleBoardState(width, height);
            FillEmptyBoard(board);
            board.SetCell(new BattleCellState
            {
                Position = startPosition,
                NodeType = BattleNodeType.Start,
                HazardGroupId = -1,
            });

            var explicitPositions = new HashSet<BattleGridPosition> { startPosition };
            var placements = patternData?.CellPlacements;
            if (placements != null)
            {
                for (var i = 0; i < placements.Count; i++)
                {
                    var placement = placements[i];
                    if (placement?.Position == null)
                    {
                        continue;
                    }

                    var position = new BattleGridPosition(placement.Position.X, placement.Position.Y);
                    if (!board.IsInside(position))
                    {
                        continue;
                    }

                    var nodeType = placement.NodeType;
                    if (position == startPosition)
                    {
                        nodeType = BattleNodeType.Start;
                    }

                    board.SetCell(new BattleCellState
                    {
                        Position = position,
                        NodeType = nodeType,
                        HazardGroupId = -1,
                    });
                    explicitPositions.Add(position);
                }
            }

            AssignConnectedHazardGroups(board);
            return new Layout(board, BuildFallbackTracePositions(board, explicitPositions));
        }

        /// <summary>
        /// 現在の player/enemy 状態と pattern 情報から、ターン解決用コンテキストを組み立てる。
        /// </summary>
        public static BattleTurnContext CreateTurnContext(
            StageBattlePatternData patternData,
            BattleStatBlockData enemyStatBlock,
            bool hazardBoosted,
            SkillSlotRuntime selectedSkill,
            int currentEnemyHp,
            int currentPlayerHp,
            BattlePlayerStats playerStats,
            AttributeData playerAttackAttribute,
            AttributeData enemyAttackAttribute,
            IReadOnlyList<EquipmentAttributeModifierData> enemyDefenseAttributeModifiers,
            IReadOnlyList<EquipmentAttributeModifierData> playerDefenseAttributeModifiers)
        {
            return new BattleTurnContext
            {
                CurrentEnemyHp = currentEnemyHp,
                CurrentPlayerHp = currentPlayerHp,
                PlayerStats = playerStats ?? new BattlePlayerStats(baseAttackPower: 1),
                EnemyStats = CombatantStatsFactory.CreateCombatantStats(enemyStatBlock, currentEnemyHp),
                EnemyActionMultiplier = ResolveEnemyActionMultiplier(patternData),
                EnemyActionDamageFormula = patternData?.EnemyActionDamageFormula,
                PlayerAttackAttribute = playerAttackAttribute,
                EnemyAttackAttribute = enemyAttackAttribute,
                EnemyDefenseAttributeModifiers = enemyDefenseAttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>(),
                PlayerDefenseAttributeModifiers = playerDefenseAttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>(),
                HazardBoosted = hazardBoosted,
                EnemyAction = patternData?.EnemyAction ?? BattleEnemyActionType.NormalAttack,
                SelectedSkill = selectedSkill,
            };
        }

        private static BattleGridPosition ResolveStartPosition(StageBattlePatternData patternData, int width, int height)
        {
            var start = patternData?.Board?.StartPosition;
            var x = start != null ? Clamp(start.X, 0, width - 1) : width / 2;
            var y = start != null ? Clamp(start.Y, 0, height - 1) : height - 1;
            return new BattleGridPosition(x, y);
        }

        private static void FillEmptyBoard(BattleBoardState board)
        {
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    board.SetCell(new BattleCellState
                    {
                        Position = new BattleGridPosition(x, y),
                        NodeType = BattleNodeType.Empty,
                        HazardGroupId = -1,
                    });
                }
            }
        }

        private static IReadOnlyList<BattleGridPosition> BuildFallbackTracePositions(
            BattleBoardState board,
            HashSet<BattleGridPosition> explicitPositions)
        {
            if (board.StartPosition == null)
            {
                return Array.Empty<BattleGridPosition>();
            }

            var start = board.StartPosition.Value;
            var queue = new Queue<BattleGridPosition>();
            var visited = new HashSet<BattleGridPosition> { start };
            var previous = new Dictionary<BattleGridPosition, BattleGridPosition>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (board.IsGoal(current))
                {
                    return ReconstructPath(start, current, previous);
                }

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!board.IsInside(neighbor) ||
                        visited.Contains(neighbor) ||
                        !explicitPositions.Contains(neighbor))
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }

            return Array.Empty<BattleGridPosition>();
        }

        private static IEnumerable<BattleGridPosition> GetNeighbors(BattleGridPosition origin)
        {
            yield return new BattleGridPosition(origin.X + 1, origin.Y);
            yield return new BattleGridPosition(origin.X - 1, origin.Y);
            yield return new BattleGridPosition(origin.X, origin.Y + 1);
            yield return new BattleGridPosition(origin.X, origin.Y - 1);
        }

        private static void AssignConnectedHazardGroups(BattleBoardState board)
        {
            if (board == null)
            {
                return;
            }

            var visited = new HashSet<BattleGridPosition>();
            var nextGroupId = 0;

            foreach (var cell in board.Cells)
            {
                if (cell == null)
                {
                    continue;
                }

                if (!cell.IsHazard)
                {
                    cell.HazardGroupId = -1;
                    continue;
                }

                if (visited.Contains(cell.Position))
                {
                    continue;
                }

                var queue = new Queue<BattleGridPosition>();
                queue.Enqueue(cell.Position);
                visited.Add(cell.Position);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    if (!board.TryGetCell(current, out var currentCell) || !currentCell.IsHazard)
                    {
                        continue;
                    }

                    currentCell.HazardGroupId = nextGroupId;

                    var orthogonalNeighbors = new[]
                    {
                        new BattleGridPosition(current.X + 1, current.Y),
                        new BattleGridPosition(current.X - 1, current.Y),
                        new BattleGridPosition(current.X, current.Y + 1),
                        new BattleGridPosition(current.X, current.Y - 1),
                    };

                    for (var i = 0; i < orthogonalNeighbors.Length; i++)
                    {
                        var neighbor = orthogonalNeighbors[i];
                        if (!board.IsInside(neighbor) ||
                            visited.Contains(neighbor) ||
                            !board.TryGetCell(neighbor, out var neighborCell) ||
                            !neighborCell.IsHazard)
                        {
                            continue;
                        }

                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }

                nextGroupId++;
            }
        }

        private static IReadOnlyList<BattleGridPosition> ReconstructPath(
            BattleGridPosition start,
            BattleGridPosition goal,
            IReadOnlyDictionary<BattleGridPosition, BattleGridPosition> previous)
        {
            var path = new List<BattleGridPosition> { goal };
            var current = goal;
            while (current != start && previous.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private static float ResolveEnemyActionMultiplier(StageBattlePatternData patternData)
        {
            return patternData != null ? Math.Max(0f, patternData.EnemyActionDamageMultiplier) : 1f;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}

