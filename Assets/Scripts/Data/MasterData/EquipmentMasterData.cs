using System.ComponentModel;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Equipment Master Data", fileName = "EquipmentMasterData")]
    public class EquipmentMasterData : ScriptableObject
    {
        [field: SerializeField]
        [Description("装備を一意に識別するID。保存データから参照されるキー。")]
        public string EquipmentId { get; private set; } = "";

        [field: SerializeField]
        [Description("画面表示用の装備名。")]
        public string DisplayName { get; private set; } = "";

        [field: SerializeField]
        [Description("どの装備枠に属する装備か。")]
        public PartsType PartType { get; private set; }

        [field: SerializeField]
        [Description("現在の PartsManager が見た目を切り替えるために使う index。partsIndex 依存はここに閉じ込める。")]
        public int PartsIndex { get; private set; } = -1;

        [field: SerializeField]
        [Description("一覧表示やボタン表示に使うアイコン。")]
        public Sprite Icon { get; private set; }

        [field: SerializeField]
        [Description("新規セーブ時に最初から所持している装備かどうか。")]
        public bool IsDefaultOwned { get; private set; }

        [Description("partType と partsIndex から導ける既定の装備ID。旧データ移行との対応に使える。")]
        public string FallbackEquipmentId => EquipmentIdUtility.Build(PartType, PartsIndex);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(EquipmentId) && PartsIndex >= 0)
            {
                EquipmentId = FallbackEquipmentId;
            }
        }
    }
}
