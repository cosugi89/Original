using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum BattleSkillOperationType
    {
        [Description("ダメージを与える効果")]
        Damage = 0,

        [Description("味方を強化する効果")]
        Buff = 1,

        [Description("敵を弱体化する効果")]
        Debuff = 2,

        [Description("その他の効果")]
        Utility = 3,
    }

    public enum BattleSkillTargetType
    {
        [Description("対象なし")]
        None = 0,

        [Description("敵単体")]
        Enemy = 1,

        [Description("敵全体")]
        AllEnemies = 2,

        [Description("自分")]
        Self = 3,
    }

    public enum BattleSkillValueType
    {
        [Description("効果値なし")]
        None = 0,

        [Description("攻撃力などに対する倍率。例: 2.0倍ダメージ")]
        Multiplier = 1,

        [Description("固定値。例: 10回復、チャージ+1")]
        FixedValue = 2,

        [Description("割合。例: 攻撃力20%上昇、防御力30%低下")]
        Percentage = 3,
    }

    public enum BattleSkillParameterType
    {
        None = 0,

        Hp = 1,
        Attack = 2,
        Defense = 3,
        Speed = 4,

        Charge = 100,
        Guard = 101,
    }

    public enum BattleStatModifierTargetType
    {
        Parameter = 0,
        NamedValue = 1,
    }

    public enum BattleStatModifierOperationType
    {
        Set = 0,
        Add = 1,
        Multiply = 2,
    }

    public enum ValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    [System.Serializable]
    public class BattleDamageFormulaDefinition
    {
        [SerializeField]
        private bool isEnabled;

        [SerializeField]
        private BattleSkillParameterType parameterType = BattleSkillParameterType.Attack;

        [SerializeField]
        private BattleSkillValueType valueType = BattleSkillValueType.Multiplier;

        [SerializeField]
        private float value = 1f;

        [SerializeField]
        private int flatBonus = 0;

        [SerializeField]
        private float finalMultiplier = 1f;

        [SerializeField]
        private string sourceScalarKey = string.Empty;

        [SerializeField]
        private float sourceScalarFallback = 1f;

        [SerializeField]
        private int minimumDamage = 0;

        public bool IsEnabled => isEnabled;
        public BattleSkillParameterType ParameterType => parameterType;
        public BattleSkillValueType ValueType => valueType;
        public float Value => value;
        public int FlatBonus => Mathf.Max(0, flatBonus);
        public float FinalMultiplier => Mathf.Max(0f, finalMultiplier);
        public string SourceScalarKey => sourceScalarKey ?? string.Empty;
        public float SourceScalarFallback => sourceScalarFallback;
        public int MinimumDamage => Mathf.Max(0, minimumDamage);

        public void Normalize()
        {
            flatBonus = Mathf.Max(0, flatBonus);
            finalMultiplier = Mathf.Max(0f, finalMultiplier);
            minimumDamage = Mathf.Max(0, minimumDamage);
            sourceScalarKey = (sourceScalarKey ?? string.Empty).Trim();
        }
    }

    [System.Serializable]
    public class BattleStatModifierDefinitionEntry
    {
        [SerializeField]
        private BattleStatModifierTargetType targetType = BattleStatModifierTargetType.Parameter;

        [SerializeField]
        private BattleSkillParameterType parameterType = BattleSkillParameterType.Attack;

        [SerializeField]
        private string namedValueKey = string.Empty;

        [SerializeField]
        private BattleStatModifierOperationType operationType = BattleStatModifierOperationType.Add;

        [SerializeField]
        private float value = 0f;

        public BattleStatModifierTargetType TargetType => targetType;
        public BattleSkillParameterType ParameterType => parameterType;
        public string NamedValueKey => namedValueKey ?? string.Empty;
        public BattleStatModifierOperationType OperationType => operationType;
        public float Value => value;

        public void Normalize()
        {
            namedValueKey = (namedValueKey ?? string.Empty).Trim();
        }
    }

    [System.Serializable]
    public class BattleStatBlockDefinition
    {
        [SerializeField]
        private int attack = 0;

        [SerializeField]
        private int defense = 0;

        [SerializeField]
        private int speed = 0;

        [SerializeField]
        private int guard = 0;

        [SerializeField]
        private List<BattleStatModifierDefinitionEntry> modifiers = new();

        public int Attack => Mathf.Max(0, attack);
        public int Defense => Mathf.Max(0, defense);
        public int Speed => Mathf.Max(0, speed);
        public int Guard => Mathf.Max(0, guard);
        public IReadOnlyList<BattleStatModifierDefinitionEntry> Modifiers =>
            modifiers ??= new List<BattleStatModifierDefinitionEntry>();

        public void Normalize()
        {
            attack = Mathf.Max(0, attack);
            defense = Mathf.Max(0, defense);
            speed = Mathf.Max(0, speed);
            guard = Mathf.Max(0, guard);
            modifiers ??= new List<BattleStatModifierDefinitionEntry>();

            for (var i = 0; i < modifiers.Count; i++)
            {
                modifiers[i]?.Normalize();
            }
        }
    }

    public readonly struct ValidationIssue
    {
        public ValidationIssue(string code, string message, ValidationSeverity severity)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Severity = severity;
        }

        public string Code { get; }
        public string Message { get; }
        public ValidationSeverity Severity { get; }
    }

    public abstract class ValidatedMasterDataAsset : ScriptableObject
    {
        public IReadOnlyList<ValidationIssue> GetValidationIssues()
        {
            var issues = new List<ValidationIssue>();
            CollectValidationIssues(issues);
            return issues;
        }

        protected virtual void OnValidate()
        {
            NormalizeFields();
        }

        protected abstract void NormalizeFields();

        protected abstract void CollectValidationIssues(List<ValidationIssue> issues);

        protected static void AddIssue(List<ValidationIssue> issues, string code, string message, ValidationSeverity severity)
        {
            if (issues == null)
            {
                return;
            }

            issues.Add(new ValidationIssue(code, message, severity));
        }

        protected static void AddRequiredStringIssue(
            List<ValidationIssue> issues,
            string value,
            string code,
            string message,
            ValidationSeverity severity)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            AddIssue(issues, code, message, severity);
        }

        protected static void AddMinValueIssue(
            List<ValidationIssue> issues,
            int value,
            int min,
            string code,
            string message,
            ValidationSeverity severity)
        {
            if (value >= min)
            {
                return;
            }

            AddIssue(issues, code, message, severity);
        }

        protected static void AddNullReferenceIssue(
            List<ValidationIssue> issues,
            object value,
            string code,
            string message,
            ValidationSeverity severity)
        {
            if (value != null)
            {
                return;
            }

            AddIssue(issues, code, message, severity);
        }
    }
}
