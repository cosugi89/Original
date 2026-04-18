using System;
using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    public class AvatarPartOptionViewData
    {
        public string EquipmentId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UICategory Category { get; set; }
        public PartsType PartType { get; set; }
        public Sprite Icon { get; set; }
        public bool IsOwned { get; set; }
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
        public bool CanSelect { get; set; }
        public int SortOrder { get; set; }
    }

    public class AvatarPartSectionViewData
    {
        public UICategory Category { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public bool HasSelection { get; set; }
        public PartsType ActivePartType { get; set; }
        public string SelectedEquipmentId { get; set; } = string.Empty;
        public string SelectedDisplayName { get; set; } = string.Empty;
        public Sprite SelectedIcon { get; set; }
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
