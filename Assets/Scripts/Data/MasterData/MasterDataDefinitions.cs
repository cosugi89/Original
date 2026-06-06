using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum BattleSkillEffectType
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

    public enum ValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
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
