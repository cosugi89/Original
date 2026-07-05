using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Runtime;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// BattleScene 初期化に必要なステージ・敵・プレイヤー情報をまとめて構築する。
    /// </summary>
    public sealed class BattleSceneSetupFactory
    {
        private readonly GameSaveService _saveService;
        private readonly AvatarService _avatarService;
        private readonly InventoryService _inventoryService;
        private readonly BattleProgressService _battleProgressService;
        private readonly BattlePlayerSnapshotFactory _playerSnapshotFactory;

        public BattleSceneSetupFactory(
            GameSaveService saveService = null,
            AvatarService avatarService = null,
            InventoryService inventoryService = null,
            BattleProgressService battleProgressService = null,
            BattlePlayerSnapshotFactory playerSnapshotFactory = null)
        {
            _saveService = saveService ?? GameSaveService.EnsureInitialized();
            _avatarService = avatarService ?? AvatarService.EnsureInitialized();
            _inventoryService = inventoryService ?? InventoryService.EnsureInitialized();
            _battleProgressService = battleProgressService ?? BattleProgressService.EnsureInitialized();
            _playerSnapshotFactory = playerSnapshotFactory ?? new BattlePlayerSnapshotFactory();
        }

        public bool TryCreate(
            int slotViewCount,
            int unlockedSkillSlotCount,
            out BattleSceneSetup setup)
        {
            setup = null;

            if (!TryResolveStageData(out var stageData))
            {
                return false;
            }

            var snapshotSource = ResolvePlayerSnapshotSource();
            var playerSnapshot = _playerSnapshotFactory.Create(
                snapshotSource,
                slotViewCount,
                unlockedSkillSlotCount);

            var enemyData = stageData.Enemy ?? new StageBattleEnemyData();
            var patterns = enemyData.Patterns ?? Array.Empty<StageBattlePatternData>();
            var enemyName = !string.IsNullOrWhiteSpace(enemyData.Name)
                ? enemyData.Name
                : string.Empty;
            var initialEnemyHp = !string.IsNullOrWhiteSpace(enemyData.Name)
                ? Math.Max(1, enemyData.MaxHp)
                : 1;

            setup = new BattleSceneSetup(
                stageData,
                playerSnapshot,
                enemyData,
                patterns,
                enemyName,
                initialEnemyHp);
            return true;
        }

        private bool TryResolveStageData(out StageData stageData)
        {
            stageData = null;
            var stageId = BattleSceneTransitionState.ConsumeSelectedStageId();
            if (stageId < 0)
            {
                stageId = _battleProgressService.GetCurrentOrRecommendedStageId();
                if (stageId < 0)
                {
                    return false;
                }
            }

            return MasterDataResourceLoader.TryLoadStageData(stageId, out stageData);
        }

        private BattlePlayerSnapshotSource ResolvePlayerSnapshotSource()
        {
            var profile = _saveService.Session.UserData?.Profile ?? new UserProfileDemoData();
            var equipmentDerivedData = ResolveEquippedEquipmentDerivedData();
            return new BattlePlayerSnapshotSource(profile, equipmentDerivedData);
        }

        private BattleEquipmentDerivedData ResolveEquippedEquipmentDerivedData()
        {
            var equippedParts = _avatarService.GetAllPartStates();
            if (equippedParts == null || equippedParts.Count == 0)
            {
                return new BattleEquipmentDerivedData();
            }

            var definitions = new List<EquipmentData>();
            for (var i = 0; i < equippedParts.Count; i++)
            {
                var partState = equippedParts[i];
                if (partState == null ||
                    partState.EquipmentId <= 0 ||
                    !_inventoryService.TryGetDefinition(partState.EquipmentId, out var definition) ||
                    definition == null)
                {
                    continue;
                }

                definitions.Add(definition);
            }

            return BattleEquipmentDerivedDataResolver.Resolve(definitions);
        }
    }
}

