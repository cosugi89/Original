using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
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
        private static bool _loggedBattleStageFallbackToStageMasterDatabase;

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
            if (catalog != null)
            {
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

            return LoadBattleStageDataFromStageMasterDatabase();
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
                Enemies = BuildEnemyData(master),
                Battle = BuildBattleData(master),
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

        private static IReadOnlyList<BattleStageData> LoadBattleStageDataFromStageMasterDatabase()
        {
            var database = LoadStageMasterDatabase();
            if (database == null)
                return Array.Empty<BattleStageData>();

            if (!_loggedBattleStageFallbackToStageMasterDatabase)
            {
                Debug.LogWarning("[MasterDataResourceLoader] BattleStageMasterCatalog was not found. Falling back to StageMasterDatabase for stage definitions.");
                _loggedBattleStageFallbackToStageMasterDatabase = true;
            }

            return database.Stages
                .Where(m => m != null)
                .OrderBy(m => m.StageId)
                .ThenBy(m => m.StageId)
                .Select((m, index) => new BattleStageData
                {
                    StageId = m.StageId,
                    DisplayName = m.StageName,
                    DescriptionText = m.Description,
                    SortOrder = m.StageId,
                    IsInitiallyUnlocked = index == 0,
                    PreviewImage = m.PreviewImage,
                })
                .ToArray();
        }

        private static IReadOnlyList<StageData.EnemyData> BuildEnemyData(StageMasterData master)
        {
            var enemyData = (master.Enemies ?? new List<StageMasterData.EnemyMasterData>())
                .Where(e => e != null)
                .Select(e => new StageData.EnemyData
                {
                    Name = e.Name,
                    Hp = Mathf.Max(1, e.Hp),
                })
                .ToArray();

            if (enemyData.Length > 0)
            {
                return enemyData;
            }

            var battleEnemy = master.Battle?.Enemy;
            if (battleEnemy == null || string.IsNullOrWhiteSpace(battleEnemy.Name))
            {
                return Array.Empty<StageData.EnemyData>();
            }

            return new[]
            {
                new StageData.EnemyData
                {
                    Name = battleEnemy.Name,
                    Hp = Mathf.Max(1, battleEnemy.MaxHp),
                },
            };
        }

        private static StageBattleData BuildBattleData(StageMasterData master)
        {
            var board = BuildBattleBoardData(master.Battle?.Board);
            var legacyEnemies = BuildEnemyData(master);
            var battleEnemy = BuildBattleEnemyData(master.Battle?.Enemy, legacyEnemies);

            return new StageBattleData
            {
                Board = board,
                Enemy = battleEnemy,
                TurnDefinitions = BuildTurnDefinitions(master.Battle),
            };
        }

        private static StageBattleBoardData BuildBattleBoardData(StageBattleBoardMasterData boardMaster)
        {
            var width = boardMaster != null && boardMaster.Width > 0 ? boardMaster.Width : 5;
            var height = boardMaster != null && boardMaster.Height > 0 ? boardMaster.Height : 6;
            var startX = boardMaster != null ? Mathf.Clamp(boardMaster.StartX, 0, width - 1) : width / 2;
            var startY = boardMaster != null ? Mathf.Clamp(boardMaster.StartY, 0, height - 1) : height - 1;

            return new StageBattleBoardData
            {
                Width = width,
                Height = height,
                StartPosition = new StageGridPositionData
                {
                    X = startX,
                    Y = startY,
                },
            };
        }

        private static StageBattleEnemyData BuildBattleEnemyData(
            StageBattleEnemyMasterData battleEnemyMaster,
            IReadOnlyList<StageData.EnemyData> legacyEnemies)
        {
            var fallbackEnemy = legacyEnemies != null && legacyEnemies.Count > 0 ? legacyEnemies[0] : null;
            var fallbackName = fallbackEnemy?.Name ?? string.Empty;
            var fallbackHp = fallbackEnemy != null ? Mathf.Max(1, fallbackEnemy.Hp) : 1;

            if (battleEnemyMaster == null)
            {
                return new StageBattleEnemyData
                {
                    Name = fallbackName,
                    MaxHp = fallbackHp,
                };
            }

            return new StageBattleEnemyData
            {
                Name = !string.IsNullOrWhiteSpace(battleEnemyMaster.Name) ? battleEnemyMaster.Name : fallbackName,
                MaxHp = battleEnemyMaster.MaxHp > 0 ? battleEnemyMaster.MaxHp : fallbackHp,
            };
        }

        private static IReadOnlyList<StageTurnData> BuildTurnDefinitions(StageBattleMasterData battleMaster)
        {
            if (battleMaster == null || battleMaster.TurnDefinitions == null)
            {
                return Array.Empty<StageTurnData>();
            }

            return battleMaster.TurnDefinitions
                .Where(turn => turn != null)
                .Select(turn => new StageTurnData
                {
                    DebugLabel = turn.Label,
                    EnemyAction = turn.EnemyAction,
                    BoardSummary = turn.BoardSummary,
                    ConfirmText = turn.ConfirmText,
                    Notes = turn.Notes,
                    EnemyActionDamage = Mathf.Max(0, turn.EnemyActionDamage),
                    HazardGroups = BuildHazardGroups(turn.HazardGroups),
                    CellPlacements = BuildCellPlacements(turn.CellPlacements),
                })
                .ToArray();
        }

        private static IReadOnlyList<StageTurnHazardGroupData> BuildHazardGroups(
            IReadOnlyList<StageTurnHazardGroupMasterData> hazardGroupMasters)
        {
            if (hazardGroupMasters == null)
            {
                return Array.Empty<StageTurnHazardGroupData>();
            }

            return hazardGroupMasters
                .Where(group => group != null)
                .Select(group => new StageTurnHazardGroupData
                {
                    GroupId = group.GroupId,
                    Damage = Mathf.Max(0, group.Damage),
                })
                .ToArray();
        }

        private static IReadOnlyList<StageTurnCellData> BuildCellPlacements(
            IReadOnlyList<StageTurnCellMasterData> cellMasters)
        {
            if (cellMasters == null)
            {
                return Array.Empty<StageTurnCellData>();
            }

            return cellMasters
                .Where(cell => cell != null)
                .Select(cell => new StageTurnCellData
                {
                    Position = new StageGridPositionData
                    {
                        X = cell.X,
                        Y = cell.Y,
                    },
                    NodeType = NormalizeNodeType(cell.NodeType),
                    HazardGroupId = cell.HazardGroupId,
                })
                .ToArray();
        }

        private static BattleNodeType NormalizeNodeType(BattleNodeType nodeType)
        {
            return Enum.IsDefined(typeof(BattleNodeType), nodeType)
                ? nodeType
                : BattleNodeType.Empty;
        }
    }
}
