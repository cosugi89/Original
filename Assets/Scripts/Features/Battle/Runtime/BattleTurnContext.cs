using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Data.DTO;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 1 ターンぶんの解決に必要な入力値を束ねるコンテキスト。
    /// 将来は盤面状態や敵意図、装備補正をここから参照できるように拡張する。
    /// </summary>
    public class BattleTurnContext
    {
        public int CurrentEnemyHp { get; set; }

        public int CurrentPlayerHp { get; set; }

        public BattlePlayerStats PlayerStats { get; set; } = new(baseAttackPower: 100);

        public BattleCombatantStats EnemyStats { get; set; } = new();

        public float EnemyActionMultiplier { get; set; } = 1f;

        public BattleDamageFormulaData EnemyActionDamageFormula { get; set; }

        public AttributeData PlayerAttackAttribute { get; set; }

        public AttributeData EnemyAttackAttribute { get; set; }

        public IReadOnlyList<EquipmentAttributeModifierData> EnemyDefenseAttributeModifiers { get; set; } =
            Array.Empty<EquipmentAttributeModifierData>();

        public IReadOnlyList<EquipmentAttributeModifierData> PlayerDefenseAttributeModifiers { get; set; } =
            Array.Empty<EquipmentAttributeModifierData>();

        public bool HazardBoosted { get; set; }

        public BattleEnemyActionType EnemyAction { get; set; } = BattleEnemyActionType.NormalAttack;

        public SkillSlotRuntime SelectedSkill { get; set; }

        public BattleTurnContext Clone()
        {
            return new BattleTurnContext
            {
                CurrentEnemyHp = CurrentEnemyHp,
                CurrentPlayerHp = CurrentPlayerHp,
                PlayerStats = PlayerStats,
                EnemyStats = EnemyStats,
                EnemyActionMultiplier = EnemyActionMultiplier,
                EnemyActionDamageFormula = EnemyActionDamageFormula,
                PlayerAttackAttribute = PlayerAttackAttribute,
                EnemyAttackAttribute = EnemyAttackAttribute,
                EnemyDefenseAttributeModifiers = EnemyDefenseAttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>(),
                PlayerDefenseAttributeModifiers = PlayerDefenseAttributeModifiers ?? Array.Empty<EquipmentAttributeModifierData>(),
                HazardBoosted = HazardBoosted,
                EnemyAction = EnemyAction,
                SelectedSkill = SelectedSkill,
            };
        }
    }
}
