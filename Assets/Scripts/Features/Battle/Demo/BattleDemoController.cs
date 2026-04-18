using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 の処理フローを、ボタン押下 + ログだけで追える簡易デモ。
    /// 本番の盤面入力、UI、演出はまだ繋がず、ログとコメントで差し込み位置を明示する。
    /// </summary>
    public class BattleDemoController : MonoBehaviour
    {
        private const string LogPrefix = "[BattleDemo]";
        private const string DefaultEnemyName = "Training Bandit";
        private const int DefaultEnemyHp = 700;

        [Header("TODO: userInfoは別手段で取得する")]
        [SerializeField] private int initialPlayerHp = 450;
        [SerializeField] private int normalAttackDamage = 100;
        [SerializeField] private int doubleAttackFollowUpDamage = 125;
        [SerializeField] private int jumpAttackDamage = 150;

        [Header("Boot")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool loopTurnScripts = true;

        [Header("Stage Bootstrap")]
        [SerializeField] private bool applyStageSetupOnInitialize = true;
        [FormerlySerializedAs("stageSetupCatalogOverride")]
        [SerializeField] private StageDatabase stageDatabaseOverride;
        [FormerlySerializedAs("editorPlayFallbackStageId")]
        [SerializeField] private int debugStageId = 1;

        [Header("Stage Visual Targets")]
        [SerializeField] public Image backgroundImage;
        [SerializeField] public Image previewImage;

        [Header("Demo Skill Palette")]
        [SerializeField] private List<BattleDemoSkillSlot> skillSlots = new();

        [Header("Demo Buttons")]
        [SerializeField] private Button skillButton1;
        [SerializeField] private Button skillButton2;
        [SerializeField] private Button skillButton3;
        [SerializeField] private Button skillButton4;
        [SerializeField] private Button pathExecuteButton;

        [Header("Skill Button Visuals")]
        [SerializeField] private Color normalSkillButtonColor = new(1f, 1f, 1f, 1f);
        [SerializeField] private Color selectedSkillButtonColor = new(1f, 0.85f, 0.35f, 1f);

        [Header("Scripted Turn Flow")]
        [SerializeField] private List<BattleDemoTurnScript> turnScripts = new();

        private string _enemyName;
        private int _playerHp;
        private int _enemyHp;
        private int _initialEnemyHpForSession;
        private int _turnNumber;
        private int _turnScriptIndex;
        private int _hiddenInputFailureCount;
        private int _selectedSkillSlotIndex = -1;
        private bool _isInitialized;
        private bool _battleEnded;
        private bool _isPathInputLocked;
        private bool _nextTurnHazardBoosted;
        private int _resolvedStageIdForSession;

        private struct PathResolutionSummary
        {
            public bool PathConfirmed;
            public bool GoalEffectResolved;
            public bool TookHit;
            public int ResolvedAttackCount;
        }

        /// <summary>
        /// シリアライズされたUIボタンへデモ用コールバックを登録する。
        /// </summary>
        private void Awake()
        {
            BindButtons();
        }

        /// <summary>
        /// 起動時設定に応じてデモ初期化を実行する。
        /// </summary>
        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeDemo();
            }
        }

        /// <summary>
        /// 登録したボタンコールバックを解除する。
        /// </summary>
        private void OnDestroy()
        {
            UnbindButtons();
        }

        /// <summary>
        /// デモ用バトル状態と表示を初期化する。
        /// </summary>
        [ContextMenu("Initialize Demo")]
        public void InitializeDemo()
        {
            EnsureDemoData();

            _enemyName = DefaultEnemyName;
            _initialEnemyHpForSession = DefaultEnemyHp;
            ApplyStageSetupForSceneInitialization();

            _playerHp = initialPlayerHp;
            _enemyHp = _initialEnemyHpForSession;
            _turnNumber = 0;
            _turnScriptIndex = 0;
            _hiddenInputFailureCount = 0;
            _selectedSkillSlotIndex = -1;
            _battleEnded = false;
            _isPathInputLocked = false;
            _nextTurnHazardBoosted = false;
            _isInitialized = true;

            foreach (var slot in skillSlots)
            {
                slot.ResetRuntime();
            }

            Debug.Log($"{LogPrefix} Demo initialized.");
            Debug.Log($"{LogPrefix} 本番ではここで BattleScene の HUD 初期化、盤面生成、敵表示初期化を行う。");
            RefreshSkillButtonVisuals();
            LogBattleHud();
            LogSkillPalette();
            Debug.Log($"{LogPrefix} 操作用メモ: Skill を選んでから `RunNextDemoTurn()` を押すと、盤面ドラッグの代わりにプリセット経路を即時解決する。");
        }

        /// <summary>
        /// 次のプリセットターンを解決してログ出力する。
        /// </summary>
        [ContextMenu("Run Next Demo Turn")]
        public void RunNextDemoTurn()
        {
            EnsureInitialized();

            if (_battleEnded)
            {
                Debug.LogWarning($"{LogPrefix} Battle has already ended. Reset the demo to run it again.");
                return;
            }

            var turnScript = GetNextTurnScript();
            var hazardBoostedThisTurn = _nextTurnHazardBoosted;
            _nextTurnHazardBoosted = false;
            _turnNumber++;

            LogTurnHeader(turnScript, hazardBoostedThisTurn);
            ApplyTurnStartEffects();
            LogBoardPresentation(turnScript, hazardBoostedThisTurn);

            _isPathInputLocked = true;
            Debug.Log($"{LogPrefix} [Input] パネル操作の代わりに、1ボタンでプリセット経路を確定する。");
            Debug.Log($"{LogPrefix} [Input] パス入力中は Skill ボタンや他 UI を触れない想定。");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここでドラッグ入力、無効パス判定、仮スタック可視化を差し込む。");

            var result = ResolvePath(turnScript, hazardBoostedThisTurn);

            _isPathInputLocked = false;

            ConsumeSelectedSkillIfNeeded(result);
            ApplyAttackChargeGain(result.ResolvedAttackCount);
            ApplyTurnEndEffects(turnScript);

            LogBattleHud();
            LogSkillPalette();

            if (_battleEnded)
            {
                Debug.Log($"{LogPrefix} Battle finished on turn {_turnNumber}.");
            }
        }

        /// <summary>
        /// デモを初期状態へ戻す。
        /// </summary>
        [ContextMenu("Reset Demo")]
        public void ResetDemo()
        {
            InitializeDemo();
        }

        /// <summary>
        /// Goal未到達の入力失敗を擬似的に発生させる。
        /// </summary>
        [ContextMenu("Simulate Goal Miss")]
        public void SimulateGoalMiss()
        {
            RegisterInputFailure("Goal に届かず指を離した");
        }

        /// <summary>
        /// 不正パスの入力失敗を擬似的に発生させる。
        /// </summary>
        [ContextMenu("Simulate Invalid Path")]
        public void SimulateInvalidPath()
        {
            RegisterInputFailure("交差または再訪を含む不正パス");
        }

        /// <summary>
        /// 1番目のスキルスロットを選択する。
        /// </summary>
        public void SelectSkillSlot1()
        {
            SelectSkillSlot(0);
        }

        /// <summary>
        /// 2番目のスキルスロットを選択する。
        /// </summary>
        public void SelectSkillSlot2()
        {
            SelectSkillSlot(1);
        }

        /// <summary>
        /// 3番目のスキルスロットを選択する。
        /// </summary>
        public void SelectSkillSlot3()
        {
            SelectSkillSlot(2);
        }

        /// <summary>
        /// 4番目のスキルスロットを選択する。
        /// </summary>
        public void SelectSkillSlot4()
        {
            SelectSkillSlot(3);
        }

        /// <summary>
        /// 現在選択中のスキルを解除する。
        /// </summary>
        public void ClearSelectedSkill()
        {
            EnsureInitialized();

            if (_isPathInputLocked)
            {
                Debug.LogWarning($"{LogPrefix} Skill selection cannot be changed while path input is locked.");
                return;
            }

            _selectedSkillSlotIndex = -1;
            Debug.Log($"{LogPrefix} Selected Skill cleared.");
            RefreshSkillButtonVisuals();
            LogSkillPalette();
        }

        /// <summary>
        /// 指定スロットのスキル選択状態を更新する。
        /// </summary>
        public void SelectSkillSlot(int slotIndex)
        {
            EnsureInitialized();

            if (_battleEnded)
            {
                Debug.LogWarning($"{LogPrefix} Battle has already ended. Skill selection is ignored.");
                return;
            }

            if (_isPathInputLocked)
            {
                Debug.LogWarning($"{LogPrefix} Skill selection cannot be changed while path input is locked.");
                return;
            }

            if (slotIndex < 0 || slotIndex >= skillSlots.Count)
            {
                Debug.LogWarning($"{LogPrefix} Skill slot {slotIndex + 1} does not exist.");
                return;
            }

            var slot = skillSlots[slotIndex];
            if (!slot.IsUnlocked)
            {
                Debug.LogWarning($"{LogPrefix} Skill slot {slotIndex + 1} is locked (未取得).");
                return;
            }

            if (!slot.IsConfigured)
            {
                Debug.LogWarning($"{LogPrefix} Skill slot {slotIndex + 1} is not configured on the weapon (未設定).");
                return;
            }

            if (!slot.IsReady)
            {
                var remainingTurns = slot.GetRemainingTurnsUntilReady();
                if (remainingTurns == int.MaxValue)
                {
                    Debug.LogWarning($"{LogPrefix} Skill slot {slotIndex + 1} is not ready, and this demo has no turn-based progress configured for it.");
                }
                else
                {
                    Debug.LogWarning($"{LogPrefix} Skill slot {slotIndex + 1} はあと{remainingTurns}ターンで使用可能。");
                }

                return;
            }

            if (_selectedSkillSlotIndex == slotIndex)
            {
                _selectedSkillSlotIndex = -1;
                Debug.Log($"{LogPrefix} Deselected Skill: Slot {slotIndex + 1} / {slot.DisplayName}");
                RefreshSkillButtonVisuals();
                LogSkillPalette();
                return;
            }

            var previousSelectedSkill = GetSelectedSkill();
            _selectedSkillSlotIndex = slotIndex;
            if (previousSelectedSkill != null)
            {
                Debug.Log($"{LogPrefix} Skill selection changed: {previousSelectedSkill.DisplayName} -> {slot.DisplayName}");
            }
            else
            {
                Debug.Log($"{LogPrefix} Selected Skill: Slot {slotIndex + 1} / {slot.DisplayName}");
            }

            Debug.Log($"{LogPrefix} [TODO] 本番ではここで Skill ボタンの選択演出、ハイライト、説明表示を更新する。");
            RefreshSkillButtonVisuals();
            LogSkillPalette();
        }

        /// <summary>
        /// 未初期化ならデモ初期化を実行する。
        /// </summary>
        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            InitializeDemo();
        }

        /// <summary>
        /// Inspectorで割り当てたボタンへ処理をバインドする。
        /// </summary>
        private void BindButtons()
        {
            BindButton(skillButton1, SelectSkillSlot1);
            BindButton(skillButton2, SelectSkillSlot2);
            BindButton(skillButton3, SelectSkillSlot3);
            BindButton(skillButton4, SelectSkillSlot4);
            BindButton(pathExecuteButton, RunNextDemoTurn);
        }

        /// <summary>
        /// Inspectorで割り当てたボタンから処理を解除する。
        /// </summary>
        private void UnbindButtons()
        {
            UnbindButton(skillButton1, SelectSkillSlot1);
            UnbindButton(skillButton2, SelectSkillSlot2);
            UnbindButton(skillButton3, SelectSkillSlot3);
            UnbindButton(skillButton4, SelectSkillSlot4);
            UnbindButton(pathExecuteButton, RunNextDemoTurn);
        }

        /// <summary>
        /// 単一ボタンへコールバックを安全に登録する。
        /// </summary>
        private static void BindButton(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(callback);
            button.onClick.AddListener(callback);
        }

        /// <summary>
        /// 単一ボタンからコールバックを解除する。
        /// </summary>
        private static void UnbindButton(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(callback);
        }

        /// <summary>
        /// デモ用スキルとターンスクリプトの初期データを補完する。
        /// </summary>
        private void EnsureDemoData()
        {
            if (skillSlots == null || skillSlots.Count == 0)
            {
                skillSlots = CreateDefaultSkillSlots();
            }

            if (turnScripts == null || turnScripts.Count == 0)
            {
                turnScripts = CreateDefaultTurnScripts();
            }
        }

        /// <summary>
        /// 解決したStageDataを現在のデモ初期値へ反映する。
        /// </summary>
        private void ApplyStageSetupForSceneInitialization()
        {
            if (!applyStageSetupOnInitialize)
            {
                return;
            }

            var stageData = ResolveStageDataForInitialization();
            if (stageData == null)
            {
                Debug.LogWarning($"{LogPrefix} No StageData was resolved. Existing serialized BattleScene values will be used.");
                return;
            }

            var firstEnemy = GetPrimaryEnemy(stageData);
            if (firstEnemy != null)
            {
                _enemyName = string.IsNullOrWhiteSpace(firstEnemy.Name)
                    ? _enemyName
                    : firstEnemy.Name;
                _initialEnemyHpForSession = Mathf.Max(1, firstEnemy.Hp);
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} [StageInit] StageData has no enemy entry. Default enemy values will be used.");
            }

            // TODO: プレイヤーHPや武器由来の攻撃値は、将来的に別のプレイヤーデータ/装備データから取得する。
            // この段階では BattleScene に置かれた固定のデモ値をそのまま使う。
            initialPlayerHp = Mathf.Max(1, initialPlayerHp);
            normalAttackDamage = Mathf.Max(1, normalAttackDamage);
            doubleAttackFollowUpDamage = Mathf.Max(1, doubleAttackFollowUpDamage);
            jumpAttackDamage = Mathf.Max(1, jumpAttackDamage);

            ApplyStageVisuals(stageData);

            Debug.Log($"{LogPrefix} [StageInit] Applied stage '{stageData.StageId}' / '{stageData.StageName}'.");
            Debug.Log($"{LogPrefix} [StageInit] EnemyName={_enemyName}, PlayerHP={initialPlayerHp}, EnemyHP={_initialEnemyHpForSession}");
            Debug.Log($"{LogPrefix} [StageInit] Player-side params are using fixed demo values for now.");
            Debug.Log($"{LogPrefix} [StageInit] Description={stageData.Description}");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここでステージ固有の盤面データ、敵AI、演出差分を ScriptableObject から反映する。");
        }

        /// <summary>
        /// 遷移状態またはデバッグIDからStageDataを解決する。
        /// </summary>
        private StageData ResolveStageDataForInitialization()
        {
            var stageId = ResolveRequestedStageIdForSession();
            if (stageId <= 0)
            {
                Debug.LogWarning($"{LogPrefix} [StageInit] No stageId was supplied from scene transition or inspector fallback.");
                return null;
            }

            var database = stageDatabaseOverride != null
                ? stageDatabaseOverride
                : MasterDataResourceLoader.LoadStageDatabase();

            if (database == null)
            {
                Debug.LogWarning($"{LogPrefix} [StageInit] StageDatabase is not assigned.");
                return null;
            }

            if (database.TryGetById(stageId, out var stageData))
            {
                Debug.Log($"{LogPrefix} [StageInit] StageData resolved from ScriptableObject database.");
                return stageData;
            }

            Debug.LogWarning($"{LogPrefix} [StageInit] StageId={stageId} was not found in StageDatabase.");
            return null;
        }

        /// <summary>
        /// StageDataから先頭の敵情報を取得する。
        /// </summary>
        private StageData.EnemyData GetPrimaryEnemy(StageData stageData)
        {
            if (stageData?.Enemies == null || stageData.Enemies.Count == 0)
            {
                return null;
            }

            return stageData.Enemies[0];
        }

        /// <summary>
        /// StageDataの画像をシーン上のUIへ反映する。
        /// </summary>
        private void ApplyStageVisuals(StageData stageData)
        {
            if (backgroundImage != null)
            {
                backgroundImage.sprite = stageData.BackgroundImage;
                backgroundImage.enabled = stageData.BackgroundImage != null;
            }

            if (previewImage != null)
            {
                previewImage.sprite = stageData.PreviewImage;
                previewImage.enabled = stageData.PreviewImage != null;
            }

            Debug.Log($"{LogPrefix} [TODO] 本番ではここで StageData の表示情報を UI テキストや背景演出に反映する。");
        }

        /// <summary>
        /// 遷移状態またはデバッグ設定から使用するステージIDを決定する。
        /// </summary>
        private int ResolveRequestedStageIdForSession()
        {
            if (_resolvedStageIdForSession > 0)
            {
                return _resolvedStageIdForSession;
            }

            var transitionStageId = BattleSceneTransitionState.ConsumeSelectedStageId();
            if (transitionStageId > 0)
            {
                _resolvedStageIdForSession = transitionStageId;
                Debug.Log($"{LogPrefix} [StageInit] StageId={_resolvedStageIdForSession} resolved from static transition state.");
                return _resolvedStageIdForSession;
            }

            _resolvedStageIdForSession = Mathf.Max(0, debugStageId);
            if (_resolvedStageIdForSession > 0)
            {
                Debug.Log($"{LogPrefix} [StageInit] BattleSceneTransitionState had no StageId. Using BattleScene inspector debug StageId={_resolvedStageIdForSession}.");
            }

            return _resolvedStageIdForSession;
        }

        /// <summary>
        /// スキルボタンの選択状態に応じて見た目を更新する。
        /// </summary>
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

                var isSelected = i == _selectedSkillSlotIndex;
                var targetColor = isSelected ? selectedSkillButtonColor : normalSkillButtonColor;
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

        /// <summary>
        /// 次に解決するデモ用ターンスクリプトを取得する。
        /// </summary>
        private BattleDemoTurnScript GetNextTurnScript()
        {
            if (turnScripts.Count == 0)
            {
                turnScripts = CreateDefaultTurnScripts();
            }

            var index = Mathf.Clamp(_turnScriptIndex, 0, turnScripts.Count - 1);
            var turnScript = turnScripts[index];

            if (loopTurnScripts)
            {
                _turnScriptIndex = (_turnScriptIndex + 1) % turnScripts.Count;
            }
            else
            {
                _turnScriptIndex = Mathf.Min(_turnScriptIndex + 1, turnScripts.Count - 1);
            }

            return turnScript;
        }

        /// <summary>
        /// ターン開始時のゲージ加算と開始ログを処理する。
        /// </summary>
        private void ApplyTurnStartEffects()
        {
            Debug.Log($"{LogPrefix} [Flow] Turn start effects.");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここで持続効果、ターン開始バフ、危険配置確定後の開始演出を処理する。");

            foreach (var slot in skillSlots)
            {
                var before = slot.CurrentCharge;
                slot.GainTurnCharge();

                if (slot.CurrentCharge != before)
                {
                    Debug.Log($"{LogPrefix} [SkillCharge] {slot.DisplayName}: {before} -> {slot.CurrentCharge} (turn gain)");
                }
            }
        }

        /// <summary>
        /// 攻撃回数由来のスキルゲージ加算の差し込み位置を示す。
        /// </summary>
        private void ApplyAttackChargeGain(int resolvedAttackCount)
        {
            Debug.Log($"{LogPrefix} [SkillCharge] Demo では攻撃回数によるゲージ加算はまだ行わない。");
            Debug.Log($"{LogPrefix} [TODO] 本番では resolvedAttackCount={resolvedAttackCount} を使って攻撃回数ぶんの加算を差し込む。");
        }

        /// <summary>
        /// ターン終了時の効果と敵Dance予約を処理する。
        /// </summary>
        private void ApplyTurnEndEffects(BattleDemoTurnScript turnScript)
        {
            Debug.Log($"{LogPrefix} [Flow] Turn end effects.");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここでターン終了時の持続効果や敵次ターン通知を処理する。");

            if (turnScript.EnemyAction == BattleDemoEnemyActionType.Dance)
            {
                _nextTurnHazardBoosted = true;
                Debug.Log($"{LogPrefix} [EnemyDance] 次ターン危険強化が予約された。");
                Debug.Log($"{LogPrefix} [UI-Log代用] 次ターン危険強化");
            }
        }

        /// <summary>
        /// 現在ターンのヘッダ情報をログ出力する。
        /// </summary>
        private void LogTurnHeader(BattleDemoTurnScript turnScript, bool hazardBoostedThisTurn)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{LogPrefix} ====================");
            builder.AppendLine($"{LogPrefix} Turn {_turnNumber} / {turnScript.Label}");
            builder.AppendLine($"{LogPrefix} Enemy: {_enemyName}");
            builder.AppendLine($"{LogPrefix} Enemy action this turn: {turnScript.EnemyAction}");
            builder.AppendLine($"{LogPrefix} Hazard boost active: {(hazardBoostedThisTurn ? "YES" : "NO")}");
            builder.AppendLine($"{LogPrefix} ====================");
            Debug.Log(builder.ToString());
        }

        /// <summary>
        /// 盤面サマリと入力前情報をログ出力する。
        /// </summary>
        private void LogBoardPresentation(BattleDemoTurnScript turnScript, bool hazardBoostedThisTurn)
        {
            Debug.Log($"{LogPrefix} [Board] {turnScript.BoardSummary}");
            if (hazardBoostedThisTurn)
            {
                Debug.Log($"{LogPrefix} [Board] Enemy Dance buff is active. Hazard damage is treated as 1.5x in this demo.");
            }

            Debug.Log($"{LogPrefix} [Board] Confirm text: {turnScript.ConfirmText}");
            Debug.Log($"{LogPrefix} [Board] Preset path: {turnScript.GetPathSummary()}");
            Debug.Log($"{LogPrefix} [Board] Notes: {turnScript.Notes}");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここで盤面ノード、危険マス、Goal、短文テキストを実 UI に描画する。");
        }

        /// <summary>
        /// プリセットされた経路を順番に解決して結果を返す。
        /// </summary>
        private PathResolutionSummary ResolvePath(BattleDemoTurnScript turnScript, bool hazardBoostedThisTurn)
        {
            var result = new PathResolutionSummary
            {
                PathConfirmed = turnScript.HasGoalInPath(),
                GoalEffectResolved = false,
                TookHit = false,
                ResolvedAttackCount = 0,
            };

            var jumpCanEvade = false;
            var jumpAttackPrimed = false;
            var rollCanEvade = false;
            var hazardGroupResolved = false;
            var basicAttackResolvedCount = 0;
            var selectedSkill = GetSelectedSkill();

            if (selectedSkill != null)
            {
                Debug.Log($"{LogPrefix} [Skill] {selectedSkill.DisplayName} is armed for this turn. All Attack nodes will convert to that Skill.");
            }

            for (var i = 0; i < turnScript.Path.Count; i++)
            {
                var step = turnScript.Path[i];
                Debug.Log($"{LogPrefix} [Path] Step {i + 1}: {step}");

                switch (step)
                {
                    case BattleDemoNodeType.Start:
                        Debug.Log($"{LogPrefix} [Path] Start node reached.");
                        break;

                    case BattleDemoNodeType.Empty:
                        Debug.Log($"{LogPrefix} [Path] Empty tile. Evasion state is preserved.");
                        break;

                    case BattleDemoNodeType.Jump:
                        jumpCanEvade = true;
                        jumpAttackPrimed = true;
                        rollCanEvade = false;
                        Debug.Log($"{LogPrefix} [Action] Jump primed. Next normal hazard can be evaded, and the next basic Attack can become a jump attack.");
                        break;

                    case BattleDemoNodeType.Roll:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = true;
                        Debug.Log($"{LogPrefix} [Action] Roll primed. Next normal or skill hazard can be evaded.");
                        break;

                    case BattleDemoNodeType.Dance:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = false;
                        Debug.Log($"{LogPrefix} [Action] Dance resolved as a support action placeholder.");
                        Debug.Log($"{LogPrefix} [TODO] 本番ではここで装備依存の Dance 効果、回復、バフ表示を差し込む。");
                        break;

                    case BattleDemoNodeType.Attack:
                        ResolveAttackStep(selectedSkill, ref result, ref basicAttackResolvedCount, ref jumpCanEvade, ref jumpAttackPrimed, ref rollCanEvade);
                        if (_battleEnded)
                        {
                            Debug.Log($"{LogPrefix} [Flow] Battle ended during attack resolution. Remaining stack is discarded.");
                            return result;
                        }

                        break;

                    case BattleDemoNodeType.HazardNormal:
                    case BattleDemoNodeType.HazardSkill:
                        if (hazardGroupResolved)
                        {
                            Debug.Log($"{LogPrefix} [Hazard] Same attack group already resolved. Additional tiles do not deal extra damage.");
                            break;
                        }

                        if (step == BattleDemoNodeType.HazardNormal && jumpCanEvade)
                        {
                            jumpCanEvade = false;
                            hazardGroupResolved = true;
                            Debug.Log($"{LogPrefix} [Hazard] Jump evaded the normal hazard group.");
                            Debug.Log($"{LogPrefix} [TODO] 本番ではここで敵攻撃モーションと回避モーションを同時再生する。");
                            break;
                        }

                        if (rollCanEvade)
                        {
                            rollCanEvade = false;
                            jumpAttackPrimed = false;
                            hazardGroupResolved = true;
                            Debug.Log($"{LogPrefix} [Hazard] Roll evaded the hazard group.");
                            Debug.Log($"{LogPrefix} [TODO] 本番ではここで Roll 回避の演出と SE を再生する。");
                            break;
                        }

                        hazardGroupResolved = true;
                        result.TookHit = true;

                        var hazardDamage = hazardBoostedThisTurn
                            ? Mathf.RoundToInt(turnScript.HazardDamage * 1.5f)
                            : turnScript.HazardDamage;

                        _playerHp = Mathf.Max(0, _playerHp - hazardDamage);
                        Debug.Log($"{LogPrefix} [Hazard] Player takes {hazardDamage} damage.");
                        Debug.Log($"{LogPrefix} [UI-Log代用] Damage {hazardDamage}");
                        Debug.Log($"{LogPrefix} [TODO] 本番ではここでダメージ数値、被弾アニメ、以降の未解決スタック破棄を行う。");

                        if (_playerHp <= 0)
                        {
                            _battleEnded = true;
                            Debug.Log($"{LogPrefix} [Result] Player HP reached 0. Battle ends immediately.");
                        }
                        else
                        {
                            Debug.Log($"{LogPrefix} [Result] Hit confirmed. Later unresolved actions, including Goal effect, are discarded.");
                        }

                        return result;

                    case BattleDemoNodeType.Goal:
                        if (result.TookHit)
                        {
                            Debug.Log($"{LogPrefix} [Goal] Goal effect is skipped because the path already took damage.");
                            return result;
                        }

                        result.GoalEffectResolved = true;
                        Debug.Log($"{LogPrefix} [Goal] Goal reached. Stack is considered confirmed in this demo.");
                        Debug.Log($"{LogPrefix} [TODO] 本番ではここで Goal 演出、Goal 固有効果、勝敗演出の分岐を差し込む。");
                        return result;
                }
            }

            if (!result.PathConfirmed)
            {
                Debug.LogWarning($"{LogPrefix} [Input] Preset path did not reach Goal. In the full implementation, this would stay on the same turn and allow redraw.");
            }

            return result;
        }

        /// <summary>
        /// Attackマス1回分の攻撃またはスキル解決を行う。
        /// </summary>
        private void ResolveAttackStep(
            BattleDemoSkillSlot selectedSkill,
            ref PathResolutionSummary result,
            ref int basicAttackResolvedCount,
            ref bool jumpCanEvade,
            ref bool jumpAttackPrimed,
            ref bool rollCanEvade)
        {
            result.ResolvedAttackCount++;

            if (selectedSkill != null)
            {
                _enemyHp = Mathf.Max(0, _enemyHp - selectedSkill.Damage);
                Debug.Log($"{LogPrefix} [Action] Skill '{selectedSkill.DisplayName}' resolved via Attack node. Damage: {selectedSkill.Damage}");
                Debug.Log($"{LogPrefix} [UI-Log代用] {selectedSkill.DisplayName} / Damage {selectedSkill.Damage}");
                Debug.Log($"{LogPrefix} [TODO] 本番ではここで Skill 固有演出、危険生成、属性反映を差し込む。");
            }
            else if (jumpAttackPrimed)
            {
                basicAttackResolvedCount++;
                _enemyHp = Mathf.Max(0, _enemyHp - jumpAttackDamage);
                Debug.Log($"{LogPrefix} [Action] Jump Attack resolved. Damage: {jumpAttackDamage}");
                Debug.Log($"{LogPrefix} [UI-Log代用] Jump Attack / Damage {jumpAttackDamage}");
            }
            else
            {
                basicAttackResolvedCount++;
                var damage = basicAttackResolvedCount >= 2 ? doubleAttackFollowUpDamage : normalAttackDamage;
                var label = basicAttackResolvedCount >= 2 ? "Double Attack (follow-up)" : "Attack";
                _enemyHp = Mathf.Max(0, _enemyHp - damage);
                Debug.Log($"{LogPrefix} [Action] {label} resolved. Damage: {damage}");
                Debug.Log($"{LogPrefix} [UI-Log代用] {label} / Damage {damage}");
            }

            Debug.Log($"{LogPrefix} [TODO] 本番ではここで攻撃アニメ、ヒットストップ、属性相性装飾、行動ログ更新を行う。");

            jumpCanEvade = false;
            jumpAttackPrimed = false;
            rollCanEvade = false;

            if (_enemyHp <= 0)
            {
                _battleEnded = true;
                Debug.Log($"{LogPrefix} [Result] Enemy HP reached 0. Battle ends immediately and later stack entries are skipped.");
            }
        }

        /// <summary>
        /// 確定したターン結果に応じて選択スキルを消費する。
        /// </summary>
        private void ConsumeSelectedSkillIfNeeded(PathResolutionSummary result)
        {
            if (_selectedSkillSlotIndex < 0 || _selectedSkillSlotIndex >= skillSlots.Count)
            {
                return;
            }

            if (!result.PathConfirmed)
            {
                Debug.Log($"{LogPrefix} [Skill] Path was not confirmed. Selected Skill is preserved for redraw in the full implementation.");
                return;
            }

            var slot = skillSlots[_selectedSkillSlotIndex];
            slot.Consume();
            Debug.Log($"{LogPrefix} [Skill] {slot.DisplayName} was consumed for this confirmed turn.");
            Debug.Log($"{LogPrefix} [Note] Even if no Attack node was passed, the selected Skill is consumed once the turn is confirmed.");
            _selectedSkillSlotIndex = -1;
            RefreshSkillButtonVisuals();
        }

        /// <summary>
        /// 現在選択中のスキルスロットを取得する。
        /// </summary>
        private BattleDemoSkillSlot GetSelectedSkill()
        {
            if (_selectedSkillSlotIndex < 0 || _selectedSkillSlotIndex >= skillSlots.Count)
            {
                return null;
            }

            return skillSlots[_selectedSkillSlotIndex];
        }

        /// <summary>
        /// 入力失敗を記録してログ演出を出す。
        /// </summary>
        private void RegisterInputFailure(string reason)
        {
            EnsureInitialized();

            if (_battleEnded)
            {
                Debug.LogWarning($"{LogPrefix} Battle has already ended. Input failure demo is ignored.");
                return;
            }

            _hiddenInputFailureCount++;
            Debug.LogWarning($"{LogPrefix} [InputFailure] {reason}");
            Debug.LogWarning($"{LogPrefix} [InputFailure] Hidden failure count: {_hiddenInputFailureCount}");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここで同一ターンのまま盤面を維持し、Start から引き直しを許可する。");
            Debug.Log($"{LogPrefix} [TODO] 本番では特殊敗北の演出テキストだけを匂わせとして出し、数値は表示しない。");
        }

        /// <summary>
        /// 現在のバトルHUD相当情報をログ出力する。
        /// </summary>
        private void LogBattleHud()
        {
            var selectedSkill = GetSelectedSkill();
            var selectedSkillLabel = selectedSkill != null ? selectedSkill.DisplayName : "none";

            Debug.Log(
                $"{LogPrefix} [HUD] Turn={_turnNumber}, PlayerHP={_playerHp}, EnemyHP={_enemyHp}, EnemyName={_enemyName}, " +
                $"SelectedSkill={selectedSkillLabel}, NextTurnHazardBoost={_nextTurnHazardBoosted}, HiddenInputFailures={_hiddenInputFailureCount}");
            Debug.Log($"{LogPrefix} [TODO] 本番ではここで HP バー、敵名、ターン表示、危険強化表示を UI 更新する。");
        }

        /// <summary>
        /// スキルパレットの現在状態をログ出力する。
        /// </summary>
        private void LogSkillPalette()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{LogPrefix} [SkillPalette]");

            for (var i = 0; i < skillSlots.Count; i++)
            {
                var slot = skillSlots[i];
                var isSelected = i == _selectedSkillSlotIndex ? " <SELECTED>" : string.Empty;
                builder.AppendLine(
                    $"  Slot {i + 1}: {slot.DisplayName} | State={slot.GetStateLabel()} | Charge={slot.GetChargeLabel()}{isSelected}");
            }

            builder.AppendLine($"{LogPrefix} [TODO] 本番ではオレンジ色バー、READY 表示、未取得と未設定の見分けをここに対応させる。");
            Debug.Log(builder.ToString());
        }

        /// <summary>
        /// デモ用の初期スキルスロット一覧を生成する。
        /// </summary>
        private static List<BattleDemoSkillSlot> CreateDefaultSkillSlots()
        {
            return new List<BattleDemoSkillSlot>
            {
                new BattleDemoSkillSlot
                {
                    DisplayName = "Wide Blast",
                    Description = "広い範囲に危険を置く純粋攻撃寄り Skill。",
                    IsUnlocked = true,
                    IsConfigured = true,
                    RequiredCharge = 3,
                    StartingCharge = 3,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 140,
                },
                new BattleDemoSkillSlot
                {
                    DisplayName = "Pierce Volley",
                    Description = "単体高火力寄りの Skill デモ。",
                    IsUnlocked = true,
                    IsConfigured = true,
                    RequiredCharge = 5,
                    StartingCharge = 2,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 220,
                },
                new BattleDemoSkillSlot
                {
                    DisplayName = "Locked Slot",
                    Description = "ゲーム進行で解放される想定のロック枠。",
                    IsUnlocked = false,
                    IsConfigured = false,
                    RequiredCharge = 4,
                    StartingCharge = 0,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 0,
                },
                new BattleDemoSkillSlot
                {
                    DisplayName = "Empty Slot",
                    Description = "武器側に Skill が未設定の枠。",
                    IsUnlocked = true,
                    IsConfigured = false,
                    RequiredCharge = 4,
                    StartingCharge = 0,
                    TurnChargeGain = 1,
                    AttackChargeGain = 1,
                    Damage = 0,
                },
            };
        }

        /// <summary>
        /// デモ用の初期ターンスクリプト一覧を生成する。
        /// </summary>
        private static List<BattleDemoTurnScript> CreateDefaultTurnScripts()
        {
            return new List<BattleDemoTurnScript>
            {
                new BattleDemoTurnScript
                {
                    Label = "Opening Attack",
                    EnemyAction = BattleDemoEnemyActionType.NormalAttack,
                    BoardSummary = "通常危険が1グループだけ見えている基本盤面。",
                    ConfirmText = "いける",
                    Notes = "最小構成のターン。Attack から Goal までの基本解決を確認する。",
                    HazardDamage = 80,
                    Path = new List<BattleDemoNodeType>
                    {
                        BattleDemoNodeType.Start,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.Goal,
                    },
                },
                new BattleDemoTurnScript
                {
                    Label = "Jump Evade",
                    EnemyAction = BattleDemoEnemyActionType.NormalAttack,
                    BoardSummary = "通常危険の先に Goal が置かれた盤面。",
                    ConfirmText = "跳ぶ！",
                    Notes = "Jump で通常危険を回避し、そのまま Jump Attack まで繋ぐ。",
                    HazardDamage = 80,
                    Path = new List<BattleDemoNodeType>
                    {
                        BattleDemoNodeType.Start,
                        BattleDemoNodeType.Jump,
                        BattleDemoNodeType.Empty,
                        BattleDemoNodeType.HazardNormal,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.Goal,
                    },
                },
                new BattleDemoTurnScript
                {
                    Label = "Skill Showcase",
                    EnemyAction = BattleDemoEnemyActionType.Skill,
                    BoardSummary = "敵 Skill による広めの危険配置。Attack を複数取りやすい配置。",
                    ConfirmText = "決める",
                    Notes = "事前に Skill を選択していれば、すべての Attack が Skill に変換される。",
                    HazardDamage = 90,
                    Path = new List<BattleDemoNodeType>
                    {
                        BattleDemoNodeType.Start,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.Empty,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.Goal,
                    },
                },
                new BattleDemoTurnScript
                {
                    Label = "Enemy Dance Turn",
                    EnemyAction = BattleDemoEnemyActionType.Dance,
                    BoardSummary = "このターンは敵が Dance を使い、次ターン危険強化を予約する。",
                    ConfirmText = "続ける！",
                    Notes = "Attack を踏まなくても Goal へ到達できるターン。Skill を事前選択していた場合の消費確認にも使える。",
                    HazardDamage = 70,
                    Path = new List<BattleDemoNodeType>
                    {
                        BattleDemoNodeType.Start,
                        BattleDemoNodeType.Dance,
                        BattleDemoNodeType.Goal,
                    },
                },
                new BattleDemoTurnScript
                {
                    Label = "Roll Against Skill Hazard",
                    EnemyAction = BattleDemoEnemyActionType.Skill,
                    BoardSummary = "前ターンの Dance により危険強化がかかった Skill 危険盤面。",
                    ConfirmText = "危ない",
                    Notes = "Roll で Skill 危険を回避し、その後 Attack を通して Goal へ向かう。",
                    HazardDamage = 100,
                    Path = new List<BattleDemoNodeType>
                    {
                        BattleDemoNodeType.Start,
                        BattleDemoNodeType.Roll,
                        BattleDemoNodeType.Empty,
                        BattleDemoNodeType.HazardSkill,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.Goal,
                    },
                },
                new BattleDemoTurnScript
                {
                    Label = "Hit Stops Remaining Stack",
                    EnemyAction = BattleDemoEnemyActionType.NormalAttack,
                    BoardSummary = "通常危険を踏むと以降の解決が切れる確認用ターン。",
                    ConfirmText = "危ない",
                    Notes = "Attack の後に被弾すると、後続の Attack と Goal 効果が不発になる流れを確認する。",
                    HazardDamage = 85,
                    Path = new List<BattleDemoNodeType>
                    {
                        BattleDemoNodeType.Start,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.HazardNormal,
                        BattleDemoNodeType.Attack,
                        BattleDemoNodeType.Goal,
                    },
                },
            };
        }
    }
}
