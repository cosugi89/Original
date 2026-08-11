using System;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// プレイヤーの基礎値と補正値から stat block を作る。
    /// </summary>
    public sealed class BattlePlayerStatBlockFactory
    {
        public BattleStatBlockData Create(
            UserProfileData profile,
            BattleEquipmentDerivedData equipmentDerivedData = null)
        {
            var normalizedProfile = profile ?? new UserProfileDemoData();
            return new BattleStatBlockData
            {
                Attack = ResolveLevelBasedAttack(normalizedProfile),
                Defense = 0,
                Guard = 0,
                Modifiers = equipmentDerivedData?.BattleStatModifiers ?? Array.Empty<BattleStatModifierData>(),
            };
        }

        private static int ResolveLevelBasedAttack(UserProfileData profile)
        {
            return 100 + Math.Max(0, (profile?.Level ?? 1) - 1) * 5;
        }
    }
}

