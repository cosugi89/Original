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

    }
}
