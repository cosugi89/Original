using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

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

        public int NormalAttackDamage { get; set; } = 100;

        public int DoubleAttackFollowUpDamage { get; set; } = 125;

        public int JumpAttackDamage { get; set; } = 150;

        public int HazardDamage { get; set; } = 80;

        public int EnemyActionDamage { get; set; } = 80;

        public IReadOnlyDictionary<int, int> HazardDamageByGroupId { get; set; } =
            new Dictionary<int, int>();

        public bool HazardBoosted { get; set; }

        public BattleEnemyActionType EnemyAction { get; set; } = BattleEnemyActionType.NormalAttack;

        public BattleSkillSlotRuntime SelectedSkill { get; set; }

        public BattleTurnContext Clone()
        {
            return new BattleTurnContext
            {
                CurrentEnemyHp = CurrentEnemyHp,
                CurrentPlayerHp = CurrentPlayerHp,
                NormalAttackDamage = NormalAttackDamage,
                DoubleAttackFollowUpDamage = DoubleAttackFollowUpDamage,
                JumpAttackDamage = JumpAttackDamage,
                HazardDamage = HazardDamage,
                EnemyActionDamage = EnemyActionDamage,
                HazardDamageByGroupId = HazardDamageByGroupId != null
                    ? new Dictionary<int, int>(HazardDamageByGroupId)
                    : new Dictionary<int, int>(),
                HazardBoosted = HazardBoosted,
                EnemyAction = EnemyAction,
                SelectedSkill = SelectedSkill,
            };
        }
    }
}
