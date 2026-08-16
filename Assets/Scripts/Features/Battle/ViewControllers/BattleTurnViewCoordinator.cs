using System;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Components;
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
        private BattleEventNoticeView _eventNoticeView;
        private bool _preferInteractiveTraceInput;
        private bool _allowFallbackTurnExecution;
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
            bool preferInteractiveTraceInput,
            bool allowFallbackTurnExecution,
            BattleEventNoticeView eventNoticeView = null)
        {
            _hostObject = hostObject;
            _panelsRoot = panelsRoot;
            _boardController = boardController;
            _traceInputHandler = traceInputHandler;
            _traceLineView = traceLineView;
            _hudController = hudController;
            _eventNoticeView = eventNoticeView;
            _preferInteractiveTraceInput = preferInteractiveTraceInput;
            _allowFallbackTurnExecution = allowFallbackTurnExecution;
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
                _eventNoticeView?.Show(BattleUiText.BattleEnded);
                return;
            }

            PreparedTurn = CreatePreparedTurn(_patterns, _lastPreparedPattern);
            if (PreparedTurn?.Pattern == null || PreparedTurn.Board == null)
            {
                _eventNoticeView?.Show(BattleUiText.NoValidPattern);
                _boardController?.ClearTraceVisual();
                _traceLineView?.Clear();
                return;
            }

            _lastPreparedPattern = PreparedTurn.Pattern;
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
            if (!_preferInteractiveTraceInput || PreparedTurn?.Pattern == null)
            {
                return;
            }

            if (_traceInputHandler == null || PreparedTurn.Board == null)
            {
                return;
            }

            var pathPositions = _traceInputHandler.CurrentTracePath;
            if (pathPositions == null || pathPositions.Count == 0)
            {
                return;
            }

            if (!_traceInputHandler.CanConfirmCurrentTrace && !_traceInputHandler.HasConfirmedPath)
            {
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
                _eventNoticeView?.ShowInputRejected(BattleUiText.PreviewUnavailable);
                return;
            }

            var previewMessage = BattleTextFormatter.BuildTurnPreviewMessage(PreparedTurn, preview);
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
            if (_preferInteractiveTraceInput)
            {
                if (_traceInputHandler == null)
                {
                    _eventNoticeView?.ShowInputRejected(BattleUiText.DrawFromStartToGoal);
                    Debug.LogWarning("[Battle] Interactive path input が有効ですが TraceInputHandler が未接続です。");
                    return null;
                }

                if (!_traceInputHandler.HasConfirmedPath)
                {
                    RefreshPreview();
                    _eventNoticeView?.ShowInputRejected(BattleUiText.ConnectGoalToConfirm);
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

            if (!_allowFallbackTurnExecution)
            {
                _eventNoticeView?.ShowInputRejected(BattleUiText.DrawFromStartToGoal);
                Debug.LogWarning(
                    $"[Battle] Fallback turn execution は無効です。Pattern={BattleTextFormatter.GetPatternLabel(PreparedTurn.Pattern)}");
                return null;
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

        public async UniTask PlayResolvedActionAnimationsAsync(
            PartsManager playerInstance,
            PartsManager enemyInstance,
            BattleTurnResolutionReport result,
            CancellationToken cancellationToken,
            Action<BattleResolvedAction> onResolvedAction = null)
        {
            if (result == null)
            {
                return;
            }

            var summary = result.Summary;
            var notifiedActions = onResolvedAction != null
                ? new HashSet<BattleResolvedAction>()
                : null;
            var beats = BattleTurnAnimationPlanner.Build(result.ResolvedActions);
            for (var i = 0; i < beats.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await PlayAnimationBeatAsync(
                    beats[i],
                    playerInstance,
                    enemyInstance,
                    cancellationToken,
                    onResolvedAction,
                    notifiedActions);
            }

            NotifyRemainingResolvedActions(
                result.ResolvedActions,
                onResolvedAction,
                notifiedActions);

            if (summary.PlayerDefeated)
            {
                await PlayAnimationCueAsync(playerInstance, BattlePlayerAnimationCue.Defeat, "Player", cancellationToken);
                return;
            }

            if (summary.EnemyDefeated)
            {
                await PlayAnimationCueAsync(enemyInstance, BattlePlayerAnimationCue.Defeat, "Enemy", cancellationToken);
                await PlayAnimationCueAsync(playerInstance, BattlePlayerAnimationCue.Victory, "Player", cancellationToken);
                return;
            }

            PlayIdleIfAvailable(playerInstance);
            PlayIdleIfAvailable(enemyInstance);
        }

        private static async UniTask PlayAnimationBeatAsync(
            BattleTurnAnimationBeat beat,
            PartsManager playerInstance,
            PartsManager enemyInstance,
            CancellationToken cancellationToken,
            Action<BattleResolvedAction> onResolvedAction,
            HashSet<BattleResolvedAction> notifiedActions)
        {
            var events = beat.Events;
            if (events == null || events.Count == 0)
            {
                return;
            }

            var tasks = new List<UniTask>(events.Count);
            for (var i = 0; i < events.Count; i++)
            {
                if (!TryResolveAnimationActor(
                        events[i].Actor,
                        playerInstance,
                        enemyInstance,
                        out var target,
                        out var actorLabel))
                {
                    continue;
                }

                tasks.Add(PlayAnimationEventAsync(
                    target,
                    events[i],
                    actorLabel,
                    playerInstance,
                    enemyInstance,
                    cancellationToken,
                    onResolvedAction,
                    notifiedActions));
            }

            if (tasks.Count == 0)
            {
                return;
            }

            await UniTask.WhenAll(tasks);
        }

        private static async UniTask PlayAnimationEventAsync(
            PartsManager target,
            BattleTurnAnimationEvent animationEvent,
            string actorLabel,
            PartsManager playerInstance,
            PartsManager enemyInstance,
            CancellationToken cancellationToken,
            Action<BattleResolvedAction> onResolvedAction,
            HashSet<BattleResolvedAction> notifiedActions)
        {
            await WaitForAnimationStartAsync(
                animationEvent.StartTrigger,
                playerInstance,
                enemyInstance,
                cancellationToken);

            LogEffectTimelineEvent(animationEvent, actorLabel);

            var playedAnimation = false;
            var durationSeconds = 0f;
            if (animationEvent.Cue.HasValue)
            {
                playedAnimation = TryPlayAnimationCue(
                    target,
                    animationEvent.Cue.Value,
                    actorLabel,
                    out durationSeconds);
            }

            if (animationEvent.Cue.HasValue && !playedAnimation)
            {
                Debug.Log(
                    $"[Battle] {actorLabel} animation {BattleAnimationCatalog.GetClipName(animationEvent.Cue.Value)} が無いため effect fallback で進行します。");
            }

            if (animationEvent.ResolveTiming == BattleTurnAnimationResolveTiming.OnHit)
            {
                if (playedAnimation)
                {
                    await WaitForAnimationEventOrDelayAsync(
                        target,
                        BattleAnimationEventKeys.Hit,
                        animationEvent.ResolveDelaySeconds,
                        cancellationToken,
                        animationEvent.Cue);
                }
                else
                {
                    await DelayIfNeededAsync(animationEvent.ResolveDelaySeconds, cancellationToken);
                }

                NotifyResolvedActions(
                    animationEvent.ResolvedActions,
                    onResolvedAction,
                    notifiedActions);
            }
            else
            {
                await DelayIfNeededAsync(animationEvent.ResolveDelaySeconds, cancellationToken);
                NotifyResolvedActions(
                    animationEvent.ResolvedActions,
                    onResolvedAction,
                    notifiedActions);
            }

            if (playedAnimation && animationEvent.WaitForRecoverEnd)
            {
                await WaitForAnimationEventOrDelayAsync(
                    target,
                    BattleAnimationEventKeys.RecoverEnd,
                    durationSeconds,
                    cancellationToken,
                    animationEvent.Cue);
            }

            if (animationEvent.ReturnToIdle)
            {
                PlayIdleIfAvailable(target);
            }
        }

        private static void LogEffectTimelineEvent(
            BattleTurnAnimationEvent animationEvent,
            string actorLabel)
        {
            var effectAnimation = animationEvent.EffectAnimation;
            if (effectAnimation == null)
            {
                return;
            }

            Debug.Log(
                $"[Battle] {actorLabel} effect timeline {effectAnimation.EffectAnimationKey} " +
                $"VFX={effectAnimation.VisualEffectKey} SE={effectAnimation.SoundEffectKey} Wait={effectAnimation.WaitSeconds:0.###}");
        }

        private static void NotifyRemainingResolvedActions(
            IReadOnlyList<BattleResolvedAction> resolvedActions,
            Action<BattleResolvedAction> onResolvedAction,
            HashSet<BattleResolvedAction> notifiedActions)
        {
            NotifyResolvedActions(
                resolvedActions,
                onResolvedAction,
                notifiedActions);
        }

        private static void NotifyResolvedActions(
            IReadOnlyList<BattleResolvedAction> resolvedActions,
            Action<BattleResolvedAction> onResolvedAction,
            HashSet<BattleResolvedAction> notifiedActions)
        {
            if (resolvedActions == null || onResolvedAction == null)
            {
                return;
            }

            for (var i = 0; i < resolvedActions.Count; i++)
            {
                var action = resolvedActions[i];
                if (action == null)
                {
                    continue;
                }

                if (notifiedActions != null && !notifiedActions.Add(action))
                {
                    continue;
                }

                onResolvedAction.Invoke(action);
            }
        }

        private static async UniTask<bool> PlayAnimationCueAsync(
            PartsManager target,
            BattlePlayerAnimationCue cue,
            string actorLabel,
            CancellationToken cancellationToken)
        {
            if (!TryPlayAnimationCue(target, cue, actorLabel, out var durationSeconds))
            {
                return false;
            }

            await UniTask.Delay(
                TimeSpan.FromSeconds(durationSeconds),
                cancellationToken: cancellationToken);
            return true;
        }

        private static async UniTask WaitForAnimationStartAsync(
            BattleTurnAnimationStartTrigger trigger,
            PartsManager playerInstance,
            PartsManager enemyInstance,
            CancellationToken cancellationToken)
        {
            if (!trigger.WaitsForAnimationEvent)
            {
                await DelayIfNeededAsync(trigger.DelaySeconds, cancellationToken);
                return;
            }

            if (!TryResolveAnimationActor(
                    trigger.TriggerActor,
                    playerInstance,
                    enemyInstance,
                    out var triggerTarget,
                    out _))
            {
                await DelayIfNeededAsync(trigger.FallbackDelaySeconds, cancellationToken);
                return;
            }

            await WaitForAnimationEventOrDelayAsync(
                triggerTarget,
                trigger.EventKey,
                trigger.FallbackDelaySeconds,
                cancellationToken,
                trigger.TriggerCue);
        }

        private static async UniTask<bool> WaitForAnimationEventOrDelayAsync(
            PartsManager target,
            string eventKey,
            float fallbackDelaySeconds,
            CancellationToken cancellationToken,
            BattlePlayerAnimationCue? cue = null)
        {
            if (!TryGetAnimationEventReceiver(target, out var receiver) ||
                string.IsNullOrWhiteSpace(eventKey))
            {
                await DelayIfNeededAsync(fallbackDelaySeconds, cancellationToken);
                return false;
            }

            var timeoutSeconds = ResolveAnimationEventTimeoutSeconds(
                target,
                eventKey,
                fallbackDelaySeconds,
                cue);
            var received = false;
            void OnBattleAnimationEventRaised(AnimationEventReceiver source, string raisedEventKey)
            {
                if (source == receiver &&
                    string.Equals(raisedEventKey, eventKey, StringComparison.OrdinalIgnoreCase))
                {
                    received = true;
                }
            }

            receiver.BattleAnimationEventRaised += OnBattleAnimationEventRaised;
            try
            {
                if (timeoutSeconds <= 0f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    return received;
                }

                var elapsedSeconds = 0f;
                while (!received && elapsedSeconds < timeoutSeconds)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    elapsedSeconds += Time.deltaTime;
                }

                return received;
            }
            finally
            {
                receiver.BattleAnimationEventRaised -= OnBattleAnimationEventRaised;
            }
        }

        private static float ResolveAnimationEventTimeoutSeconds(
            PartsManager target,
            string eventKey,
            float fallbackDelaySeconds,
            BattlePlayerAnimationCue? cue)
        {
            if (string.Equals(eventKey, BattleAnimationEventKeys.AttackStart, StringComparison.OrdinalIgnoreCase))
            {
                return fallbackDelaySeconds;
            }

            if (!TryFindAnimationClip(target, cue, out var clip) ||
                !CanClipRaiseBattleAnimationEvent(clip, eventKey))
            {
                return fallbackDelaySeconds;
            }

            var frameSeconds = clip.frameRate > 0f
                ? 1f / clip.frameRate
                : 0.05f;
            return Mathf.Max(fallbackDelaySeconds, clip.length + frameSeconds);
        }

        private static bool TryFindAnimationClip(
            PartsManager target,
            BattlePlayerAnimationCue? cue,
            out AnimationClip clip)
        {
            clip = null;
            if (target == null)
            {
                return false;
            }

            var clipName = cue.HasValue
                ? BattleAnimationCatalog.GetClipName(cue.Value)
                : target.GetCurrentAnimation();
            if (string.IsNullOrWhiteSpace(clipName))
            {
                return false;
            }

            var animator = target.GetComponentInChildren<Animator>(true);
            var controller = animator != null ? animator.runtimeAnimatorController : null;
            var clips = controller != null ? controller.animationClips : null;
            if (clips == null)
            {
                return false;
            }

            for (var i = 0; i < clips.Length; i++)
            {
                var candidate = clips[i];
                if (candidate != null &&
                    string.Equals(candidate.name, clipName, StringComparison.OrdinalIgnoreCase))
                {
                    clip = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool CanClipRaiseBattleAnimationEvent(AnimationClip clip, string eventKey)
        {
            var animationEvents = clip != null ? clip.events : null;
            if (animationEvents == null || animationEvents.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < animationEvents.Length; i++)
            {
                if (AnimationEventMatchesKey(animationEvents[i], eventKey))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnimationEventMatchesKey(
            UnityEngine.AnimationEvent animationEvent,
            string eventKey)
        {
            if (animationEvent == null || string.IsNullOrWhiteSpace(eventKey))
            {
                return false;
            }

            var functionName = animationEvent.functionName;
            if (string.Equals(functionName, nameof(AnimationEventReceiver.OnBattleAnimationEvent), StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(animationEvent.stringParameter, eventKey, StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(eventKey, BattleAnimationEventKeys.AttackStart, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(functionName, nameof(AnimationEventReceiver.OnAttackStart), StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(eventKey, BattleAnimationEventKeys.Hit, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(functionName, nameof(AnimationEventReceiver.OnHit), StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(functionName, nameof(AnimationEventReceiver.OnAttackHit), StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(functionName, nameof(AnimationEventReceiver.OnSkillHit), StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(eventKey, BattleAnimationEventKeys.RecoverEnd, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(functionName, nameof(AnimationEventReceiver.OnRecoverEnd), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static async UniTask DelayIfNeededAsync(
            float delaySeconds,
            CancellationToken cancellationToken)
        {
            if (delaySeconds <= 0f)
            {
                return;
            }

            await UniTask.Delay(
                TimeSpan.FromSeconds(delaySeconds),
                cancellationToken: cancellationToken);
        }

        private static bool TryResolveAnimationActor(
            BattleTurnAnimationActor actor,
            PartsManager playerInstance,
            PartsManager enemyInstance,
            out PartsManager target,
            out string actorLabel)
        {
            switch (actor)
            {
                case BattleTurnAnimationActor.Enemy:
                    target = enemyInstance;
                    actorLabel = "Enemy";
                    return target != null;
                case BattleTurnAnimationActor.Player:
                default:
                    target = playerInstance;
                    actorLabel = "Player";
                    return target != null;
            }
        }

        private static bool TryGetAnimationEventReceiver(
            PartsManager target,
            out AnimationEventReceiver receiver)
        {
            receiver = target != null
                ? target.GetComponentInChildren<AnimationEventReceiver>(true)
                : null;
            return receiver != null;
        }

        private static bool TryPlayAnimationCue(
            PartsManager target,
            BattlePlayerAnimationCue cue,
            string actorLabel,
            out float durationSeconds)
        {
            durationSeconds = 0f;
            if (target == null)
            {
                return false;
            }

            var clipName = BattleAnimationCatalog.GetClipName(cue);
            if (string.IsNullOrWhiteSpace(clipName) || !target.HasAnimation(clipName))
            {
                return false;
            }

            target.PlayAnimation(clipName);
            Debug.Log($"[Battle] {actorLabel} animation {clipName} を再生します。");

            durationSeconds = Mathf.Max(
                BattleUiValues.MinimumAnimationDurationSeconds,
                target.GetAnimationLength(clipName));
            return true;
        }

        private static void PlayIdleIfAvailable(PartsManager target)
        {
            if (target != null && target.HasAnimation(BattleAnimationCatalog.Idle))
            {
                target.PlayAnimation(BattleAnimationCatalog.Idle);
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
                Debug.Log(
                    $"[Battle] panelsRoot を生成式盤面 root として接続しました ChildCount={_panelsRoot.childCount} " +
                    $"Interactive={_preferInteractiveTraceInput} Fallback={_allowFallbackTurnExecution}");
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
            Debug.Log(
                $"[Battle] View bootstrap 完了 BoardController={(_boardController != null)} " +
                $"TraceInputHandler={(_traceInputHandler != null)} HudController={(_hudController != null)} " +
                $"Interactive={_preferInteractiveTraceInput} Fallback={_allowFallbackTurnExecution}");
        }

        private void OnTraceUpdated(BattlePathTraceResult result)
        {
            if (PreparedTurn?.Pattern == null)
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
                _eventNoticeView?.ShowInputRejected(BattleUiText.GoalNotReached);
                return;
            }

            if (result.Status == BattlePathTraceStatus.Started ||
                result.Status == BattlePathTraceStatus.Appended ||
                result.Status == BattlePathTraceStatus.Backtracked)
            {
                return;
            }

        }

        private void OnTraceRejected(BattlePathTraceResult result)
        {
            var errorMessage = BattleTextFormatter.BuildTraceValidationErrorMessage(result.ValidationError);
            _eventNoticeView?.ShowInputRejected(errorMessage);
        }

        private void OnPathConfirmed(IReadOnlyList<BattleGridPosition> _)
        {
            if (PreparedTurn?.Pattern == null)
            {
                return;
            }

            _eventNoticeView?.Show(BattleUiText.GoalLabel);
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
