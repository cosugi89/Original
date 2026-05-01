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
        [Header("Player Skills")]
        [SerializeField] private List<BattleDemoSkillSlot> skillSlots = new();

        [Header("Preview")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform enemyRoot;

        [Header("UI")]
        [SerializeField] public SpriteRenderer background;
        [SerializeField] public Image previewImage;
        [SerializeField] public TMP_Text turnNumberText;
        [SerializeField] public TMP_Text waveNumberText;
        [SerializeField] public TMP_Text playerHPText;

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
        
        private List<BattleDemoTurnScript> _turnScripts;
        private PartsManager _playerInstance;
        private PartsManager _enemyInstance;
        private readonly BattleSessionState _session = new();
        private BattleDemoTurnScript _preparedTurnScript;
        private BattleBoardState _preparedBoard;
        private UserBattleProfileData _activeBattleProfile;
        private bool _presentationBootstrapped;
        private CancellationTokenSource _awaitNodePathCts;

        private bool _isInitialized;
        private int _resolvedStageIdForSession = -1;

        private void Awake()
        {
            skillButton1.onClick.AddListener(OnClickSkillButton1);
            skillButton2.onClick.AddListener(OnClickSkillButton2);
            skillButton3.onClick.AddListener(OnClickSkillButton3);
            skillButton4.onClick.AddListener(OnClickSkillButton4);
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
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            EnsureDefaults(); // デフォルト値の補完
            ResolvePlayerBattleProfile();

            if (!TryResolveStageData(out var stageData))
            {
                return;
            }

            ApplyStageProgress(stageData);
            ApplyStagePresentation(stageData);
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
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_session.BattleEnded)
            {
                Debug.LogWarning($"[Battle] バトル終了済み");
                return;
            }

            var turnScript = PeekCurrentTurnScript();
            BattleTurnResolutionReport result;

            if (preferInteractiveTraceInput && traceInputHandler != null)
            {
                if (!TryResolveInteractiveTurn(turnScript, out result))
                {
                    return;
                }
            }
            else
            {
                var hazardBoosted = _session.ConsumeHazardBoostFlag();
                _session.AdvanceTurn();
                GainTurnChargeToAllSkills();

                _session.IsPathInputLocked = true;
                result = BattleDemoRuntimeAdapter.ResolveWithRuntime(
                    turnScript,
                    hazardBoosted,
                    GetSelectedSkill(),
                    _session.EnemyHp,
                    _session.PlayerHp,
                    _activeBattleProfile.NormalAttackDamage,
                    _activeBattleProfile.DoubleAttackFollowUpDamage,
                    _activeBattleProfile.JumpAttackDamage);
                _session.IsPathInputLocked = false;
            }

            ConsumeCurrentTurnScript();

            ApplyResolutionResult(result, turnScript);
            ConsumeSelectedSkillIfNeeded(result);

            ApplyPostTurnEnemyState(turnScript);
            PrepareUpcomingTurnPresentation();
            RefreshHud();
            LogResolutionReport(turnScript, result);
            Debug.Log($"[Battle] Turn {_session.TurnNumber} 終了 PlayerHP={_session.PlayerHp} EnemyHP={_session.EnemyHp}");

            if (_session.BattleEnded)
            {
                Debug.Log($"[Battle] バトル終了");
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

        private void EnsureDefaults()
        {
            if (skillSlots == null || skillSlots.Count == 0)
            {
                skillSlots = BattleDemoContentFactory.CreateDefaultSkillSlots();
            }

            if (_turnScripts == null || _turnScripts.Count == 0)
            {
                _turnScripts = BattleDemoContentFactory.CreateDefaultTurnScripts();
            }
        }

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

            Debug.Log($"[Battle] UserData BattleProfile HP={_activeBattleProfile.MaxHp} Attack={_activeBattleProfile.NormalAttackDamage}/{_activeBattleProfile.DoubleAttackFollowUpDamage}/{_activeBattleProfile.JumpAttackDamage}");
        }

        private void RefreshSkillButtonVisuals()
        {
            var buttons = new[] { skillButton1, skillButton2, skillButton3, skillButton4 };

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
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

        private BattleDemoTurnScript GetNextTurnScript()
        {
            var turnScript = PeekCurrentTurnScript();
            ConsumeCurrentTurnScript();
            return turnScript;
        }

        private BattleDemoTurnScript PeekCurrentTurnScript()
        {
            if (_turnScripts.Count == 0)
            {
                _turnScripts = BattleDemoContentFactory.CreateDefaultTurnScripts();
            }

            var index = Mathf.Clamp(_session.TurnScriptIndex, 0, _turnScripts.Count - 1);
            return _turnScripts[index];
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

        private void ApplyStageProgress(StageData stageData)
        {
            _resolvedStageIdForSession = stageData.StageId;

            var battleProgress = BattleProgressService.EnsureInitialized();
            battleProgress.SetLastSelectedStage(stageData.StageId);
            battleProgress.Session.SetCurrentStageId(stageData.StageId);
        }

        private void ApplyStagePresentation(StageData stageData)
        {
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

            var enemyName = string.Empty;
            var initialEnemyHp = 1;
            if (stageData.Enemies != null && stageData.Enemies.Count > 0)
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

        private void ApplyResolutionResult(BattleTurnResolutionReport result, BattleDemoTurnScript turnScript)
        {
            if (result.EnemyDamageTaken > 0)
            {
                _session.ApplyEnemyDamage(result.EnemyDamageTaken);
                Debug.Log($"[Battle] {turnScript.Label} EnemyDamage={result.EnemyDamageTaken}");
            }

            if (result.PlayerDamageTaken > 0)
            {
                _session.ApplyPlayerDamage(result.PlayerDamageTaken);
                Debug.Log($"[Battle] {turnScript.Label} PlayerDamage={result.PlayerDamageTaken}");
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

        private void ApplyPostTurnEnemyState(BattleDemoTurnScript turnScript)
        {
            if (_session.BattleEnded)
            {
                return;
            }

            if (turnScript.EnemyAction == BattleDemoEnemyActionType.Dance)
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
                hudController?.SetConfirmText(BuildTurnPrompt(_preparedTurnScript));
                return;
            }

            var turnScript = PeekCurrentTurnScript();
            _preparedTurnScript = turnScript;
            hudController?.SetConfirmText(BuildTurnPrompt(turnScript));

            var layout = BattleDemoRuntimeAdapter.CreateBoardLayout(turnScript);
            _preparedBoard = layout.Board;
            Debug.Log($"[Battle] 次ターン準備 Turn={_session.TurnNumber} Label={turnScript?.Label} Board={_preparedBoard?.Width}x{_preparedBoard?.Height} Start={_preparedBoard?.StartPosition} Goals={FormatPath(_preparedBoard?.GetGoalPositions())}");

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

            BeginAwaitNodePathInput(turnScript);
        }

        private bool TryResolveInteractiveTurn(BattleDemoTurnScript turnScript, out BattleTurnResolutionReport result)
        {
            result = null;
            if (!preferInteractiveTraceInput ||
                traceInputHandler == null ||
                _preparedBoard == null ||
                !ReferenceEquals(turnScript, _preparedTurnScript) ||
                !traceInputHandler.HasConfirmedPath)
            {
                hudController?.SetConfirmText(BuildTurnPrompt(turnScript));
                Debug.LogWarning($"[Battle] ExecuteTurn を保留 Interactive={preferInteractiveTraceInput} TraceHandler={(traceInputHandler != null)} BoardReady={(_preparedBoard != null)} TurnMatched={ReferenceEquals(turnScript, _preparedTurnScript)} HasConfirmedPath={traceInputHandler != null && traceInputHandler.HasConfirmedPath}");
                return false;
            }

            var hazardBoosted = _session.ConsumeHazardBoostFlag();
            _session.AdvanceTurn();
            GainTurnChargeToAllSkills();
            _session.IsPathInputLocked = true;

            var context = BattleDemoRuntimeAdapter.CreateTurnContext(
                turnScript,
                hazardBoosted,
                GetSelectedSkill(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage);

            var path = _preparedBoard.BuildNodePath(traceInputHandler.ConfirmedPath);
            Debug.Log($"[Battle] Interactive path を解決します Path={FormatPath(traceInputHandler.ConfirmedPath)} Nodes={FormatNodePath(path)}");
            result = BattleTurnResolver.Resolve(path, context);
            result.AddLog($"Interactive path length: {traceInputHandler.ConfirmedPath.Count}");
            _session.IsPathInputLocked = false;
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
            _session.TurnScriptIndex = Mathf.Min(_session.TurnScriptIndex + 1, _turnScripts.Count - 1);
        }

        private void OnTraceUpdated(BattlePathTraceResult result)
        {
            if (_preparedTurnScript == null)
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

            hudController.SetConfirmText(BuildTurnPrompt(_preparedTurnScript));
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
            if (_preparedTurnScript == null)
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

        private void BeginAwaitNodePathInput(BattleDemoTurnScript turnScript)
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
            AwaitNodePathInputAsync(turnScript, _awaitNodePathCts.Token).Forget();
            Debug.Log($"[Battle] UniTask で path 入力待機を開始します Turn={_session.TurnNumber} Label={turnScript?.Label}");
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

        private async UniTaskVoid AwaitNodePathInputAsync(BattleDemoTurnScript turnScript, CancellationToken cancellationToken)
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
                    !ReferenceEquals(turnScript, _preparedTurnScript))
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
            if (!preferInteractiveTraceInput || hudController == null || _preparedTurnScript == null)
            {
                return;
            }

            if (traceInputHandler == null || _preparedBoard == null)
            {
                hudController.SetConfirmText(BuildTurnPrompt(_preparedTurnScript));
                return;
            }

            var pathPositions = traceInputHandler.CurrentTracePath;
            if (pathPositions == null || pathPositions.Count == 0)
            {
                hudController.SetConfirmText(BuildTurnPrompt(_preparedTurnScript));
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
                path,
                CreateCurrentTurnContext(_preparedTurnScript, _session.NextTurnHazardBoosted));
            return true;
        }

        private BattleTurnContext CreateCurrentTurnContext(BattleDemoTurnScript turnScript, bool hazardBoosted)
        {
            return BattleDemoRuntimeAdapter.CreateTurnContext(
                turnScript,
                hazardBoosted,
                GetSelectedSkill(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage);
        }

        private string BuildTurnPrompt(BattleDemoTurnScript turnScript)
        {
            if (_session.BattleEnded)
            {
                return "バトル終了";
            }

            if (!preferInteractiveTraceInput)
            {
                return turnScript != null ? turnScript.ConfirmText : string.Empty;
            }

            if (turnScript != null && !string.IsNullOrWhiteSpace(turnScript.BoardSummary))
            {
                return $"{turnScript.BoardSummary} Start から Goal まで引いてください";
            }

            return "Start から Goal まで引いてください";
        }

        private string BuildTurnPreviewMessage(BattleTurnResolutionReport preview)
        {
            if (preview == null)
            {
                return _preparedTurnScript != null ? _preparedTurnScript.ConfirmText : string.Empty;
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
                return _preparedTurnScript != null ? _preparedTurnScript.ConfirmText : "実行可能";
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

            var battleProgress = BattleProgressService.EnsureInitialized();
            if (battleProgress.Session.CurrentStageId >= 0)
            {
                return battleProgress.Session.CurrentStageId;
            }

            return battleProgress.GetRecommendedStageId();
        }

        private void HandleBattleClear()
        {
            _session.BattleEnded = true;

            if (_resolvedStageIdForSession < 0)
            {
                return;
            }

            var battleProgress = BattleProgressService.EnsureInitialized();
            battleProgress.RecordStageClear(_resolvedStageIdForSession);

            var nextStageId = battleProgress.GetNextStageId(_resolvedStageIdForSession);
            if (nextStageId >= 0)
            {
                battleProgress.UnlockStage(nextStageId);
            }
        }

        private BattleDemoSkillSlot GetSelectedSkill()
        {
            if (_session.SelectedSkillSlotIndex < 0 || _session.SelectedSkillSlotIndex >= skillSlots.Count)
            {
                return null;
            }
            return skillSlots[_session.SelectedSkillSlotIndex];
        }

        private void LogResolutionReport(BattleDemoTurnScript turnScript, BattleTurnResolutionReport result)
        {
            if (result == null || result.LogEntries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < result.LogEntries.Count; i++)
            {
                Debug.Log($"[Battle] {turnScript.Label} {result.LogEntries[i]}");
            }
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
