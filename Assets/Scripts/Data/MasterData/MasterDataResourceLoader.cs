using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Core.Appearance;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public static class MasterDataResourceLoader
    {
        private const string EquipmentMasterDatabasePath = "MasterData/EquipmentDatabase";
        private const string BattleSkillMasterDatabasePath = "MasterData/BattleSkillDatabase";
        private const string AttributeMasterDatabasePath = "MasterData/AttributeDatabase";
        private const string StageMasterDatabasePath = "MasterData/StageDatabase";
        private const string ArmoryProductDatabasePath = "MasterData/ArmoryProductDatabase";

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

            return BuildEquipmentData(database.Equipments);
        }

        public static ArmoryProductDatabase LoadArmoryProductDatabase() =>
            Resources.Load<ArmoryProductDatabase>(ArmoryProductDatabasePath);

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

        public static bool TryLoadBattleSkillData(string skillKey, out BattleSkillData skillData)
        {
            skillData = null;
            if (string.IsNullOrWhiteSpace(skillKey))
            {
                return false;
            }

            var database = LoadBattleSkillMasterDatabase();
            if (database == null || !database.TryGetByKey(skillKey.Trim(), out var master) || master == null)
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
        /// 旧属性マスタを読み込み、DTO リストに変換して返す。
        /// 新仕様の戦闘計算や UI では使用しない。
        /// </summary>
        [Obsolete("旧属性実装の互換 API です。新仕様の戦闘計算や UI では使用しません。")]
        public static IReadOnlyList<AttributeData> LoadAttributeData()
        {
            var database = LoadAttributeMasterDatabase();
            if (database == null)
            {
                return Array.Empty<AttributeData>();
            }

            return database.Attributes
                .Where(attribute => attribute != null && attribute.IsVisibleInUI)
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
                    StageKind = m.StageKind,
                    UnlockRequirementMode = m.UnlockRequirementMode,
                    RequiredClearedStageIds = m.RequiredClearedStageIds.ToArray(),
                    ClearRewards = m.ClearRewards
                        .Where(reward => reward != null && reward.Quantity > 0)
                        .Select(reward => new MissionRewardData
                        {
                            RewardType = reward.RewardType,
                            ItemId = reward.ItemId,
                            EquipmentId = reward.EquipmentId,
                            Quantity = reward.Quantity,
                        })
                        .ToArray(),
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

        private static IReadOnlyList<BattleSkillData> BuildBattleSkillData(
            IReadOnlyList<BattleSkillMasterData> skillMasters,
            int maxCount = int.MaxValue)
        {
            if (skillMasters == null || skillMasters.Count == 0 || maxCount <= 0)
            {
                return Array.Empty<BattleSkillData>();
            }

            return skillMasters
                .Where(skill => skill != null)
                .Take(maxCount)
                .Select(skill => new BattleSkillData
                {
                    SkillId = skill.SkillId,
                    SkillKey = skill.SkillKey,
                    DisplayName = skill.DisplayName,
                    Description = skill.Description,
                    Icon = skill.Icon,
                    RequiredCharge = Mathf.Max(1, skill.RequiredCharge),
                    StartingCharge = Mathf.Clamp(skill.StartingCharge, 0, Mathf.Max(1, skill.RequiredCharge)),
                    TurnChargeGain = Mathf.Max(0, skill.TurnChargeGain),
                    AttackChargeGain = Mathf.Max(0, skill.AttackChargeGain),
                    Effects = BuildBattleSkillEffectEntryData(skill.EffectData),
                    EffectAnimation = BuildEffectAnimationData(skill.EffectAnimation),
                })
                .ToArray();
        }

        private static BattleSkillData BuildBattleSkillData(BattleSkillMasterData skillMaster)
        {
            if (skillMaster == null)
            {
                return null;
            }

            var skills = BuildBattleSkillData(new[] { skillMaster }, maxCount: 1);
            return skills.Count > 0 ? skills[0] : null;
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
                AttributeKey = attribute.AttributeKey,
                DisplayName = attribute.DisplayName,
                Description = attribute.Description,
                IsVisibleInUI = attribute.IsVisibleInUI,
                Icon = attribute.Icon,
                SortOrder = attribute.SortOrder,
            };
        }

        private static IReadOnlyList<BattleSkillEffectEntryData> BuildBattleSkillEffectEntryData(
            IReadOnlyList<BattleSkillOperationData> effects)
        {
            if (effects == null || effects.Count == 0)
            {
                return Array.Empty<BattleSkillEffectEntryData>();
            }

            return effects
                .Where(effect => effect != null)
                .Select(effect => new BattleSkillEffectEntryData
                {
                    EffectType = effect.EffectType,
                    TargetType = effect.TargetType,
                    ParameterType = effect.ParameterType,
                    ValueType = effect.ValueType,
                    Value = effect.Value,
                    DamageFormula = BuildBattleDamageFormulaData(effect.DamageFormula),
                    DurationTurns = Mathf.Max(0, effect.DurationTurns),
                    SuccessRate = Mathf.Clamp01(effect.SuccessRate),
                })
                .ToArray();
        }

        private static BattleDamageFormulaData BuildBattleDamageFormulaData(BattleDamageFormulaDefinition formula)
        {
            if (formula == null)
            {
                return null;
            }

            return new BattleDamageFormulaData
            {
                IsEnabled = formula.IsEnabled,
                ParameterType = formula.ParameterType,
                ValueType = formula.ValueType,
                Value = formula.Value,
                FlatBonus = Mathf.Max(0, formula.FlatBonus),
                FinalMultiplier = Mathf.Max(0f, formula.FinalMultiplier),
                SourceScalarKey = formula.SourceScalarKey?.Trim() ?? string.Empty,
                SourceScalarFallback = formula.SourceScalarFallback,
                MinimumDamage = Mathf.Max(0, formula.MinimumDamage),
            };
        }

        private static IReadOnlyList<BattleStatModifierData> BuildBattleStatModifierData(
            IReadOnlyList<BattleStatModifierDefinitionEntry> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return Array.Empty<BattleStatModifierData>();
            }

            return modifiers
                .Where(modifier => modifier != null)
                .Select(modifier => new BattleStatModifierData
                {
                    TargetType = modifier.TargetType,
                    ParameterType = modifier.ParameterType,
                    NamedValueKey = modifier.NamedValueKey?.Trim() ?? string.Empty,
                    OperationType = modifier.OperationType,
                    Value = modifier.Value,
                })
                .ToArray();
        }

        private static BattleStatBlockData BuildBattleStatBlockData(
            BattleStatBlockDefinition statBlock,
            IReadOnlyList<BattleStatModifierData> additionalModifiers = null)
        {
            var definition = statBlock ?? new BattleStatBlockDefinition();
            var baseModifiers = BuildBattleStatModifierData(definition.Modifiers);
            var mergedModifiers = MergeBattleStatModifiers(baseModifiers, additionalModifiers);

            return new BattleStatBlockData
            {
                Attack = Mathf.Max(0, definition.Attack),
                Defense = Mathf.Max(0, definition.Defense),
                Guard = Mathf.Max(0, definition.Guard),
                Modifiers = mergedModifiers,
            };
        }

        private static IReadOnlyList<BattleStatModifierData> MergeBattleStatModifiers(
            IReadOnlyList<BattleStatModifierData> primaryModifiers,
            IReadOnlyList<BattleStatModifierData> secondaryModifiers)
        {
            var result = new List<BattleStatModifierData>();

            if (primaryModifiers != null)
            {
                for (var i = 0; i < primaryModifiers.Count; i++)
                {
                    if (primaryModifiers[i] != null)
                    {
                        result.Add(primaryModifiers[i]);
                    }
                }
            }

            if (secondaryModifiers != null)
            {
                for (var i = 0; i < secondaryModifiers.Count; i++)
                {
                    if (secondaryModifiers[i] != null)
                    {
                        result.Add(secondaryModifiers[i]);
                    }
                }
            }

            return result.Count > 0 ? result.ToArray() : Array.Empty<BattleStatModifierData>();
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
                EffectAnimationKey = effectAnimation.EffectAnimationKey,
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

                    var equipmentDerivedData = BuildEnemyEquipmentDerivedData(enemyMaster);
                    return new StageBattleEnemyData
                    {
                        Name = enemyMaster.Name ?? string.Empty,
                        MaxHp = Mathf.Max(1, enemyMaster.MaxHp),
                        Stats = BuildBattleStatBlockData(
                            enemyMaster.Stats,
                            equipmentDerivedData.BattleStatModifiers),
                        Appearance = BuildResolvedEnemyAppearanceData(enemyMaster),
                        Patterns = BuildPatterns(enemyMaster),
                    };
                }
            }

            return new StageBattleEnemyData
            {
                Name = string.Empty,
                MaxHp = 1,
                Stats = new BattleStatBlockData
                {
                    Attack = 80,
                },
                Appearance = new AppearanceData(),
                Patterns = Array.Empty<StageBattlePatternData>(),
            };
        }

        private static BattleEquipmentDerivedData BuildEnemyEquipmentDerivedData(StageBattleEnemyMasterData enemyMaster)
        {
            var slotMasters = enemyMaster?.AppearanceSlots;
            if (slotMasters == null || slotMasters.Count == 0)
            {
                return new BattleEquipmentDerivedData();
            }

            var equipmentDefinitions = new List<EquipmentData>();
            for (var i = 0; i < slotMasters.Count; i++)
            {
                var equipment = slotMasters[i]?.Equipment;
                var equipmentData = BuildEquipmentData(equipment);
                if (equipmentData != null)
                {
                    equipmentDefinitions.Add(equipmentData);
                }
            }

            return BattleEquipmentDerivedDataResolver.Resolve(equipmentDefinitions);
        }

        private static IReadOnlyList<EquipmentData> BuildEquipmentData(IReadOnlyList<EquipmentMasterData> masters)
        {
            if (masters == null || masters.Count == 0)
            {
                return Array.Empty<EquipmentData>();
            }

            var result = new List<EquipmentData>();
            for (var i = 0; i < masters.Count; i++)
            {
                var equipmentData = BuildEquipmentData(masters[i]);
                if (equipmentData != null)
                {
                    result.Add(equipmentData);
                }
            }

            return result.Count > 0 ? result.ToArray() : Array.Empty<EquipmentData>();
        }

        private static EquipmentData BuildEquipmentData(EquipmentMasterData master)
        {
            if (master == null)
            {
                return null;
            }

            return new EquipmentData
            {
                EquipmentId = master.EquipmentId,
                DisplayName = master.DisplayName,
                Description = master.Description,
                PartType = master.PartType,
                ExclusiveGroup = master.ExclusiveGroup,
                PartsIndex = master.PartsIndex,
                Icon = master.Icon,
                SortOrder = master.SortOrder > 0 ? master.SortOrder : Mathf.Max(master.PartsIndex, 0),
                IsDefaultOwned = master.IsDefaultOwned,
                Rarity = master.Rarity,
                AssignableSkills = BuildBattleSkillData(
                    master.AssignableSkills,
                    EquipmentMasterData.MaxAssignableSkillCount),
                DanceSkill = BuildBattleSkillData(master.DanceSkill),
                BattleStatModifiers = BuildBattleStatModifierData(master.BattleStatModifiers),
                CategoryTags = BuildCategoryTags(master.CategoryTags),
            };
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
                    EnemyActionDamageFormula = BuildBattleDamageFormulaData(pattern.EnemyActionDamageFormula),
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
