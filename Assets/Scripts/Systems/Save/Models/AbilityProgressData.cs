using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    /// <summary>
    /// Ability の永続状態。解放可否や能力値はマスターから都度導出する。
    /// </summary>
    public sealed class AbilityProgressData
    {
        [Description("Tree配置NodeごとのLevel。TreeIdとTreeNodeIdの組で一意に扱う。Level 0は保存しない。")]
        public List<AbilityTreeNodeLevelData> NodeLevels { get; set; } = new();
    }

    public sealed class AbilityTreeNodeLevelData
    {
        [Description("AbilityTreeMasterDataのTreeId。")]
        public string TreeId { get; set; } = "";

        [Description("Tree内の不変な配置Node ID。")]
        public string TreeNodeId { get; set; } = "";

        [Description("購入済みRank数。0は保存しない。")]
        public int Level { get; set; }
    }
}
