using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Assets.Scripts.Features.Battle.Util;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// セル View の pointer 入力を BattlePathTracer へ流し込む。
    /// </summary>
    public class BattleTraceInputHandler : MonoBehaviour
    {
        [SerializeField] private bool clearConfirmedPathOnNewTrace = true;

        private readonly BattleTraceInteractionState _traceState = new();
        private BattleBoardController _boardController;
        private BattleTraceLineView _traceLineView;

        public event Action<IReadOnlyList<BattleGridPosition>> PathConfirmed;
        public event Action<BattlePathTraceResult> TraceUpdated;
        public event Action<BattlePathTraceResult> TraceRejected;

        public bool HasConfirmedPath => _traceState.HasConfirmedPath;

        public IReadOnlyList<BattleGridPosition> ConfirmedPath => _traceState.ConfirmedPath;

        public bool CanConfirmCurrentTrace => _traceState.CanConfirmCurrentTrace;

        public IReadOnlyList<BattleGridPosition> CurrentTracePath => _traceState.CurrentTracePath;

        /// <summary>
        /// Goal 到達済みの確定パスが得られるまで待機する。
        /// </summary>
        public UniTask<IReadOnlyList<BattleGridPosition>> WaitForConfirmedPathAsync(CancellationToken cancellationToken = default)
        {
            return _traceState.WaitForConfirmedPathAsync(cancellationToken);
        }

        /// <summary>
        /// 盤面 View と tracer を接続し、以後の pointer 入力をパス追跡へ流す。
        /// </summary>
        public void Bind(BattleBoardController boardController, BattlePathTracer tracer, BattleTraceLineView traceLineView = null)
        {
            Unbind();

            _boardController = boardController;
            _traceLineView = traceLineView;
            _traceState.Bind(tracer);

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
            _traceLineView = null;
            _traceState.Unbind();
        }

        public void ResetState(bool clearConfirmedPath)
        {
            _traceState.Reset(clearConfirmedPath);
            RefreshVisuals();
        }

        private void Update()
        {
            if (_traceState.IsTracing && Input.GetMouseButtonUp(0))
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
            if (_traceState.Tracer == null)
            {
                Debug.LogWarning("[BattleTrace] tracer が未設定のため PointerDown を処理できません。");
                return;
            }

            Debug.Log($"[BattleTrace] Trace 開始 Input={position} ConfirmedCleared={clearConfirmedPathOnNewTrace}");
            ProcessTrace(position, _traceState.BeginTrace(position, clearConfirmedPathOnNewTrace));
        }

        /// <summary>
        /// ドラッグ中に入った隣接マスを順次 tracer へ渡す。
        /// </summary>
        private void HandleCellPointerEnter(BattleGridPosition position, UnityEngine.EventSystems.PointerEventData _)
        {
            if (!_traceState.IsTracing || _traceState.Tracer == null)
            {
                return;
            }

            ProcessTrace(position, _traceState.AppendTrace(position));
        }

        /// <summary>
        /// ドラッグ終了時に現在の仮パスを確定判定へ進める。
        /// </summary>
        private void HandleCellPointerUp(BattleGridPosition _, UnityEngine.EventSystems.PointerEventData __)
        {
            if (_traceState.IsTracing)
            {
                FinalizeTrace();
            }
        }

        /// <summary>
        /// 1 マス分の入力を tracer に適用し、受理/拒否結果をイベントと見た目へ反映する。
        /// </summary>
        private void ProcessTrace(BattleGridPosition position, BattlePathTraceResult result)
        {
            if (result == null)
            {
                return;
            }

            if (result.IsAccepted)
            {
                Debug.Log($"[BattleTrace] Accepted Status={result.Status} Input={position} PathLength={result.PathLength} CanConfirm={result.CanConfirm} Path={BattleTextFormatter.FormatPath(CurrentTracePath)}");
                TraceUpdated?.Invoke(result);
                RefreshVisuals();
                return;
            }

            Debug.LogWarning($"[BattleTrace] Rejected Status={result.Status} Input={position} Error={result.ValidationError} PathLength={result.PathLength} Path={BattleTextFormatter.FormatPath(CurrentTracePath)}");
            TraceRejected?.Invoke(result);
            RefreshVisuals();
        }

        /// <summary>
        /// 指離し時に Goal 到達済みなら確定し、未到達なら仮パスを破棄する。
        /// </summary>
        private void FinalizeTrace()
        {
            var result = _traceState.FinalizeTrace();
            if (result == null)
            {
                return;
            }

            TraceUpdated?.Invoke(result);
            if (result.Status == BattlePathTraceStatus.Confirmed)
            {
                Debug.Log($"[BattleTrace] Trace 確定 PathLength={ConfirmedPath.Count} Path={BattleTextFormatter.FormatPath(ConfirmedPath)}");
                PathConfirmed?.Invoke(ConfirmedPath);
            }
            else
            {
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

            var positions = CurrentTracePath;
            var canConfirm = CanConfirmCurrentTrace;

            _boardController.SetTraceVisual(positions, canConfirm);
            _traceLineView?.SetLine(_boardController.BuildWorldPointPath(positions), canConfirm);
        }
    }
}
