using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Skill Master Data", fileName = "BattleSkillMasterData")]
    public class BattleSkillMasterData : ValidatedMasterDataAsset
    {
        [SerializeField][Description("スキルID")]
        private int skillId = 0;

        [SerializeField][Description("内部処理用キー")]
        private string skillKey = "skill";

        [SerializeField][Description("UI表示用のスキル名")]
        private string displayName = "スキル";

        [SerializeField][Description("説明")]
        private string description = string.Empty;

        [SerializeField][Description("アイコン")]
        private Sprite icon;

        [SerializeField][Description("属性")]
        private AttributeMasterData[] attribute = Array.Empty<AttributeMasterData>();

        [SerializeField][Description("効果データ")]
        private BattleSkillEffectData[] effectData = Array.Empty<BattleSkillEffectData>();

        [SerializeField][Description("エフェクトデータ")]
        private EffectAnimationMasterData effectAnimation;

        [SerializeField][Description("必要なチャージ量")]
        private int requiredCharge = 3;

        [SerializeField][Description("初期チャージ量")]
        private int startingCharge = 0;

        [SerializeField][Description("ターン毎に加算するチャージ量")]
        private int turnChargeGain = 1;

        public int SkillId => skillId;
        public string SkillKey => skillKey ?? string.Empty;
        public string DisplayName => displayName ?? "Skill";
        public string Description => description ?? string.Empty;
        public Sprite Icon => icon;
        public IReadOnlyList<AttributeMasterData> Attributes => attribute ?? Array.Empty<AttributeMasterData>();
        public AttributeMasterData PrimaryAttribute => ResolvePrimaryAttribute();
        public IReadOnlyList<BattleSkillEffectData> EffectData => effectData ?? Array.Empty<BattleSkillEffectData>();
        public BattleSkillEffectData PrimaryEffectData => ResolvePrimaryEffectData();
        public EffectAnimationMasterData EffectAnimation => effectAnimation;
        public int RequiredCharge => requiredCharge;
        public int StartingCharge => startingCharge;
        public int TurnChargeGain => turnChargeGain;

        private AttributeMasterData ResolvePrimaryAttribute()
        {
            var attributes = Attributes;
            for (var i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] != null)
                {
                    return attributes[i];
                }
            }

            return null;
        }

        private BattleSkillEffectData ResolvePrimaryEffectData()
        {
            BattleSkillEffectData firstNonNullEffect = null;
            var effects = EffectData;

            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                firstNonNullEffect ??= effect;
                if (effect.EffectType == BattleSkillEffectType.Damage)
                {
                    return effect;
                }
            }

            return firstNonNullEffect;
        }

        #region Validation
        protected override void NormalizeFields()
        {
            skillId = Mathf.Max(0, skillId);
            skillKey = (skillKey ?? string.Empty).Trim();
            requiredCharge = Mathf.Max(1, requiredCharge);
            startingCharge = Mathf.Clamp(startingCharge, 0, requiredCharge);
            turnChargeGain = Mathf.Max(0, turnChargeGain);
            attribute ??= Array.Empty<AttributeMasterData>();
            effectData ??= Array.Empty<BattleSkillEffectData>();
        }

        protected override void CollectValidationIssues(List<ValidationIssue> issues)
        {
            AddMinValueIssue(
                issues,
                skillId,
                1,
                "missing_skill_id",
                "SkillId が未設定です。1以上の一意な値を入れてください。",
                ValidationSeverity.Error);

            AddRequiredStringIssue(
                issues,
                skillKey,
                "missing_skill_key",
                "SkillKey が空です。内部処理用の一意キーを設定してください。",
                ValidationSeverity.Error);

            AddRequiredStringIssue(
                issues,
                displayName,
                "missing_display_name",
                "DisplayName が空です。装備詳細やロードアウト画面で識別しづらくなります。",
                ValidationSeverity.Warning);

            AddMinValueIssue(
                issues,
                requiredCharge,
                1,
                "invalid_required_charge",
                "RequiredCharge は1以上にしてください。",
                ValidationSeverity.Error);

            if (startingCharge < 0 || startingCharge > Mathf.Max(1, requiredCharge))
            {
                AddIssue(
                    issues,
                    "invalid_starting_charge",
                    "StartingCharge は 0 以上 RequiredCharge 以下にしてください。",
                    ValidationSeverity.Error);
            }

            AddMinValueIssue(
                issues,
                turnChargeGain,
                0,
                "invalid_turn_charge_gain",
                "TurnChargeGain は 0 以上にしてください。",
                ValidationSeverity.Error);

            if (!HasConfiguredEffects())
            {
                AddIssue(
                    issues,
                    "missing_effect_data",
                    "EffectData が未設定です。少なくとも 1 件の効果データを設定してください。",
                    ValidationSeverity.Warning);
            }

            AddNullReferenceIssue(
                issues,
                effectAnimation,
                "missing_effect_animation",
                "EffectAnimation が未設定です。将来の演出接続先が分からなくなるため、対応する演出 asset を設定してください。",
                ValidationSeverity.Warning);

            if (HasUnsupportedDamageEffectValue())
            {
                AddIssue(
                    issues,
                    "unsupported_damage_effect_value_type",
                    "現在の BattleTurnResolver は Damage 効果の FixedValue/None を直接扱います。Multiplier/Percentage は将来ロジック追加が必要です。",
                    ValidationSeverity.Warning);
            }
        }

        private bool HasConfiguredEffects()
        {
            var effects = EffectData;
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasUnsupportedDamageEffectValue()
        {
            var primaryEffect = PrimaryEffectData;
            if (primaryEffect == null || primaryEffect.EffectType != BattleSkillEffectType.Damage)
            {
                return false;
            }

            return primaryEffect.ValueType != BattleSkillValueType.FixedValue &&
                   primaryEffect.ValueType != BattleSkillValueType.None;
        }
        #endregion
    }
}
