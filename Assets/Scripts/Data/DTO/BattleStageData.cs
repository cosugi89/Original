using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using UnityEngine;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// バトルステージ1件の転送用データ。ロジック層が参照するステージ選択情報。
    /// マスターデータ（StageMasterData）から生成され、読み取り専用で使用する。
    /// </summary>
    public class BattleStageData
    {
        public int StageId { get; init; }
        public string DisplayName { get; init; } = "";
        public string DescriptionText { get; init; } = "";
        public int SortOrder { get; init; }
        public StageKind StageKind { get; init; }
        public StageUnlockRequirementMode UnlockRequirementMode { get; init; }
        public IReadOnlyList<int> RequiredClearedStageIds { get; init; } = Array.Empty<int>();
        public IReadOnlyList<MissionRewardData> ClearRewards { get; init; } = Array.Empty<MissionRewardData>();
        public Sprite PreviewImage { get; init; }
    }

    public class MissionRewardData
    {
        public MissionRewardType RewardType { get; init; }
        public int ItemId { get; init; }
        public int EquipmentId { get; init; }
        public int Quantity { get; init; }
    }
}
