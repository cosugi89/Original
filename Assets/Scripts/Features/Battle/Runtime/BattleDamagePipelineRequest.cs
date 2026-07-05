using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// ダメージパイプラインへ渡す計算依頼を表す。
    /// </summary>
    public sealed class BattleDamagePipelineRequest
    {
        public BattleDamageKind DamageKind { get; init; } = BattleDamageKind.NormalAttack;

        public BattleTurnContext Context { get; init; }

        public SkillSlotRuntime Skill { get; init; }

        public int? BaseDamageOverride { get; init; }

        public AttributeData AttackAttribute { get; init; }

        public IReadOnlyList<EquipmentAttributeModifierData> DefenseAttributeModifiers { get; init; } =
            Array.Empty<EquipmentAttributeModifierData>();

        public bool HazardBoosted { get; init; }
    }
}

