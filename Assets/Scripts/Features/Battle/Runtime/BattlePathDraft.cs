using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 入力中の仮パス状態。
    /// </summary>
    public class BattlePathDraft
    {
        private readonly List<BattleGridPosition> _positions = new();
        private readonly HashSet<BattleGridPosition> _visited = new();
        private readonly List<BattlePathSegment> _segments = new();
        private int _goalIndex = -1;

        public IReadOnlyList<BattleGridPosition> Positions => _positions;

        public IReadOnlyList<BattlePathSegment> Segments => _segments;

        public int Count => _positions.Count;

        public bool HasPath => _positions.Count > 0;

        public bool HasReachedGoal => _goalIndex >= 0;

        public bool CanConfirm => HasReachedGoal;

        public BattleGridPosition? CurrentPosition =>
            _positions.Count > 0 ? _positions[_positions.Count - 1] : (BattleGridPosition?)null;

        public BattleGridPosition? PreviousPosition =>
            _positions.Count > 1 ? _positions[_positions.Count - 2] : (BattleGridPosition?)null;

        public BattleGridPosition? GoalPosition =>
            _goalIndex >= 0 ? _positions[_goalIndex] : (BattleGridPosition?)null;

        public bool Contains(BattleGridPosition position)
        {
            return _visited.Contains(position);
        }

        public void Reset()
        {
            _positions.Clear();
            _visited.Clear();
            _segments.Clear();
            _goalIndex = -1;
        }

        public void StartAt(BattleGridPosition startPosition, bool isGoal)
        {
            Reset();
            _positions.Add(startPosition);
            _visited.Add(startPosition);
            if (isGoal)
            {
                _goalIndex = 0;
            }
        }

        public void Append(BattleGridPosition position, bool isGoal)
        {
            if (_positions.Count == 0)
            {
                StartAt(position, isGoal);
                return;
            }

            var last = _positions[_positions.Count - 1];
            _segments.Add(new BattlePathSegment(last, position));
            _positions.Add(position);
            _visited.Add(position);

            if (isGoal)
            {
                _goalIndex = _positions.Count - 1;
            }
        }

        public bool Backtrack()
        {
            if (_positions.Count <= 1)
            {
                return false;
            }

            var removed = _positions[_positions.Count - 1];
            _positions.RemoveAt(_positions.Count - 1);
            _visited.Remove(removed);

            if (_segments.Count > 0)
            {
                _segments.RemoveAt(_segments.Count - 1);
            }

            if (_goalIndex >= _positions.Count)
            {
                _goalIndex = -1;
            }

            return true;
        }

        public List<BattleGridPosition> CreateSnapshot()
        {
            return new List<BattleGridPosition>(_positions);
        }
    }
}
