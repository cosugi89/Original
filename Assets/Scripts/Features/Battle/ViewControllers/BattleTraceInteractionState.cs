using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// トレース中パスと確定パスの状態、および待機タスクを管理する。
    /// </summary>
    public sealed class BattleTraceInteractionState
    {
        private BattlePathTracer _tracer;
        private bool _isTracing;
        private List<BattleGridPosition> _confirmedPath = new();
        private UniTaskCompletionSource<IReadOnlyList<BattleGridPosition>> _pathConfirmedTcs;

        public BattlePathTracer Tracer => _tracer;

        public bool IsTracing => _isTracing;

        public bool HasConfirmedPath => _confirmedPath.Count > 0;

        public IReadOnlyList<BattleGridPosition> ConfirmedPath => _confirmedPath;

        public bool CanConfirmCurrentTrace
        {
            get
            {
                if (_tracer != null && _tracer.Draft.HasPath)
                {
                    return _tracer.Draft.CanConfirm;
                }

                return HasConfirmedPath;
            }
        }

        public IReadOnlyList<BattleGridPosition> CurrentTracePath
        {
            get
            {
                if (_tracer != null && _tracer.Draft.HasPath)
                {
                    return _tracer.Draft.Positions;
                }

                return _confirmedPath;
            }
        }

        public void Bind(BattlePathTracer tracer)
        {
            Unbind();
            _tracer = tracer;
            Reset(clearConfirmedPath: true);
        }

        public void Unbind()
        {
            _tracer = null;
            _isTracing = false;
            _confirmedPath = new List<BattleGridPosition>();
            CancelConfirmedPathAwait();
        }

        public void Reset(bool clearConfirmedPath)
        {
            _isTracing = false;
            _tracer?.Reset();

            if (clearConfirmedPath)
            {
                _confirmedPath = new List<BattleGridPosition>();
            }
        }

        public UniTask<IReadOnlyList<BattleGridPosition>> WaitForConfirmedPathAsync(CancellationToken cancellationToken = default)
        {
            if (HasConfirmedPath)
            {
                return UniTask.FromResult<IReadOnlyList<BattleGridPosition>>(new List<BattleGridPosition>(_confirmedPath));
            }

            _pathConfirmedTcs ??= new UniTaskCompletionSource<IReadOnlyList<BattleGridPosition>>();
            return cancellationToken.CanBeCanceled
                ? _pathConfirmedTcs.Task.AttachExternalCancellation(cancellationToken)
                : _pathConfirmedTcs.Task;
        }

        public BattlePathTraceResult BeginTrace(BattleGridPosition position, bool clearConfirmedPathOnNewTrace)
        {
            if (_tracer == null)
            {
                return null;
            }

            _isTracing = true;
            _tracer.Reset();
            if (clearConfirmedPathOnNewTrace)
            {
                _confirmedPath = new List<BattleGridPosition>();
            }

            return _tracer.TryTrace(position);
        }

        public BattlePathTraceResult AppendTrace(BattleGridPosition position)
        {
            if (!_isTracing || _tracer == null)
            {
                return null;
            }

            return _tracer.TryTrace(position);
        }

        public BattlePathTraceResult FinalizeTrace()
        {
            _isTracing = false;
            if (_tracer == null)
            {
                return null;
            }

            var result = _tracer.TryRelease();
            if (result.Status == BattlePathTraceStatus.Confirmed)
            {
                _confirmedPath = _tracer.Draft.CreateSnapshot();
                _pathConfirmedTcs?.TrySetResult(new List<BattleGridPosition>(_confirmedPath));
                _pathConfirmedTcs = null;
            }
            else
            {
                _confirmedPath = new List<BattleGridPosition>();
            }

            return result;
        }

        public void CancelConfirmedPathAwait()
        {
            _pathConfirmedTcs?.TrySetCanceled();
            _pathConfirmedTcs = null;
        }
    }
}
