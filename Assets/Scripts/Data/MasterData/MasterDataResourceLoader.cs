using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public static class MasterDataResourceLoader
    {
        private const string EquipmentMasterCatalogPath = "MasterData/EquipmentCatalog";
        private const string BattleStageMasterCatalogPath = "MasterData/BattleStageCatalog";
        private const string StageMasterDatabasePath = "MasterData/StageDatabase";

        private static bool _loggedMissingEquipmentMasterCatalog;
        private static bool _loggedMissingBattleStageMasterCatalog;
        private static bool _loggedMissingStageMasterDatabase;

        // ── public ──────────────────────────

        /// <summary>
        /// 装備マスタを読み込み、DTO リストに変換して返す。
        /// </summary>
        public static IReadOnlyList<EquipmentData> LoadEquipmentData()
        {
            var catalog = LoadEquipmentMasterCatalog();
            if (catalog == null)
                return Array.Empty<EquipmentData>();

            return catalog.Equipments
                .Where(m => m != null)
                .Select(m => new EquipmentData
                {
                    EquipmentId = m.EquipmentId,
                    DisplayName = m.DisplayName,
                    PartType = m.PartType,
                    PartsIndex = m.PartsIndex,
                    Icon = m.Icon,
                    IsDefaultOwned = m.IsDefaultOwned,
                })
                .ToArray();
        }

        /// <summary>
        /// バトルステージマスタを読み込み、DTO リストに変換して返す。
        /// </summary>
        public static IReadOnlyList<BattleStageData> LoadBattleStageData()
        {
            var catalog = LoadBattleStageMasterCatalog();
            if (catalog == null)
                return Array.Empty<BattleStageData>();

            return catalog.Stages
                .Where(m => m != null)
                .Select(m => new BattleStageData
                {
                    StageId = m.StageId,
                    DisplayName = m.DisplayName,
                    DescriptionText = m.DescriptionText,
                    SortOrder = m.SortOrder,
                    IsInitiallyUnlocked = m.IsInitiallyUnlocked,
                    PreviewImage = m.PreviewImage,
                })
                .ToArray();
        }

        /// <summary>
        /// stageId に対応するステージデータを DTO に変換して返す
        /// </summary>
        public static bool TryLoadStageData(int stageId, out StageData stageData)
        {
            stageData = null;
            var database = LoadStageMasterDatabase();
            if (database == null)
                return false;

            if (!database.TryGetById(stageId, out var master))
                return false;

            stageData = new StageData
            {
                StageId = master.StageId,
                StageName = master.StageName,
                Description = master.Description,
                BackgroundImage = master.BackgroundImage,
                PreviewImage = master.PreviewImage,
                Enemies = (master.Enemies ?? new List<StageMasterData.EnemyMasterData>())
                    .Where(e => e != null)
                    .Select(e => new StageData.EnemyData { Name = e.Name, Hp = e.Hp })
                    .ToArray(),
            };
            return true;
        }

        // ── private ─────────────────

        private static EquipmentMasterCatalog LoadEquipmentMasterCatalog()
        {
            var catalog = Resources.Load<EquipmentMasterCatalog>(EquipmentMasterCatalogPath);
            if (catalog == null && !_loggedMissingEquipmentMasterCatalog)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] EquipmentMasterCatalog was not found at Resources/{EquipmentMasterCatalogPath}.");
                _loggedMissingEquipmentMasterCatalog = true;
            }
            return catalog;
        }

        private static BattleStageMasterCatalog LoadBattleStageMasterCatalog()
        {
            var catalog = Resources.Load<BattleStageMasterCatalog>(BattleStageMasterCatalogPath);
            if (catalog == null && !_loggedMissingBattleStageMasterCatalog)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] BattleStageMasterCatalog was not found at Resources/{BattleStageMasterCatalogPath}.");
                _loggedMissingBattleStageMasterCatalog = true;
            }
            return catalog;
        }

        private static StageMasterDatabase LoadStageMasterDatabase()
        {
            var database = Resources.Load<StageMasterDatabase>(StageMasterDatabasePath);
            if (database == null && !_loggedMissingStageMasterDatabase)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] StageMasterDatabase was not found at Resources/{StageMasterDatabasePath}.");
                _loggedMissingStageMasterDatabase = true;
            }
            return database;
        }
    }
}
