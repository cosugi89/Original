using System;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// ドラッグ入力を仮パスへ反映する。
    /// </summary>
    public class BattlePathTracer
    {
        public BattlePathTracer(BattleBoardState board, BattlePathRuleEvaluator ruleEvaluator = null)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            RuleEvaluator = ruleEvaluator ?? new BattlePathRuleEvaluator();
            Draft = new BattlePathDraft();
        }

        public BattleBoardState Board { get; }

        public BattlePathRuleEvaluator RuleEvaluator { get; }

        public BattlePathDraft Draft { get; }

        /// <summary>
        /// 1 マス分の入力を仮パスへ反映し、開始/追加/巻き戻し/拒否の結果を返す。
        /// </summary>
        public BattlePathTraceResult TryTrace(BattleGridPosition position)
        {
            if (!Draft.HasPath)
            {
                return TryStart(position);
            }

            if (Draft.CurrentPosition.HasValue && Draft.CurrentPosition.Value == position)
            {
                return CreateResult(BattlePathTraceStatus.Ignored, BattlePathValidationError.None, false);
            }

            if (Draft.HasReachedGoal)
            {
                return CreateResult(BattlePathTraceStatus.Invalid, BattlePathValidationError.ExtendedAfterGoal, false);
            }

            if (RuleEvaluator.IsBacktrackCandidate(Draft, position))
            {
                Draft.Backtrack();
                return CreateResult(BattlePathTraceStatus.Backtracked, BattlePathValidationError.None, false);
            }

            var validationError = RuleEvaluator.ValidateAppend(Board, Draft, position);
            if (validationError != BattlePathValidationError.None)
            {
                return CreateResult(BattlePathTraceStatus.Invalid, validationError, false);
            }

            var isGoal = Board.IsGoal(position);
            Draft.Append(position, isGoal);
            return CreateResult(BattlePathTraceStatus.Appended, BattlePathValidationError.None, isGoal);
        }

        /// <summary>
        /// 入力終了時に仮パスを確定し、Goal 未到達なら破棄する。
        /// </summary>
        public BattlePathTraceResult TryRelease()
        {
            if (!Draft.HasPath)
            {
                return CreateResult(BattlePathTraceStatus.Ignored, BattlePathValidationError.None, false);
            }

            if (Draft.CanConfirm)
            {
                return CreateResult(BattlePathTraceStatus.Confirmed, BattlePathValidationError.None, false);
            }

            Draft.Reset();
            return CreateResult(BattlePathTraceStatus.ReleasedWithoutGoal, BattlePathValidationError.GoalNotReached, false);
        }

        /// <summary>
        /// 現在の仮パスを完全にリセットする。
        /// </summary>
        public void Reset()
        {
            Draft.Reset();
        }

        private BattlePathTraceResult TryStart(BattleGridPosition position)
        {
            var validationError = RuleEvaluator.ValidateStart(Board, position);
            if (validationError != BattlePathValidationError.None)
            {
                return CreateResult(BattlePathTraceStatus.Invalid, validationError, false);
            }

            var isGoal = Board.IsGoal(position);
            Draft.StartAt(position, isGoal);
            return CreateResult(BattlePathTraceStatus.Started, BattlePathValidationError.None, isGoal);
        }

        private BattlePathTraceResult CreateResult(
            BattlePathTraceStatus status,
            BattlePathValidationError validationError,
            bool goalReachedNow)
        {
            return new BattlePathTraceResult
            {
                Status = status,
                ValidationError = validationError,
                GoalReachedNow = goalReachedNow,
                CanConfirm = Draft.CanConfirm,
                PathLength = Draft.Count,
            };
        }
    }
}
