using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using LayerLab.ArtMakerUnity;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public static class MasterDataResourceLoader
    {
        private const string EquipmentMasterDatabasePath = "MasterData/EquipmentDatabase";
        private const string BattleSkillMasterDatabasePath = "MasterData/BattleSkillDatabase";
        private const string AttributeMasterDatabasePath = "MasterData/AttributeDatabase";
        private const string StageMasterDatabasePath = "MasterData/StageDatabase";

        private static bool _loggedMissingEquipmentMasterDatabase;
        private static bool _loggedMissingBattleSkillMasterDatabase;
        private static bool _loggedMissingAttributeMasterDatabase;
        private static bool _loggedMissingStageMasterDatabase;

        // ── public ──────────────────────────

        /// <summary>
        /// 装備マスタを読み込み、DTO リストに変換して返す。
        /// </summary>
        public static IReadOnlyList<EquipmentData> LoadEquipmentData()
        {
            var database = LoadEquipmentMasterDatabase();
            if (database == null)
                return Array.Empty<EquipmentData>();

            return database.Equipments
                .Where(m => m != null)
                .Select(m => new EquipmentData
                {
                    EquipmentId = m.EquipmentId,
                    DisplayName = m.DisplayName,
                    PartType = m.PartType,
                    ExclusiveGroup = m.ExclusiveGroup,
                    PartsIndex = m.PartsIndex,
                    Icon = m.Icon,
                    SortOrder = m.SortOrder > 0 ? m.SortOrder : Mathf.Max(m.PartsIndex, 0),
                    IsDefaultOwned = m.IsDefaultOwned,
                    AssignableSkills = BuildBattleSkillData(m.AssignableSkills),
                    NormalAttackAttribute = BuildAttributeData(m.NormalAttackAttribute),
                    AttributeModifiers = BuildEquipmentAttributeModifierData(m.AttributeModifiers),
                    CategoryTags = BuildCategoryTags(m.CategoryTags),
                })
                .ToArray();
        }

        /// <summary>
        /// バトルスキルマスタを読み込み、DTO リストに変換して返す。
        /// </summary>
        public static IReadOnlyList<BattleSkillData> LoadBattleSkillData()
        {
            var database = LoadBattleSkillMasterDatabase();
            if (database == null)
                return Array.Empty<BattleSkillData>();

            return BuildBattleSkillData(database.Skills);
        }

        public static bool TryLoadBattleSkillData(int skillId, out BattleSkillData skillData)
        {
            skillData = null;
            if (skillId <= 0)
            {
                return false;
            }

            var database = LoadBattleSkillMasterDatabase();
            if (database == null || !database.TryGetById(skillId, out var master) || master == null)
            {
                return false;
            }

            var skills = BuildBattleSkillData(new[] { master });
            if (skills.Count == 0)
            {
                return false;
            }

            skillData = skills[0];
            return skillData != null;
        }

        /// <summary>
        /// 属性マスタを読み込み、DTO リストに変換して返す。
        /// </summary>
        public static IReadOnlyList<AttributeData> LoadAttributeData()
        {
            var database = LoadAttributeMasterDatabase();
            if (database == null)
            {
                return Array.Empty<AttributeData>();
            }

            return database.Attributes
                .Where(attribute => attribute != null)
                .OrderBy(attribute => attribute.SortOrder)
                .ThenBy(attribute => attribute.AttributeId)
                .Select(BuildAttributeData)
                .ToArray();
        }

        /// <summary>
        /// バトルステージマスタを読み込み、DTO リストに変換して返す。
        /// </summary>
        public static IReadOnlyList<BattleStageData> LoadBattleStageData()
        {
            var database = LoadStageMasterDatabase();
            if (database == null)
                return Array.Empty<BattleStageData>();

            return database.Stages
                .Where(m => m != null)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.StageId)
                .Select(m => new BattleStageData
                {
                    StageId = m.StageId,
                    DisplayName = m.StageName,
                    DescriptionText = m.Description,
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

        private static EquipmentMasterDatabase LoadEquipmentMasterDatabase()
        {
            var database = Resources.Load<EquipmentMasterDatabase>(EquipmentMasterDatabasePath);
            if (database == null && !_loggedMissingEquipmentMasterDatabase)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] EquipmentMasterDatabase was not found at Resources/{EquipmentMasterDatabasePath}.");
                _loggedMissingEquipmentMasterDatabase = true;
            }
            return database;
        }

        private static BattleSkillMasterDatabase LoadBattleSkillMasterDatabase()
        {
            var database = Resources.Load<BattleSkillMasterDatabase>(BattleSkillMasterDatabasePath);
            if (database == null && !_loggedMissingBattleSkillMasterDatabase)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] BattleSkillMasterDatabase was not found at Resources/{BattleSkillMasterDatabasePath}.");
                _loggedMissingBattleSkillMasterDatabase = true;
            }
            return database;
        }

        private static AttributeMasterDatabase LoadAttributeMasterDatabase()
        {
            var database = Resources.Load<AttributeMasterDatabase>(AttributeMasterDatabasePath);
            if (database == null && !_loggedMissingAttributeMasterDatabase)
            {
                Debug.LogWarning($"[MasterDataResourceLoader] AttributeMasterDatabase was not found at Resources/{AttributeMasterDatabasePath}.");
                _loggedMissingAttributeMasterDatabase = true;
            }
            return database;
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

        private static IReadOnlyList<BattleSkillData> BuildBattleSkillData(IReadOnlyList<BattleSkillMasterData> skillMasters)
        {
            if (skillMasters == null || skillMasters.Count == 0)
            {
                return Array.Empty<BattleSkillData>();
            }

            return skillMasters
                .Where(skill => skill != null)
                .Select(skill => new BattleSkillData
                {
                    SkillId = skill.SkillId,
                    DisplayName = skill.DisplayName,
                    Description = skill.Description,
                    Icon = skill.Icon,
                    RequiredCharge = Mathf.Max(1, skill.RequiredCharge),
                    StartingCharge = Mathf.Clamp(skill.StartingCharge, 0, Mathf.Max(1, skill.RequiredCharge)),
                    TurnChargeGain = Mathf.Max(0, skill.TurnChargeGain),
                    AttackChargeGain = Mathf.Max(0, skill.AttackChargeGain),
                    Damage = Mathf.Max(0, skill.Damage),
                    Attribute = BuildAttributeData(skill.Attribute),
                    EffectType = skill.EffectType,
                    EffectAnimation = BuildEffectAnimationData(skill.EffectAnimation),
                    DescriptionSupplement = skill.DescriptionSupplement ?? string.Empty,
                })
                .ToArray();
        }

        private static AttributeData BuildAttributeData(AttributeMasterData attribute)
        {
            if (attribute == null)
            {
                return null;
            }

            return new AttributeData
            {
                AttributeId = attribute.AttributeId,
                DisplayName = attribute.DisplayName,
                Description = attribute.Description,
                AccentColor = attribute.AccentColor,
                Icon = attribute.Icon,
                SortOrder = attribute.SortOrder,
            };
        }

        private static IReadOnlyList<EquipmentAttributeModifierData> BuildEquipmentAttributeModifierData(
            IReadOnlyList<EquipmentAttributeModifierEntry> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return Array.Empty<EquipmentAttributeModifierData>();
            }

            return modifiers
                .Where(modifier => modifier != null && modifier.Attribute != null)
                .Select(modifier => new EquipmentAttributeModifierData
                {
                    Attribute = BuildAttributeData(modifier.Attribute),
                    DamagePercent = Mathf.Max(0, modifier.DamagePercent),
                })
                .ToArray();
        }

        private static IReadOnlyList<string> BuildCategoryTags(IReadOnlyList<string> categoryTags)
        {
            if (categoryTags == null || categoryTags.Count == 0)
            {
                return Array.Empty<string>();
            }

            return categoryTags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .ToArray();
        }

        private static EffectAnimationData BuildEffectAnimationData(EffectAnimationMasterData effectAnimation)
        {
            if (effectAnimation == null)
            {
                return null;
            }

            return new EffectAnimationData
            {
                EffectAnimationId = effectAnimation.EffectAnimationId,
                DisplayName = effectAnimation.DisplayName,
                Description = effectAnimation.Description,
                CharacterAnimationName = effectAnimation.CharacterAnimationName,
                VisualEffectKey = effectAnimation.VisualEffectKey,
                SoundEffectKey = effectAnimation.SoundEffectKey,
                WaitSeconds = Mathf.Max(0f, effectAnimation.WaitSeconds),
            };
        }

        private static StageBattleEnemyData BuildBattleEnemyData(StageMasterData master)
        {
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
                        Name = enemyMaster.Name ?? string.Empty,
                        MaxHp = Mathf.Max(1, enemyMaster.MaxHp),
                        Damage = Mathf.Max(0, enemyMaster.Damage),
                        AttackAttribute = BuildAttributeData(ResolveEnemyRightHandWeaponAttribute(enemyMaster)),
                        DefenseAttributeModifiers = BuildEquipmentAttributeModifierData(ResolveEnemyChestAttributeModifiers(enemyMaster)),
                        Appearance = BuildResolvedEnemyAppearanceData(enemyMaster),
                        Patterns = BuildPatterns(enemyMaster),
                    };
                }
            }

            return new StageBattleEnemyData
            {
                Name = string.Empty,
                MaxHp = 1,
                Damage = 80,
                AttackAttribute = null,
                DefenseAttributeModifiers = Array.Empty<EquipmentAttributeModifierData>(),
                Appearance = new AppearanceData(),
                Patterns = Array.Empty<StageBattlePatternData>(),
            };
        }

        private static AttributeMasterData ResolveEnemyRightHandWeaponAttribute(StageBattleEnemyMasterData enemyMaster)
        {
            var slotMasters = enemyMaster?.AppearanceSlots;
            if (slotMasters == null)
            {
                return null;
            }

            for (var i = 0; i < slotMasters.Count; i++)
            {
                var equipment = slotMasters[i]?.Equipment;
                if (equipment != null && equipment.IsRightHandEquipment && equipment.NormalAttackAttribute != null)
                {
                    return equipment.NormalAttackAttribute;
                }
            }

            return null;
        }

        private static IReadOnlyList<EquipmentAttributeModifierEntry> ResolveEnemyChestAttributeModifiers(
            StageBattleEnemyMasterData enemyMaster)
        {
            var slotMasters = enemyMaster?.AppearanceSlots;
            if (slotMasters == null)
            {
                return Array.Empty<EquipmentAttributeModifierEntry>();
            }

            for (var i = 0; i < slotMasters.Count; i++)
            {
                var equipment = slotMasters[i]?.Equipment;
                if (equipment != null && equipment.PartType == PartsType.Chest)
                {
                    return equipment.AttributeModifiers;
                }
            }

            return Array.Empty<EquipmentAttributeModifierEntry>();
        }

        private static AppearanceData BuildResolvedEnemyAppearanceData(StageBattleEnemyMasterData enemyMaster)
        {
            if (enemyMaster == null)
            {
                return new AppearanceData();
            }

            var slotMasters = enemyMaster.AppearanceSlots;
            var colorMasters = enemyMaster.AppearanceColors;
            var appearanceData = new AppearanceData();
            var partStates = new Dictionary<PartsType, AppearanceData.PartsEntry>();
            var visibilityStates = new Dictionary<PartsType, AppearanceData.VisibilityEntry>();

            foreach (PartsType partType in Enum.GetValues(typeof(PartsType)))
            {
                if (IsDerivedEnemyAppearancePart(partType) || partType == PartsType.Skin)
                {
                    continue;
                }

                partStates[partType] = new AppearanceData.PartsEntry
                {
                    type = partType,
                    index = -1,
                };
                visibilityStates[partType] = new AppearanceData.VisibilityEntry
                {
                    type = partType,
                    visible = false,
                };
            }

            if (slotMasters != null)
            {
                for (var i = 0; i < slotMasters.Count; i++)
                {
                    var slot = slotMasters[i];
                    if (slot == null || IsDerivedEnemyAppearancePart(slot.PartType) || slot.PartType == PartsType.Skin)
                    {
                        continue;
                    }

                    var resolvedIndex = -1;
                    var resolvedVisible = false;
                    var equipment = slot.Equipment;
                    if (equipment != null && equipment.PartType == slot.PartType && equipment.PartsIndex >= 0)
                    {
                        resolvedIndex = equipment.PartsIndex;
                        resolvedVisible = slot.IsVisible;
                    }

                    partStates[slot.PartType] = new AppearanceData.PartsEntry
                    {
                        type = slot.PartType,
                        index = resolvedIndex,
                    };
                    visibilityStates[slot.PartType] = new AppearanceData.VisibilityEntry
                    {
                        type = slot.PartType,
                        visible = resolvedVisible,
                    };
                }
            }

            foreach (var entry in partStates.Values)
            {
                appearanceData.parts.Add(entry);
            }

            foreach (var entry in visibilityStates.Values)
            {
                appearanceData.visibility.Add(entry);
            }

            ApplyResolvedEnemyColors(appearanceData, colorMasters);

            return appearanceData;
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

        private static bool IsDerivedEnemyAppearancePart(PartsType partType)
        {
            return partType == PartsType.Arrow || partType == PartsType.HelmetHair;
        }

        private static void ApplyResolvedEnemyColors(
            AppearanceData appearanceData,
            IReadOnlyList<StageBattleEnemyAppearanceColorMasterData> colorMasters)
        {
            if (appearanceData == null || colorMasters == null || colorMasters.Count == 0)
            {
                return;
            }

            var colorMap = new Dictionary<ColorTargetType, Color>();
            if (appearanceData.colors != null)
            {
                for (var i = 0; i < appearanceData.colors.Count; i++)
                {
                    var existing = appearanceData.colors[i];
                    if (existing == null)
                    {
                        continue;
                    }

                    colorMap[existing.target] = existing.color;
                }
            }

            for (var i = 0; i < colorMasters.Count; i++)
            {
                var colorEntry = colorMasters[i];
                if (colorEntry == null)
                {
                    continue;
                }

                colorMap[colorEntry.Target] = colorEntry.Color;
            }

            appearanceData.colors.Clear();
            foreach (var pair in colorMap)
            {
                appearanceData.colors.Add(new AppearanceData.ColorEntry
                {
                    target = pair.Key,
                    color = pair.Value,
                });
            }
        }
    }
}
