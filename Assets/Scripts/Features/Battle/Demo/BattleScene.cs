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
        
        private IReadOnlyList<StageBattlePatternData> _patterns = Array.Empty<StageBattlePatternData>();
        private PartsManager _playerInstance;
        private PartsManager _enemyInstance;
        private readonly BattleSessionState _session = new();
        private StageBattleEnemyData _activeEnemyData;
        private StageBattlePatternData _preparedPattern;
        private BattleBoardState _preparedBoard;
        private UserBattleProfileData _activeBattleProfile;
        private AttributeData _playerAttackAttribute;
        private IReadOnlyList<EquipmentAttributeModifierData> _playerDefenseAttributeModifiers = Array.Empty<EquipmentAttributeModifierData>();
        private bool _presentationBootstrapped;
        private CancellationTokenSource _awaitNodePathCts;
        private CancellationTokenSource _turnExecutionCts;

        private bool _isInitialized;
        private bool _isExecutingTurn;
        private StageBattlePatternData _lastPreparedPattern;

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

        /// <summary>
        /// ステージ、戦闘状態、表示を初期化し、最初のターン入力を開始する。
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            if (!TryResolveStageData(out var stageData)) return;

            ApplyStageSceneState(stageData);
            ResetRuntimeState(stageData);
            BootstrapPresentationIfNeeded();
            PrepareUpcomingTurnPresentation();
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

            var pattern = _preparedPattern;
            if (pattern == null)
            {
                Debug.LogWarning("[Battle] 現在ステージに有効な pattern がありません。");
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
                    if (!TryResolveInteractiveTurn(pattern, out result, out executedCellPath))
                    {
                        return;
                    }
                }
                else
                {
                    if (!TryResolveFallbackTurn(pattern, out result, out executedCellPath))
                    {
                        return;
                    }
                }

                await PlayPlayerActionAnimationsAsync(result, _turnExecutionCts.Token);

                ApplyResolutionResult(result, pattern);
                ConsumeSelectedSkillIfNeeded(result);

                ApplyPostTurnEnemyState(pattern);
                PrepareUpcomingTurnPresentation();
                RefreshHud();
                LogResolutionReport(pattern, result);
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

            var avatarService = AvatarService.EnsureInitialized();
            var inventoryService = InventoryService.EnsureInitialized();
            _playerAttackAttribute = null;
            _playerDefenseAttributeModifiers = Array.Empty<EquipmentAttributeModifierData>();

            var equippedParts = avatarService.GetAllPartStates();
            if (equippedParts != null)
            {
                for (var i = 0; i < equippedParts.Count; i++)
                {
                    var partState = equippedParts[i];
                    if (partState == null ||
                        partState.EquipmentId <= 0 ||
                        !inventoryService.TryGetDefinition(partState.EquipmentId, out var definition))
                    {
                        continue;
                    }

                    if (_playerAttackAttribute == null && definition.IsRightHandEquipment && definition.NormalAttackAttribute != null)
                    {
                        _playerAttackAttribute = definition.NormalAttackAttribute;
                    }

                    if (partState.PartType == LayerLab.ArtMakerUnity.PartsType.Chest &&
                        definition.AttributeModifiers != null &&
                        definition.AttributeModifiers.Count > 0)
                    {
                        _playerDefenseAttributeModifiers = definition.AttributeModifiers;
                    }
                }
            }

            Debug.Log($"[Battle] UserData BattleProfile HP={_activeBattleProfile.MaxHp} Attack={_activeBattleProfile.NormalAttackDamage}/{_activeBattleProfile.DoubleAttackFollowUpDamage}/{_activeBattleProfile.JumpAttackDamage} Attr={_playerAttackAttribute?.DisplayName ?? "-"} SkillSlots={skillSlots.Count}");
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

        #region init
        private bool TryResolveStageData(out StageData stageData)
        {
            stageData = null;
            var stageId = BattleSceneTransitionState.ConsumeSelectedStageId();
            if (stageId < 0)
            {
                // 遷移前にステージIDがセットされていない場合は、進行状況から現在選択中のステージIDを取得してみる
                stageId = BattleProgressService.EnsureInitialized().GetCurrentOrRecommendedStageId();

                if (stageId < 0)
                {
                    Debug.LogWarning("[Battle] StageId を解決できなかったため、バトル初期化を中断しました。");
                    return false;
                }
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

        private void ResetRuntimeState(StageData stageData)
        {
            ResolvePlayerBattleProfile();
            _activeEnemyData = stageData.Enemy ?? new StageBattleEnemyData();
            _patterns = _activeEnemyData.Patterns ?? Array.Empty<StageBattlePatternData>();

            var enemyName = string.Empty;
            var initialEnemyHp = 1;
            if (_activeEnemyData != null &&
                !string.IsNullOrWhiteSpace(_activeEnemyData.Name))
            {
                enemyName = _activeEnemyData.Name;
                initialEnemyHp = Mathf.Max(1, _activeEnemyData.MaxHp);
            }

            _session.ResetForBattle(enemyName, _activeBattleProfile.MaxHp, initialEnemyHp);
            _preparedPattern = null;
            _lastPreparedPattern = null;
            _isInitialized = true;

            if (_patterns.Count == 0)
            {
                Debug.LogWarning($"[Battle] stageId={stageData.StageId} に pattern がありません。");
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

            if (_activeEnemyData?.Appearance != null)
            {
                _enemyInstance.ApplyAppearanceData(_activeEnemyData.Appearance);
            }
        }
        #endregion init

        private void GainTurnChargeToAllSkills()
        {
            foreach (var slot in skillSlots)
            {
                slot.GainTurnCharge();
            }
        }

        /// <summary>
        /// resolver の結果を session の HP と勝敗状態へ反映する。
        /// </summary>
        private void ApplyResolutionResult(BattleTurnResolutionReport result, StageBattlePatternData pattern)
        {
            if (result.EnemyDamageTaken > 0)
            {
                _session.ApplyEnemyDamage(result.EnemyDamageTaken);
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} EnemyDamage={result.EnemyDamageTaken}");
            }

            if (result.PlayerDamageTaken > 0)
            {
                _session.ApplyPlayerDamage(result.PlayerDamageTaken);
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} PlayerDamage={result.PlayerDamageTaken}");
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

        /// <summary>
        /// ターン終了後に敵行動由来の継続状態を次ターンへ持ち越す。
        /// </summary>
        private void ApplyPostTurnEnemyState(StageBattlePatternData pattern)
        {
            if (_session.BattleEnded)
            {
                return;
            }

            if (pattern != null && pattern.EnemyAction == BattleEnemyActionType.Dance)
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

        /// <summary>
        /// 次ターンの pattern を選び、盤面生成、入力待機、HUD 文言更新までを行う。
        /// </summary>
        private void PrepareUpcomingTurnPresentation()
        {
            if (_session.BattleEnded)
            {
                Debug.Log("[Battle] バトル終了のため盤面入力をクリアします。");
                CancelAwaitNodePathInput();
                traceInputHandler?.ResetState(clearConfirmedPath: true);
                boardController?.ClearTraceVisual();
                traceLineView?.Clear();
                hudController?.SetConfirmText(BuildTurnPrompt(_preparedPattern));
                return;
            }

            if (_patterns == null || _patterns.Count == 0)
            {
                _preparedPattern = null;
                _preparedBoard = null;
                hudController?.SetConfirmText("有効な pattern がありません");
                boardController?.ClearTraceVisual();
                traceLineView?.Clear();
                return;
            }

            var candidateIndices = new List<int>(_patterns.Count);
            for (var i = 0; i < _patterns.Count; i++)
            {
                var candidate = _patterns[i];
                if (candidate != null)
                {
                    candidateIndices.Add(i);
                }
            }

            if (candidateIndices.Count == 0)
            {
                _preparedPattern = null;
                _preparedBoard = null;
                hudController?.SetConfirmText("有効な pattern がありません");
                boardController?.ClearTraceVisual();
                traceLineView?.Clear();
                return;
            }

            if (candidateIndices.Count > 1 && _lastPreparedPattern != null)
            {
                candidateIndices.RemoveAll(index =>
                {
                    var candidate = _patterns[index];
                    return ReferenceEquals(candidate, _lastPreparedPattern);
                });

                if (candidateIndices.Count == 0)
                {
                    for (var i = 0; i < _patterns.Count; i++)
                    {
                        if (_patterns[i] != null)
                        {
                            candidateIndices.Add(i);
                        }
                    }
                }
            }

            var selectedIndex = candidateIndices[UnityEngine.Random.Range(0, candidateIndices.Count)];
            _preparedPattern = _patterns[selectedIndex];
            _lastPreparedPattern = _preparedPattern;
            hudController?.SetConfirmText(BuildTurnPrompt(_preparedPattern));

            var layout = StageBattleRuntimeAdapter.CreateBoardLayout(_preparedPattern);
            _preparedBoard = layout.Board;
            Debug.Log($"[Battle] 次ターン準備 Turn={_session.TurnNumber} Pattern={GetPatternLabel(_preparedPattern)} Action={_preparedPattern.EnemyAction} Board={_preparedBoard?.Width}x{_preparedBoard?.Height} Start={_preparedBoard?.StartPosition} Goals={FormatPath(_preparedBoard?.GetGoalPositions())}");

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

            BeginAwaitNodePathInput(_preparedPattern);
        }

        /// <summary>
        /// 確定済みのプレイヤーパスを論理セル列へ変換し、実際のターン解決を行う。
        /// </summary>
        private bool TryResolveInteractiveTurn(
            StageBattlePatternData pattern,
            out BattleTurnResolutionReport result,
            out IReadOnlyList<BattleCellState> executedCellPath)
        {
            result = null;
            executedCellPath = null;
            if (!preferInteractiveTraceInput ||
                traceInputHandler == null ||
                _preparedBoard == null ||
                !ReferenceEquals(pattern, _preparedPattern) ||
                !traceInputHandler.HasConfirmedPath)
            {
                hudController?.SetConfirmText(BuildTurnPrompt(pattern));
                Debug.LogWarning($"[Battle] ExecuteTurn を保留 Interactive={preferInteractiveTraceInput} TraceHandler={(traceInputHandler != null)} BoardReady={(_preparedBoard != null)} PatternMatched={ReferenceEquals(pattern, _preparedPattern)} HasConfirmedPath={traceInputHandler != null && traceInputHandler.HasConfirmedPath}");
                return false;
            }

            var hazardBoosted = _session.ConsumeHazardBoostFlag();
            _session.AdvanceTurn();
            GainTurnChargeToAllSkills();
            _session.IsPathInputLocked = true;

            var context = StageBattleRuntimeAdapter.CreateTurnContext(
                pattern,
                _activeEnemyData != null ? _activeEnemyData.Damage : 0,
                hazardBoosted,
                CreateSelectedSkillRuntime(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage,
                _playerAttackAttribute,
                _activeEnemyData?.AttackAttribute,
                _activeEnemyData?.DefenseAttributeModifiers,
                _playerDefenseAttributeModifiers);

            var cellPath = _preparedBoard.BuildCellPath(traceInputHandler.ConfirmedPath);
            var path = _preparedBoard.BuildNodePath(traceInputHandler.ConfirmedPath);
            Debug.Log($"[Battle] Interactive path を解決します Path={FormatPath(traceInputHandler.ConfirmedPath)} Nodes={FormatNodePath(path)}");
            result = BattleTurnResolver.Resolve(cellPath, context);
            executedCellPath = cellPath;
            result.AddLog($"Interactive path length: {traceInputHandler.ConfirmedPath.Count}");
            return true;
        }

        private bool TryResolveFallbackTurn(
            StageBattlePatternData pattern,
            out BattleTurnResolutionReport result,
            out IReadOnlyList<BattleCellState> executedCellPath)
        {
            result = null;
            executedCellPath = null;

            var layout = StageBattleRuntimeAdapter.CreateBoardLayout(pattern);
            var tracePositions = layout.FallbackTracePositions;
            if (layout.Board == null || tracePositions == null || tracePositions.Count == 0)
            {
                Debug.LogWarning($"[Battle] Fallback turn path を構築できませんでした Pattern={GetPatternLabel(pattern)}");
                return false;
            }

            var hazardBoosted = _session.ConsumeHazardBoostFlag();
            _session.AdvanceTurn();
            GainTurnChargeToAllSkills();
            _session.IsPathInputLocked = true;

            var context = StageBattleRuntimeAdapter.CreateTurnContext(
                pattern,
                _activeEnemyData != null ? _activeEnemyData.Damage : 0,
                hazardBoosted,
                CreateSelectedSkillRuntime(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage,
                _playerAttackAttribute,
                _activeEnemyData?.AttackAttribute,
                _activeEnemyData?.DefenseAttributeModifiers,
                _playerDefenseAttributeModifiers);

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

                boardController.ConfigureGeneratedCells(
                    panelsRoot as RectTransform,
                    panelsRoot.GetComponent<GridLayoutGroup>());
                preferInteractiveTraceInput = true;
                Debug.Log($"[Battle] panelsRoot を生成式盤面 root として接続しました ChildCount={panelsRoot.childCount} Interactive={preferInteractiveTraceInput}");
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

        private void OnTraceUpdated(BattlePathTraceResult result)
        {
            if (_preparedPattern == null)
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

            hudController.SetConfirmText(BuildTurnPrompt(_preparedPattern));
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
            if (_preparedPattern == null)
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

        /// <summary>
        /// 現在の pattern に対するパス確定待機 UniTask を開始する。
        /// </summary>
        private void BeginAwaitNodePathInput(StageBattlePatternData pattern)
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
            AwaitNodePathInputAsync(pattern, _awaitNodePathCts.Token).Forget();
            Debug.Log($"[Battle] UniTask で path 入力待機を開始します Turn={_session.TurnNumber} Pattern={GetPatternLabel(pattern)}");
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

        /// <summary>
        /// パス確定を待ち、条件が変わっていなければ自動で ExecuteTurn へ進める。
        /// </summary>
        private async UniTaskVoid AwaitNodePathInputAsync(StageBattlePatternData pattern, CancellationToken cancellationToken)
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
                    !ReferenceEquals(pattern, _preparedPattern))
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

        /// <summary>
        /// 入力中または確定済みのパスを先読み解決し、HUD にターン予測を表示する。
        /// </summary>
        private void UpdateInteractiveTurnPreview()
        {
            if (!preferInteractiveTraceInput || hudController == null || _preparedPattern == null)
            {
                return;
            }

            if (traceInputHandler == null || _preparedBoard == null)
            {
                hudController.SetConfirmText(BuildTurnPrompt(_preparedPattern));
                return;
            }

            var pathPositions = traceInputHandler.CurrentTracePath;
            if (pathPositions == null || pathPositions.Count == 0)
            {
                hudController.SetConfirmText(BuildTurnPrompt(_preparedPattern));
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

        /// <summary>
        /// 現在のパスから preview 用の node/cell 列を組み、resolver で結果を先読みする。
        /// </summary>
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
                CreateCurrentTurnContext(_preparedPattern, _session.NextTurnHazardBoosted));
            return true;
        }

        /// <summary>
        /// 現在の battle session と選択スキルを元に、このターン専用の解決コンテキストを作る。
        /// </summary>
        private BattleTurnContext CreateCurrentTurnContext(StageBattlePatternData pattern, bool hazardBoosted)
        {
            return StageBattleRuntimeAdapter.CreateTurnContext(
                pattern,
                _activeEnemyData != null ? _activeEnemyData.Damage : 0,
                hazardBoosted,
                CreateSelectedSkillRuntime(),
                _session.EnemyHp,
                _session.PlayerHp,
                _activeBattleProfile.NormalAttackDamage,
                _activeBattleProfile.DoubleAttackFollowUpDamage,
                _activeBattleProfile.JumpAttackDamage,
                _playerAttackAttribute,
                _activeEnemyData?.AttackAttribute,
                _activeEnemyData?.DefenseAttributeModifiers,
                _playerDefenseAttributeModifiers);
        }

        private string BuildTurnPrompt(StageBattlePatternData pattern)
        {
            if (_session.BattleEnded)
            {
                return "バトル終了";
            }

            if (!preferInteractiveTraceInput)
            {
                return pattern != null ? pattern.ConfirmText : string.Empty;
            }

            if (pattern != null && !string.IsNullOrWhiteSpace(pattern.Description))
            {
                return $"{pattern.Description} Start から Goal まで引いてください";
            }

            return "Start から Goal まで引いてください";
        }

        private string BuildTurnPreviewMessage(BattleTurnResolutionReport preview)
        {
            if (preview == null)
            {
                return _preparedPattern != null ? _preparedPattern.ConfirmText : string.Empty;
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

            if (preview.EnemyDamageTaken > 0 || !string.IsNullOrWhiteSpace(preview.EnemyDamageAttributeSummary))
            {
                parts.Add(string.IsNullOrWhiteSpace(preview.EnemyDamageAttributeSummary)
                    ? $"敵-{preview.EnemyDamageTaken}"
                    : $"敵-{preview.EnemyDamageTaken} [{preview.EnemyDamageAttributeSummary}]");
            }

            if (preview.ResolvedHazardCount > 0)
            {
                parts.Add(preview.PlayerDamageTaken > 0 || !string.IsNullOrWhiteSpace(preview.PlayerDamageAttributeSummary)
                    ? string.IsNullOrWhiteSpace(preview.PlayerDamageAttributeSummary)
                        ? $"自分-{preview.PlayerDamageTaken}"
                        : $"自分-{preview.PlayerDamageTaken} [{preview.PlayerDamageAttributeSummary}]"
                    : "被弾なし");
            }
            else if (preview.ResolvedEnemyActionCount > 0 &&
                     (preview.PlayerDamageTaken > 0 || !string.IsNullOrWhiteSpace(preview.PlayerDamageAttributeSummary)))
            {
                parts.Add(string.IsNullOrWhiteSpace(preview.PlayerDamageAttributeSummary)
                    ? $"敵攻撃-{preview.PlayerDamageTaken}"
                    : $"敵攻撃-{preview.PlayerDamageTaken} [{preview.PlayerDamageAttributeSummary}]");
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
                return _preparedPattern != null ? _preparedPattern.ConfirmText : "実行可能";
            }

            return string.Join(" / ", parts);
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

            if (slotData.SkillId > 0 &&
                MasterDataResourceLoader.TryLoadBattleSkillData(slotData.SkillId, out var skillDefinition) &&
                skillDefinition != null)
            {
                return new BattleSkillSlotRuntime
                {
                    SkillId = skillDefinition.SkillId,
                    DisplayName = skillDefinition.DisplayName,
                    Description = skillDefinition.Description,
                    DescriptionSupplement = skillDefinition.DescriptionSupplement ?? string.Empty,
                    IsUnlocked = slotData.IsUnlocked,
                    IsConfigured = slotData.IsConfigured,
                    RequiredCharge = Mathf.Max(1, skillDefinition.RequiredCharge),
                    StartingCharge = Mathf.Max(0, skillDefinition.StartingCharge),
                    TurnChargeGain = Mathf.Max(0, skillDefinition.TurnChargeGain),
                    AttackChargeGain = Mathf.Max(0, skillDefinition.AttackChargeGain),
                    Damage = Mathf.Max(0, skillDefinition.Damage),
                    Attribute = skillDefinition.Attribute,
                    EffectType = skillDefinition.EffectType,
                    EffectAnimation = skillDefinition.EffectAnimation,
                };
            }

            return new BattleSkillSlotRuntime
            {
                SkillId = Mathf.Max(0, slotData.SkillId),
                DisplayName = slotData.DisplayName ?? "Skill",
                Description = slotData.Description ?? string.Empty,
                DescriptionSupplement = string.Empty,
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
                SkillId = source.SkillId,
                DisplayName = source.DisplayName,
                Description = source.Description,
                DescriptionSupplement = source.DescriptionSupplement,
                IsUnlocked = source.IsUnlocked,
                IsConfigured = source.IsConfigured,
                RequiredCharge = source.RequiredCharge,
                StartingCharge = source.StartingCharge,
                TurnChargeGain = source.TurnChargeGain,
                AttackChargeGain = source.AttackChargeGain,
                Damage = source.Damage,
                Attribute = source.Attribute,
                EffectType = source.EffectType,
                EffectAnimation = source.EffectAnimation,
            };
            clone.SetCurrentCharge(source.CurrentCharge);
            return clone;
        }

        private void LogResolutionReport(StageBattlePatternData pattern, BattleTurnResolutionReport result)
        {
            if (result == null || result.LogEntries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < result.LogEntries.Count; i++)
            {
                Debug.Log($"[Battle] {GetPatternLabel(pattern)} {result.LogEntries[i]}");
            }
        }

        private static string GetPatternLabel(StageBattlePatternData pattern)
        {
            if (pattern == null || string.IsNullOrWhiteSpace(pattern.DebugLabel))
            {
                return "Pattern";
            }

            return pattern.DebugLabel;
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
