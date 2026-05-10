using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Skill Master Data", fileName = "BattleSkillMasterData")]
    public class BattleSkillMasterData : ScriptableObject
    {
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
                Code = code ?? "";
                Message = message ?? "";
                Severity = severity;
            }

            public string Code { get; }
            public string Message { get; }
            public ValidationSeverity Severity { get; }
        }

        [SerializeField]
        [Tooltip("スキルを一意に識別するIDです。将来のロードアウト保存や装備参照のキーになります。1以上の一意な値を入れてください。")]
        [Description("スキルを一意に識別するID。将来の保存やロードアウト参照に使う。")]
        private int skillId = 0;

        [SerializeField]
        [Tooltip("UI表示用のスキル名です。装備詳細やバトル前の選択画面で見える前提です。")]
        [Description("UI表示用のスキル名。")]
        private string displayName = "Skill";

        [SerializeField]
        [Tooltip("スキル説明文です。装備詳細やロードアウト画面で使います。")]
        [Description("スキルの説明文。")]
        private string description = string.Empty;

        [SerializeField]
        [Tooltip("一覧表示用のアイコンです。現時点では未設定でもよいですが、将来の詳細UIでは使用予定です。")]
        [Description("一覧表示や詳細表示に使うアイコン。")]
        private Sprite icon;

        [SerializeField]
        [Tooltip("READY に必要なチャージ量です。1以上を想定します。")]
        [Description("READY に必要なチャージ量。")]
        private int requiredCharge = 3;

        [SerializeField]
        [Tooltip("バトル開始時の初期チャージ量です。RequiredCharge を超えない値にします。")]
        [Description("バトル開始時の初期チャージ量。")]
        private int startingCharge = 0;

        [SerializeField]
        [Tooltip("毎ターン開始時に加算するチャージ量です。")]
        [Description("毎ターン開始時に加算するチャージ量。")]
        private int turnChargeGain = 1;

        [SerializeField]
        [Tooltip("Attack マス解決ごとに加算するチャージ量です。")]
        [Description("Attack マス解決ごとに加算するチャージ量。")]
        private int attackChargeGain = 1;

        [SerializeField]
        [Tooltip("スキル発動時のダメージ量です。純粋支援スキルなら 0 も許容します。")]
        [Description("スキル発動時のダメージ量。")]
        private int damage = 0;

        [SerializeField]
        [Tooltip("このスキルが持つ属性です。通常攻撃属性とは別に、スキルごとに個別設定できます。")]
        [Description("スキルが持つ属性。")]
        private AttributeMasterData attribute;

        [SerializeField]
        [Tooltip("このスキルの主効果種別です。解決ロジックや UI 表示の分岐に使います。")]
        [Description("スキルの主効果種別。")]
        private BattleSkillEffectType effectType = BattleSkillEffectType.Damage;

        [SerializeField]
        [Tooltip("このスキルが使用する演出定義です。文字列キーではなく ScriptableObject の生参照で持ちます。")]
        [Description("このスキルに紐づく演出定義。")]
        private EffectAnimationMasterData effectAnimation;

        [SerializeField]
        [TextArea]
        [Tooltip("説明文の補足です。条件付きの強みや使いどころなど、短い補助テキストを入れます。")]
        [Description("説明文の補足テキスト。")]
        private string descriptionSupplement = string.Empty;

        public int SkillId => skillId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int RequiredCharge => requiredCharge;
        public int StartingCharge => startingCharge;
        public int TurnChargeGain => turnChargeGain;
        public int AttackChargeGain => attackChargeGain;
        public int Damage => damage;
        public AttributeMasterData Attribute => attribute;
        public BattleSkillEffectType EffectType => effectType;
        public EffectAnimationMasterData EffectAnimation => effectAnimation;
        public string DescriptionSupplement => descriptionSupplement;

        public IReadOnlyList<ValidationIssue> GetValidationIssues()
        {
            var issues = new List<ValidationIssue>();

            if (skillId <= 0)
            {
                issues.Add(new ValidationIssue(
                    "missing_skill_id",
                    "SkillId が未設定です。1以上の一意な値を入れてください。",
                    ValidationSeverity.Error));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                issues.Add(new ValidationIssue(
                    "missing_display_name",
                    "DisplayName が空です。装備詳細やロードアウト画面で識別しづらくなります。",
                    ValidationSeverity.Warning));
            }

            if (requiredCharge <= 0)
            {
                issues.Add(new ValidationIssue(
                    "invalid_required_charge",
                    "RequiredCharge は1以上にしてください。",
                    ValidationSeverity.Error));
            }

            if (startingCharge < 0 || startingCharge > Mathf.Max(1, requiredCharge))
            {
                issues.Add(new ValidationIssue(
                    "invalid_starting_charge",
                    "StartingCharge は 0 以上 RequiredCharge 以下にしてください。",
                    ValidationSeverity.Error));
            }

            if (turnChargeGain < 0)
            {
                issues.Add(new ValidationIssue(
                    "invalid_turn_charge_gain",
                    "TurnChargeGain は 0 以上にしてください。",
                    ValidationSeverity.Error));
            }

            if (attackChargeGain < 0)
            {
                issues.Add(new ValidationIssue(
                    "invalid_attack_charge_gain",
                    "AttackChargeGain は 0 以上にしてください。",
                    ValidationSeverity.Error));
            }

            if (damage < 0)
            {
                issues.Add(new ValidationIssue(
                    "invalid_damage",
                    "Damage は 0 以上にしてください。",
                    ValidationSeverity.Error));
            }

            if (effectAnimation == null)
            {
                issues.Add(new ValidationIssue(
                    "missing_effect_animation",
                    "EffectAnimation が未設定です。将来の演出接続先が分からなくなるため、対応する演出 asset を設定してください。",
                    ValidationSeverity.Warning));
            }

            return issues;
        }

        private void OnValidate()
        {
            skillId = Mathf.Max(0, skillId);
            requiredCharge = Mathf.Max(1, requiredCharge);
            startingCharge = Mathf.Clamp(startingCharge, 0, requiredCharge);
            turnChargeGain = Mathf.Max(0, turnChargeGain);
            attackChargeGain = Mathf.Max(0, attackChargeGain);
            damage = Mathf.Max(0, damage);
        }
    }
}
