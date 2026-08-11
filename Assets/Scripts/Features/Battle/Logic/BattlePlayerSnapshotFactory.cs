using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Runtime;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// 保存済みプロフィールから戦闘用プレイヤースナップショットを生成する。
    /// </summary>
    public sealed class BattlePlayerSnapshotFactory
    {
        private readonly BattlePlayerStatBlockFactory _playerStatBlockFactory;
        private readonly BattleCombatantStatsFactory _combatantStatsFactory;

        public BattlePlayerSnapshotFactory()
        {
            _playerStatBlockFactory = new BattlePlayerStatBlockFactory();
            _combatantStatsFactory = new BattleCombatantStatsFactory();
        }

        public BattlePlayerSnapshot Create(
            BattlePlayerSnapshotSource snapshotSource,
            int slotViewCount,
            int unlockedSkillSlotCount)
        {
            var resolvedSource = snapshotSource ?? new BattlePlayerSnapshotSource(
                new UserProfileDemoData(),
                new BattleEquipmentDerivedData());
            var profile = resolvedSource.Profile ?? new UserProfileDemoData();
            var equipmentDerivedData = resolvedSource.EquipmentDerivedData ?? new BattleEquipmentDerivedData();
            var stats = CreatePlayerStats(profile, equipmentDerivedData);
            var skillSlots = CreateSkillSlots(profile, slotViewCount, unlockedSkillSlotCount);

            Debug.Log(
                $"[Battle] UserData Profile HP={profile.MaxHp} " +
                $"Attack={stats.BaseAttackPower} " +
                $"x{stats.NormalAttackMultiplier:0.##}/x{stats.DoubleAttackFollowUpMultiplier:0.##}/x{stats.JumpAttackMultiplier:0.##} " +
                $"SkillSlots={skillSlots.Length}");

            return new BattlePlayerSnapshot(profile, stats, skillSlots);
        }

        private BattlePlayerStats CreatePlayerStats(
            UserProfileData profile,
            BattleEquipmentDerivedData equipmentDerivedData)
        {
            var normalizedProfile = profile ?? new UserProfileDemoData();
            var statBlock = _playerStatBlockFactory.Create(normalizedProfile, equipmentDerivedData);
            return _combatantStatsFactory.CreatePlayerStats(statBlock, Math.Max(1, normalizedProfile.MaxHp));
        }

        private static SkillSlotRuntime[] CreateSkillSlots(
            UserProfileData profile,
            int slotViewCount,
            int unlockedSkillSlotCount)
        {
            var skillIdList = ResolveSkillIdList(profile);
            var runtimes = new SkillSlotRuntime[Math.Max(0, slotViewCount)];

            for (var i = 0; i < runtimes.Length; i++)
            {
                var skillId = i < skillIdList.Count ? Math.Max(0, skillIdList[i]) : 0;
                var runtime = new SkillSlotRuntime(
                    skillId,
                    ResolveSkillDefinition(skillId),
                    i >= 0 && i < unlockedSkillSlotCount);
                runtime.ResetRuntime();
                runtimes[i] = runtime;
            }

            return runtimes;
        }

        private static IReadOnlyList<int> ResolveSkillIdList(UserProfileData profile)
        {
            if (profile?.SkillLoadout?.SkillIds != null && profile.SkillLoadout.SkillIds.Count > 0)
            {
                return profile.SkillLoadout.SkillIds;
            }

            if (profile?.SkillSlotSkillIds != null && profile.SkillSlotSkillIds.Count > 0)
            {
                return profile.SkillSlotSkillIds;
            }

            return new UserProfileDemoData().SkillSlotSkillIds;
        }

        private static BattleSkillData ResolveSkillDefinition(int skillId)
        {
            if (skillId <= 0)
            {
                return null;
            }

            return MasterDataResourceLoader.TryLoadBattleSkillData(skillId, out var skillDefinition)
                ? skillDefinition
                : null;
        }
    }
}

