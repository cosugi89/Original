using System;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Components
{
    /// <summary>
    /// アニメーションクリップから攻撃ヒット通知を受け取り、購読先へ中継する。
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        /// <summary>
        /// 通常攻撃ヒット時に発火するイベント。
        /// </summary>
        public event Action OnAttackHitEvent;

        /// <summary>
        /// スキルヒット時に発火するイベント。
        /// </summary>
        public event Action OnSkillHitEvent;

        public void OnAttackHit()
        {
            OnAttackHitEvent?.Invoke();
        }

        public void OnSkillHit()
        {
            OnSkillHitEvent?.Invoke();
        }
    }
}
