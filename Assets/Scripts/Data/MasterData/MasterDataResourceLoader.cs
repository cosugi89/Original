using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public static class MasterDataResourceLoader
    {
        public const string EquipmentCatalogPath = "MasterData/EquipmentCatalog";
        public const string StageDatabasePath = "MasterData/StageDatabase";

        private static bool _loggedMissingEquipmentCatalog;
        private static bool _loggedMissingStageDatabase;

        public static EquipmentDatabase LoadEquipmentCatalog()
        {
            var catalog = Resources.Load<EquipmentDatabase>(EquipmentCatalogPath);
            if (catalog == null && !_loggedMissingEquipmentCatalog)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] EquipmentCatalog was not found at Resources/{EquipmentCatalogPath}.");
                _loggedMissingEquipmentCatalog = true;
            }

            return catalog;
        }

        public static StageDatabase LoadStageDatabase()
        {
            var database = Resources.Load<StageDatabase>(StageDatabasePath);
            if (database == null && !_loggedMissingStageDatabase)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] StageDatabase was not found at Resources/{StageDatabasePath}.");
                _loggedMissingStageDatabase = true;
            }

            return database;
        }
    }
}
