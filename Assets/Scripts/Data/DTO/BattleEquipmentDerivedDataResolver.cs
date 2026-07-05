using System;
using System.Collections.Generic;
using LayerLab.ArtMakerUnity;

namespace Assets.Scripts.Data.DTO
{
    public sealed class BattleEquipmentDerivedData
    {
        public AttributeData AttackAttribute { get; init; }

        public IReadOnlyList<EquipmentAttributeModifierData> DefenseAttributeModifiers { get; init; } =
            Array.Empty<EquipmentAttributeModifierData>();

        public IReadOnlyList<BattleStatModifierData> BattleStatModifiers { get; init; } =
            Array.Empty<BattleStatModifierData>();
    }

    public static class BattleEquipmentDerivedDataResolver
    {
        public static BattleEquipmentDerivedData Resolve(IReadOnlyList<EquipmentData> equipmentDefinitions)
        {
            if (equipmentDefinitions == null || equipmentDefinitions.Count == 0)
            {
                return new BattleEquipmentDerivedData();
            }

            AttributeData attackAttribute = null;
            IReadOnlyList<EquipmentAttributeModifierData> defenseAttributeModifiers =
                Array.Empty<EquipmentAttributeModifierData>();
            var resolvedDefenseAttributeModifiers = false;
            var battleStatModifiers = new List<BattleStatModifierData>();

            for (var i = 0; i < equipmentDefinitions.Count; i++)
            {
                var definition = equipmentDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (attackAttribute == null &&
                    definition.IsRightHandEquipment &&
                    definition.NormalAttackAttribute != null)
                {
                    attackAttribute = definition.NormalAttackAttribute;
                }

                if (!resolvedDefenseAttributeModifiers && definition.PartType == PartsType.Chest)
                {
                    defenseAttributeModifiers = definition.AttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>();
                    resolvedDefenseAttributeModifiers = true;
                }

                if (definition.BattleStatModifiers == null || definition.BattleStatModifiers.Count == 0)
                {
                    continue;
                }

                for (var modifierIndex = 0; modifierIndex < definition.BattleStatModifiers.Count; modifierIndex++)
                {
                    var modifier = definition.BattleStatModifiers[modifierIndex];
                    if (modifier != null)
                    {
                        battleStatModifiers.Add(modifier);
                    }
                }
            }

            return new BattleEquipmentDerivedData
            {
                AttackAttribute = attackAttribute,
                DefenseAttributeModifiers = defenseAttributeModifiers,
                BattleStatModifiers = battleStatModifiers.Count > 0
                    ? battleStatModifiers.ToArray()
                    : Array.Empty<BattleStatModifierData>(),
            };
        }
    }
}
