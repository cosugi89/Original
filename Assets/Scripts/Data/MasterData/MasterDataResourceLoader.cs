using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public static class MasterDataResourceLoader
    {
        public const string EquipmentCatalogPath = "MasterData/EquipmentCatalog";
        public const string BattleStageCatalogPath = "MasterData/BattleStageCatalog";
        public const string StageDatabasePath = "MasterData/StageDatabase";

        private static bool _loggedMissingEquipmentCatalog;
        private static bool _loggedMissingBattleStageCatalog;
        private static bool _loggedMissingStageDatabase;

        public static EquipmentCatalog LoadEquipmentCatalog()
        {
            var catalog = Resources.Load<EquipmentCatalog>(EquipmentCatalogPath);
            if (catalog == null && !_loggedMissingEquipmentCatalog)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] EquipmentCatalog was not found at Resources/{EquipmentCatalogPath}.");
                _loggedMissingEquipmentCatalog = true;
            }

            return catalog;
        }

        public static BattleStageCatalog LoadBattleStageCatalog()
        {
            var catalog = Resources.Load<BattleStageCatalog>(BattleStageCatalogPath);
            if (catalog == null && !_loggedMissingBattleStageCatalog)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] BattleStageCatalog was not found at Resources/{BattleStageCatalogPath}.");
                _loggedMissingBattleStageCatalog = true;
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
