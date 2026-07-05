using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// パス走査中の位置、カウント、停止理由などを保持する。
    /// </summary>
    internal sealed class BattleTurnTraversalState
    {
        private readonly HashSet<int> _resolvedHazardGroups = new();

        public bool JumpCanEvade { get; private set; }

        public bool JumpAttackPrimed { get; private set; }

        public bool RollCanEvade { get; private set; }

        public int BasicAttackCount { get; private set; }

        public void PrepareJump()
        {
            JumpCanEvade = true;
            JumpAttackPrimed = true;
            RollCanEvade = false;
        }

        public void PrepareRoll()
        {
            JumpCanEvade = false;
            JumpAttackPrimed = false;
            RollCanEvade = true;
        }

        public void ResolveDance()
        {
            JumpCanEvade = false;
            JumpAttackPrimed = false;
            RollCanEvade = false;
        }

        public void ResolveAttackExecution()
        {
            JumpCanEvade = false;
            RollCanEvade = false;
        }

        public bool IsHazardResolved(int hazardGroupKey)
        {
            return _resolvedHazardGroups.Contains(hazardGroupKey);
        }

        public void MarkHazardResolved(int hazardGroupKey)
        {
            _resolvedHazardGroups.Add(hazardGroupKey);
        }

        public bool TryEvadeHazardWithJump(BattleNodeType nodeType, int hazardGroupKey)
        {
            if (nodeType != BattleNodeType.HazardNormal || !JumpCanEvade)
            {
                return false;
            }

            JumpCanEvade = false;
            MarkHazardResolved(hazardGroupKey);
            return true;
        }

        public bool TryEvadeHazardWithRoll(int hazardGroupKey)
        {
            if (!RollCanEvade)
            {
                return false;
            }

            RollCanEvade = false;
            JumpAttackPrimed = false;
            MarkHazardResolved(hazardGroupKey);
            return true;
        }

        public BattleDamageKind ConsumeBasicAttackDamageKind()
        {
            if (JumpAttackPrimed)
            {
                BasicAttackCount++;
                JumpAttackPrimed = false;
                return BattleDamageKind.JumpAttack;
            }

            BasicAttackCount++;
            return BasicAttackCount >= 2
                ? BattleDamageKind.DoubleAttackFollowUp
                : BattleDamageKind.NormalAttack;
        }
    }
}

