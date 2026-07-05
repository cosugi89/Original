using System;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Assets.Scripts.Features.Battle.Runtime;
using Assets.Scripts.Features.Battle.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// ターン開始時の盤面準備、プレビュー文言更新、入力待機とアニメーション再生をまとめて扱う。
    /// </summary>
    public sealed class BattleTurnViewCoordinator
    {
        private readonly BattleTurnExecutor _turnExecutor;

        private GameObject _hostObject;
        private Transform _panelsRoot;
        private BattleBoardController _boardController;
        private BattleTraceInputHandler _traceInputHandler;
        private BattleTraceLineView _traceLineView;
        private BattleHudController _hudController;
        private bool _preferInteractiveTraceInput;
        private bool _viewBootstrapped;
        private CancellationTokenSource _awaitNodePathCts;
        private BattleSessionState _session;
        private IReadOnlyList<StageBattlePatternData> _patterns = Array.Empty<StageBattlePatternData>();
        private BattlePlayerSnapshot _playerSnapshot;
        private StageBattleEnemyData _activeEnemyData;
        private Func<SkillSlotRuntime> _selectedSkillResolver;
        private Action _executeTurnRequested;
        private StageBattlePatternData _lastPreparedPattern;

        public BattleTurnViewCoordinator(BattleTurnExecutor turnExecutor = null)
        {
            _turnExecutor = turnExecutor ?? new BattleTurnExecutor();
        }

        public BattlePreparedTurn PreparedTurn { get; private set; }

        public BattleHudController HudController => _hudController;

        public void ConfigureScene(
            GameObject hostObject,
            Transform panelsRoot,
            BattleBoardController boardController,
            BattleTraceInputHandler traceInputHandler,
            BattleTraceLineView traceLineView,
            BattleHudController hudController,
            bool preferInteractiveTraceInput)
        {
            _hostObject = hostObject;
            _panelsRoot = panelsRoot;
            _boardController = boardController;
            _traceInputHandler = traceInputHandler;
            _traceLineView = traceLineView;
            _hudController = hudController;
            _preferInteractiveTraceInput = preferInteractiveTraceInput;
        }

        public void SetTurnContext(
            BattleSessionState session,
            IReadOnlyList<StageBattlePatternData> patterns,
            BattlePlayerSnapshot playerSnapshot,
            StageBattleEnemyData enemyData,
            Func<SkillSlotRuntime> selectedSkillResolver,
            Action executeTurnRequested)
        {
            _session = session;
            _patterns = patterns ?? Array.Empty<StageBattlePatternData>();
            _playerSnapshot = playerSnapshot;
            _activeEnemyData = enemyData;
            _selectedSkillResolver = selectedSkillResolver;
            _executeTurnRequested = executeTurnRequested;
        }

        public void PrepareUpcomingTurn()
        {
            BootstrapIfNeeded();

            if (_session == null)
            {
                return;
            }

            if (_session.BattleEnded)
            {
                Debug.Log("[Battle] バトル終了のため盤面入力をクリアします。");
                CancelAwaitNodePathInput();
                _traceInputHandler?.ResetState(clearConfirmedPath: true);
                _boardController?.ClearTraceVisual();
                _traceLineView?.Clear();
                _hudController?.SetConfirmText(BattleTextFormatter.BuildTurnPrompt(_session, _preferInteractiveTraceInput, PreparedTurn?.Pattern));
                return;
            }

            PreparedTurn = CreatePreparedTurn(_patterns, _lastPreparedPattern);
            if (PreparedTurn?.Pattern == null || PreparedTurn.Board == null)
            {
                _hudController?.SetConfirmText(BattleUiText.NoValidPattern);
                _boardController?.ClearTraceVisual();
                _traceLineView?.Clear();
                return;
            }

            _lastPreparedPattern = PreparedTurn.Pattern;
            _hudController?.SetConfirmText(BattleTextFormatter.BuildTurnPrompt(_session, _preferInteractiveTraceInput, PreparedTurn.Pattern));
            Debug.Log(
                $"[Battle] 次ターン準備 Turn={_session.TurnNumber} " +
                $"Pattern={BattleTextFormatter.GetPatternLabel(PreparedTurn.Pattern)} Action={PreparedTurn.Pattern.EnemyAction} " +
                $"Board={PreparedTurn.Board.Width}x{PreparedTurn.Board.Height} " +
                $"Start={PreparedTurn.Board.StartPosition} Goals={BattleTextFormatter.FormatPath(PreparedTurn.Board.GetGoalPositions())}");

            if (_boardController != null)
            {
                _boardController.RenderBoard(PreparedTurn.Board);
            }

            if (_traceInputHandler != null && _boardController != null)
            {
                _traceInputHandler.Bind(
                    _boardController,
                    new BattlePathTracer(PreparedTurn.Board),
                    _traceLineView);
            }
            else
            {
                _traceLineView?.Clear();
            }

            BeginAwaitNodePathInput(PreparedTurn.Pattern);
        }

        public void RefreshPreview()
        {
            if (!_preferInteractiveTraceInput || _hudController == null || PreparedTurn?.Pattern == null)
            {
                return;
            }

            if (_traceInputHandler == null || PreparedTurn.Board == null)
            {
                _hudController.SetConfirmText(BattleTextFormatter.BuildTurnPrompt(_session, _preferInteractiveTraceInput, PreparedTurn.Pattern));
                return;
            }

            var pathPositions = _traceInputHandler.CurrentTracePath;
            if (pathPositions == null || pathPositions.Count == 0)
            {
                _hudController.SetConfirmText(BattleTextFormatter.BuildTurnPrompt(_session, _preferInteractiveTraceInput, PreparedTurn.Pattern));
                return;
            }

            if (!_traceInputHandler.CanConfirmCurrentTrace && !_traceInputHandler.HasConfirmedPath)
            {
                _hudController.SetConfirmText(BattleUiText.ConnectGoalToConfirm);
                return;
            }

            var preview = _turnExecutor.Preview(
                PreparedTurn,
                pathPositions,
                _session,
                _playerSnapshot,
                _activeEnemyData,
                ResolveSelectedSkill(),
                _session.NextTurnHazardBoosted);

            if (preview == null)
            {
                _hudController.SetConfirmText(BattleUiText.PreviewUnavailable);
                return;
            }

            var previewMessage = BattleTextFormatter.BuildTurnPreviewMessage(PreparedTurn, preview);
            _hudController.SetConfirmText(previewMessage);
            Debug.Log(
                $"[Battle] PlayerTurn preview Path={BattleTextFormatter.FormatPath(pathPositions)} " +
                $"Summary={previewMessage}");
        }

        public BattleTurnResolutionReport ExecutePreparedTurn()
        {
            if (PreparedTurn?.Pattern == null)
            {
                Debug.LogWarning("[Battle] 現在ステージに有効な pattern がありません。");
                return null;
            }

            var selectedSkill = ResolveSelectedSkill();
            if (_preferInteractiveTraceInput && _traceInputHandler != null)
            {
                if (!_traceInputHandler.HasConfirmedPath)
                {
                    RefreshPreview();
                    Debug.LogWarning(
                        $"[Battle] ExecuteTurn を保留 Interactive={_preferInteractiveTraceInput} " +
                        $"TraceHandler={(_traceInputHandler != null)} BoardReady={(PreparedTurn.Board != null)} " +
                        $"HasConfirmedPath={_traceInputHandler.HasConfirmedPath}");
                    return null;
                }

                return _turnExecutor.ExecuteInteractive(
                    PreparedTurn,
                    _traceInputHandler.ConfirmedPath,
                    _session,
                    _playerSnapshot,
                    _activeEnemyData,
                    selectedSkill);
            }

            var fallbackResult = _turnExecutor.ExecuteFallback(
                PreparedTurn,
                _session,
                _playerSnapshot,
                _activeEnemyData,
                selectedSkill);
            if (fallbackResult == null)
            {
                Debug.LogWarning($"[Battle] Fallback turn path を構築できませんでした Pattern={BattleTextFormatter.GetPatternLabel(PreparedTurn.Pattern)}");
            }

            return fallbackResult;
        }

        public void LogResolutionReport(StageBattlePatternData pattern, BattleTurnResolutionReport result)
        {
            var summary = result?.Summary;
            if (summary == null || summary.LogEntries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < summary.LogEntries.Count; i++)
            {
                Debug.Log($"[Battle] {BattleTextFormatter.GetPatternLabel(pattern)} {summary.LogEntries[i]}");
            }
        }

        public async UniTask PlayPlayerActionAnimationsAsync(
            PartsManager playerInstance,
            BattleTurnResolutionReport result,
            CancellationToken cancellationToken)
        {
            if (playerInstance == null || result == null)
            {
                return;
            }

            var summary = result.Summary;
            var cues = summary.AnimationSequence;
            if (cues.Count == 0)
            {
                return;
            }

            for (var i = 0; i < cues.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var clipName = BattleAnimationCatalog.GetClipName(cues[i]);
                if (string.IsNullOrWhiteSpace(clipName) || !playerInstance.HasAnimation(clipName))
                {
                    continue;
                }

                playerInstance.PlayAnimation(clipName);
                Debug.Log($"[Battle] Player animation {clipName} を再生します。");

                var durationSeconds = Mathf.Max(
                    BattleUiValues.MinimumAnimationDurationSeconds,
                    playerInstance.GetAnimationLength(clipName));

                await UniTask.Delay(
                    TimeSpan.FromSeconds(durationSeconds),
                    cancellationToken: cancellationToken);
            }

            if (!summary.EnemyDefeated &&
                !summary.PlayerDefeated &&
                playerInstance.HasAnimation(BattleAnimationCatalog.Idle))
            {
                playerInstance.PlayAnimation(BattleAnimationCatalog.Idle);
            }
        }

        public void Shutdown()
        {
            CancelAwaitNodePathInput();

            if (_traceInputHandler != null)
            {
                _traceInputHandler.TraceUpdated -= OnTraceUpdated;
                _traceInputHandler.TraceRejected -= OnTraceRejected;
                _traceInputHandler.PathConfirmed -= OnPathConfirmed;
            }
        }

        private void BootstrapIfNeeded()
        {
            if (_viewBootstrapped)
            {
                return;
            }

            if (_panelsRoot == null)
            {
                Debug.LogWarning("[Battle] panelsRoot が未設定のため、盤面入力を初期化できません。");
            }
            else
            {
                if (_boardController == null)
                {
                    _boardController = _panelsRoot.GetComponent<BattleBoardController>();
                    if (_boardController == null)
                    {
                        _boardController = _panelsRoot.gameObject.AddComponent<BattleBoardController>();
                    }
                }

                _boardController.ConfigureGeneratedCells(
                    _panelsRoot as RectTransform,
                    _panelsRoot.GetComponent<GridLayoutGroup>());
                _preferInteractiveTraceInput = true;
                Debug.Log($"[Battle] panelsRoot を生成式盤面 root として接続しました ChildCount={_panelsRoot.childCount} Interactive={_preferInteractiveTraceInput}");
            }

            if (_traceInputHandler == null && _hostObject != null)
            {
                _traceInputHandler = _hostObject.GetComponent<BattleTraceInputHandler>();
                if (_traceInputHandler == null)
                {
                    _traceInputHandler = _hostObject.AddComponent<BattleTraceInputHandler>();
                }
            }

            if (_hudController == null && _hostObject != null)
            {
                _hudController = _hostObject.GetComponent<BattleHudController>();
            }

            if (_traceInputHandler != null)
            {
                _traceInputHandler.TraceUpdated -= OnTraceUpdated;
                _traceInputHandler.TraceRejected -= OnTraceRejected;
                _traceInputHandler.PathConfirmed -= OnPathConfirmed;
                _traceInputHandler.TraceUpdated += OnTraceUpdated;
                _traceInputHandler.TraceRejected += OnTraceRejected;
                _traceInputHandler.PathConfirmed += OnPathConfirmed;
            }

            _viewBootstrapped = true;
            Debug.Log($"[Battle] View bootstrap 完了 BoardController={(_boardController != null)} TraceInputHandler={(_traceInputHandler != null)} HudController={(_hudController != null)} Interactive={_preferInteractiveTraceInput}");
        }

        private void OnTraceUpdated(BattlePathTraceResult result)
        {
            if (PreparedTurn?.Pattern == null || _hudController == null)
            {
                return;
            }

            if (result.Status == BattlePathTraceStatus.Confirmed)
            {
                return;
            }

            if (result.CanConfirm)
            {
                RefreshPreview();
                return;
            }

            if (result.Status == BattlePathTraceStatus.ReleasedWithoutGoal)
            {
                _hudController.SetConfirmText(BattleUiText.GoalNotReached);
                return;
            }

            if (result.Status == BattlePathTraceStatus.Started ||
                result.Status == BattlePathTraceStatus.Appended ||
                result.Status == BattlePathTraceStatus.Backtracked)
            {
                _hudController.SetConfirmText(BattleUiText.ConnectGoalToConfirm);
                return;
            }

            _hudController.SetConfirmText(BattleTextFormatter.BuildTurnPrompt(_session, _preferInteractiveTraceInput, PreparedTurn.Pattern));
        }

        private void OnTraceRejected(BattlePathTraceResult result)
        {
            if (_hudController == null)
            {
                return;
            }

            _hudController.SetConfirmText(BattleTextFormatter.BuildTraceValidationErrorMessage(result.ValidationError));
        }

        private void OnPathConfirmed(IReadOnlyList<BattleGridPosition> _)
        {
            if (PreparedTurn?.Pattern == null)
            {
                return;
            }

            RefreshPreview();
        }

        private void BeginAwaitNodePathInput(StageBattlePatternData pattern)
        {
            CancelAwaitNodePathInput();

            if (!_preferInteractiveTraceInput ||
                _traceInputHandler == null ||
                PreparedTurn?.Board == null ||
                _session == null ||
                _session.BattleEnded)
            {
                return;
            }

            _awaitNodePathCts = new CancellationTokenSource();
            AwaitNodePathInputAsync(pattern, _awaitNodePathCts.Token).Forget();
            Debug.Log($"[Battle] UniTask で path 入力待機を開始します Turn={_session.TurnNumber} Pattern={BattleTextFormatter.GetPatternLabel(pattern)}");
        }

        private void CancelAwaitNodePathInput()
        {
            if (_awaitNodePathCts == null)
            {
                return;
            }

            _awaitNodePathCts.Cancel();
            _awaitNodePathCts.Dispose();
            _awaitNodePathCts = null;
        }

        private async UniTaskVoid AwaitNodePathInputAsync(StageBattlePatternData pattern, CancellationToken cancellationToken)
        {
            try
            {
                var confirmedPath = await _traceInputHandler.WaitForConfirmedPathAsync(cancellationToken);
                Debug.Log($"[Battle] UniTask 待機完了 Path={BattleTextFormatter.FormatPath(confirmedPath)}");
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (cancellationToken.IsCancellationRequested ||
                    _session == null ||
                    _session.BattleEnded ||
                    _session.IsPathInputLocked ||
                    _traceInputHandler == null ||
                    !_traceInputHandler.HasConfirmedPath ||
                    !ReferenceEquals(pattern, PreparedTurn?.Pattern))
                {
                    return;
                }

                Debug.Log("[Battle] UniTask 待機の完了によりターンを実行します。");
                _executeTurnRequested?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[Battle] UniTask path 入力待機をキャンセルしました。");
            }
        }

        private SkillSlotRuntime ResolveSelectedSkill()
        {
            return _selectedSkillResolver?.Invoke();
        }

        private static BattlePreparedTurn CreatePreparedTurn(
            IReadOnlyList<StageBattlePatternData> patterns,
            StageBattlePatternData lastPreparedPattern)
        {
            if (patterns == null || patterns.Count == 0)
            {
                return null;
            }

            var candidateIndices = new List<int>(patterns.Count);
            for (var i = 0; i < patterns.Count; i++)
            {
                if (patterns[i] != null)
                {
                    candidateIndices.Add(i);
                }
            }

            if (candidateIndices.Count == 0)
            {
                return null;
            }

            if (candidateIndices.Count > 1 && lastPreparedPattern != null)
            {
                candidateIndices.RemoveAll(index => ReferenceEquals(patterns[index], lastPreparedPattern));
                if (candidateIndices.Count == 0)
                {
                    for (var i = 0; i < patterns.Count; i++)
                    {
                        if (patterns[i] != null)
                        {
                            candidateIndices.Add(i);
                        }
                    }
                }
            }

            var selectedIndex = candidateIndices[UnityEngine.Random.Range(0, candidateIndices.Count)];
            var pattern = patterns[selectedIndex];
            var layout = BattleStageRuntimeAdapter.CreateBoardLayout(pattern);
            return new BattlePreparedTurn(pattern, layout.Board, layout.FallbackTracePositions);
        }
    }
}
