using UnityEngine;

namespace Assets.Scripts.Features.Battle.Demo
{
    /// <summary>
    /// Path-Activation System v2 のデモで解決するマスの種別。
    /// 本番盤面のノード種別を段階的に増やす想定で最小構成のみ列挙する。
    /// </summary>
    public enum BattleDemoNodeType
    {
        /// <summary>スタート地点。実際の効果解決は行わない。</summary>
        Start,
        /// <summary>空マス。回避状態などは引き継ぐ。</summary>
        Empty,
        /// <summary>Jump マス。通常危険を回避でき、次の Attack が Jump Attack になる。</summary>
        Jump,
        /// <summary>Roll マス。通常/スキル危険を1回だけ回避する。</summary>
        Roll,
        /// <summary>Dance マス。デモでは装備依存効果のプレースホルダ。</summary>
        Dance,
        /// <summary>Attack マス。通常攻撃または選択中スキルを解決する。</summary>
        Attack,
        /// <summary>通常種別の危険マス。</summary>
        HazardNormal,
        /// <summary>スキル種別の危険マス。</summary>
        HazardSkill,
        /// <summary>ゴール地点。到達でターン確定。</summary>
        Goal,
    }

    /// <summary>
    /// Battle デモの経路解決ロジックを BattleScene から切り出した pure helper。
    /// </summary>
    public static class BattleDemoPathResolver
    {
        public struct Settings
        {
            public int CurrentEnemyHp;
            public int CurrentPlayerHp;
            public int NormalAttackDamage;
            public int DoubleAttackFollowUpDamage;
            public int JumpAttackDamage;
        }

        public struct Result
        {
            public bool PathConfirmed;
            public bool TookHit;
            public bool EnemyDefeated;
            public bool PlayerDefeated;
            public int EnemyDamageTaken;
            public int PlayerDamageTaken;
        }

        public static Result Resolve(BattleDemoTurnScript turnScript, bool hazardBoosted, BattleDemoSkillSlot selectedSkill, Settings settings)
        {
            var result = new Result
            {
                PathConfirmed = turnScript != null && turnScript.HasGoalInPath(),
                TookHit = false,
                EnemyDefeated = false,
                PlayerDefeated = false,
                EnemyDamageTaken = 0,
                PlayerDamageTaken = 0,
            };

            if (turnScript?.Path == null)
                return result;

            var jumpCanEvade = false;
            var jumpAttackPrimed = false;
            var rollCanEvade = false;
            var hazardGroupResolved = false;
            var basicAttackCount = 0;

            for (var i = 0; i < turnScript.Path.Count; i++)
            {
                var step = turnScript.Path[i];
                switch (step)
                {
                    case BattleDemoNodeType.Start:
                    case BattleDemoNodeType.Empty:
                        break;

                    case BattleDemoNodeType.Jump:
                        jumpCanEvade = true;
                        jumpAttackPrimed = true;
                        rollCanEvade = false;
                        break;

                    case BattleDemoNodeType.Roll:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = true;
                        break;

                    case BattleDemoNodeType.Dance:
                        jumpCanEvade = false;
                        jumpAttackPrimed = false;
                        rollCanEvade = false;
                        break;

                    case BattleDemoNodeType.Attack:
                    {
                        result.EnemyDamageTaken += ResolveAttackDamage(
                            selectedSkill,
                            ref jumpAttackPrimed,
                            ref basicAttackCount,
                            settings);

                        jumpCanEvade = false;
                        rollCanEvade = false;

                        if (result.EnemyDamageTaken >= settings.CurrentEnemyHp)
                        {
                            result.EnemyDefeated = true;
                            return result;
                        }
                        break;
                    }

                    case BattleDemoNodeType.HazardNormal:
                    case BattleDemoNodeType.HazardSkill:
                    {
                        if (hazardGroupResolved)
                            break;

                        if (step == BattleDemoNodeType.HazardNormal && jumpCanEvade)
                        {
                            jumpCanEvade = false;
                            hazardGroupResolved = true;
                            break;
                        }

                        if (rollCanEvade)
                        {
                            rollCanEvade = false;
                            jumpAttackPrimed = false;
                            hazardGroupResolved = true;
                            break;
                        }

                        hazardGroupResolved = true;
                        result.TookHit = true;
                        result.PlayerDamageTaken += hazardBoosted
                            ? Mathf.RoundToInt(turnScript.HazardDamage * 1.5f)
                            : turnScript.HazardDamage;

                        if (result.PlayerDamageTaken >= settings.CurrentPlayerHp)
                        {
                            result.PlayerDefeated = true;
                        }
                        return result;
                    }

                    case BattleDemoNodeType.Goal:
                        return result;
                }
            }

            return result;
        }

        private static int ResolveAttackDamage(
            BattleDemoSkillSlot selectedSkill,
            ref bool jumpAttackPrimed,
            ref int basicAttackCount,
            Settings settings)
        {
            if (selectedSkill != null)
                return selectedSkill.Damage;

            if (jumpAttackPrimed)
            {
                basicAttackCount++;
                jumpAttackPrimed = false;
                return settings.JumpAttackDamage;
            }

            basicAttackCount++;
            return basicAttackCount >= 2
                ? settings.DoubleAttackFollowUpDamage
                : settings.NormalAttackDamage;
        }
    }
}
