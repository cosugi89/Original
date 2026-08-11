using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum BattleSkillOperationType
    {
        [Description("ダメージを与える効果")]
        Damage = 0,

        [Obsolete("旧 Buff 値の互換用です。新規データでは Buff を使ってください。")]
        [Description("旧 Buff 値の互換用。新規データでは使わない。")]
        LegacyBuff = 1,

        [Obsolete("旧 Debuff 値の互換用です。新規データでは Debuff を使ってください。")]
        [Description("旧 Debuff 値の互換用。新規データでは使わない。")]
        LegacyDebuff = 2,

        [Description("自分またはスキルのチャージを増やす効果")]
        ChargeGain = 3,

        [Description("自分のHPを回復する効果")]
        Heal = 4,

        [Description("一時的なGuardまたは防御値を得る効果")]
        GuardGain = 5,

        [Description("自分のステータスやNamedValueを一時強化する効果")]
        Buff = 20,

        [Description("敵のステータスや行動性能を一時弱体化する効果")]
        Debuff = 21,

        [Description("スキルチャージの増減や回転補助、阻害を行う効果")]
        CooldownOrChargeControl = 22,

        [Description("毒、スタン、脆弱などの状態を付与する効果")]
        Status = 30,

        [Description("盤面セルの生成、削除、変換を行う効果")]
        BoardManipulation = 31,

        [Description("危険マスの軽減、消去、移動、弱体化を行う効果")]
        HazardControl = 32,

        [Description("追加行動、再行動、追加パス入力を行う効果")]
        ExtraAction = 33,

        [Description("召喚や一時ユニットを扱う保留効果")]
        Summon = 90,

        [Description("HP、チャージ、Guardなどのリソースを変換する保留効果")]
        ResourceConvert = 91,

        [Description("一時的にパス接続や危険判定などのルールを書き換える保留効果")]
        RuleOverride = 92,
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

        [Obsolete("旧 Speed 値の互換用です。新仕様の戦闘計算では使用しません。")]
        [Description("旧 Speed 値の互換用。新仕様では使わない。")]
        LegacySpeed = 4,

        Charge = 100,
        Guard = 101,
    }

    public static class BattleSkillParameterTypeUtility
    {
        private const int LegacySpeedRawValue = 4;

        public static bool IsLegacySpeed(BattleSkillParameterType parameterType)
        {
            return (int)parameterType == LegacySpeedRawValue;
        }
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
        [Tooltip("旧 Speed 実装の互換フィールドです。新仕様の戦闘計算では使用しません。")]
        private int speed = 0;

        [SerializeField]
        private int guard = 0;

        [SerializeField]
        private List<BattleStatModifierDefinitionEntry> modifiers = new();

        public int Attack => Mathf.Max(0, attack);
        public int Defense => Mathf.Max(0, defense);
        public int LegacySpeed => Mathf.Max(0, speed);
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
