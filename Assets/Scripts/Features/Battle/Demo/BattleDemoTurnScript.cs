using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 デモで1ターンぶんの進行をシナリオ再生するためのデータ。
    /// Inspector で Path と敵行動を固定し、ドラッグ入力の代わりにプリセット経路を再生する。
    /// </summary>
    [Serializable]
    public class BattleDemoTurnScript
    {
        [Header("Overview")]
        [Tooltip("ログ表示用のターン識別名。")]
        public string Label = "Turn";

        [Tooltip("このターン敵が行う行動の種別。")]
        public BattleDemoEnemyActionType EnemyAction = BattleDemoEnemyActionType.NormalAttack;

        [TextArea]
        [Tooltip("盤面の概要を一言で伝えるログ。")]
        public string BoardSummary = string.Empty;

        [Tooltip("確定演出時に出す短文。")]
        public string ConfirmText = "いける";

        [TextArea]
        [Tooltip("このターンで確認したい挙動のメモ。")]
        public string Notes = string.Empty;

        [Header("Parameters")]
        [Tooltip("被弾時に与えられるダメージ量。")]
        public int HazardDamage = 80;

        [Header("Path")]
        [Tooltip("Start から Goal までのプリセット経路。デモではドラッグ入力の代わりとなる。")]
        public List<BattleDemoNodeType> Path = new();

        /// <summary>
        /// 経路内に Goal ノードが含まれているか。
        /// </summary>
        public bool HasGoalInPath()
        {
            if (Path == null)
            {
                return false;
            }

            for (var i = 0; i < Path.Count; i++)
            {
                if (Path[i] == BattleDemoNodeType.Goal)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 経路の並びを "Start -> Attack -> ..." 形式で返す。
        /// </summary>
        public string GetPathSummary()
        {
            if (Path == null || Path.Count == 0)
            {
                return "(empty)";
            }

            return string.Join(" -> ", Path);
        }
    }

    /// <summary>
    /// Battle デモで使う既定の Skill / ターンスクリプトをまとめる。
    /// BattleScene からデータ定義の詳細を追い出し、責務を軽くする。
    /// </summary>
    public static class BattleDemoContentFactory
    {
        public static List<BattleDemoSkillSlot> CreateDefaultSkillSlots()
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

        public static List<BattleDemoTurnScript> CreateDefaultTurnScripts()
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
