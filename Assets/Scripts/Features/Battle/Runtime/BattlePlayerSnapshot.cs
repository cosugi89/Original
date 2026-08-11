using System;
using System.Collections.Generic;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 戦闘中に参照するプレイヤー状態のスナップショット。
    /// </summary>
    public sealed class BattlePlayerSnapshot
    {
        public BattlePlayerSnapshot(
            UserProfileData profile,
            BattlePlayerStats stats,
            IReadOnlyList<SkillSlotRuntime> skillSlots)
        {
            Profile = profile ?? new UserProfileDemoData();
            Stats = stats ?? new BattlePlayerStats(baseAttackPower: 1);
            SkillSlots = skillSlots ?? Array.Empty<SkillSlotRuntime>();
        }

        public UserProfileData Profile { get; }

        public BattlePlayerStats Stats { get; }

        public int MaxHp => Math.Max(1, Profile?.MaxHp ?? 1);

        public IReadOnlyList<SkillSlotRuntime> SkillSlots { get; }
    }
}

