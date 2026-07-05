using UnityEngine;

namespace Assets.Scripts.Features.Battle.Components
{
    /// <summary>
    /// AnimationEventReceiver を購読して攻撃ヒット通知を受けるサンプルコンポーネント。
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
