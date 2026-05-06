using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Enemy Database", fileName = "EnemyDatabase")]
    public class StageBattleEnemyMasterDatabase : ScriptableObject
    {
        [SerializeField]
        [Tooltip("敵マスタ一覧。EnemyId で検索する。")]
        private List<StageBattleEnemyMasterData> enemies = new();

        public IReadOnlyList<StageBattleEnemyMasterData> Enemies => enemies;

        public bool TryGetById(string enemyId, out StageBattleEnemyMasterData enemyData)
        {
            if (!string.IsNullOrWhiteSpace(enemyId) && enemies != null)
            {
                for (var i = 0; i < enemies.Count; i++)
                {
                    var candidate = enemies[i];
                    if (candidate != null && candidate.EnemyId == enemyId)
                    {
                        enemyData = candidate;
                        return true;
                    }
                }
            }

            enemyData = null;
            return false;
        }
    }
}
