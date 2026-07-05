using Assets.Scripts.Features.Battle.Runtime;

namespace Assets.Scripts.Features.Battle.Logic
{
    /// <summary>
    /// ターン解決中に蓄積する行動結果と中間状態を保持する。
    /// </summary>
    internal sealed class BattleTurnResolutionState
    {
        public BattleTurnResolutionState(int currentEnemyHp, int currentPlayerHp)
        {
            CurrentEnemyHp = Max(0, currentEnemyHp);
            CurrentPlayerHp = Max(0, currentPlayerHp);
        }

        public int CurrentEnemyHp { get; private set; }

        public int CurrentPlayerHp { get; private set; }

        public int EnemyDamageTaken { get; private set; }

        public int PlayerDamageTaken { get; private set; }

        public int ResolvedHazardCount { get; private set; }

        public bool PathConfirmed { get; private set; }

        public bool GoalReached { get; private set; }

        public bool TookHit { get; private set; }

        public bool StoppedByHazardHit { get; private set; }

        public bool EnemyDefeated { get; private set; }

        public bool PlayerDefeated { get; private set; }

        public void Apply(BattleResolvedAction action)
        {
            if (action == null)
            {
                return;
            }

            var resolution = action.DamageResolution;
            if (resolution != null && resolution.Damage > 0)
            {
                if (action.TargetsEnemy)
                {
                    EnemyDamageTaken += resolution.Damage;
                    CurrentEnemyHp = Max(0, CurrentEnemyHp - resolution.Damage);
                }
                else
                {
                    PlayerDamageTaken += resolution.Damage;
                    CurrentPlayerHp = Max(0, CurrentPlayerHp - resolution.Damage);
                }
            }

            if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedHazard))
            {
                ResolvedHazardCount++;
            }

            PathConfirmed |= action.HasSemantic(BattleResolvedActionSemantics.PathConfirmed);
            GoalReached |= action.HasSemantic(BattleResolvedActionSemantics.GoalReached);
            TookHit |= action.HasSemantic(BattleResolvedActionSemantics.TookHit);
            StoppedByHazardHit |= action.HasSemantic(BattleResolvedActionSemantics.StoppedByHazardHit);
            EnemyDefeated |= action.HasSemantic(BattleResolvedActionSemantics.EnemyDefeated) || CurrentEnemyHp <= 0;
            PlayerDefeated |= action.HasSemantic(BattleResolvedActionSemantics.PlayerDefeated) || CurrentPlayerHp <= 0;
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }
    }
}

