using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 1 ターン中に変化しない盤面状態。
    /// </summary>
    public class BattleBoardState
    {
        private readonly Dictionary<BattleGridPosition, BattleCellState> _cells = new();
        private BattleGridPosition? _startPosition;

        public BattleBoardState(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyCollection<BattleCellState> Cells => _cells.Values;

        public BattleGridPosition? StartPosition => _startPosition;

        public void SetCell(BattleCellState cell)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            if (!IsInside(cell.Position))
            {
                throw new ArgumentOutOfRangeException(nameof(cell), $"Cell position {cell.Position} is outside the board.");
            }

            _cells[cell.Position] = cell;
            if (cell.NodeType == BattleNodeType.Start)
            {
                _startPosition = cell.Position;
            }
        }

        public bool IsInside(BattleGridPosition position)
        {
            return position.X >= 0 &&
                   position.X < Width &&
                   position.Y >= 0 &&
                   position.Y < Height;
        }

        public bool TryGetCell(BattleGridPosition position, out BattleCellState cell)
        {
            return _cells.TryGetValue(position, out cell);
        }

        public BattleCellState GetCell(BattleGridPosition position)
        {
            if (!TryGetCell(position, out var cell))
            {
                throw new KeyNotFoundException($"Cell at {position} was not found.");
            }

            return cell;
        }

        public bool IsGoal(BattleGridPosition position)
        {
            return TryGetCell(position, out var cell) && cell.IsGoal;
        }

        public List<BattleGridPosition> GetGoalPositions()
        {
            var result = new List<BattleGridPosition>();
            foreach (var pair in _cells)
            {
                if (pair.Value.IsGoal)
                {
                    result.Add(pair.Key);
                }
            }

            return result;
        }

        public List<BattleNodeType> BuildNodePath(IReadOnlyList<BattleGridPosition> positions)
        {
            var result = new List<BattleNodeType>();
            if (positions == null)
            {
                return result;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                result.Add(GetCell(positions[i]).NodeType);
            }

            return result;
        }
    }
}
