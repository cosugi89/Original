using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// スキルの効果データ
    /// 例: 90%の確率で敵単体に攻撃力の1.5倍のダメージを与える、など
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Skill Effect Data", fileName = "BattleSkillEffectData")]
    public class BattleSkillEffectData : ScriptableObject
    {
        [SerializeField][Description("効果種別")]
        private BattleSkillEffectType effectType = BattleSkillEffectType.Damage;

        [SerializeField][Description("対象")]
        private BattleSkillTargetType targetType = BattleSkillTargetType.Enemy;

        [SerializeField][Description("対象パラメータ")]
        private BattleSkillParameterType parameterType = BattleSkillParameterType.None;

        [SerializeField][Description("値の種類")]
        private BattleSkillValueType valueType = BattleSkillValueType.Multiplier;

        [SerializeField][Description("効果値")]
        private float value = 1.0f;

        [SerializeField][Description("継続ターン数")]
        private int durationTurns = 0;

        [SerializeField][Description("成功率")]
        [Range(0f, 1f)]
        private float successRate = 1.0f;

        public BattleSkillEffectType EffectType => effectType;
        public BattleSkillTargetType TargetType => targetType;
        public BattleSkillParameterType ParameterType => parameterType;
        public BattleSkillValueType ValueType => valueType;
        public float Value => value;
        public int DurationTurns => durationTurns;
        public float SuccessRate => successRate;

        private void OnValidate()
        {
            durationTurns = Mathf.Max(0, durationTurns);
            successRate = Mathf.Clamp01(successRate);
        }
    }
}
