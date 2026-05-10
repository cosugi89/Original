using System.Collections.Generic;
using System.ComponentModel;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Equipment Master Data", fileName = "EquipmentMasterData")]
    public class EquipmentMasterData : ScriptableObject
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
        [Tooltip("保存データから参照される装備IDです。0は未装備扱いなので、手動作成時は1以上の一意な値を入れてください。")]
        [Description("装備/見た目定義を一意に識別するID。保存データから参照されるキー。")]
        private int equipmentId = 0;

        [SerializeField]
        [Tooltip("UI上で見せる名前です。自動生成後に意味のある名称へ直しておくと管理しやすくなります。")]
        [Description("画面表示用の装備名・見た目名。")]
        private string displayName = "";

        [SerializeField]
        [Tooltip("この装備が属する見た目枠です。設定した PartType と PartsIndex の組み合わせが PartsManager 解決に使われます。")]
        [Description("どの見た目枠に属する定義か。Hair、Sword、Shield など。")]
        private PartsType partType;

        [SerializeField]
        [Tooltip("PartsManager 上の見た目 index です。実際の sprite 切り替えに使われるため、Character.prefab の定義と揃えてください。")]
        [Description("現在の PartsManager が見た目を切り替えるために使う index。partsIndex 依存はここに閉じ込める。")]
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
        [Tooltip("この装備に紐づく候補スキル一覧です。最大3件までを想定します。ここに登録した候補群の中から、将来は実戦用4枠を別途選びます。")]
        [Description("この装備が提供する候補スキル一覧。最大3件を想定。")]
        private List<BattleSkillMasterData> assignableSkills = new();

        [SerializeField]
        [Tooltip("右手武器が持つ通常攻撃用の属性です。右手武器以外では未設定でも構いません。")]
        [Description("通常攻撃に適用する属性。主に右手武器で使う。")]
        private AttributeMasterData normalAttackAttribute;

        [SerializeField]
        [Tooltip("装備が持つ属性補正一覧です。100 が等倍、50 が半減、150 が弱点などを表します。")]
        [Description("属性ごとの被ダメージ補正一覧。")]
        private List<EquipmentAttributeModifierEntry> attributeModifiers = new();

        [SerializeField]
        [Tooltip("装備の補助タグ一覧です。UI分類や絞り込み、将来のロジック分岐補助に使います。")]
        [Description("装備の補助タグ一覧。")]
        private List<string> categoryTags = new();

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

        public AttributeMasterData NormalAttackAttribute => normalAttackAttribute;

        public IReadOnlyList<EquipmentAttributeModifierEntry> AttributeModifiers =>
            attributeModifiers ??= new List<EquipmentAttributeModifierEntry>();

        public IReadOnlyList<string> CategoryTags => categoryTags ??= new List<string>();

        [Description("partType と partsIndex から導ける既定の装備ID。旧データ移行との対応に使える。")]
        public int FallbackEquipmentId => EquipmentIdUtility.Build(PartType, PartsIndex);

        public IReadOnlyList<ValidationIssue> GetValidationIssues()
        {
            var issues = new List<ValidationIssue>();

            if (equipmentId <= 0)
            {
                issues.Add(new ValidationIssue(
                    "missing_equipment_id",
                    "EquipmentId が未設定です。保存データから参照されるため、1以上の一意な値を入れてください。",
                    ValidationSeverity.Error));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                issues.Add(new ValidationIssue(
                    "missing_display_name",
                    "DisplayName が空です。Inspector や UI 上で識別しづらくなるため、意味のある名前を付けてください。",
                    ValidationSeverity.Warning));
            }

            if (partsIndex < 0)
            {
                issues.Add(new ValidationIssue(
                    "invalid_parts_index",
                    "PartsIndex が未設定です。PartsManager で見た目を解決できません。",
                    ValidationSeverity.Error));
            }

            if (icon == null)
            {
                issues.Add(new ValidationIssue(
                    "missing_icon",
                    "Icon が未設定です。ID から見た目を推測しない運用なので、一覧用のアイコンを設定してください。",
                    ValidationSeverity.Warning));
            }

            if (partType == PartsType.Arrow || partType == PartsType.HelmetHair)
            {
                issues.Add(new ValidationIssue(
                    "derived_part_type",
                    $"{partType} は派生部位です。通常は直接 authoring せず、親装備の表示同期に任せる方が安全です。",
                    ValidationSeverity.Info));
            }

            if (partType == PartsType.Skin)
            {
                issues.Add(new ValidationIssue(
                    "skin_part_type",
                    "Skin は通常 sprite index ではなく色で扱います。意図的な運用か確認してください。",
                    ValidationSeverity.Info));
            }

            if (AssignableSkills.Count > 3)
            {
                issues.Add(new ValidationIssue(
                    "too_many_skills",
                    "装備に設定できる候補スキルは最大3件想定です。4件以上は登録しないでください。",
                    ValidationSeverity.Error));
            }

            var seenSkills = new HashSet<BattleSkillMasterData>();
            for (var i = 0; i < AssignableSkills.Count; i++)
            {
                var skill = AssignableSkills[i];
                if (skill == null)
                {
                    issues.Add(new ValidationIssue(
                        "null_skill_reference",
                        $"AssignableSkills[{i}] が未設定です。空欄を残さず、不要なら要素自体を削除してください。",
                        ValidationSeverity.Warning));
                    continue;
                }

                if (!seenSkills.Add(skill))
                {
                    issues.Add(new ValidationIssue(
                        "duplicate_skill_reference",
                        $"候補スキルに重複参照があります: {skill.DisplayName}",
                        ValidationSeverity.Warning));
                }
            }

            return issues;
        }

        private void OnValidate()
        {
            equipmentId = Mathf.Max(0, equipmentId);
            partsIndex = Mathf.Max(-1, partsIndex);
            sortOrder = Mathf.Max(0, sortOrder);
        }

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
    }

    [System.Serializable]
    public class EquipmentAttributeModifierEntry
    {
        [SerializeField]
        private AttributeMasterData attribute;

        [SerializeField]
        [Tooltip("100 が等倍、50 が半減、150 が弱点です。")]
        private int damagePercent = 100;

        public AttributeMasterData Attribute => attribute;
        public int DamagePercent => damagePercent;
    }
}
