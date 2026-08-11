namespace Assets.Scripts.Features.Battle.Runtime
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// バトルシーンで共有するセッション進行状態。
    /// BattleScene からターン数、HP、選択スキルなどの散在した状態をまとめる。
    /// </summary>
    public class BattleSessionState
    {
        private readonly List<BattleTemporaryStatModifier> _temporaryStatModifiers = new();

        public string EnemyName { get; private set; } = string.Empty;

        public int PlayerHp { get; private set; }

        public int InitialPlayerHp { get; private set; } = 1;

        public int EnemyHp { get; private set; }

        public int InitialEnemyHp { get; private set; } = 1;

        public int TurnNumber { get; private set; }

        public int WaveNumber { get; private set; } = 1;

        public int SelectedSkillSlotIndex { get; set; } = -1;

        public bool BattleEnded { get; set; }

        public bool IsPathInputLocked { get; set; }

        public bool NextTurnHazardBoosted { get; private set; }

        public IReadOnlyList<BattleTemporaryStatModifier> TemporaryStatModifiers => _temporaryStatModifiers;

        public void ResetForBattle(string enemyName, int initialPlayerHp, int initialEnemyHp)
        {
            EnemyName = enemyName ?? string.Empty;
            InitialPlayerHp = Max(1, initialPlayerHp);
            PlayerHp = InitialPlayerHp;
            InitialEnemyHp = Max(1, initialEnemyHp);
            EnemyHp = InitialEnemyHp;
            TurnNumber = 1;
            WaveNumber = 1;
            SelectedSkillSlotIndex = -1;
            BattleEnded = false;
            IsPathInputLocked = false;
            NextTurnHazardBoosted = false;
            _temporaryStatModifiers.Clear();
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

        public void ApplyPlayerHealing(int healing)
        {
            if (healing <= 0 || PlayerHp <= 0)
            {
                return;
            }

            PlayerHp = Min(InitialPlayerHp, PlayerHp + healing);
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

        public void AddTemporaryStatModifier(BattleTemporaryStatModifier modifier)
        {
            if (modifier == null || modifier.IsExpired)
            {
                return;
            }

            _temporaryStatModifiers.Add(modifier);
        }

        public void ConsumeTemporaryStatModifierTurn()
        {
            if (_temporaryStatModifiers.Count == 0)
            {
                return;
            }

            for (var i = _temporaryStatModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = _temporaryStatModifiers[i];
                if (modifier == null)
                {
                    _temporaryStatModifiers.RemoveAt(i);
                    continue;
                }

                modifier.ConsumeTurn();
                if (modifier.IsExpired)
                {
                    _temporaryStatModifiers.RemoveAt(i);
                }
            }
        }

        private static int Max(int left, int right)
        {
            return left > right ? left : right;
        }

        private static int Min(int left, int right)
        {
            return left < right ? left : right;
        }
    }
}
