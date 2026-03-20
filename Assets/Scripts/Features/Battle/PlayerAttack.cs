using UnityEngine;

namespace Assets.Scripts.Features.Battle
{
    /// <summary>
    /// AnimationEventReceiver の使用例
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private AnimationEventReceiver receiver;

        private void Awake()
        {
            receiver.OnAttackHitEvent += OnAttackHit;
        }

        private void OnDestroy()
        {
            receiver.OnAttackHitEvent -= OnAttackHit;
        }

        private void OnAttackHit()
        {
            Debug.Log("攻撃ヒット！");
        }
    }
}