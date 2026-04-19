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
        public string EquipmentId { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public PartsType PartType { get; init; }
        public int PartsIndex { get; init; } = -1;
        public Sprite Icon { get; init; }
        public bool IsDefaultOwned { get; init; }
    }
}
