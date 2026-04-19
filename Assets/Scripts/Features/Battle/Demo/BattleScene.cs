using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
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

        [Header("UI")]
        [SerializeField] public Image backgroundImage;
        [SerializeField] public Image previewImage;
        [SerializeField] public Text turnNumberText;
        [SerializeField] public Text waveNumberText;

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
        private int _resolvedStageIdForSession;

        private struct PathResolutionSummary
        {
            public bool PathConfirmed;
            public bool TookHit;
        }

        private void Awake()
        {
            skillButton1.onClick.AddListener(() => SelectSkillSlot(0));
            skillButton2.onClick.AddListener(() => SelectSkillSlot(1));
            skillButton3.onClick.AddListener(() => SelectSkillSlot(2));
            skillButton4.onClick.AddListener(() => SelectSkillSlot(3));
            pathExecuteButton.onClick.AddListener(ExecuteTurn);
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            EnsureDefaults(); // デフォルト値の補完

            var stageId = BattleSceneTransitionState.ConsumeSelectedStageId();

            if (!MasterDataResourceLoader.TryLoadStageData(stageId, out StageData stageData))
            {
                return;
            }

            if (stageData.Enemies != null && stageData.Enemies.Count > 0)
            {
                var firstEnemy = stageData.Enemies[0];
                if (!string.IsNullOrWhiteSpace(firstEnemy.Name))
                {
                    _enemyName = firstEnemy.Name;
                }
                _initialEnemyHp = Mathf.Max(1, firstEnemy.Hp);
            }

            backgroundImage.sprite = stageData.BackgroundImage;
            backgroundImage.enabled = stageData.BackgroundImage != null;
            previewImage.sprite = stageData.PreviewImage;
            previewImage.enabled = stageData.PreviewImage != null;

            initialPlayerHp = Mathf.Max(1, initialPlayerHp);
            normalAttackDamage = Mathf.Max(1, normalAttackDamage);
            doubleAttackFollowUpDamage = Mathf.Max(1, doubleAttackFollowUpDamage);
            jumpAttackDamage = Mathf.Max(1, jumpAttackDamage);

            _playerHp = initialPlayerHp;
            _enemyHp = _initialEnemyHp;
            _turnNumber = 0;
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

            RefreshSkillButtonVisuals();
            Debug.Log($"[Battle] 初期化完了 Enemy={_enemyName} HP={_initialEnemyHp}");
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
            var hazardBoosted = _nextTurnHazardBoosted;
            _nextTurnHazardBoosted = false;
            _turnNumber++;

            foreach (var slot in skillSlots)
            {
                slot.GainTurnCharge();
            }

            _isPathInputLocked = true;
            var result = ResolvePath(turnScript, hazardBoosted);
            _isPathInputLocked = false;

            ConsumeSelectedSkillIfNeeded(result);

            if (turnScript.EnemyAction == BattleDemoEnemyActionType.Dance)
            {
                _nextTurnHazardBoosted = true;
            }

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

        private void EnsureDefaults()
        {
            if (skillSlots == null || skillSlots.Count == 0)
            {
                skillSlots = CreateDefaultSkillSlots();
            }

            if (_turnScripts == null || _turnScripts.Count == 0)
            {
                _turnScripts = CreateDefaultTurnScripts();
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
                _turnScripts = CreateDefaultTurnScripts();
            }

            var index = Mathf.Clamp(_turnScriptIndex, 0, _turnScripts.Count - 1);
            var turnScript = _turnScripts[index];

            _turnScriptIndex = Mathf.Min(_turnScriptIndex + 1, _turnScripts.Count - 1);

            return turnScript;
        }

        private PathResolutionSummary ResolvePath(BattleDemoTurnScript turnScript, bool hazardBoosted)
        {
            var result = new PathResolutionSummary
            {
                PathConfirmed = turnScript.HasGoalInPath(),
                TookHit = false,
            };

            var jumpCanEvade = false;
            var jumpAttackPrimed = false;
            var rollCanEvade = false;
            var hazardGroupResolved = false;
            var basicAttackCount = 0;
            var selectedSkill = GetSelectedSkill();

            for (var i = 0; i < turnScript.Path.Count; i++)
            {
                var step = turnScript.Path[i];

                switch (step)
                {
                    case BattleDemoNodeType.Start:
                    case BattleDemoNodeType.Empty:
                        break;

                    case BattleDemoNodeType.Jump:
                        jumpCanEvade = true;
                        jumpAttackPrimed = true;
                        rollCanEvade = false;
                        break;

                    case BattleDemoNodeType.Roll:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = true;
                        break;

                    case BattleDemoNodeType.Dance:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = false;
                        break;

                    case BattleDemoNodeType.Attack:
                    {
                        int damage;
                        string label;

                        if (selectedSkill != null)
                        {
                            damage = selectedSkill.Damage;
                            label = selectedSkill.DisplayName;
                        }
                        else if (jumpAttackPrimed)
                        {
                            basicAttackCount++;
                            damage = jumpAttackDamage;
                            label = "Jump Attack";
                        }
                        else
                        {
                            basicAttackCount++;
                            damage = basicAttackCount >= 2 ? doubleAttackFollowUpDamage : normalAttackDamage;
                            label = basicAttackCount >= 2 ? "Double Attack" : "Attack";
                        }

                        _enemyHp = Mathf.Max(0, _enemyHp - damage);
                        Debug.Log($"[Battle] {label} Damage={damage}");

                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = false;

                        if (_enemyHp <= 0)
                        {
                            _battleEnded = true;
                            return result;
                        }
                        break;
                    }

                    case BattleDemoNodeType.HazardNormal:
                    case BattleDemoNodeType.HazardSkill:
                    {
                        if (hazardGroupResolved)
                        {
                            break;
                        }

                        if (step == BattleDemoNodeType.HazardNormal && jumpCanEvade)
                        {
                            jumpCanEvade = false;
                            hazardGroupResolved = true;
                            break;
                        }

                        if (rollCanEvade)
                        {
                            rollCanEvade = false;
                            jumpAttackPrimed = false;
                            hazardGroupResolved = true;
                            break;
                        }

                        hazardGroupResolved = true;
                        result.TookHit = true;

                        var hazardDamage = hazardBoosted
                            ? Mathf.RoundToInt(turnScript.HazardDamage * 1.5f)
                            : turnScript.HazardDamage;
                        _playerHp = Mathf.Max(0, _playerHp - hazardDamage);
                        Debug.Log($"[Battle] 被弾 Damage={hazardDamage}");

                        if (_playerHp <= 0)
                        {
                            _battleEnded = true;
                        }
                        return result;
                    }

                    case BattleDemoNodeType.Goal:
                        return result;
                }
            }

            return result;
        }

        private void ConsumeSelectedSkillIfNeeded(PathResolutionSummary result)
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

        private BattleDemoSkillSlot GetSelectedSkill()
        {
            if (_selectedSkillSlotIndex < 0 || _selectedSkillSlotIndex >= skillSlots.Count)
            {
                return null;
            }
            return skillSlots[_selectedSkillSlotIndex];
        }

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
                    Description = "単体高火力寄りの Skill。",
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
