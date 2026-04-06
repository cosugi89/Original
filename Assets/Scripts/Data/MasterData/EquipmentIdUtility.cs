using System;
using LayerLab.ArtMakerUnity;

namespace Assets.Scripts.Data.MasterData
{
    public static class EquipmentIdUtility
    {
        public static string Build(PartsType partType, int partsIndex)
        {
            return $"{partType}:{partsIndex}";
        }

        public static bool TryParse(string equipmentId, out PartsType partType, out int partsIndex)
        {
            partType = default;
            partsIndex = -1;

            if (string.IsNullOrWhiteSpace(equipmentId))
                return false;

            var separatorIndex = equipmentId.LastIndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= equipmentId.Length - 1)
                return false;

            var partTypeText = equipmentId.Substring(0, separatorIndex);
            var indexText = equipmentId.Substring(separatorIndex + 1);

            return Enum.TryParse(partTypeText, true, out partType) &&
                   int.TryParse(indexText, out partsIndex);
        }
    }
}
