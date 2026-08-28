using System.Collections.Generic;
using System.ComponentModel;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Equipment Master Data", fileName = "EquipmentMasterData")]
    public class EquipmentMasterData : ValidatedMasterDataAsset
    {
        public const int MaxAssignableSkillCount = 2;

        [SerializeField]
        [Tooltip("保存データから参照される装備IDです。0は未装備扱いなので、手動作成時は1以上の一意な値を入れてください。")]
        [Description("装備/見た目定義を一意に識別するID。保存データから参照されるキー。")]
        private int equipmentId = 0;

        [SerializeField]
        [Tooltip("UI上で見せる名前です。自動生成後に意味のある名称へ直しておくと管理しやすくなります。")]
        [Description("画面表示用の装備名・見た目名。")]
        private string displayName = "";

        [SerializeField]
        [Tooltip("この装備が属する見た目枠です。設定した PartType と PartsIndex の組み合わせが AppearanceController 解決に使われます。")]
        [Description("どの見た目枠に属する定義か。Hair、Sword、Shield など。")]
        private PartsType partType;

        [SerializeField]
        [Tooltip("AppearanceController 上の見た目 index です。実際の sprite 切り替えに使われるため、Character.prefab の定義と揃えてください。")]
        [Description("現在の AppearanceController が見た目を切り替えるために使う index。partsIndex 依存はここに閉じ込める。")]
        private int partsIndex = -1;

        [SerializeField]
        [Tooltip("一覧表示に使うアイコンです。IDから見た目は復元しないので、null のままにしないでください。")]
        [Description("一覧表示やボタン表示に使うアイコン。appearance 選択UIでも使い回せる。")]
        private Sprite icon;

        [SerializeField]
        [Tooltip("UIでの並び順です。小さいほど前に表示されます。未設定なら partsIndex ベースの並びにフォールバックします。")]
        [Description("UI上の並び順。0以下なら partsIndex ベースの既定順を使う。")]
        private int sortOrder = 0;

        [SerializeField]
        [Tooltip("新規セーブ時に自動で所持済みとして配るかどうかです。初期装備や初期見た目に使います。")]
        [Description("新規セーブ時に最初から所持している装備かどうか。")]
        private bool isDefaultOwned;

        [SerializeField]
        [Tooltip("この装備に紐づく候補スキル一覧です。最大2件までを想定します。ここに登録した候補群の中から、将来は実戦用4枠を別途選びます。")]
        [Description("この装備が提供する候補スキル一覧。最大2件を想定。")]
        private List<BattleSkillMasterData> assignableSkills = new();

        [SerializeField]
        [Tooltip("アイテム相当の装備に紐づける Dance 効果です。未設定なら Dance 効果なしとして扱います。")]
        [Description("アイテム相当の装備が提供する Dance 効果。")]
        private BattleSkillMasterData danceSkill;

        [SerializeField]
        [Tooltip("旧属性実装の互換フィールドです。新仕様では戦闘計算やUIに使用しません。")]
        [Description("旧属性実装の互換フィールド。新仕様では未使用。")]
        private AttributeMasterData normalAttackAttribute;

        [SerializeField]
        [Tooltip("旧属性実装の互換フィールドです。新仕様では戦闘計算やUIに使用しません。")]
        [Description("旧属性実装の互換フィールド。新仕様では未使用。")]
        private List<EquipmentAttributeModifierEntry> attributeModifiers = new();

        [SerializeField]
        [Tooltip("装備の補助タグ一覧です。UI分類や絞り込み、将来のロジック分岐補助に使います。")]
        [Description("装備の補助タグ一覧。")]
        private List<string> categoryTags = new();

        [SerializeField]
        [Tooltip("戦闘計算へ流し込む汎用ステータス補正です。Attack などの parameter 加算や、attack.normal.multiplier のような named value 補正を定義できます。")]
        [Description("BattlePlayerStatsBuilder へ適用する汎用ステータス補正一覧。")]
        private List<BattleStatModifierDefinitionEntry> battleStatModifiers = new();

        public int EquipmentId => equipmentId;
        public string DisplayName => displayName;
        public PartsType PartType => partType;
        public PartsExclusiveGroup ExclusiveGroup => ResolveExclusiveGroup(partType);
        public bool IsRightHandEquipment => ExclusiveGroup == PartsExclusiveGroup.HandRight;
        public bool IsLeftHandEquipment => ExclusiveGroup == PartsExclusiveGroup.HandLeft;
        public int PartsIndex => partsIndex;
        public Sprite Icon => icon;
        public int SortOrder => sortOrder;
        public bool IsDefaultOwned => isDefaultOwned;
        public IReadOnlyList<BattleSkillMasterData> AssignableSkills => assignableSkills ??= new List<BattleSkillMasterData>();
        public BattleSkillMasterData DanceSkill => danceSkill;
        public IReadOnlyList<string> CategoryTags => categoryTags ??= new List<string>();
        public IReadOnlyList<BattleStatModifierDefinitionEntry> BattleStatModifiers =>
            battleStatModifiers ??= new List<BattleStatModifierDefinitionEntry>();

        [Description("partType と partsIndex から導ける既定の装備ID。旧データ移行との対応に使える。")]
        public int FallbackEquipmentId => EquipmentIdUtility.Build(PartType, PartsIndex);

        private static PartsExclusiveGroup ResolveExclusiveGroup(PartsType type)
        {
            return type switch
            {
                PartsType.Sword => PartsExclusiveGroup.HandRight,
                PartsType.Axe => PartsExclusiveGroup.HandRight,
                PartsType.Bow => PartsExclusiveGroup.HandRight,
                PartsType.Wand => PartsExclusiveGroup.HandRight,
                PartsType.Staff => PartsExclusiveGroup.HandRight,
                PartsType.Spear => PartsExclusiveGroup.HandRight,
                PartsType.Blunt => PartsExclusiveGroup.HandRight,
                PartsType.Crossbow => PartsExclusiveGroup.HandRight,
                PartsType.Shield => PartsExclusiveGroup.HandLeft,
                PartsType.SubItem => PartsExclusiveGroup.HandLeft,
                _ => PartsExclusiveGroup.None,
            };
        }

        #region Validation
        protected override void NormalizeFields()
        {
            equipmentId = Mathf.Max(0, equipmentId);
            partsIndex = Mathf.Max(-1, partsIndex);
            sortOrder = Mathf.Max(0, sortOrder);
            battleStatModifiers ??= new List<BattleStatModifierDefinitionEntry>();

            for (var i = 0; i < battleStatModifiers.Count; i++)
            {
                battleStatModifiers[i]?.Normalize();
            }
        }

        protected override void CollectValidationIssues(List<ValidationIssue> issues)
        {
            AddMinValueIssue(
                issues,
                equipmentId,
                1,
                "missing_equipment_id",
                "EquipmentId が未設定です。保存データから参照されるため、1以上の一意な値を入れてください。",
                ValidationSeverity.Error);

            AddRequiredStringIssue(
                issues,
                displayName,
                "missing_display_name",
                "DisplayName が空です。Inspector や UI 上で識別しづらくなるため、意味のある名前を付けてください。",
                ValidationSeverity.Warning);

            if (partsIndex < 0)
            {
                AddIssue(
                    issues,
                    "invalid_parts_index",
                    "PartsIndex が未設定です。AppearanceController で見た目を解決できません。",
                    ValidationSeverity.Error);
            }

            AddNullReferenceIssue(
                issues,
                icon,
                "missing_icon",
                "Icon が未設定です。ID から見た目を推測しない運用なので、一覧用のアイコンを設定してください。",
                ValidationSeverity.Warning);

            if (partType == PartsType.Arrow || partType == PartsType.HelmetHair)
            {
                AddIssue(
                    issues,
                    "derived_part_type",
                    $"{partType} は派生部位です。通常は直接 authoring せず、親装備の表示同期に任せる方が安全です。",
                    ValidationSeverity.Info);
            }

            if (partType == PartsType.Skin)
            {
                AddIssue(
                    issues,
                    "skin_part_type",
                    "Skin は通常 sprite index ではなく色で扱います。意図的な運用か確認してください。",
                    ValidationSeverity.Info);
            }

            if (AssignableSkills.Count > MaxAssignableSkillCount)
            {
                AddIssue(
                    issues,
                    "too_many_skills",
                    $"装備に設定できる候補スキルは最大{MaxAssignableSkillCount}件です。{MaxAssignableSkillCount + 1}件以上は登録しないでください。",
                    ValidationSeverity.Error);
            }

            var seenSkills = new HashSet<BattleSkillMasterData>();
            for (var i = 0; i < AssignableSkills.Count; i++)
            {
                var skill = AssignableSkills[i];
                if (skill == null)
                {
                    AddIssue(
                        issues,
                        "null_skill_reference",
                        $"AssignableSkills[{i}] が未設定です。空欄を残さず、不要なら要素自体を削除してください。",
                        ValidationSeverity.Warning);
                    continue;
                }

                if (!seenSkills.Add(skill))
                {
                    AddIssue(
                        issues,
                        "duplicate_skill_reference",
                        $"候補スキルに重複参照があります: {skill.DisplayName}",
                        ValidationSeverity.Warning);
                }
            }

            for (var i = 0; i < BattleStatModifiers.Count; i++)
            {
                var modifier = BattleStatModifiers[i];
                if (modifier == null)
                {
                    AddIssue(
                        issues,
                        "null_battle_stat_modifier",
                        $"BattleStatModifiers[{i}] が未設定です。空欄を残さず、不要なら要素自体を削除してください。",
                        ValidationSeverity.Warning);
                    continue;
                }

                if (modifier.TargetType == BattleStatModifierTargetType.NamedValue &&
                    string.IsNullOrWhiteSpace(modifier.NamedValueKey))
                {
                    AddIssue(
                        issues,
                        "missing_named_value_key",
                        $"BattleStatModifiers[{i}] は NamedValue 対象ですが key が空です。",
                        ValidationSeverity.Warning);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 旧属性実装の互換エントリ。新仕様では戦闘計算や UI に使用しません。
    /// </summary>
    [System.Serializable]
    public class EquipmentAttributeModifierEntry
    {
        [SerializeField]
        private AttributeMasterData attribute;

        [SerializeField]
        [Tooltip("旧属性実装の互換値です。新仕様では戦闘計算やUIに使用しません。")]
        private int damagePercent = 100;

        public AttributeMasterData Attribute => attribute;
        public int DamagePercent => damagePercent;
    }
}
