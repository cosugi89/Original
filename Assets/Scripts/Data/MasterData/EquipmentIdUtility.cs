using System;
using LayerLab.ArtMakerUnity;

namespace Assets.Scripts.Data.MasterData
{
    public static class EquipmentIdUtility
    {
        private const int IdStride = 1000;

        public static int Build(PartsType partType, int partsIndex)
        {
            if (partsIndex < 0)
                return 0;

            return (((int)partType + 1) * IdStride) + partsIndex;
        }

        public static bool TryParse(int equipmentId, out PartsType partType, out int partsIndex)
        {
            partType = default;
            partsIndex = -1;

            if (equipmentId <= 0)
                return false;

            var rawType = (equipmentId / IdStride) - 1;
            var rawIndex = equipmentId % IdStride;
            if (!Enum.IsDefined(typeof(PartsType), rawType) || rawIndex < 0)
                return false;

            partType = (PartsType)rawType;
            partsIndex = rawIndex;
            return true;
        }

        public static bool TryConvertLegacyStringToId(string legacyEquipmentId, out int equipmentId)
        {
            equipmentId = 0;
            if (string.IsNullOrWhiteSpace(legacyEquipmentId))
                return false;

            if (int.TryParse(legacyEquipmentId, out var parsedId) &&
                parsedId > 0)
            {
                equipmentId = parsedId;
                return true;
            }

            if (!TryParseLegacyString(legacyEquipmentId, out var partType, out var partsIndex))
                return false;

            equipmentId = Build(partType, partsIndex);
            return equipmentId > 0;
        }

        public static bool TryParseLegacyString(string legacyEquipmentId, out PartsType partType, out int partsIndex)
        {
            partType = default;
            partsIndex = -1;

            if (string.IsNullOrWhiteSpace(legacyEquipmentId))
                return false;

            var separatorIndex = legacyEquipmentId.LastIndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= legacyEquipmentId.Length - 1)
                return false;

            var partTypeText = legacyEquipmentId.Substring(0, separatorIndex);
            var indexText = legacyEquipmentId.Substring(separatorIndex + 1);

            return Enum.TryParse(partTypeText, true, out partType) &&
                   int.TryParse(indexText, out partsIndex);
        }
    }
}
