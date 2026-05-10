using System;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// スキルパレット 1 枠ぶんの本番ランタイム状態。
    /// ひとまずデモ版の能力を引き継ぎつつ、将来の HUD / 装備連携の受け皿にする。
    /// </summary>
    [Serializable]
    public class BattleSkillSlotRuntime
    {
        public enum SlotState
        {
            Locked,
            Empty,
            Charging,
            Ready,
        }

        public string DisplayName { get; set; } = "Skill";

        public string Description { get; set; } = string.Empty;

        public string DescriptionSupplement { get; set; } = string.Empty;

        public int SkillId { get; set; }

        public bool IsUnlocked { get; set; } = true;

        public bool IsConfigured { get; set; } = true;

        public int RequiredCharge { get; set; } = 3;

        public int StartingCharge { get; set; }

        public int TurnChargeGain { get; set; } = 1;

        public int AttackChargeGain { get; set; } = 1;

        public int Damage { get; set; }

        public AttributeData Attribute { get; set; }

        public BattleSkillEffectType EffectType { get; set; } = BattleSkillEffectType.Damage;

        public EffectAnimationData EffectAnimation { get; set; }

        private int _currentCharge;

        public int CurrentCharge => _currentCharge;

        public bool IsReady => State == SlotState.Ready;

        public SlotState State
        {
            get
            {
                if (!IsUnlocked)
                {
                    return SlotState.Locked;
                }

                if (!IsConfigured)
                {
                    return SlotState.Empty;
                }

                return _currentCharge >= GetRequiredCharge()
                    ? SlotState.Ready
                    : SlotState.Charging;
            }
        }

        public void ResetRuntime()
        {
            _currentCharge = Clamp(StartingCharge, 0, GetRequiredCharge());
        }

        public void SetCurrentCharge(int currentCharge)
        {
            _currentCharge = Clamp(currentCharge, 0, GetRequiredCharge());
        }

        public void GainTurnCharge()
        {
            GainCharge(TurnChargeGain);
        }

        public void GainAttackCharge(int resolvedAttackCount = 1)
        {
            if (resolvedAttackCount <= 0)
            {
                return;
            }

            GainCharge(AttackChargeGain * resolvedAttackCount);
        }

        public void Consume()
        {
            _currentCharge = 0;
        }

        public int GetRemainingTurnsUntilReady()
        {
            if (State == SlotState.Ready)
            {
                return 0;
            }

            if (State == SlotState.Locked || State == SlotState.Empty || TurnChargeGain <= 0)
            {
                return int.MaxValue;
            }

            var missing = GetRequiredCharge() - _currentCharge;
            return (missing + TurnChargeGain - 1) / TurnChargeGain;
        }

        public string GetStateLabel()
        {
            switch (State)
            {
                case SlotState.Locked:
                    return "LOCKED (未取得)";
                case SlotState.Empty:
                    return "EMPTY (未設定)";
                case SlotState.Ready:
                    return "READY";
                default:
                    return "CHARGING";
            }
        }

        public string GetChargeLabel()
        {
            if (State == SlotState.Locked || State == SlotState.Empty)
            {
                return "-/-";
            }

            return $"{_currentCharge}/{GetRequiredCharge()}";
        }

        private void GainCharge(int delta)
        {
            if (!IsUnlocked || !IsConfigured || delta <= 0)
            {
                return;
            }

            _currentCharge = Min(GetRequiredCharge(), _currentCharge + delta);
        }

        private int GetRequiredCharge()
        {
            return Max(1, RequiredCharge);
        }

        private static int Clamp(int value, int min, int max)
        {
            return Min(max, Max(min, value));
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
