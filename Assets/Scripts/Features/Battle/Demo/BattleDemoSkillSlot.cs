using System;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 デモで用いるスキルスロット1枠分のランタイムデータ。
    /// Inspector で初期値を設定し、実行時にチャージ量を加算・消費する想定。
    /// </summary>
    [Serializable]
    public class BattleDemoSkillSlot
    {
        [Header("Display")]
        [Tooltip("UI 表示用の名称。")]
        public string DisplayName = "Skill";

        [TextArea]
        [Tooltip("説明文。デモではログにも使用。")]
        public string Description = string.Empty;

        [Header("Availability")]
        [Tooltip("プレイヤーがこのスロットを解放済みかどうか。")]
        public bool IsUnlocked = true;

        [Tooltip("武器側に Skill が設定済みかどうか。")]
        public bool IsConfigured = true;

        [Header("Charge")]
        [Tooltip("READY に必要なチャージ量。")]
        public int RequiredCharge = 3;

        [Tooltip("Initialize / ResetRuntime 時にセットされるチャージ量。")]
        public int StartingCharge = 0;

        [Tooltip("ターン開始時に加算されるチャージ量。")]
        public int TurnChargeGain = 1;

        [Tooltip("Attack マス1回あたりの加算チャージ量。デモでは参考値として保持。")]
        public int AttackChargeGain = 1;

        [Header("Effect")]
        [Tooltip("Skill 発動時に敵へ与えるダメージ。")]
        public int Damage = 0;

        private int _currentCharge;

        /// <summary>現在のチャージ量。</summary>
        public int CurrentCharge => _currentCharge;

        /// <summary>チャージが満タンで選択可能な状態か。</summary>
        public bool IsReady => IsUnlocked && IsConfigured && _currentCharge >= Mathf.Max(1, RequiredCharge);

        /// <summary>
        /// 実行時の状態を初期値へ戻す。
        /// </summary>
        public void ResetRuntime()
        {
            var required = Mathf.Max(1, RequiredCharge);
            _currentCharge = Mathf.Clamp(StartingCharge, 0, required);
        }

        /// <summary>
        /// ターン開始時のチャージ加算を行う。
        /// </summary>
        public void GainTurnCharge()
        {
            if (!IsUnlocked || !IsConfigured)
            {
                return;
            }

            if (TurnChargeGain <= 0)
            {
                return;
            }

            var required = Mathf.Max(1, RequiredCharge);
            _currentCharge = Mathf.Min(required, _currentCharge + TurnChargeGain);
        }

        /// <summary>
        /// Attack マスを解決するたびに1回ぶんのチャージ加算を行う。
        /// </summary>
        public void GainAttackCharge()
        {
            if (!IsUnlocked || !IsConfigured)
            {
                return;
            }

            if (AttackChargeGain <= 0)
            {
                return;
            }

            var required = Mathf.Max(1, RequiredCharge);
            _currentCharge = Mathf.Min(required, _currentCharge + AttackChargeGain);
        }

        /// <summary>
        /// スキル発動時の消費処理。
        /// </summary>
        public void Consume()
        {
            _currentCharge = 0;
        }

        /// <summary>
        /// READY になるまでに必要なターン数を返す。
        /// ターン加算が設定されていない場合は int.MaxValue を返す。
        /// </summary>
        public int GetRemainingTurnsUntilReady()
        {
            var required = Mathf.Max(1, RequiredCharge);
            if (_currentCharge >= required)
            {
                return 0;
            }

            if (TurnChargeGain <= 0)
            {
                return int.MaxValue;
            }

            var missing = required - _currentCharge;
            return (missing + TurnChargeGain - 1) / TurnChargeGain;
        }

        /// <summary>
        /// ログ用の状態ラベルを返す。
        /// </summary>
        public string GetStateLabel()
        {
            if (!IsUnlocked)
            {
                return "LOCKED (未取得)";
            }

            if (!IsConfigured)
            {
                return "EMPTY (未設定)";
            }

            if (IsReady)
            {
                return "READY";
            }

            return "CHARGING";
        }

        /// <summary>
        /// ログ用のチャージラベル "current/required" を返す。
        /// </summary>
        public string GetChargeLabel()
        {
            if (!IsUnlocked || !IsConfigured)
            {
                return "-/-";
            }

            return $"{_currentCharge}/{Mathf.Max(1, RequiredCharge)}";
        }
    }
}
