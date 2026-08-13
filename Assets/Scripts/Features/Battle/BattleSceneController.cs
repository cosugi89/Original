using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Assets.Scripts.Features.Battle.Runtime;
using Assets.Scripts.Features.Battle.Util;
using Assets.Scripts.Features.Battle.ViewControllers;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Scene;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle
{
    /// <summary>
    /// 戦闘シーン全体の初期化、ターン進行、各 ViewController への反映を束ねるシーンコントローラ。
    /// </summary>
    public class BattleSceneController : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform enemyRoot;

        [Header("UI")]
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Image previewImage;

        [FormerlySerializedAs("skillSlots")]
        [SerializeField] private BattleSkillSlot[] skillSlots;
        [SerializeField] private int unlockedSkillSlotCount = 2;

        [Header("Screen Structure")]
        [SerializeField] private BattleScreenRoot screenRoot;
        [SerializeField] private BattleTopHudView topHudView;
        [SerializeField] private BattlePresentationView presentationView;
        [SerializeField] private EnemyAreaView enemyAreaView;
        [SerializeField] private EnemyUnitView enemyUnitView;
        [SerializeField] private PlayerAreaView playerAreaView;
        [SerializeField] private SkillPaletteView skillPaletteView;
        [SerializeField] private BattleBoardView boardView;
        [SerializeField] private BattleEventNoticeView eventNoticeView;

        [Header("View Controllers")]
        [SerializeField] private Transform panelsRoot;
        [SerializeField] private BattleBoardController boardController;
        [SerializeField] private BattleTraceInputHandler traceInputHandler;
        [SerializeField] private BattleTraceLineView traceLineView;
        [SerializeField] private BattleHudController hudController;
        [SerializeField] private BattleActionLogView actionLogView;
        [SerializeField] private BattleDamageFeedbackView damageFeedbackView;
        [SerializeField] private BattleEnemyBattleTypeView enemyBattleTypeView;
        [SerializeField] private BattleNarrativeView narrativeView;
        [SerializeField] private BattleResultView resultView;
        [SerializeField] private bool preferInteractiveTraceInput = true;
        [SerializeField] private bool allowFallbackTurnExecution;

        [Header("Skill Button Visuals")]
        [SerializeField] private Color normalSkillButtonColor = BattleUiColors.NormalSkillButton;
        [SerializeField] private Color selectedSkillButtonColor = BattleUiColors.SelectedSkillButton;
        
        private IReadOnlyList<StageBattlePatternData> _patterns = Array.Empty<StageBattlePatternData>();
        private BattleSceneSetupFactory _sceneSetupFactory;
        private readonly BattleTurnViewCoordinator _turnViewCoordinator = new();
        private readonly BattleTurnResultApplier _turnResultApplier = new();
        private readonly List<string> _actionLogEntries = new();
        private readonly List<string> _playerActionHistoryEntries = new();
        private readonly List<string> _enemyActionHistoryEntries = new();
        private PartsManager _playerInstance;
        private PartsManager _enemyInstance;
        private readonly BattleSessionState _session = new();
        private StageBattleEnemyData _activeEnemyData;
        private BattlePlayerSnapshot _playerSnapshot;
        private CancellationTokenSource _turnExecutionCts;
        private int _currentStageId = -1;
        private int _nextStageId = -1;

        private bool _isInitialized;
        private bool _isExecutingTurn;

        private void Awake()
        {
            _sceneSetupFactory ??= new BattleSceneSetupFactory();
            ResolveScreenReferences();
            BindSkillInput();

            ResolveResultView()?.BindActions(
                HandleRetryButtonClicked,
                HandleNextStageButtonClicked,
                HandleBackToStageButtonClicked);
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            UnbindSkillInput();

            ResolveResultView()?.UnbindActions(
                HandleRetryButtonClicked,
                HandleNextStageButtonClicked,
                HandleBackToStageButtonClicked);

            _turnViewCoordinator.Shutdown();
            CancelTurnExecution();
        }

        /// <summary>
        /// ステージ、戦闘状態、表示を初期化し、最初のターン入力を開始する。
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            _sceneSetupFactory ??= new BattleSceneSetupFactory();
            ResolveScreenReferences();
            if (!_sceneSetupFactory.TryCreate(GetSkillSlotCount(), unlockedSkillSlotCount, out var setup))
            {
                Debug.LogWarning("[Battle] 戦闘セットアップを構築できなかったため、バトル初期化を中断しました。");
                return;
            }

            ApplyStageSceneState(setup.StageData);
            ApplyRuntimeSetup(setup);
            _turnViewCoordinator.ConfigureScene(
                gameObject,
                panelsRoot,
                ResolveBoardController(),
                traceInputHandler,
                traceLineView,
                hudController,
                preferInteractiveTraceInput,
                allowFallbackTurnExecution,
                ResolveEventNoticeView());
            _turnViewCoordinator.SetTurnContext(
                _session,
                _patterns,
                _playerSnapshot,
                _activeEnemyData,
                GetSelectedSkill,
                ExecuteTurn);
            ResetConnectedBattleUi();
            _turnViewCoordinator.PrepareUpcomingTurn();
            RefreshHud();
            RefreshSkillButtonVisuals();
            Debug.Log($"[Battle] 初期化完了 Enemy={_session.EnemyName} HP={_session.InitialEnemyHp}");

            SpawnPreviewCharacters();
        }

        /// <summary>
        /// 現在確定しているプレイヤーパスをもとにターン実行を開始する。
        /// </summary>
        public void ExecuteTurn()
        {
            ExecuteTurnAsync().Forget();
        }

        /// <summary>
        /// パス解決、アニメーション再生、HP反映、次ターン準備までを 1 ターンとして進める。
        /// </summary>
        private async UniTaskVoid ExecuteTurnAsync()
        {
            if (_isExecutingTurn)
            {
                return;
            }

            if (!_isInitialized)
            {
                Initialize();
            }

            if (_session.BattleEnded)
            {
                Debug.LogWarning($"[Battle] バトル終了済み");
                return;
            }

            var preparedTurn = _turnViewCoordinator.PreparedTurn;
            if (preparedTurn?.Pattern == null)
            {
                Debug.LogWarning("[Battle] 現在ステージに有効な pattern がありません。");
                return;
            }

            var executingTurnNumber = _session.TurnNumber;
            _isExecutingTurn = true;
            CancelTurnExecution();
            _turnExecutionCts = new CancellationTokenSource();

            try
            {
                var result = _turnViewCoordinator.ExecutePreparedTurn();
                if (result == null)
                {
                    return;
                }

                await _turnViewCoordinator.PlayResolvedActionAnimationsAsync(
                    _playerInstance,
                    _enemyInstance,
                    result,
                    _turnExecutionCts.Token);

                _turnResultApplier.Apply(_session, _playerSnapshot, result, preparedTurn.Pattern, HandleBattleClear);
                ApplyTurnUi(preparedTurn.Pattern, result.Summary);
                if (!_session.BattleEnded)
                {
                    _session.AdvanceTurn();
                }

                _turnViewCoordinator.PrepareUpcomingTurn();
                RefreshHud();
                RefreshSkillButtonVisuals();
                _turnViewCoordinator.LogResolutionReport(preparedTurn.Pattern, result);
                Debug.Log($"[Battle] Turn {executingTurnNumber} 終了 PlayerHP={_session.PlayerHp} EnemyHP={_session.EnemyHp}");

                if (_session.BattleEnded)
                {
                    ShowBattleResultUi(result.Summary);
                    Debug.Log($"[Battle] バトル終了");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[Battle] ターン実行中のアニメーション待機をキャンセルしました。");
            }
            finally
            {
                _session.IsPathInputLocked = false;
                _isExecutingTurn = false;
                CancelTurnExecution();
            }
        }

        /// <summary>
        /// 選択中のスキルを解除する。
        /// </summary>
        public void ClearSelectedSkill()
        {
            if (_session.IsPathInputLocked || _session.SelectedSkillSlotIndex < 0)
            {
                return;
            }

            _session.SelectedSkillSlotIndex = -1;
            RefreshSkillButtonVisuals();
            _turnViewCoordinator.RefreshPreview();
        }

        /// <summary>
        /// 指定スロットのスキル選択状態を更新する。
        /// </summary>
        private void SelectSkillSlot(int slotIndex)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_session.BattleEnded || _session.IsPathInputLocked)
            {
                return;
            }

            var slot = GetSkillSlotRuntime(slotIndex);
            if (slot == null)
            {
                return;
            }
            
            if (!slot.IsReady)
            {
                Debug.Log($"[Battle] スキル{slotIndex + 1}は使用不可 ({slot.GetStateLabel()})");
                return;
            }

            _session.SelectedSkillSlotIndex = _session.SelectedSkillSlotIndex == slotIndex ? -1 : slotIndex;
            RefreshSkillButtonVisuals();
            _turnViewCoordinator.RefreshPreview();
        }

        private void ApplyPlayerSnapshot(BattlePlayerSnapshot playerSnapshot)
        {
            var paletteView = ResolveSkillPaletteView();
            if (paletteView != null && paletteView.SlotCount > 0)
            {
                paletteView.ApplyPlayerSnapshot(playerSnapshot);
                return;
            }

            var runtimes = playerSnapshot?.SkillSlots ?? Array.Empty<SkillSlotRuntime>();
            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                var slotView = skillSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                slotView.SetRuntime(i < runtimes.Count ? runtimes[i] : null);
            }
        }

        private void RefreshSkillButtonVisuals()
        {
            var paletteView = ResolveSkillPaletteView();
            if (paletteView != null && paletteView.SlotCount > 0)
            {
                paletteView.RefreshSlots(
                    _session.SelectedSkillSlotIndex,
                    normalSkillButtonColor,
                    selectedSkillButtonColor);
                return;
            }

            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                var slotView = skillSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                slotView.RefreshRuntime();
                slotView.SetSelectedVisual(
                    i == _session.SelectedSkillSlotIndex,
                    normalSkillButtonColor,
                    selectedSkillButtonColor);
            }
        }

        #region init
        private void ApplyStageSceneState(StageData stageData)
        {
            if (stageData == null) return;

            BattleProgressService.EnsureInitialized().SetCurrentStage(stageData.StageId);

            if (background != null)
            {
                background.sprite = stageData.BackgroundImage;
            }

            if (previewImage != null)
            {
                previewImage.sprite = stageData.PreviewImage;
            }

            // TODO: BGMなど
        }

        private void ApplyRuntimeSetup(BattleSceneSetup setup)
        {
            if (setup == null)
            {
                return;
            }

            _playerSnapshot = setup.PlayerSnapshot;
            ApplyPlayerSnapshot(_playerSnapshot);
            _activeEnemyData = setup.EnemyData;
            _patterns = setup.Patterns;
            _currentStageId = setup.StageData?.StageId ?? -1;
            _nextStageId = _currentStageId >= 0
                ? BattleProgressService.EnsureInitialized().GetNextStageId(_currentStageId)
                : -1;

            _session.ResetForBattle(setup.EnemyName, setup.InitialPlayerHp, setup.InitialEnemyHp);
            _isInitialized = true;

            if (_patterns.Count == 0)
            {
                Debug.LogWarning($"[Battle] stageId={setup.StageData?.StageId ?? -1} に pattern がありません。");
            }
        }

        private void SpawnPreviewCharacters()
        {
            if (playerPrefab == null || playerRoot == null || enemyRoot == null)
            {
                return;
            }

            var playerObject = Instantiate(playerPrefab, playerRoot);
            var enemyObject = Instantiate(playerPrefab, enemyRoot);
            _playerInstance = playerObject.GetComponent<PartsManager>();
            _enemyInstance = enemyObject.GetComponent<PartsManager>();
            _playerInstance.Init();
            _enemyInstance.Init();

            var avatarRender = AvatarRenderService.EnsureInitialized();
            avatarRender.SyncSessionFromRendererIfNeeded(_playerInstance, saveAfterSync: true);
            avatarRender.ApplyTo(_playerInstance);

            if (_activeEnemyData?.Appearance != null)
            {
                _enemyInstance.ApplyAppearanceData(_activeEnemyData.Appearance);
            }
        }
        #endregion init

        private void RefreshHud()
        {
            ResolveHudController()?.ApplySession(_session);
            ResolveTopHudView()?.ApplySession(_session);
            ResolvePresentationView()?.ApplySession(_session);
            ResolveEnemyAreaView()?.ApplySingleEnemy(_session);
            ResolveEnemyUnitView()?.ApplySession(_session);
            ResolvePlayerAreaView()?.ApplySession(_session);
            RefreshBattleTypeUi();
        }

        private void HandleBattleClear()
        {
            _session.BattleEnded = true;

            var battleProgress = BattleProgressService.EnsureInitialized();
            var clearedStageId = battleProgress.Session.CurrentStageId;
            if (clearedStageId < 0)
            {
                return;
            }

            battleProgress.RecordStageClear(clearedStageId);

            var nextStageId = battleProgress.GetNextStageId(clearedStageId);
            if (nextStageId >= 0)
            {
                battleProgress.UnlockStage(nextStageId);
            }
        }

        private SkillSlotRuntime GetSelectedSkill()
        {
            return GetSkillSlotRuntime(_session.SelectedSkillSlotIndex);
        }

        private int GetSkillSlotCount()
        {
            var paletteView = ResolveSkillPaletteView();
            if (paletteView != null && paletteView.SlotCount > 0)
            {
                return paletteView.SlotCount;
            }

            return skillSlots != null ? skillSlots.Length : 0;
        }

        private SkillSlotRuntime GetSkillSlotRuntime(int slotIndex)
        {
            var runtimes = _playerSnapshot?.SkillSlots;
            if (runtimes == null || slotIndex < 0 || slotIndex >= runtimes.Count)
            {
                return null;
            }

            return runtimes[slotIndex];
        }

        private void CancelTurnExecution()
        {
            if (_turnExecutionCts == null)
            {
                return;
            }

            _turnExecutionCts.Cancel();
            _turnExecutionCts.Dispose();
            _turnExecutionCts = null;
        }

        private void BindSkillInput()
        {
            var paletteView = ResolveSkillPaletteView();
            if (paletteView != null && paletteView.SlotCount > 0)
            {
                paletteView.BindSlotClickActions(SelectSkillSlot);
                return;
            }

            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                var skillSlot = skillSlots[i];
                var slotIndex = i;
                skillSlot?.BindClickAction(() => SelectSkillSlot(slotIndex));
            }
        }

        private void UnbindSkillInput()
        {
            var paletteView = ResolveSkillPaletteView();
            if (paletteView != null && paletteView.SlotCount > 0)
            {
                paletteView.UnbindSlotClickActions();
                return;
            }

            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                skillSlots[i]?.UnbindClickAction();
            }
        }

        private void ResolveScreenReferences()
        {
            if (screenRoot == null)
            {
                screenRoot = FindFirstObjectByType<BattleScreenRoot>(FindObjectsInactive.Include);
            }

            screenRoot?.ResolveMissingReferences();

            if (topHudView == null)
            {
                topHudView = screenRoot?.TopHudView ?? hudController?.TopHudView;
            }

            if (presentationView == null)
            {
                presentationView = screenRoot?.PresentationView;
            }

            if (enemyAreaView == null)
            {
                enemyAreaView = screenRoot?.EnemyAreaView;
            }

            if (enemyUnitView == null)
            {
                enemyUnitView = screenRoot?.EnemyUnitView ?? hudController?.EnemyUnitView;
            }

            if (playerAreaView == null)
            {
                playerAreaView = screenRoot?.PlayerAreaView ?? hudController?.PlayerAreaView;
            }

            if (skillPaletteView == null)
            {
                skillPaletteView = screenRoot?.SkillPaletteView;
            }

            if (boardView == null)
            {
                boardView = screenRoot?.BoardView;
            }

            if (eventNoticeView == null)
            {
                eventNoticeView = screenRoot?.EventNoticeView ?? presentationView?.EventNoticeView;
            }

            if (resultView == null)
            {
                resultView = screenRoot?.ResultOverlayView;
            }
        }

        private BattleScreenRoot ResolveScreenRoot()
        {
            ResolveScreenReferences();
            return screenRoot;
        }

        private BattleBoardController ResolveBoardController()
        {
            ResolveScreenReferences();
            return boardView != null ? boardView : boardController;
        }

        private BattleHudController ResolveHudController()
        {
            return _turnViewCoordinator.HudController ?? hudController;
        }

        private BattleTopHudView ResolveTopHudView()
        {
            ResolveScreenReferences();
            return topHudView;
        }

        private BattlePresentationView ResolvePresentationView()
        {
            ResolveScreenReferences();
            return presentationView;
        }

        private EnemyAreaView ResolveEnemyAreaView()
        {
            ResolveScreenReferences();
            return enemyAreaView;
        }

        private EnemyUnitView ResolveEnemyUnitView()
        {
            ResolveScreenReferences();
            return enemyUnitView ?? enemyAreaView?.PrimaryEnemyUnit;
        }

        private PlayerAreaView ResolvePlayerAreaView()
        {
            ResolveScreenReferences();
            return playerAreaView;
        }

        private SkillPaletteView ResolveSkillPaletteView()
        {
            ResolveScreenReferences();
            return skillPaletteView;
        }

        private BattleDamageFeedbackView ResolveDamageFeedbackView()
        {
            ResolveScreenReferences();
            return presentationView?.DamageFeedbackView ?? damageFeedbackView;
        }

        private BattleEnemyBattleTypeView ResolveEnemyBattleTypeView()
        {
            ResolveScreenReferences();
            return ResolveEnemyUnitView()?.BattleTypeView ?? enemyBattleTypeView;
        }

        private BattleEventNoticeView ResolveEventNoticeView()
        {
            ResolveScreenReferences();
            if (eventNoticeView == null && hudController != null && hudController.LegacyConfirmText != null)
            {
                eventNoticeView = hudController.GetComponent<BattleEventNoticeView>();
                if (eventNoticeView == null)
                {
                    eventNoticeView = hudController.gameObject.AddComponent<BattleEventNoticeView>();
                }

                eventNoticeView.Configure(hudController.LegacyConfirmRoot, hudController.LegacyConfirmText);
            }

            return eventNoticeView;
        }

        private BattleResultView ResolveResultView()
        {
            ResolveScreenReferences();
            return resultView;
        }

        private void ResetConnectedBattleUi()
        {
            _actionLogEntries.Clear();
            _playerActionHistoryEntries.Clear();
            _enemyActionHistoryEntries.Clear();
            actionLogView?.ResetView();
            ResolveDamageFeedbackView()?.ResetView();
            narrativeView?.ResetView();
            ResolveHudController()?.ClearConfirmText();
            ResolveEventNoticeView()?.ResetView();
            ResolveResultView()?.ResetView();
            RefreshBattleTypeUi();
            RefreshResultButtons();
        }

        private void RefreshBattleTypeUi()
        {
            var guardValue = _activeEnemyData?.Stats?.Guard ?? 0;
            var gaugeColor = guardValue > 0
                ? BattleUiColors.GuardGauge
                : BattleUiColors.AttackGauge;
            var battleTypeName = guardValue > 0 ? BattleUiText.GuardBattleType : BattleUiText.BattleType;
            var valueText = BattleTextFormatter.BuildBattleTypeValueText(guardValue);

            var battleTypeView = ResolveEnemyBattleTypeView();
            battleTypeView?.SetGauge(
                battleTypeName,
                guardValue,
                Mathf.Max(1, guardValue),
                valueText,
                gaugeColor,
                battleTypeView.CurrentIcon);
        }

        private void ApplyTurnUi(StageBattlePatternData pattern, BattleResolvedActionSummary summary)
        {
            if (summary == null)
            {
                return;
            }

            actionLogView?.ResetView();
            ResolveDamageFeedbackView()?.Apply(summary);
            ResolveEventNoticeView()?.ShowTurnResult(summary);
            narrativeView?.SetPlayerNarrative(
                string.Empty,
                BattleTextFormatter.BuildPlayerSpeech(summary));
            narrativeView?.SetEnemyNarrative(
                string.Empty,
                BattleTextFormatter.BuildEnemySpeech(pattern, summary),
                BattleTextFormatter.BuildEnemyFlavor(pattern, summary));
        }

        private void AppendActionLogs(IReadOnlyList<string> logEntries)
        {
            if (logEntries == null)
            {
                return;
            }

            for (var i = 0; i < logEntries.Count; i++)
            {
                var entry = logEntries[i];
                if (!string.IsNullOrWhiteSpace(entry))
                {
                    _actionLogEntries.Add(entry);
                }
            }
        }

        private void ShowBattleResultUi(BattleResolvedActionSummary summary)
        {
            if (summary == null)
            {
                return;
            }

            if (summary.EnemyDefeated)
            {
                ResolveResultView()?.ShowVictory(BattleTextFormatter.BuildEnemyDefeatedMessage(_session.EnemyName));
            }
            else if (summary.PlayerDefeated || _session.PlayerHp <= 0)
            {
                ResolveResultView()?.ShowDefeat(BattleUiText.DefeatMessage);
            }

            RefreshResultButtons();
        }

        private void RefreshResultButtons()
        {
            ResolveResultView()?.SetButtonsVisible(_session.BattleEnded, _nextStageId >= 0);
        }

        private void HandleRetryButtonClicked()
        {
            BattleSceneTransitionState.SelectedStageId = _currentStageId;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        private void HandleNextStageButtonClicked()
        {
            if (_nextStageId < 0)
            {
                SceneManager.LoadScene(SceneNames.Main);
                return;
            }

            BattleSceneTransitionState.SelectedStageId = _nextStageId;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        private void HandleBackToStageButtonClicked()
        {
            SceneManager.LoadScene(SceneNames.Main);
        }
    }
}
