namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// バトルシーンで共有するセッション進行状態。
    /// BattleScene からターン数、HP、選択スキルなどの散在した状態をまとめる。
    /// </summary>
    public class BattleSessionState
    {
        public string EnemyName { get; private set; } = string.Empty;

        public int PlayerHp { get; private set; }

        public int InitialPlayerHp { get; private set; } = 1;

        public int EnemyHp { get; private set; }

        public int InitialEnemyHp { get; private set; } = 1;

        public int TurnNumber { get; private set; }

        public int WaveNumber { get; private set; } = 1;

        public int TurnScriptIndex { get; set; }

        public int SelectedSkillSlotIndex { get; set; } = -1;

        public bool BattleEnded { get; set; }

        public bool IsPathInputLocked { get; set; }

        public bool NextTurnHazardBoosted { get; private set; }

        public void ResetForBattle(string enemyName, int initialPlayerHp, int initialEnemyHp)
        {
            EnemyName = enemyName ?? string.Empty;
            InitialPlayerHp = Max(1, initialPlayerHp);
            PlayerHp = InitialPlayerHp;
            InitialEnemyHp = Max(1, initialEnemyHp);
            EnemyHp = InitialEnemyHp;
            TurnNumber = 1;
            WaveNumber = 1;
            TurnScriptIndex = 0;
            SelectedSkillSlotIndex = -1;
            BattleEnded = false;
            IsPathInputLocked = false;
            NextTurnHazardBoosted = false;
        }

        public void AdvanceTurn()
        {
            TurnNumber++;
        }

        public void ApplyEnemyDamage(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            EnemyHp = Max(0, EnemyHp - damage);
        }

        public void ApplyPlayerDamage(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            PlayerHp = Max(0, PlayerHp - damage);
            if (PlayerHp <= 0)
            {
                BattleEnded = true;
            }
        }

        public bool ConsumeHazardBoostFlag()
        {
            var result = NextTurnHazardBoosted;
            NextTurnHazardBoosted = false;
            return result;
        }

        public void ReserveNextTurnHazardBoost()
        {
            NextTurnHazardBoosted = true;
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }
    }
}
