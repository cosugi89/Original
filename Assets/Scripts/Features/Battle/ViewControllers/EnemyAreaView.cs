using System;
using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 敵表示領域を管理する。初期は単体敵、将来は複数 EnemyUnitView を束ねる。
    /// </summary>
    public sealed class EnemyAreaView : MonoBehaviour
    {
        [SerializeField] private EnemyUnitView[] enemyUnits = Array.Empty<EnemyUnitView>();

        public EnemyUnitView PrimaryEnemyUnit
        {
            get
            {
                ResolveMissingReferences();
                return enemyUnits != null && enemyUnits.Length > 0 ? enemyUnits[0] : null;
            }
        }

        public void ApplySingleEnemy(BattleSessionState session)
        {
            PrimaryEnemyUnit?.ApplySession(session);
        }

        public void ResolveMissingReferences()
        {
            if (enemyUnits != null && enemyUnits.Length > 0)
            {
                return;
            }

            var unit = GetComponentInChildren<EnemyUnitView>(true);
            enemyUnits = unit != null ? new[] { unit } : Array.Empty<EnemyUnitView>();
        }
    }
}
