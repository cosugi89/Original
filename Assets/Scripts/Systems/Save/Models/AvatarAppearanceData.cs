using System.Collections.Generic;
using System.ComponentModel;
using Assets.Scripts.Systems.Save;
using LayerLab.ArtMakerUnity;
using Newtonsoft.Json;

namespace Assets.Scripts.Systems.Save.Models
{
    public class AvatarAppearanceData
    {
        [Description("装備枠ごとの見た目状態。何を装備しているかと表示状態を持つ。")]
        public List<AvatarPartStateData> Parts { get; set; } = new();

        [Description("色変更対象ごとの色設定。")]
        public List<AvatarColorData> Colors { get; set; } = new();
    }

    public class AvatarPartStateData
    {
        [Description("どの装備枠の状態かを表す。Hair、Sword、Shield など。")]
        public PartsType PartType { get; set; }

        [JsonConverter(typeof(EquipmentIdJsonConverter))]
        [Description("装備中の装備ID。未装備は0で表す。")]
        public int EquipmentId { get; set; } = 0;

        [Description("その装備枠を見た目上表示するかどうか。")]
        public bool IsVisible { get; set; } = true;
    }

    public class AvatarColorData
    {
        [Description("どの色変更対象の設定かを表す。Skin、Hair、Eye など。")]
        public ColorTargetType Target { get; set; }

        [Description("色をHTMLカラー文字列で持つ。例: #FFFFFFFF。UnityEngine.Colorに依存しないための保持形式。")]
        public string HtmlColor { get; set; } = "#FFFFFFFF";
    }
}
