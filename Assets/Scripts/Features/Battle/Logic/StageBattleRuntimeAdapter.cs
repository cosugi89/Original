using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// StageData.Enemy.Patterns の定義を戦闘コアで扱う盤面と解決コンテキストへ変換する。
    /// BattleScene の本番経路から BattleDemoTurnScript 依存を外すための橋渡し。
    /// </summary>
    public static class StageBattleRuntimeAdapter
    {
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

        public static Layout CreateBoardLayout(StageBattlePatternData patternData)
        {
            var width = Math.Max(1, patternData?.Board?.Width ?? 5);
            var height = Math.Max(1, patternData?.Board?.Height ?? 6);
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

        public static BattleTurnContext CreateTurnContext(
            StageBattlePatternData patternData,
            int enemyDamage,
            bool hazardBoosted,
            BattleSkillSlotRuntime selectedSkill,
            int currentEnemyHp,
            int currentPlayerHp,
            int normalAttackDamage,
            int doubleAttackFollowUpDamage,
            int jumpAttackDamage,
            AttributeData playerAttackAttribute,
            AttributeData enemyAttackAttribute,
            IReadOnlyList<EquipmentAttributeModifierData> enemyDefenseAttributeModifiers,
            IReadOnlyList<EquipmentAttributeModifierData> playerDefenseAttributeModifiers)
        {
            return new BattleTurnContext
            {
                CurrentEnemyHp = currentEnemyHp,
                CurrentPlayerHp = currentPlayerHp,
                NormalAttackDamage = normalAttackDamage,
                DoubleAttackFollowUpDamage = doubleAttackFollowUpDamage,
                JumpAttackDamage = jumpAttackDamage,
                EnemyActionDamage = ResolveEnemyActionDamage(patternData, enemyDamage),
                PlayerAttackAttribute = playerAttackAttribute,
                EnemyAttackAttribute = enemyAttackAttribute,
                EnemyDefenseAttributeModifiers = enemyDefenseAttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>(),
                PlayerDefenseAttributeModifiers = playerDefenseAttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>(),
                HazardBoosted = hazardBoosted,
                EnemyAction = patternData?.EnemyAction ?? BattleEnemyActionType.NormalAttack,
                SelectedSkill = selectedSkill,
            };
        }

        public static BattleTurnResolutionReport ResolveWithRuntime(
            StageBattlePatternData patternData,
            bool hazardBoosted,
            BattleSkillSlotRuntime selectedSkill,
            int currentEnemyHp,
            int currentPlayerHp,
            int normalAttackDamage,
            int doubleAttackFollowUpDamage,
            int jumpAttackDamage,
            AttributeData playerAttackAttribute,
            AttributeData enemyAttackAttribute,
            IReadOnlyList<EquipmentAttributeModifierData> enemyDefenseAttributeModifiers,
            IReadOnlyList<EquipmentAttributeModifierData> playerDefenseAttributeModifiers)
        {
            var layout = CreateBoardLayout(patternData);
            var tracer = new BattlePathTracer(layout.Board);
            var traceResult = TraceFallbackPath(tracer, layout.FallbackTracePositions);
            if (traceResult != null)
            {
                return traceResult;
            }

            var context = CreateTurnContext(
                patternData,
                enemyDamage: 80,
                hazardBoosted,
                selectedSkill,
                currentEnemyHp,
                currentPlayerHp,
                normalAttackDamage,
                doubleAttackFollowUpDamage,
                jumpAttackDamage,
                playerAttackAttribute,
                enemyAttackAttribute,
                enemyDefenseAttributeModifiers,
                playerDefenseAttributeModifiers);

            var path = layout.Board.BuildCellPath(tracer.Draft.Positions);
            var report = BattleTurnResolver.Resolve(path, context);
            report.AddLog($"Stage runtime path length: {tracer.Draft.Count}");
            return report;
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
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    yield return new BattleGridPosition(origin.X + dx, origin.Y + dy);
                }
            }
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

        private static BattleTurnResolutionReport TraceFallbackPath(
            BattlePathTracer tracer,
            IReadOnlyList<BattleGridPosition> tracePositions)
        {
            if (tracePositions == null || tracePositions.Count == 0)
            {
                return CreateTraceFailureReport("Stage fallback trace path is empty.");
            }

            for (var i = 0; i < tracePositions.Count; i++)
            {
                var result = tracer.TryTrace(tracePositions[i]);
                if (!result.IsAccepted)
                {
                    return CreateTraceFailureReport(
                        $"Stage trace failed at step {i} ({tracePositions[i]}): {result.ValidationError}");
                }
            }

            var releaseResult = tracer.TryRelease();
            if (releaseResult.Status != BattlePathTraceStatus.Confirmed)
            {
                return CreateTraceFailureReport($"Stage trace release failed: {releaseResult.ValidationError}");
            }

            return null;
        }

        private static BattleTurnResolutionReport CreateTraceFailureReport(string message)
        {
            var report = new BattleTurnResolutionReport();
            report.AddLog(message);
            return report;
        }

        private static int ResolveEnemyActionDamage(StageBattlePatternData patternData, int enemyDamage)
        {
            if (enemyDamage <= 0)
            {
                return 0;
            }

            var multiplier = patternData != null ? Math.Max(0f, patternData.EnemyActionDamageMultiplier) : 0f;
            return RoundToInt(enemyDamage * multiplier);
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

        private static int RoundToInt(float value)
        {
            return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}
