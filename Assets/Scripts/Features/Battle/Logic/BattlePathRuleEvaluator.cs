using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// パス入力の妥当性を pure C# で判定する。
    /// </summary>
    public class BattlePathRuleEvaluator
    {
        public BattlePathValidationError ValidateStart(BattleBoardState board, BattleGridPosition position)
        {
            if (board == null || !board.IsInside(position))
            {
                return BattlePathValidationError.OutOfBounds;
            }

            if (!board.TryGetCell(position, out _))
            {
                return BattlePathValidationError.MissingCell;
            }

            if (!board.StartPosition.HasValue || board.StartPosition.Value != position)
            {
                return BattlePathValidationError.InvalidStart;
            }

            return BattlePathValidationError.None;
        }

        public bool IsBacktrackCandidate(BattlePathDraft draft, BattleGridPosition candidate)
        {
            return draft != null &&
                   draft.PreviousPosition.HasValue &&
                   draft.PreviousPosition.Value == candidate;
        }

        public BattlePathValidationError ValidateAppend(
            BattleBoardState board,
            BattlePathDraft draft,
            BattleGridPosition candidate)
        {
            if (board == null || draft == null)
            {
                return BattlePathValidationError.OutOfBounds;
            }

            if (!draft.CurrentPosition.HasValue)
            {
                return ValidateStart(board, candidate);
            }

            if (!board.IsInside(candidate))
            {
                return BattlePathValidationError.OutOfBounds;
            }

            if (!board.TryGetCell(candidate, out _))
            {
                return BattlePathValidationError.MissingCell;
            }

            if (draft.HasReachedGoal)
            {
                return BattlePathValidationError.ExtendedAfterGoal;
            }

            var current = draft.CurrentPosition.Value;
            if (!current.IsAdjacent8Way(candidate))
            {
                return BattlePathValidationError.NotAdjacent;
            }

            if (draft.Contains(candidate))
            {
                return BattlePathValidationError.Revisit;
            }

            var newSegment = new BattlePathSegment(current, candidate);
            if (CrossesExistingSegments(draft, newSegment))
            {
                return BattlePathValidationError.CrossedSegment;
            }

            return BattlePathValidationError.None;
        }

        private static bool CrossesExistingSegments(BattlePathDraft draft, BattlePathSegment newSegment)
        {
            var segments = draft.Segments;
            for (var i = 0; i < segments.Count; i++)
            {
                var existing = segments[i];
                if (existing.SharesEndpointWith(newSegment))
                {
                    continue;
                }

                if (SegmentsIntersect(existing, newSegment))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SegmentsIntersect(BattlePathSegment left, BattlePathSegment right)
        {
            var a = left.From;
            var b = left.To;
            var c = right.From;
            var d = right.To;

            var o1 = Orientation(a, b, c);
            var o2 = Orientation(a, b, d);
            var o3 = Orientation(c, d, a);
            var o4 = Orientation(c, d, b);

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            if (o1 == 0 && OnSegment(a, c, b))
            {
                return true;
            }

            if (o2 == 0 && OnSegment(a, d, b))
            {
                return true;
            }

            if (o3 == 0 && OnSegment(c, a, d))
            {
                return true;
            }

            if (o4 == 0 && OnSegment(c, b, d))
            {
                return true;
            }

            return false;
        }

        private static int Orientation(BattleGridPosition from, BattleGridPosition via, BattleGridPosition to)
        {
            var value = ((long)(via.Y - from.Y) * (to.X - via.X)) -
                        ((long)(via.X - from.X) * (to.Y - via.Y));
            if (value == 0)
            {
                return 0;
            }

            return value > 0 ? 1 : 2;
        }

        private static bool OnSegment(BattleGridPosition from, BattleGridPosition candidate, BattleGridPosition to)
        {
            return candidate.X <= Max(from.X, to.X) &&
                   candidate.X >= Min(from.X, to.X) &&
                   candidate.Y <= Max(from.Y, to.Y) &&
                   candidate.Y >= Min(from.Y, to.Y);
        }

        private static int Min(int left, int right)
        {
            return left < right ? left : right;
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }
    }
}
