using Assets.Scripts.Core;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 のバトル進行を制御するコントローラ。
    /// スキル選択とパス実行ボタンを受け付け、プリセット経路を解決して
    /// HP・スキル状態・敵行動フラグを更新する。
    /// </summary>
    public class BattleScene : MonoBehaviour
    {
        [Header("Player Params (TODO: 将来的にユーザーデータから取得)")]
        [SerializeField] private int initialPlayerHp = 450;
        [SerializeField] private int normalAttackDamage = 100;
        [SerializeField] private int doubleAttackFollowUpDamage = 125;
        [SerializeField] private int jumpAttackDamage = 150;
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

        [Header("Input")]
        [SerializeField] private Button skillButton1;
        [SerializeField] private Button skillButton2;
        [SerializeField] private Button skillButton3;
        [SerializeField] private Button skillButton4;
        [SerializeField] private Button pathExecuteButton;

        [Header("Skill Button Visuals")]
        [SerializeField] private Color normalSkillButtonColor = new(1f, 1f, 1f, 1f);
        [SerializeField] private Color selectedSkillButtonColor = new(1f, 0.85f, 0.35f, 1f);
        
        private List<BattleDemoTurnScript> _turnScripts;
        private PartsManager _playerInstance;
        private PartsManager _enemyInstance;

        private string _enemyName;
        private int _playerHp;
        private int _enemyHp;
        private int _initialEnemyHp;
        private int _turnNumber;
        private int _waveNumber;
        private int _turnScriptIndex;
        private int _selectedSkillSlotIndex = -1;
        private bool _isInitialized;
        private bool _battleEnded;
        private bool _isPathInputLocked;
        private bool _nextTurnHazardBoosted;
        private int _resolvedStageIdForSession = -1;

        private void Awake()
        {
            skillButton1.onClick.AddListener(OnClickSkillButton1);
            skillButton2.onClick.AddListener(OnClickSkillButton2);
            skillButton3.onClick.AddListener(OnClickSkillButton3);
            skillButton4.onClick.AddListener(OnClickSkillButton4);

            pathExecuteButton.onClick.AddListener(ExecuteTurn);
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
            if (pathExecuteButton != null) pathExecuteButton.onClick.RemoveListener(ExecuteTurn);
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            EnsureDefaults(); // デフォルト値の補完

            if (!TryResolveStageData(out var stageData))
            {
                return;
            }

            ApplyStageProgress(stageData);
            ApplyStagePresentation(stageData);
            ResetRuntimeState(stageData);
            RefreshHud();
            RefreshSkillButtonVisuals();
            Debug.Log($"[Battle] 初期化完了 Enemy={_enemyName} HP={_initialEnemyHp}");

            SpawnPreviewCharacters();
        }

        public void ExecuteTurn()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_battleEnded)
            {
                Debug.LogWarning($"[Battle] バトル終了済み");
                return;
            }

            var turnScript = GetNextTurnScript();
            var hazardBoosted = ConsumeHazardBoostFlag();
            _turnNumber++;
            GainTurnChargeToAllSkills();

            _isPathInputLocked = true;
            var result = BattleDemoPathResolver.Resolve(
                turnScript,
                hazardBoosted,
                GetSelectedSkill(),
                new BattleDemoPathResolver.Settings
                {
                    CurrentEnemyHp = _enemyHp,
                    CurrentPlayerHp = _playerHp,
                    NormalAttackDamage = normalAttackDamage,
                    DoubleAttackFollowUpDamage = doubleAttackFollowUpDamage,
                    JumpAttackDamage = jumpAttackDamage,
                });
            _isPathInputLocked = false;

            ApplyResolutionResult(result, turnScript);
            ConsumeSelectedSkillIfNeeded(result);

            ApplyPostTurnEnemyState(turnScript);
            RefreshHud();
            Debug.Log($"[Battle] Turn {_turnNumber} 終了 PlayerHP={_playerHp} EnemyHP={_enemyHp}");

            if (_battleEnded)
            {
                Debug.Log($"[Battle] バトル終了");
            }
        }

        /// <summary>
        /// 選択中のスキルを解除する。
        /// </summary>
        public void ClearSelectedSkill()
        {
            if (_isPathInputLocked || _selectedSkillSlotIndex < 0)
            {
                return;
            }

            _selectedSkillSlotIndex = -1;
            RefreshSkillButtonVisuals();
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

            if (_battleEnded || _isPathInputLocked)
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

            _selectedSkillSlotIndex = _selectedSkillSlotIndex == slotIndex ? -1 : slotIndex;
            RefreshSkillButtonVisuals();
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

        private void RefreshSkillButtonVisuals()
        {
            var buttons = new[] { skillButton1, skillButton2, skillButton3, skillButton4 };

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                var targetColor = i == _selectedSkillSlotIndex ? selectedSkillButtonColor : normalSkillButtonColor;

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
            if (_turnScripts.Count == 0)
            {
                _turnScripts = BattleDemoContentFactory.CreateDefaultTurnScripts();
            }

            var index = Mathf.Clamp(_turnScriptIndex, 0, _turnScripts.Count - 1);
            var turnScript = _turnScripts[index];

            _turnScriptIndex = Mathf.Min(_turnScriptIndex + 1, _turnScripts.Count - 1);

            return turnScript;
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
            initialPlayerHp = Mathf.Max(1, initialPlayerHp);
            normalAttackDamage = Mathf.Max(1, normalAttackDamage);
            doubleAttackFollowUpDamage = Mathf.Max(1, doubleAttackFollowUpDamage);
            jumpAttackDamage = Mathf.Max(1, jumpAttackDamage);

            _enemyName = string.Empty;
            _initialEnemyHp = 1;
            if (stageData.Enemies != null && stageData.Enemies.Count > 0)
            {
                var firstEnemy = stageData.Enemies[0];
                if (!string.IsNullOrWhiteSpace(firstEnemy.Name))
                {
                    _enemyName = firstEnemy.Name;
                }

                _initialEnemyHp = Mathf.Max(1, firstEnemy.Hp);
            }

            _playerHp = initialPlayerHp;
            _enemyHp = _initialEnemyHp;
            _turnNumber = 0;
            _waveNumber = 1;
            _turnScriptIndex = 0;
            _selectedSkillSlotIndex = -1;
            _battleEnded = false;
            _isPathInputLocked = false;
            _nextTurnHazardBoosted = false;
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

        private bool ConsumeHazardBoostFlag()
        {
            var hazardBoosted = _nextTurnHazardBoosted;
            _nextTurnHazardBoosted = false;
            return hazardBoosted;
        }

        private void ApplyResolutionResult(BattleDemoPathResolver.Result result, BattleDemoTurnScript turnScript)
        {
            if (result.EnemyDamageTaken > 0)
            {
                _enemyHp = Mathf.Max(0, _enemyHp - result.EnemyDamageTaken);
                Debug.Log($"[Battle] {turnScript.Label} EnemyDamage={result.EnemyDamageTaken}");
            }

            if (result.PlayerDamageTaken > 0)
            {
                _playerHp = Mathf.Max(0, _playerHp - result.PlayerDamageTaken);
                Debug.Log($"[Battle] {turnScript.Label} PlayerDamage={result.PlayerDamageTaken}");
            }

            if (result.EnemyDefeated)
            {
                HandleBattleClear();
                return;
            }

            if (result.PlayerDefeated || _playerHp <= 0)
            {
                _battleEnded = true;
            }
        }

        private void ApplyPostTurnEnemyState(BattleDemoTurnScript turnScript)
        {
            if (_battleEnded)
            {
                return;
            }

            if (turnScript.EnemyAction == BattleDemoEnemyActionType.Dance)
            {
                _nextTurnHazardBoosted = true;
            }
        }

        private void RefreshHud()
        {
            if (turnNumberText != null)
            {
                turnNumberText.text = _turnNumber.ToString();
            }

            if (waveNumberText != null)
            {
                waveNumberText.text = _waveNumber.ToString();
            }
        }

        private void ConsumeSelectedSkillIfNeeded(BattleDemoPathResolver.Result result)
        {
            if (_selectedSkillSlotIndex < 0 || _selectedSkillSlotIndex >= skillSlots.Count)
            {
                return;
            }

            if (!result.PathConfirmed)
            {
                return;
            }

            skillSlots[_selectedSkillSlotIndex].Consume();
            _selectedSkillSlotIndex = -1;
            RefreshSkillButtonVisuals();
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
            _battleEnded = true;

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
            if (_selectedSkillSlotIndex < 0 || _selectedSkillSlotIndex >= skillSlots.Count)
            {
                return null;
            }
            return skillSlots[_selectedSkillSlotIndex];
        }
    }
}
