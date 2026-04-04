using LayerLab.ArtMakerUnity;

namespace Assets.Scripts.Data.MasterData
{
    public static class EquipmentIdUtility
    {
        public static string Build(PartsType partType, int partsIndex)
        {
            return $"{partType}:{partsIndex}";
        }
    }
}
