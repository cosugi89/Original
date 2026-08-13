using System;
using Assets.Scripts.Features.Battle.Core;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Components
{
    /// <summary>
    /// アニメーションクリップから戦闘演出イベントを受け取り、購読先へ中継する。
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        /// <summary>
        /// AnimationEvent から発火した戦闘演出イベントを中継する。
        /// </summary>
        public event Action<AnimationEventReceiver, string> BattleAnimationEventRaised;

        /// <summary>
        /// 通常攻撃ヒット時に発火するイベント。
        /// </summary>
        public event Action OnAttackHitEvent;

        /// <summary>
        /// スキルヒット時に発火するイベント。
        /// </summary>
        public event Action OnSkillHitEvent;

        public void OnAttackStart()
        {
            RaiseBattleAnimationEvent(BattleAnimationEventKeys.AttackStart);
        }

        public void OnAttackHit()
        {
            OnAttackHitEvent?.Invoke();
            RaiseBattleAnimationEvent(BattleAnimationEventKeys.Hit);
        }

        public void OnSkillHit()
        {
            OnSkillHitEvent?.Invoke();
            RaiseBattleAnimationEvent(BattleAnimationEventKeys.Hit);
        }

        public void OnHit()
        {
            RaiseBattleAnimationEvent(BattleAnimationEventKeys.Hit);
        }

        public void OnRecoverEnd()
        {
            RaiseBattleAnimationEvent(BattleAnimationEventKeys.RecoverEnd);
        }

        public void OnBattleAnimationEvent(string eventKey)
        {
            RaiseBattleAnimationEvent(eventKey);
        }

        private void RaiseBattleAnimationEvent(string eventKey)
        {
            if (string.IsNullOrWhiteSpace(eventKey))
            {
                return;
            }

            BattleAnimationEventRaised?.Invoke(this, eventKey);
        }
    }
}
