using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Core.Appearance;
using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    public class AvatarPartOptionViewData
    {
        public int EquipmentId { get; set; } = 0;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public UICategory Category { get; set; }
        public PartsType PartType { get; set; }
        public Sprite Icon { get; set; }
        public bool IsOwned { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public bool CanSelect { get; set; }
        public int SortOrder { get; set; }
        public EquipmentRarity Rarity { get; set; } = EquipmentRarity.Common;
        public IReadOnlyList<string> CategoryTags { get; set; } = Array.Empty<string>();
        public IReadOnlyList<BattleSkillData> AssignableSkills { get; set; } = Array.Empty<BattleSkillData>();
        public IReadOnlyList<BattleStatModifierData> BattleStatModifiers { get; set; } = Array.Empty<BattleStatModifierData>();
        public IReadOnlyList<string> AssignableSkillNames { get; set; } = Array.Empty<string>();
        public string DanceSkillName { get; set; } = string.Empty;
    }

    public class AvatarPartSectionViewData
    {
        public UICategory Category { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public bool HasSelection { get; set; }
        public PartsType ActivePartType { get; set; }
        public int SelectedEquipmentId { get; set; } = 0;
        public string SelectedDisplayName { get; set; } = string.Empty;
        public Sprite SelectedIcon { get; set; }
        public string SelectedTagsText { get; set; } = string.Empty;
        public string SelectedSkillsText { get; set; } = string.Empty;
        public string SelectedDanceSkillText { get; set; } = string.Empty;
        public string TagBonusText { get; set; } = string.Empty;
        public bool SupportsColor { get; set; }
        public ColorTargetType ColorTarget { get; set; }
        public string CurrentColorHtml { get; set; } = "#FFFFFFFF";
        public IReadOnlyList<AvatarPartOptionViewData> Options { get; set; } = Array.Empty<AvatarPartOptionViewData>();
    }

    public class AvatarColorOptionViewData
    {
        public ColorTargetType Target { get; set; }
        public string HtmlColor { get; set; } = "#FFFFFFFF";
        public Color SwatchColor { get; set; } = Color.white;
        public bool IsSelected { get; set; }
    }
}
