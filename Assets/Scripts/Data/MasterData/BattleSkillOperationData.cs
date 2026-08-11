using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// スキルの効果データ。
    /// 例: 敵単体に攻撃力の1.5倍ダメージ、自分のチャージ+1、HP回復など。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Skill Operation Data", fileName = "BattleSkillOperationData")]
    public class BattleSkillOperationData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Damage / ChargeGain / Heal / GuardGain など、効果の処理カテゴリです。")]
        [Description("効果種別")]
        private BattleSkillOperationType effectType = BattleSkillOperationType.Damage;

        [SerializeField][Description("対象")]
        private BattleSkillTargetType targetType = BattleSkillTargetType.Enemy;

        [SerializeField][Description("対象パラメータ")]
        private BattleSkillParameterType parameterType = BattleSkillParameterType.None;

        [SerializeField][Description("値の種類")]
        private BattleSkillValueType valueType = BattleSkillValueType.Multiplier;

        [SerializeField][Description("効果値")]
        private float value = 1.0f;

        [SerializeField]
        [Description("有効化時のみ BattleDamageCalculator へ渡すカスタム damage formula。未設定なら parameter/value をそのまま使う。")]
        private BattleDamageFormulaDefinition damageFormula = new();

        [SerializeField][Description("継続ターン数")]
        private int durationTurns = 0;

        [SerializeField][Description("成功率")]
        [Range(0f, 1f)]
        private float successRate = 1.0f;

        public BattleSkillOperationType EffectType => effectType;
        public BattleSkillTargetType TargetType => targetType;
        public BattleSkillParameterType ParameterType => parameterType;
        public BattleSkillValueType ValueType => valueType;
        public float Value => value;
        public BattleDamageFormulaDefinition DamageFormula => damageFormula ??= new BattleDamageFormulaDefinition();
        public int DurationTurns => durationTurns;
        public float SuccessRate => successRate;

        private void OnValidate()
        {
            durationTurns = Mathf.Max(0, durationTurns);
            successRate = Mathf.Clamp01(successRate);
            damageFormula ??= new BattleDamageFormulaDefinition();
            damageFormula.Normalize();
        }
    }
}
