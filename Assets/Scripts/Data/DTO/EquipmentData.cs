using System.Collections.Generic;
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
    }
}
