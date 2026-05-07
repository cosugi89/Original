using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// ステージ戦闘の敵定義。
    /// appearance は EquipmentMasterData を再利用した見た目定義参照として扱い、
    /// patterns はこの敵が使用可能な共有固定盤面候補を直接参照する。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Enemy", fileName = "BattleEnemyMasterData")]
    public class StageBattleEnemyMasterData : ScriptableObject
    {
        [SerializeField]
        private int enemyId = 0;

        [SerializeField]
        private string enemyName = string.Empty;

        [SerializeField]
        private int maxHp = 1;

        [SerializeField]
        private int damage = 80;

        [SerializeField]
        private List<StageBattleEnemyAppearanceSlotMasterData> appearanceSlots = new();

        [SerializeField]
        private List<StageBattleEnemyAppearanceColorMasterData> appearanceColors = new();

        [SerializeField]
        private List<StageBattlePatternMasterData> patterns = new();

        public int EnemyId => enemyId;

        public string Name => enemyName;

        public int MaxHp => maxHp;

        public int Damage => damage;

        public IReadOnlyList<StageBattleEnemyAppearanceSlotMasterData> AppearanceSlots =>
            appearanceSlots ??= new List<StageBattleEnemyAppearanceSlotMasterData>();

        public IReadOnlyList<StageBattleEnemyAppearanceColorMasterData> AppearanceColors =>
            appearanceColors ??= new List<StageBattleEnemyAppearanceColorMasterData>();

        public IReadOnlyList<StageBattlePatternMasterData> Patterns => patterns ??= new List<StageBattlePatternMasterData>();
    }

    [System.Serializable]
    public class StageBattleEnemyAppearanceSlotMasterData
    {
        [SerializeField]
        private PartsType partType;

        [SerializeField]
        private EquipmentMasterData equipment;

        [SerializeField]
        private bool isVisible = true;

        public PartsType PartType => partType;

        public EquipmentMasterData Equipment => equipment;

        public bool IsVisible => isVisible;
    }

    [System.Serializable]
    public class StageBattleEnemyAppearanceColorMasterData
    {
        [SerializeField]
        private ColorTargetType target;

        [SerializeField]
        private Color color = Color.white;

        public ColorTargetType Target => target;

        public Color Color => color;
    }
}
