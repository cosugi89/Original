using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// 既存デモのノード列を、暫定的な 5x5 盤面と座標列へ変換する。
    /// 実グリッド UI が入るまでの橋渡しとして使う。
    /// </summary>
    public static class BattleDemoBoardFactory
    {
        public sealed class Layout
        {
            public Layout(BattleBoardState board, IReadOnlyList<BattleGridPosition> tracePositions)
            {
                Board = board;
                TracePositions = tracePositions;
            }

            public BattleBoardState Board { get; }

            public IReadOnlyList<BattleGridPosition> TracePositions { get; }
        }

        public static Layout Create(BattleDemoTurnScript turnScript, int width = 5, int height = 6)
        {
            var board = new BattleBoardState(width, height);
            FillEmptyBoard(board);

            var tracePositions = new List<BattleGridPosition>();
            var demoPath = turnScript?.Path;
            if (demoPath == null || demoPath.Count == 0)
            {
                return new Layout(board, tracePositions);
            }

            var route = CreateCanonicalRoute(width, height, new BattleGridPosition(width / 2, height - 1));
            if (demoPath.Count > route.Count)
            {
                throw new InvalidOperationException(
                    $"Demo path length {demoPath.Count} exceeds the generated route capacity {route.Count}.");
            }

            var nextHazardGroupId = 0;
            var previousWasHazard = false;

            for (var i = 0; i < demoPath.Count; i++)
            {
                var position = route[i];
                tracePositions.Add(position);

                var runtimeNode = demoPath[i].ToRuntime();
                var cell = new BattleCellState
                {
                    Position = position,
                    NodeType = runtimeNode,
                    HazardGroupId = -1,
                };

                if (cell.IsHazard)
                {
                    if (!previousWasHazard)
                    {
                        nextHazardGroupId++;
                    }

                    cell.HazardGroupId = nextHazardGroupId;
                    previousWasHazard = true;
                }
                else
                {
                    previousWasHazard = false;
                }

                board.SetCell(cell);
            }

            return new Layout(board, tracePositions);
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

        private static List<BattleGridPosition> CreateCanonicalRoute(int width, int height, BattleGridPosition startPosition)
        {
            var route = new List<BattleGridPosition>(width * height);
            var visited = new bool[width, height];
            if (!TryBuildRoute(width, height, startPosition, visited, route))
            {
                throw new InvalidOperationException("Failed to build a canonical demo route for the board.");
            }

            return route;
        }

        private static bool TryBuildRoute(
            int width,
            int height,
            BattleGridPosition current,
            bool[,] visited,
            List<BattleGridPosition> route)
        {
            visited[current.X, current.Y] = true;
            route.Add(current);

            if (route.Count == width * height)
            {
                return true;
            }

            var neighbors = GetOrderedNeighbors(width, height, current, visited);
            for (var i = 0; i < neighbors.Count; i++)
            {
                if (TryBuildRoute(width, height, neighbors[i], visited, route))
                {
                    return true;
                }
            }

            visited[current.X, current.Y] = false;
            route.RemoveAt(route.Count - 1);
            return false;
        }

        private static List<BattleGridPosition> GetOrderedNeighbors(
            int width,
            int height,
            BattleGridPosition current,
            bool[,] visited)
        {
            var candidates = new List<BattleGridPosition>();
            TryAddNeighbor(width, height, current.X + 1, current.Y, visited, candidates);
            TryAddNeighbor(width, height, current.X - 1, current.Y, visited, candidates);
            TryAddNeighbor(width, height, current.X, current.Y + 1, visited, candidates);
            TryAddNeighbor(width, height, current.X, current.Y - 1, visited, candidates);

            candidates.Sort((left, right) =>
            {
                var leftDegree = CountAvailableMoves(width, height, left, visited);
                var rightDegree = CountAvailableMoves(width, height, right, visited);
                var degreeCompare = leftDegree.CompareTo(rightDegree);
                if (degreeCompare != 0)
                {
                    return degreeCompare;
                }

                var rowCompare = right.Y.CompareTo(left.Y);
                if (rowCompare != 0)
                {
                    return rowCompare;
                }

                return left.X.CompareTo(right.X);
            });

            return candidates;
        }

        private static int CountAvailableMoves(int width, int height, BattleGridPosition position, bool[,] visited)
        {
            var count = 0;
            if (CanVisit(width, height, position.X + 1, position.Y, visited)) count++;
            if (CanVisit(width, height, position.X - 1, position.Y, visited)) count++;
            if (CanVisit(width, height, position.X, position.Y + 1, visited)) count++;
            if (CanVisit(width, height, position.X, position.Y - 1, visited)) count++;
            return count;
        }

        private static void TryAddNeighbor(
            int width,
            int height,
            int x,
            int y,
            bool[,] visited,
            List<BattleGridPosition> candidates)
        {
            if (CanVisit(width, height, x, y, visited))
            {
                candidates.Add(new BattleGridPosition(x, y));
            }
        }

        private static bool CanVisit(int width, int height, int x, int y, bool[,] visited)
        {
            return x >= 0 &&
                   x < width &&
                   y >= 0 &&
                   y < height &&
                   !visited[x, y];
        }
    }
}
