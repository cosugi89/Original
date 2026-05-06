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
            var stageDatabase = LoadStageMasterDatabase();
            if (stageDatabase == null)
                return false;

            if (!stageDatabase.TryGetById(stageId, out var master))
                return false;

            stageData = new StageData
            {
                StageId = master.StageId,
                StageName = master.StageName,
                Description = master.Description,
                BackgroundImage = master.BackgroundImage,
                PreviewImage = master.PreviewImage,
                Enemy = BuildBattleEnemyData(master),
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

        private static StageBattleBoardData BuildBattleBoardData(StageBattlePatternMasterData patternMaster)
        {
            var board = patternMaster?.Board;
            var width = board != null && board.Width > 0 ? board.Width : 5;
            var height = board != null && board.Height > 0 ? board.Height : 6;
            var startX = board != null ? Mathf.Clamp(board.StartX, 0, width - 1) : width / 2;
            var startY = board != null ? Mathf.Clamp(board.StartY, 0, height - 1) : height - 1;

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

        private static StageBattleEnemyData BuildBattleEnemyData(StageMasterData master)
        {
            var legacyEnemy = master?.LegacyEnemies != null && master.LegacyEnemies.Count > 0
                ? master.LegacyEnemies.FirstOrDefault(enemy => enemy != null)
                : null;
            var fallbackName = legacyEnemy?.Name ?? string.Empty;
            var fallbackHp = legacyEnemy != null ? Mathf.Max(1, legacyEnemy.Hp) : 1;
            var enemyRefs = master?.EnemyRefs;

            if (enemyRefs != null)
            {
                for (var i = 0; i < enemyRefs.Count; i++)
                {
                    var enemyMaster = enemyRefs[i];
                    if (enemyMaster == null)
                    {
                        continue;
                    }

                    return new StageBattleEnemyData
                    {
                        Name = !string.IsNullOrWhiteSpace(enemyMaster.Name) ? enemyMaster.Name : fallbackName,
                        MaxHp = enemyMaster.MaxHp > 0 ? enemyMaster.MaxHp : fallbackHp,
                        Damage = Mathf.Max(0, enemyMaster.Damage),
                        Appearance = CloneAppearanceData(enemyMaster.Appearance),
                        Patterns = BuildPatterns(enemyMaster),
                    };
                }
            }

            return new StageBattleEnemyData
            {
                Name = fallbackName,
                MaxHp = fallbackHp,
                Damage = 80,
                Appearance = new AppearanceData(),
                Patterns = Array.Empty<StageBattlePatternData>(),
            };
        }

        private static IReadOnlyList<StageBattlePatternData> BuildPatterns(StageBattleEnemyMasterData enemyMaster)
        {
            var patternRefs = enemyMaster?.Patterns;
            if (patternRefs == null || patternRefs.Count == 0)
            {
                return Array.Empty<StageBattlePatternData>();
            }

            var result = new List<StageBattlePatternData>(patternRefs.Count);
            for (var i = 0; i < patternRefs.Count; i++)
            {
                var pattern = patternRefs[i];
                if (pattern == null)
                {
                    continue;
                }

                result.Add(new StageBattlePatternData
                {
                    Board = BuildBattleBoardData(pattern),
                    DebugLabel = pattern.Label,
                    EnemyAction = pattern.EnemyAction,
                    Description = pattern.Description,
                    ConfirmText = pattern.ConfirmText,
                    EnemyActionDamageMultiplier = Mathf.Max(0f, pattern.DamageMultiplier),
                    CellPlacements = BuildCellPlacements(pattern.CellPlacements),
                });
            }

            return result;
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
                })
                .ToArray();
        }

        private static BattleNodeType NormalizeNodeType(BattleNodeType nodeType)
        {
            return Enum.IsDefined(typeof(BattleNodeType), nodeType)
                ? nodeType
                : BattleNodeType.Empty;
        }

        private static AppearanceData CloneAppearanceData(AppearanceData source)
        {
            if (source == null)
            {
                return new AppearanceData();
            }

            var clone = new AppearanceData();

            if (source.parts != null)
            {
                for (var i = 0; i < source.parts.Count; i++)
                {
                    var entry = source.parts[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    clone.parts.Add(new AppearanceData.PartsEntry
                    {
                        type = entry.type,
                        index = entry.index,
                    });
                }
            }

            if (source.colors != null)
            {
                for (var i = 0; i < source.colors.Count; i++)
                {
                    var entry = source.colors[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    clone.colors.Add(new AppearanceData.ColorEntry
                    {
                        target = entry.target,
                        color = entry.color,
                    });
                }
            }

            if (source.visibility != null)
            {
                for (var i = 0; i < source.visibility.Count; i++)
                {
                    var entry = source.visibility[i];
                    if (entry == null)
                    {
                        continue;
                    }

                    clone.visibility.Add(new AppearanceData.VisibilityEntry
                    {
                        type = entry.type,
                        visible = entry.visible,
                    });
                }
            }

            return clone;
        }
    }
}
