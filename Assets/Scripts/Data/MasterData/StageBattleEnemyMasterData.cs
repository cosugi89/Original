using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// ステージ戦闘の敵定義。
    /// appearance は現在の敵装備・見た目定義として扱い、
    /// patterns はこの敵が使用可能な共有固定盤面候補を直接参照する。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Enemy", fileName = "BattleEnemyMasterData")]
    public class StageBattleEnemyMasterData : ScriptableObject
    {
        [SerializeField]
        private string enemyId = string.Empty;

        [SerializeField]
        private string enemyName = string.Empty;

        [SerializeField]
        private int maxHp = 1;

        [SerializeField]
        private int damage = 80;

        [SerializeField]
        private AppearanceData appearance = new();

        [SerializeField]
        private List<StageBattlePatternMasterData> patterns = new();

        public string EnemyId => enemyId;

        public string Name => enemyName;

        public int MaxHp => maxHp;

        public int Damage => damage;

        public AppearanceData Appearance => appearance ??= new AppearanceData();

        public IReadOnlyList<StageBattlePatternMasterData> Patterns => patterns ??= new List<StageBattlePatternMasterData>();
    }
}
