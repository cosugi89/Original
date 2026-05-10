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
        public IReadOnlyList<BattleSkillData> AssignableSkills { get; init; } = System.Array.Empty<BattleSkillData>();
        public AttributeData NormalAttackAttribute { get; init; }
        public IReadOnlyList<EquipmentAttributeModifierData> AttributeModifiers { get; init; } = System.Array.Empty<EquipmentAttributeModifierData>();
        public IReadOnlyList<string> CategoryTags { get; init; } = System.Array.Empty<string>();
    }

    /// <summary>
    /// バトルスキル1件の転送用データ。装備詳細や将来のロードアウト構築で使う。
    /// マスターデータ（BattleSkillMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class BattleSkillData
    {
        public int SkillId { get; init; } = 0;
        public string DisplayName { get; init; } = "Skill";
        public string Description { get; init; } = string.Empty;
        public Sprite Icon { get; init; }
        public int RequiredCharge { get; init; } = 1;
        public int StartingCharge { get; init; } = 0;
        public int TurnChargeGain { get; init; } = 0;
        public int AttackChargeGain { get; init; } = 0;
        public int Damage { get; init; } = 0;
        public AttributeData Attribute { get; init; }
        public BattleSkillEffectType EffectType { get; init; } = BattleSkillEffectType.Damage;
        public EffectAnimationData EffectAnimation { get; init; }
        public string DescriptionSupplement { get; init; } = string.Empty;
    }

    /// <summary>
    /// スキル演出1件の転送用データ。BattleSkillMasterData が参照する演出定義を runtime へ渡す。
    /// </summary>
    public class EffectAnimationData
    {
        public int EffectAnimationId { get; init; } = 0;
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
        public string DisplayName { get; init; } = "Attribute";
        public string Description { get; init; } = string.Empty;
        public Color AccentColor { get; init; } = Color.white;
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
