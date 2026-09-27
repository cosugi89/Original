using System;
using Assets.Scripts.Data.DTO;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// プレイヤーの基礎値と補正値から stat block を作る。
    /// </summary>
    public sealed class BattlePlayerStatBlockFactory
    {
        private const int BaseAttack = 100;

        public BattleStatBlockData Create(BattleEquipmentDerivedData equipmentDerivedData = null)
        {
            return new BattleStatBlockData
            {
                Attack = BaseAttack,
                Defense = 0,
                Guard = 0,
                Modifiers = equipmentDerivedData?.BattleStatModifiers ?? Array.Empty<BattleStatModifierData>(),
            };
        }
    }
}

