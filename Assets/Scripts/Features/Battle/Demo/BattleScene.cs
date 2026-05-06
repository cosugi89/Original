using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Logic;
using Assets.Scripts.Features.Battle.Presentation;
using Assets.Scripts.Features.Battle.Runtime;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 のバトル進行を制御するコントローラ。
    /// スキル選択とパス入力を受け付け、確定パスを解決して
    /// HP・スキル状態・敵行動フラグを更新する。
    /// </summary>
    public class BattleScene : MonoBehaviour
    {
        private const float MinimumAnimationDurationSeconds = 0.1f;

        private List<BattleSkillSlotRuntime> skillSlots = new();

        [Header("Preview")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform enemyRoot;

        [Header("UI")]
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Image previewImage;
        [SerializeField] private TMP_Text turnNumberText;
        [SerializeField] private TMP_Text waveNumberText;
        [SerializeField] private TMP_Text playerHPText;

        [Header("Characters")]
        // Player
        [SerializeField] private Slider playerHPSlider;
        [SerializeField] private GameObject playerActionHistory; // TODO: 実装。UI装飾なので優先度低
        [SerializeField] private GameObject playerSpeechBubble; // TODO

        // Enemy
        [SerializeField] private TMP_Text enemyNameText;
        [SerializeField] private Slider enemyHPSlider;
        [SerializeField] private GameObject enemyActionHistory; // TODO
        [SerializeField] private GameObject enemySpeechBubble; // TODO

        [Header("Input")]
        [SerializeField] private Button skillButton1;
        [SerializeField] private Button skillButton2;
        [SerializeField] private Button skillButton3;
        [SerializeField] private Button skillButton4;

        [Header("Presentation")]
        [SerializeField] private Transform panelsRoot;
        [SerializeField] private BattleBoardController boardController;
        [SerializeField] private BattleTraceInputHandler traceInputHandler;
        [SerializeField] private BattleTraceLineView traceLineView;
        [SerializeField] private BattleHudController hudController;
        [SerializeField] private bool preferInteractiveTraceInput;

        [Header("Skill Button Visuals")]
        [SerializeField] private Color normalSkillButtonColor = new(1f, 1f, 1f, 1f);
        [SerializeField] private Color selectedSkillButtonColor = new(1f, 0.85f, 0.35f, 1f);
        
        private IReadOnlyList<StageTurnData> _turnDefinitions = Array.Empty<StageTurnData>();
        private PartsManager _playerInstance;
        private PartsManager _enemyInstance;
        private readonly BattleSessionState _session = new();
        private StageBattleData _activeBattleData;
        private StageTurnData _preparedTurnDefinition;
        private BattleBoardState _preparedBoard;
        private UserBattleProfileData _activeBattleProfile;
        private bool _presentationBootstrapped;
        private CancellationTokenSource _awaitNodePathCts;
        private CancellationTokenSource _turnExecutionCts;

        private bool _isInitialized;
        private bool _isExecutingTurn;

        private void Awake()
        {
            if (skillButton1 != null) skillButton1.onClick.AddListener(OnClickSkillButton1);
            if (skillButton2 != null) skillButton2.onClick.AddListener(OnClickSkillButton2);
            if (skillButton3 != null) skillButton3.onClick.AddListener(OnClickSkillButton3);
            if (skillButton4 != null) skillButton4.onClick.AddListener(OnClickSkillButton4);
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (skillButton1 != null) skillButton1.onClick.RemoveListener(OnClickSkillButton1);
            if (skillButton2 != null) skillButton2.onClick.RemoveListener(OnClickSkillButton2);
            if (skillButton3 != null) skillButton3.onClick.RemoveListener(OnClickSkillButton3);
            if (skillButton4 != null) skillButton4.onClick.RemoveListener(OnClickSkillButton4);
            CancelAwaitNodePathInput();
            CancelTurnExecution();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (!TryResolveStageData(out var stageData))
            {
                return;
            }

            ApplyStageSceneState(stageData);
            ResetRuntimeState(stageData);
            BootstrapPresentationIfNeeded();
            PrepareUpcomingTurnPresentation();
            RefreshHud();
            RefreshSkillButtonVisuals();
            Debug.Log($"[Battle] 初期化完了 Enemy={_session.EnemyName} HP={_session.InitialEnemyHp}");

            SpawnPreviewCharacters();
        }

        public void ExecuteTurn()
        {
            ExecuteTurnAsync().Forget();
        }

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

            var turnDefinition = PeekCurrentTurnDefinition();
            if (turnDefinition == null)
            {
                Debug.LogWarning("[Battle] 現在ステージに有効な turnDefinition がありません。");
                return;
            }
            _isExecutingTurn = true;
            CancelTurnExecution();
            _turnExecutionCts = new CancellationTokenSource();

            try
            {
                BattleTurnResolutionReport result;
                IReadOnlyList<BattleCellState> executedCellPath;

                if (preferInteractiveTraceInput && traceInputHandler != null)
                {
                    if (!TryResolveInteractiveTurn(turnDefinition, out result, out executedCellPath))
                    {
                        return;
                    }
                }
                else
                {
                    if (!TryResolveFallbackTurn(turnDefinition, out result, out executedCellPath))
                    {
                        return;
                    }
                }

                await PlayPlayerActionAnimationsAsync(result, _turnExecutionCts.Token);

                ConsumeCurrentTurnScript();

                ApplyResolutionResult(result, turnDefinition);
                ConsumeSelectedSkillIfNeeded(result);

                ApplyPostTurnEnemyState(turnDefinition);
                PrepareUpcomingTurnPresentation();
                RefreshHud();
                LogResolutionReport(turnDefinition, result);
                Debug.Log($"[Battle] Turn {_session.TurnNumber} 終了 PlayerHP={_session.PlayerHp} EnemyHP={_session.EnemyHp}");

                if (_session.BattleEnded)
                {
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
            UpdateInteractiveTurnPreview();
        }

        /// <summary>
        /// 指定スロットのスキル選択状態を更新する。
        /// </summary>
        public void SelectSkillSlot(int slotIndex)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_session.BattleEnded || _session.IsPathInputLocked)
            {
                return;
            }

            if (slotIndex < 0 || slotIndex >= skillSlots.Count)
            {
                return;
            }

            var slot = skillSlots[slotIndex];
            if (!slot.IsReady)
            {
                Debug.Log($"[Battle] スキル{slotIndex + 1}は使用不可 ({slot.GetStateLabel()})");
                return;
            }

            _session.SelectedSkillSlotIndex = _session.SelectedSkillSlotIndex == slotIndex ? -1 : slotIndex;
            RefreshSkillButtonVisuals();
            UpdateInteractiveTurnPreview();
        }

        private void OnClickSkillButton1() => SelectSkillSlot(0);

        private void OnClickSkillButton2() => SelectSkillSlot(1);

        private void OnClickSkillButton3() => SelectSkillSlot(2);

        private void OnClickSkillButton4() => SelectSkillSlot(3);

        private void ResolvePlayerBattleProfile()
        {
            var saveService = GameSaveService.EnsureInitialized();
            var userData = saveService.Session.UserData ?? new UserData();
            userData.Profile ??= new UserProfileData();
            userData.Profile.BattleProfile ??= new UserBattleProfileData();

            _activeBattleProfile = userData.Profile.BattleProfile;
            _activeBattleProfile.MaxHp = Mathf.Max(1, _activeBattleProfile.MaxHp);
            _activeBattleProfile.NormalAttackDamage = Mathf.Max(1, _activeBattleProfile.NormalAttackDamage);
            _activeBattleProfile.DoubleAttackFollowUpDamage = Mathf.Max(1, _activeBattleProfile.DoubleAttackFollowUpDamage);
            _activeBattleProfile.JumpAttackDamage = Mathf.Max(1, _activeBattleProfile.JumpAttackDamage);
            skillSlots = CreateRuntimeSkillSlots(_activeBattleProfile);

            Debug.Log($"[Battle] UserData BattleProfile HP={_activeBattleProfile.MaxHp} Attack={_activeBattleProfile.NormalAttackDamage}/{_activeBattleProfile.DoubleAttackFollowUpDamage}/{_activeBattleProfile.JumpAttackDamage} SkillSlots={skillSlots.Count}");
        }

        private void RefreshSkillButtonVisuals()
        {
            var buttons = new[] { skillButton1, skillButton2, skillButton3, skillButton4 };

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                var targetColor = i == _session.SelectedSkillSlotIndex ? selectedSkillButtonColor : normalSkillButtonColor;

                var colors = button.colors;
                colors.normalColor = targetColor;
                colors.highlightedColor = targetColor;
                colors.selectedColor = targetColor;
                colors.pressedColor = targetColor * 0.9f;
                button.colors = colors;

                if (button.targetGraphic != null)
                {
                    button.targetGraphic.color = targetColor;
                }
            }
        }

        private StageTurnData PeekCurrentTurnDefinition()
        {
            if (_turnDefinitions == null || _turnDefinitions.Count == 0)
            {
                return null;
            }

            var index = Mathf.Clamp(_session.TurnScriptIndex, 0, _turnDefinitions.Count - 1);
            return _turnDefinitions[index];
        }

        private bool TryResolveStageData(out StageData stageData)
        {
            stageData = null;
            var stageId = ResolveStageIdForSession();
            if (stageId < 0)
            {
                Debug.LogWarning("[Battle] StageId を解決できなかったため、バトル初期化を中断しました。");
                return false;
            }

            if (!MasterDataResourceLoader.TryLoadStageData(stageId, out stageData))
            {
                Debug.LogWarning($"[Battle] stageId={stageId} のステージデータ読込に失敗しました。");
                return false;
            }

            return true;
        }

        private void ApplyStageSceneState(StageData stageData)
        {
            if (stageData == null)
            {
                return;
            }

            BattleProgressService.EnsureInitialized().SetCurrentStage(stageData.StageId);

            if (background != null)
            {
                background.sprite = stageData.BackgroundImage;
            }

            if (previewImage != null)
            {
                previewImage.sprite = stageData.PreviewImage;
            }
        }

        private void ResetRuntimeState(StageData stageData)
        {
            ResolvePlayerBattleProfile();
            _activeBattleData = stageData.Battle ?? new StageBattleData();
            _turnDefinitions = _activeBattleData.TurnDefinitions ?? Array.Empty<StageTurnData>();

            var enemyName = string.Empty;
            var initialEnemyHp = 1;
            if (_activeBattleData.Enemy != null &&
                !string.IsNullOrWhiteSpace(_activeBattleData.Enemy.Name))
            {
                enemyName = _activeBattleData.Enemy.Name;
                initialEnemyHp = Mathf.Max(1, _activeBattleData.Enemy.MaxHp);
            }
            else if (stageData.Enemies != null && stageData.Enemies.Count > 0)
            {
                var firstEnemy = stageData.Enemies[0];
                if (!string.IsNullOrWhiteSpace(firstEnemy.Name))
                {
                    enemyName = firstEnemy.Name;
                }

                initialEnemyHp = Mathf.Max(1, firstEnemy.Hp);
            }

            _session.ResetForBattle(enemyName, _activeBattleProfile.MaxHp, initialEnemyHp);
            _isInitialized = true;

            if (_turnDefinitions.Count == 0)
            {
                Debug.LogWarning($"[Battle] stageId={stageData.StageId} に turnDefinitions がありません。");
            }

            foreach (var slot in skillSlots)
            {
                slot.ResetRuntime();
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
        }

        private void GainTurnChargeToAllSkills()
        {
            foreach (var slot in skillSlots)
            {
                slot.GainTurnCharge();
            }
        }

        private void ApplyResolutionResult(BattleTurnResolutionReport result, StageTurnData turnDefinition)
        {
            if (result.EnemyDamageTaken > 0)
            {
                _session.ApplyEnemyDamage(result.EnemyDamageTaken);
                Debug.Log($"[Battle] {GetTurnLabel(turnDefinition)} EnemyDamage={result.EnemyDamageTaken}");
            }

            if (result.PlayerDamageTaken > 0)
            {
                _session.ApplyPlayerDamage(result.PlayerDamageTaken);
                Debug.Log($"[Battle] {GetTurnLabel(turnDefinition)} PlayerDamage={result.PlayerDamageTaken}");
            }

            if (result.EnemyDefeated)
            {
                HandleBattleClear();
                return;
            }

            if (result.PlayerDefeated || _session.PlayerHp <= 0)
            {
                _session.BattleEnded = true;
            }
        }

        private void ApplyPostTurnEnemyState(StageTurnData turnDefinition)
        {
            if (_session.BattleEnded)
            {
                return;
            }

            if (turnDefinition != null && turnDefinition.EnemyAction == BattleEnemyActionType.Dance)
            {
                _session.ReserveNextTurnHazardBoost();
            }
        }

        private void RefreshHud()
        {
            if (hudController != null)
            {
                hudController.ApplySession(_session);
            }

            if (turnNumberText != null)
            {
                turnNumberText.text = $"Turn {_session.TurnNumber}";
            }

            if (waveNumberText != null)
            {
                waveNumberText.text = $"Wave {_session.WaveNumber}";
            }
        }

        private void PrepareUpcomingTurnPresentation()
        {
            if (_session.BattleEnded)
            {
                Debug.Log("[Battle] バトル終了のため盤面入力をクリアします。");
                CancelAwaitNodePathInput();
                traceInputHandler?.ResetState(clearConfirmedPath: true);
                boardController?.ClearTraceVisual();
                traceLineView?.Clear();
                hudController?.SetConfirmText(BuildTurnPrompt(_preparedTurnDefinition));
                return;
            }

            var turnDefinition = PeekCurrentTurnDefinition();
            _preparedTurnDefinition = turnDefinition;
            hudController?.SetConfirmText(BuildTurnPrompt(turnDefinition));

            var layout = StageBattleRuntimeAdapter.CreateBoardLayout(_activeBattleData, turnDefinition);
            _preparedBoard = layout.Board;
            Debug.Log($"[Battle] 次ターン準備 Turn={_session.TurnNumber} Label={GetTurnLabel(turnDefinition)} Board={_preparedBoard?.Width}x{_preparedBoard?.Height} Start={_preparedBoard?.StartPosition} Goals={FormatPath(_preparedBoard?.GetGoalPositions())}");

            if (boardController != null)
            {
                boardController.RenderBoard(_preparedBoard);
            }

            if (traceInputHandler != null && boardController != null && _preparedBoard != null)
            {
                traceInputHandler.Bind(
                    boardController,
                    new BattlePathTracer(_preparedBoard),
                    traceLineView);
            }
            else
            {
                traceLineView?.Clear();
            }

            BeginAwaitNodePathInput(turnDefinition);
        }

        private bool TryResolveInteractiveTurn(
            StageTurnData turnDefinition,
            out BattleTurnResolutionReport result,
            out IReadOnlyList<BattleCellState> executedCellPath)
        {
            result = null;
            executedCellPath = null;
            if (!preferInteractiveTraceInput ||
                traceInputHandler == null ||
                _preparedBoard == null ||
                !ReferenceEquals(turnDefinition, _preparedTurnDefinition) ||
                !traceInputHandler.HasConfirmedPath)
            {
                hudController?.SetConfirmText(BuildTurnPrompt(turnDefinition));
                Debug.LogWarning($"[Battle] ExecuteTurn を保留 Interactive={preferInteractiveTraceInput} TraceHandler={(traceInputHandler != null)} BoardReady={(_preparedBoard != null)} TurnMatched={ReferenceEquals(turnDefinition, _preparedTurnDefinition)} HasConfirmedPath={traceInputHandler != null && traceInputHandler.HasConfirmedPath}");
                return false;
            }

            var hazardBoosted = _session.ConsumeHazardBoostFlag();
            _session.AdvanceTurn();
            GainTurnChargeToAllSkills();
            _session.IsPathInputLocked = true;

            var context = StageBattleRuntimeAdapter.CreateTurnContext(
                turnDefinition,
                hazardBoosted,
                CreateSelectedSkillRuntime(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage);

            var cellPath = _preparedBoard.BuildCellPath(traceInputHandler.ConfirmedPath);
            var path = _preparedBoard.BuildNodePath(traceInputHandler.ConfirmedPath);
            Debug.Log($"[Battle] Interactive path を解決します Path={FormatPath(traceInputHandler.ConfirmedPath)} Nodes={FormatNodePath(path)}");
            result = BattleTurnResolver.Resolve(cellPath, context);
            executedCellPath = cellPath;
            result.AddLog($"Interactive path length: {traceInputHandler.ConfirmedPath.Count}");
            return true;
        }

        private bool TryResolveFallbackTurn(
            StageTurnData turnDefinition,
            out BattleTurnResolutionReport result,
            out IReadOnlyList<BattleCellState> executedCellPath)
        {
            result = null;
            executedCellPath = null;

            var layout = StageBattleRuntimeAdapter.CreateBoardLayout(_activeBattleData, turnDefinition);
            var tracePositions = layout.FallbackTracePositions;
            if (layout.Board == null || tracePositions == null || tracePositions.Count == 0)
            {
                Debug.LogWarning($"[Battle] Fallback turn path を構築できませんでした Label={GetTurnLabel(turnDefinition)}");
                return false;
            }

            var hazardBoosted = _session.ConsumeHazardBoostFlag();
            _session.AdvanceTurn();
            GainTurnChargeToAllSkills();
            _session.IsPathInputLocked = true;

            var context = StageBattleRuntimeAdapter.CreateTurnContext(
                turnDefinition,
                hazardBoosted,
                CreateSelectedSkillRuntime(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage);

            executedCellPath = layout.Board.BuildCellPath(tracePositions);
            result = BattleTurnResolver.Resolve(executedCellPath, context);
            result.AddLog($"Fallback path length: {tracePositions.Count}");
            return true;
        }

        private void BootstrapPresentationIfNeeded()
        {
            if (_presentationBootstrapped)
            {
                return;
            }

            if (panelsRoot == null)
            {
                Debug.LogWarning("[Battle] panelsRoot が未設定のため、盤面入力を初期化できません。");
            }
            else
            {
                if (boardController == null)
                {
                    boardController = panelsRoot.GetComponent<BattleBoardController>();
                    if (boardController == null)
                    {
                        boardController = panelsRoot.gameObject.AddComponent<BattleBoardController>();
                    }
                }

                boardController.ConfigureExistingChildren(
                    panelsRoot as RectTransform,
                    panelsRoot.GetComponent<GridLayoutGroup>());
                preferInteractiveTraceInput = true;
                Debug.Log($"[Battle] panelsRoot を接続しました ChildCount={panelsRoot.childCount} Interactive={preferInteractiveTraceInput}");
            }

            if (traceInputHandler == null)
            {
                traceInputHandler = GetComponent<BattleTraceInputHandler>();
                if (traceInputHandler == null)
                {
                    traceInputHandler = gameObject.AddComponent<BattleTraceInputHandler>();
                }
            }

            if (hudController == null)
            {
                hudController = GetComponent<BattleHudController>();
                if (hudController == null)
                {
                    hudController = gameObject.AddComponent<BattleHudController>();
                }
            }

            hudController?.ConfigureLegacyReferences(turnNumberText, waveNumberText, playerHp: playerHPText);

            if (traceInputHandler != null)
            {
                traceInputHandler.TraceUpdated -= OnTraceUpdated;
                traceInputHandler.TraceRejected -= OnTraceRejected;
                traceInputHandler.PathConfirmed -= OnPathConfirmed;
                traceInputHandler.TraceUpdated += OnTraceUpdated;
                traceInputHandler.TraceRejected += OnTraceRejected;
                traceInputHandler.PathConfirmed += OnPathConfirmed;
            }

            _presentationBootstrapped = true;
            Debug.Log($"[Battle] Presentation bootstrap 完了 BoardController={(boardController != null)} TraceInputHandler={(traceInputHandler != null)} HudController={(hudController != null)} Interactive={preferInteractiveTraceInput}");
        }

        private void ConsumeCurrentTurnScript()
        {
            var lastIndex = _turnDefinitions != null && _turnDefinitions.Count > 0
                ? _turnDefinitions.Count - 1
                : 0;
            _session.TurnScriptIndex = Mathf.Min(_session.TurnScriptIndex + 1, lastIndex);
        }

        private void OnTraceUpdated(BattlePathTraceResult result)
        {
            if (_preparedTurnDefinition == null)
            {
                return;
            }

            if (hudController == null)
            {
                return;
            }

            if (result.Status == BattlePathTraceStatus.Confirmed)
            {
                return;
            }

            if (result.CanConfirm)
            {
                UpdateInteractiveTurnPreview();
                return;
            }

            if (result.Status == BattlePathTraceStatus.ReleasedWithoutGoal)
            {
                hudController.SetConfirmText("Goal まで届いていません");
                return;
            }

            if (result.Status == BattlePathTraceStatus.Started ||
                result.Status == BattlePathTraceStatus.Appended ||
                result.Status == BattlePathTraceStatus.Backtracked)
            {
                hudController.SetConfirmText("Goal までつなげると行動確定");
                return;
            }

            hudController.SetConfirmText(BuildTurnPrompt(_preparedTurnDefinition));
        }

        private void OnTraceRejected(BattlePathTraceResult result)
        {
            if (hudController == null)
            {
                return;
            }

            hudController.SetConfirmText(result.ValidationError switch
            {
                BattlePathValidationError.NotAdjacent => "隣接マスのみです",
                BattlePathValidationError.Revisit => "同じマスは再訪できません",
                BattlePathValidationError.CrossedSegment => "線は交差できません",
                BattlePathValidationError.ExtendedAfterGoal => "Goal 到達後は延長できません",
                BattlePathValidationError.InvalidStart => "開始マスから引いてください",
                _ => "そのルートは使えません",
            });
        }

        private void OnPathConfirmed(IReadOnlyList<Assets.Scripts.Features.Battle.Core.BattleGridPosition> _)
        {
            if (_preparedTurnDefinition == null)
            {
                return;
            }

            UpdateInteractiveTurnPreview();
        }

        private void ConsumeSelectedSkillIfNeeded(BattleTurnResolutionReport result)
        {
            if (_session.SelectedSkillSlotIndex < 0 || _session.SelectedSkillSlotIndex >= skillSlots.Count)
            {
                return;
            }

            if (!result.SelectedSkillConsumed)
            {
                return;
            }

            skillSlots[_session.SelectedSkillSlotIndex].Consume();
            _session.SelectedSkillSlotIndex = -1;
            RefreshSkillButtonVisuals();
        }

        private void BeginAwaitNodePathInput(StageTurnData turnDefinition)
        {
            CancelAwaitNodePathInput();

            if (!preferInteractiveTraceInput ||
                traceInputHandler == null ||
                _preparedBoard == null ||
                _session.BattleEnded)
            {
                return;
            }

            _awaitNodePathCts = new CancellationTokenSource();
            AwaitNodePathInputAsync(turnDefinition, _awaitNodePathCts.Token).Forget();
            Debug.Log($"[Battle] UniTask で path 入力待機を開始します Turn={_session.TurnNumber} Label={GetTurnLabel(turnDefinition)}");
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

        private async UniTaskVoid AwaitNodePathInputAsync(StageTurnData turnDefinition, CancellationToken cancellationToken)
        {
            try
            {
                var confirmedPath = await traceInputHandler.WaitForConfirmedPathAsync(cancellationToken);
                Debug.Log($"[Battle] UniTask 待機完了 Path={FormatPath(confirmedPath)}");
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (cancellationToken.IsCancellationRequested ||
                    _session.BattleEnded ||
                    _session.IsPathInputLocked ||
                    traceInputHandler == null ||
                    !traceInputHandler.HasConfirmedPath ||
                    !ReferenceEquals(turnDefinition, _preparedTurnDefinition))
                {
                    return;
                }

                Debug.Log("[Battle] UniTask 待機の完了によりターンを実行します。");
                ExecuteTurn();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[Battle] UniTask path 入力待機をキャンセルしました。");
            }
        }

        private void UpdateInteractiveTurnPreview()
        {
            if (!preferInteractiveTraceInput || hudController == null || _preparedTurnDefinition == null)
            {
                return;
            }

            if (traceInputHandler == null || _preparedBoard == null)
            {
                hudController.SetConfirmText(BuildTurnPrompt(_preparedTurnDefinition));
                return;
            }

            var pathPositions = traceInputHandler.CurrentTracePath;
            if (pathPositions == null || pathPositions.Count == 0)
            {
                hudController.SetConfirmText(BuildTurnPrompt(_preparedTurnDefinition));
                return;
            }

            if (!traceInputHandler.CanConfirmCurrentTrace && !traceInputHandler.HasConfirmedPath)
            {
                hudController.SetConfirmText("Goal までつなげると行動確定");
                return;
            }

            if (!TryBuildInteractiveTurnPreview(pathPositions, out var preview, out var path))
            {
                hudController.SetConfirmText("このルートはまだ解決できません");
                return;
            }

            hudController.SetConfirmText(BuildTurnPreviewMessage(preview));
            Debug.Log($"[Battle] PlayerTurn preview Path={FormatPath(pathPositions)} Nodes={FormatNodePath(path)} Summary={BuildTurnPreviewMessage(preview)}");
        }

        private bool TryBuildInteractiveTurnPreview(
            IReadOnlyList<BattleGridPosition> positions,
            out BattleTurnResolutionReport preview,
            out List<BattleNodeType> path)
        {
            preview = null;
            path = null;

            if (_preparedBoard == null || positions == null || positions.Count == 0)
            {
                return false;
            }

            path = _preparedBoard.BuildNodePath(positions);
            if (path.Count == 0)
            {
                return false;
            }

            preview = BattleTurnResolver.Resolve(
                _preparedBoard.BuildCellPath(positions),
                CreateCurrentTurnContext(_preparedTurnDefinition, _session.NextTurnHazardBoosted));
            return true;
        }

        private BattleTurnContext CreateCurrentTurnContext(StageTurnData turnDefinition, bool hazardBoosted)
        {
            return StageBattleRuntimeAdapter.CreateTurnContext(
                turnDefinition,
                hazardBoosted,
                CreateSelectedSkillRuntime(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage);
        }

        private string BuildTurnPrompt(StageTurnData turnDefinition)
        {
            if (_session.BattleEnded)
            {
                return "バトル終了";
            }

            if (!preferInteractiveTraceInput)
            {
                return turnDefinition != null ? turnDefinition.ConfirmText : string.Empty;
            }

            if (turnDefinition != null && !string.IsNullOrWhiteSpace(turnDefinition.BoardSummary))
            {
                return $"{turnDefinition.BoardSummary} Start から Goal まで引いてください";
            }

            return "Start から Goal まで引いてください";
        }

        private string BuildTurnPreviewMessage(BattleTurnResolutionReport preview)
        {
            if (preview == null)
            {
                return _preparedTurnDefinition != null ? _preparedTurnDefinition.ConfirmText : string.Empty;
            }

            var parts = new List<string>();

            if (preview.ResolvedSkillCount > 0 && GetSelectedSkill() != null)
            {
                parts.Add($"Skill:{GetSelectedSkill().DisplayName}");
            }
            else if (preview.ResolvedAttackCount > 0)
            {
                parts.Add($"攻撃{preview.ResolvedAttackCount}回");
            }

            if (preview.EnemyDamageTaken > 0)
            {
                parts.Add($"敵-{preview.EnemyDamageTaken}");
            }

            if (preview.ResolvedHazardCount > 0)
            {
                parts.Add(preview.PlayerDamageTaken > 0
                    ? $"自分-{preview.PlayerDamageTaken}"
                    : "被弾なし");
            }
            else if (preview.ResolvedEnemyActionCount > 0 && preview.PlayerDamageTaken > 0)
            {
                parts.Add($"敵攻撃-{preview.PlayerDamageTaken}");
            }

            if (preview.StoppedByHazardHit)
            {
                parts.Add("被弾で停止");
            }

            if (preview.GoalReached)
            {
                parts.Add("Goal確定");
            }

            if (parts.Count == 0)
            {
                return _preparedTurnDefinition != null ? _preparedTurnDefinition.ConfirmText : "実行可能";
            }

            return string.Join(" / ", parts);
        }

        private int ResolveStageIdForSession()
        {
            var transitionStageId = BattleSceneTransitionState.ConsumeSelectedStageId();
            if (transitionStageId >= 0)
            {
                return transitionStageId;
            }

            return BattleProgressService.EnsureInitialized().GetCurrentOrRecommendedStageId();
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

        private BattleSkillSlotRuntime GetSelectedSkill()
        {
            if (_session.SelectedSkillSlotIndex < 0 || _session.SelectedSkillSlotIndex >= skillSlots.Count)
            {
                return null;
            }
            return skillSlots[_session.SelectedSkillSlotIndex];
        }

        private BattleSkillSlotRuntime CreateSelectedSkillRuntime()
        {
            var selectedSkill = GetSelectedSkill();
            return CloneSkillSlotRuntime(selectedSkill);
        }

        private static List<BattleSkillSlotRuntime> CreateRuntimeSkillSlots(UserBattleProfileData battleProfile)
        {
            var runtimeSlots = new List<BattleSkillSlotRuntime>();
            var slotDataList = battleProfile?.SkillSlots;
            if (slotDataList == null)
            {
                return runtimeSlots;
            }

            for (var i = 0; i < slotDataList.Count; i++)
            {
                runtimeSlots.Add(CreateRuntimeSkillSlot(slotDataList[i]));
            }

            return runtimeSlots;
        }

        private static BattleSkillSlotRuntime CreateRuntimeSkillSlot(UserBattleSkillSlotData slotData)
        {
            slotData ??= new UserBattleSkillSlotData();

            return new BattleSkillSlotRuntime
            {
                DisplayName = slotData.DisplayName ?? "Skill",
                Description = slotData.Description ?? string.Empty,
                IsUnlocked = slotData.IsUnlocked,
                IsConfigured = slotData.IsConfigured,
                RequiredCharge = Mathf.Max(1, slotData.RequiredCharge),
                StartingCharge = Mathf.Max(0, slotData.StartingCharge),
                TurnChargeGain = Mathf.Max(0, slotData.TurnChargeGain),
                AttackChargeGain = Mathf.Max(0, slotData.AttackChargeGain),
                Damage = Mathf.Max(0, slotData.Damage),
            };
        }

        private static BattleSkillSlotRuntime CloneSkillSlotRuntime(BattleSkillSlotRuntime source)
        {
            if (source == null)
            {
                return null;
            }

            var clone = new BattleSkillSlotRuntime
            {
                DisplayName = source.DisplayName,
                Description = source.Description,
                IsUnlocked = source.IsUnlocked,
                IsConfigured = source.IsConfigured,
                RequiredCharge = source.RequiredCharge,
                StartingCharge = source.StartingCharge,
                TurnChargeGain = source.TurnChargeGain,
                AttackChargeGain = source.AttackChargeGain,
                Damage = source.Damage,
            };
            clone.SetCurrentCharge(source.CurrentCharge);
            return clone;
        }

        private void LogResolutionReport(StageTurnData turnDefinition, BattleTurnResolutionReport result)
        {
            if (result == null || result.LogEntries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < result.LogEntries.Count; i++)
            {
                Debug.Log($"[Battle] {GetTurnLabel(turnDefinition)} {result.LogEntries[i]}");
            }
        }

        private static string GetTurnLabel(StageTurnData turnDefinition)
        {
            if (turnDefinition == null || string.IsNullOrWhiteSpace(turnDefinition.DebugLabel))
            {
                return "Turn";
            }

            return turnDefinition.DebugLabel;
        }

        private async UniTask PlayPlayerActionAnimationsAsync(
            BattleTurnResolutionReport result,
            CancellationToken cancellationToken)
        {
            if (_playerInstance == null || result == null)
            {
                return;
            }

            var cues = BuildPlayerAnimationCueSequence(result);
            if (cues.Count == 0)
            {
                return;
            }

            for (var i = 0; i < cues.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var clipName = BattleAnimationCatalog.GetClipName(cues[i]);
                if (string.IsNullOrWhiteSpace(clipName) || !_playerInstance.HasAnimation(clipName))
                {
                    continue;
                }

                _playerInstance.PlayAnimation(clipName);
                Debug.Log($"[Battle] Player animation {clipName} を再生します。");

                var durationSeconds = Mathf.Max(
                    MinimumAnimationDurationSeconds,
                    _playerInstance.GetAnimationLength(clipName));

                await UniTask.Delay(
                    TimeSpan.FromSeconds(durationSeconds),
                    cancellationToken: cancellationToken);
            }

            if (!result.EnemyDefeated && !result.PlayerDefeated && _playerInstance.HasAnimation(BattleAnimationCatalog.Idle))
            {
                _playerInstance.PlayAnimation(BattleAnimationCatalog.Idle);
            }
        }

        private static List<BattlePlayerAnimationCue> BuildPlayerAnimationCueSequence(BattleTurnResolutionReport result)
        {
            var sequence = new List<BattlePlayerAnimationCue>();
            if (result?.PlayerAnimationCues != null)
            {
                sequence.AddRange(result.PlayerAnimationCues);
            }

            if (result != null)
            {
                if (result.PlayerDefeated)
                {
                    sequence.Add(BattlePlayerAnimationCue.Defeat);
                }
                else if (result.EnemyDefeated)
                {
                    sequence.Add(BattlePlayerAnimationCue.Victory);
                }
            }

            return sequence;
        }

        private static string FormatPath(IReadOnlyList<BattleGridPosition> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                return "(empty)";
            }

            return string.Join(" -> ", positions);
        }

        private static string FormatNodePath(IReadOnlyList<BattleNodeType> nodeTypes)
        {
            if (nodeTypes == null || nodeTypes.Count == 0)
            {
                return "(empty)";
            }

            return string.Join(" -> ", nodeTypes);
        }
    }
}
