using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Presentation
{
    /// <summary>
    /// セル View の pointer 入力を BattlePathTracer へ流し込む。
    /// </summary>
    public class BattleTraceInputHandler : MonoBehaviour
    {
        [SerializeField] private bool clearConfirmedPathOnNewTrace = true;

        private BattleBoardController _boardController;
        private BattlePathTracer _tracer;
        private BattleTraceLineView _traceLineView;
        private bool _isTracing;
        private List<BattleGridPosition> _confirmedPath = new();
        private UniTaskCompletionSource<IReadOnlyList<BattleGridPosition>> _pathConfirmedTcs;

        public event Action<IReadOnlyList<BattleGridPosition>> PathConfirmed;
        public event Action<BattlePathTraceResult> TraceUpdated;
        public event Action<BattlePathTraceResult> TraceRejected;

        public bool HasConfirmedPath => _confirmedPath.Count > 0;

        public IReadOnlyList<BattleGridPosition> ConfirmedPath => _confirmedPath;

        public bool HasActiveTrace => _tracer != null && _tracer.Draft.HasPath;

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

        /// <summary>
        /// Goal 到達済みの確定パスが得られるまで待機する。
        /// </summary>
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

        /// <summary>
        /// 盤面 View と tracer を接続し、以後の pointer 入力をパス追跡へ流す。
        /// </summary>
        public void Bind(BattleBoardController boardController, BattlePathTracer tracer, BattleTraceLineView traceLineView = null)
        {
            Unbind();

            _boardController = boardController;
            _tracer = tracer;
            _traceLineView = traceLineView;

            if (_boardController != null)
            {
                _boardController.CellPointerDown += HandleCellPointerDown;
                _boardController.CellPointerEnter += HandleCellPointerEnter;
                _boardController.CellPointerUp += HandleCellPointerUp;
            }

            ResetState(clearConfirmedPath: true);
            Debug.Log($"[BattleTrace] Bind 完了 Board={_boardController?.Board?.Width}x{_boardController?.Board?.Height} Start={_boardController?.Board?.StartPosition}");
        }

        public void Unbind()
        {
            if (_boardController != null)
            {
                _boardController.CellPointerDown -= HandleCellPointerDown;
                _boardController.CellPointerEnter -= HandleCellPointerEnter;
                _boardController.CellPointerUp -= HandleCellPointerUp;
            }

            _boardController = null;
            _tracer = null;
            _traceLineView = null;
            _isTracing = false;
            _confirmedPath = new List<BattleGridPosition>();
            CancelConfirmedPathAwait();
        }

        public void ResetState(bool clearConfirmedPath)
        {
            _isTracing = false;
            _tracer?.Reset();

            if (clearConfirmedPath)
            {
                _confirmedPath = new List<BattleGridPosition>();
            }

            RefreshVisuals();
        }

        private void Update()
        {
            if (_isTracing && Input.GetMouseButtonUp(0))
            {
                FinalizeTrace();
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// Start 入力で新しいトレースを開始し、最初のマスを path draft に反映する。
        /// </summary>
        private void HandleCellPointerDown(BattleGridPosition position, UnityEngine.EventSystems.PointerEventData _)
        {
            if (_tracer == null)
            {
                Debug.LogWarning("[BattleTrace] tracer が未設定のため PointerDown を処理できません。");
                return;
            }

            _isTracing = true;
            _tracer.Reset();
            if (clearConfirmedPathOnNewTrace)
            {
                _confirmedPath = new List<BattleGridPosition>();
            }

            Debug.Log($"[BattleTrace] Trace 開始 Input={position} ConfirmedCleared={clearConfirmedPathOnNewTrace}");
            ProcessTrace(position);
        }

        /// <summary>
        /// ドラッグ中に入った隣接マスを順次 tracer へ渡す。
        /// </summary>
        private void HandleCellPointerEnter(BattleGridPosition position, UnityEngine.EventSystems.PointerEventData _)
        {
            if (!_isTracing || _tracer == null)
            {
                return;
            }

            ProcessTrace(position);
        }

        /// <summary>
        /// ドラッグ終了時に現在の仮パスを確定判定へ進める。
        /// </summary>
        private void HandleCellPointerUp(BattleGridPosition _, UnityEngine.EventSystems.PointerEventData __)
        {
            if (_isTracing)
            {
                FinalizeTrace();
            }
        }

        /// <summary>
        /// 1 マス分の入力を tracer に適用し、受理/拒否結果をイベントと見た目へ反映する。
        /// </summary>
        private void ProcessTrace(BattleGridPosition position)
        {
            var result = _tracer.TryTrace(position);
            if (result.IsAccepted)
            {
                Debug.Log($"[BattleTrace] Accepted Status={result.Status} Input={position} PathLength={result.PathLength} CanConfirm={result.CanConfirm} Path={FormatPath(_tracer.Draft.Positions)}");
                TraceUpdated?.Invoke(result);
                RefreshVisuals();
                return;
            }

            Debug.LogWarning($"[BattleTrace] Rejected Status={result.Status} Input={position} Error={result.ValidationError} PathLength={result.PathLength} Path={FormatPath(_tracer.Draft.Positions)}");
            TraceRejected?.Invoke(result);
            RefreshVisuals();
        }

        /// <summary>
        /// 指離し時に Goal 到達済みなら確定し、未到達なら仮パスを破棄する。
        /// </summary>
        private void FinalizeTrace()
        {
            _isTracing = false;
            if (_tracer == null)
            {
                return;
            }

            var result = _tracer.TryRelease();
            TraceUpdated?.Invoke(result);
            if (result.Status == BattlePathTraceStatus.Confirmed)
            {
                _confirmedPath = _tracer.Draft.CreateSnapshot();
                Debug.Log($"[BattleTrace] Trace 確定 PathLength={_confirmedPath.Count} Path={FormatPath(_confirmedPath)}");
                _pathConfirmedTcs?.TrySetResult(new List<BattleGridPosition>(_confirmedPath));
                _pathConfirmedTcs = null;
                PathConfirmed?.Invoke(_confirmedPath);
            }
            else
            {
                _confirmedPath = new List<BattleGridPosition>();
                Debug.LogWarning($"[BattleTrace] Trace 未確定 Status={result.Status} Error={result.ValidationError}");
            }

            RefreshVisuals();
        }

        /// <summary>
        /// 現在のドラフトまたは確定パスを、盤面ハイライトと軌跡線へ反映する。
        /// </summary>
        private void RefreshVisuals()
        {
            if (_boardController == null)
            {
                return;
            }

            IReadOnlyList<BattleGridPosition> positions = _confirmedPath;
            var canConfirm = HasConfirmedPath;

            if (_tracer != null && _tracer.Draft.HasPath)
            {
                positions = _tracer.Draft.Positions;
                canConfirm = _tracer.Draft.CanConfirm;
            }

            _boardController.SetTraceVisual(positions, canConfirm);
            _traceLineView?.SetLine(_boardController.BuildWorldPointPath(positions), canConfirm);
        }

        private void CancelConfirmedPathAwait()
        {
            _pathConfirmedTcs?.TrySetCanceled();
            _pathConfirmedTcs = null;
        }

        private static string FormatPath(IReadOnlyList<BattleGridPosition> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                return "(empty)";
            }

            return string.Join(" -> ", positions);
        }
    }
}
