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
}
