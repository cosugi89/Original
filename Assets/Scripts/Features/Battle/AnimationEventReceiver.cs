using System;
using UnityEngine;

// MEMO: Battle固有の概念を消して汎用化できたら Core に移動
namespace Assets.Scripts.Features.Battle
{
    /// <summary>
    /// Amimator Clip のイベントを受け取る Receiver クラス
    /// 設定: Animation Window > Animation Event > Add Event でイベントを追加し関数名を指定する。
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        /// <summary>
        /// 攻撃時に呼び出されるイベント
        /// </summary>
        public event Action OnAttackHitEvent;

        /// <summary>
        /// スキル発動時に呼び出されるイベント
        /// </summary>
        public event Action OnSkillHitEvent;

        public void OnAttackHit()
        {
            // イベント追加の例：PlayerAttack
            OnAttackHitEvent?.Invoke();
        }

        public void OnSkillHit()
        {
            OnSkillHitEvent?.Invoke();
        }
    }
}
