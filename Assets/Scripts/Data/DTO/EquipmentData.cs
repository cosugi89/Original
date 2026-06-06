using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// 装備1件の転送用データ。ロジック層が参照する装備情報。
    /// マスターデータ（EquipmentMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class EquipmentData
    {
        public int EquipmentId { get; init; } = 0;
        public string DisplayName { get; init; } = "";
        public PartsType PartType { get; init; }
        public PartsExclusiveGroup ExclusiveGroup { get; init; } = PartsExclusiveGroup.None;
        public bool IsRightHandEquipment => ExclusiveGroup == PartsExclusiveGroup.HandRight;
        public bool IsLeftHandEquipment => ExclusiveGroup == PartsExclusiveGroup.HandLeft;
        public int PartsIndex { get; init; } = -1;
        public Sprite Icon { get; init; }
        public int SortOrder { get; init; } = 0;
        public bool IsDefaultOwned { get; init; }
        public IReadOnlyList<BattleSkillData> AssignableSkills { get; init; } = Array.Empty<BattleSkillData>();
        public AttributeData NormalAttackAttribute { get; init; }
        public IReadOnlyList<EquipmentAttributeModifierData> AttributeModifiers { get; init; } = Array.Empty<EquipmentAttributeModifierData>();
        public IReadOnlyList<string> CategoryTags { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// バトルスキル1件の転送用データ。装備詳細や将来のロードアウト構築で使う。
    /// マスターデータ（BattleSkillMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class BattleSkillData
    {
        public int SkillId { get; init; } = 0;
        public string SkillKey { get; init; } = string.Empty;
        public string DisplayName { get; init; } = "Skill";
        public string Description { get; init; } = string.Empty;
        public Sprite Icon { get; init; }
        public int RequiredCharge { get; init; } = 1;
        public int StartingCharge { get; init; } = 0;
        public int TurnChargeGain { get; init; } = 0;
        public IReadOnlyList<AttributeData> Attributes { get; init; } = Array.Empty<AttributeData>();
        public IReadOnlyList<BattleSkillEffectEntryData> Effects { get; init; } = Array.Empty<BattleSkillEffectEntryData>();
        public EffectAnimationData EffectAnimation { get; init; }

        public AttributeData Attribute => ResolvePrimaryAttribute();
        public BattleSkillEffectEntryData PrimaryEffect => ResolvePrimaryEffect();
        public BattleSkillEffectType EffectType => PrimaryEffect?.EffectType ?? BattleSkillEffectType.Damage;
        public int Damage => ResolveDamageValue(PrimaryEffect);

        private AttributeData ResolvePrimaryAttribute()
        {
            if (Attributes == null || Attributes.Count == 0)
            {
                return null;
            }

            for (var i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i] != null)
                {
                    return Attributes[i];
                }
            }

            return null;
        }

        private BattleSkillEffectEntryData ResolvePrimaryEffect()
        {
            if (Effects == null || Effects.Count == 0)
            {
                return null;
            }

            BattleSkillEffectEntryData firstNonNullEffect = null;
            for (var i = 0; i < Effects.Count; i++)
            {
                var effect = Effects[i];
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

        private static int ResolveDamageValue(BattleSkillEffectEntryData primaryEffect)
        {
            if (primaryEffect == null || primaryEffect.EffectType != BattleSkillEffectType.Damage)
            {
                return 0;
            }

            if (primaryEffect.ValueType != BattleSkillValueType.FixedValue &&
                primaryEffect.ValueType != BattleSkillValueType.None)
            {
                return 0;
            }

            return Mathf.Max(0, Mathf.RoundToInt(primaryEffect.Value));
        }
    }

    /// <summary>
    /// スキル効果1件の転送用データ。複数効果を持つ Skill の詳細を runtime へ渡す。
    /// </summary>
    public class BattleSkillEffectEntryData
    {
        public BattleSkillEffectType EffectType { get; init; } = BattleSkillEffectType.Damage;
        public BattleSkillTargetType TargetType { get; init; } = BattleSkillTargetType.Enemy;
        public BattleSkillParameterType ParameterType { get; init; } = BattleSkillParameterType.None;
        public BattleSkillValueType ValueType { get; init; } = BattleSkillValueType.None;
        public float Value { get; init; } = 0f;
        public int DurationTurns { get; init; } = 0;
        public float SuccessRate { get; init; } = 1f;
    }

    /// <summary>
    /// スキル演出1件の転送用データ。BattleSkillMasterData が参照する演出定義を runtime へ渡す。
    /// </summary>
    public class EffectAnimationData
    {
        public int EffectAnimationId { get; init; } = 0;
        public string EffectAnimationKey { get; init; } = string.Empty;
        public string DisplayName { get; init; } = "Effect Animation";
        public string Description { get; init; } = string.Empty;
        public string CharacterAnimationName { get; init; } = string.Empty;
        public string VisualEffectKey { get; init; } = string.Empty;
        public string SoundEffectKey { get; init; } = string.Empty;
        public float WaitSeconds { get; init; } = 0f;
    }

    /// <summary>
    /// 属性1件の転送用データ。装備とスキルが参照する共通属性定義を runtime へ渡す。
    /// </summary>
    public class AttributeData
    {
        public int AttributeId { get; init; } = 0;
        public string AttributeKey { get; init; } = string.Empty;
        public string DisplayName { get; init; } = "Attribute";
        public string Description { get; init; } = string.Empty;
        public Color AccentColor { get; init; } = Color.white;
        public bool IsVisibleInUI { get; init; } = true;
        public Sprite Icon { get; init; }
        public int SortOrder { get; init; } = 0;
    }

    /// <summary>
    /// 装備の属性補正1件ぶんの転送用データ。
    /// </summary>
    public class EquipmentAttributeModifierData
    {
        public AttributeData Attribute { get; init; }
        public int DamagePercent { get; init; } = 100;
    }
}
