using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 上部バトル演出領域の親 View。
    /// </summary>
    public sealed class BattlePresentationView : MonoBehaviour
    {
        [SerializeField] private EnemyAreaView enemyAreaView;
        [SerializeField] private PlayerAreaView playerAreaView;
        [SerializeField] private BattleDamageFeedbackView damageFeedbackView;
        [SerializeField] private BattleEventNoticeView eventNoticeView;

        public EnemyAreaView EnemyAreaView
        {
            get
            {
                ResolveMissingReferences();
                return enemyAreaView;
            }
        }

        public PlayerAreaView PlayerAreaView
        {
            get
            {
                ResolveMissingReferences();
                return playerAreaView;
            }
        }

        public BattleDamageFeedbackView DamageFeedbackView
        {
            get
            {
                ResolveMissingReferences();
                return damageFeedbackView;
            }
        }

        public BattleEventNoticeView EventNoticeView
        {
            get
            {
                ResolveMissingReferences();
                return eventNoticeView;
            }
        }

        public void ApplySession(BattleSessionState session)
        {
            EnemyAreaView?.ApplySingleEnemy(session);
            PlayerAreaView?.ApplySession(session);
        }

        public void ResolveMissingReferences()
        {
            if (enemyAreaView == null)
            {
                enemyAreaView = GetComponentInChildren<EnemyAreaView>(true);
            }

            if (playerAreaView == null)
            {
                playerAreaView = GetComponentInChildren<PlayerAreaView>(true);
            }

            if (damageFeedbackView == null)
            {
                damageFeedbackView = GetComponentInChildren<BattleDamageFeedbackView>(true);
            }

            if (eventNoticeView == null)
            {
                eventNoticeView = GetComponentInChildren<BattleEventNoticeView>(true);
            }
        }
    }
}
