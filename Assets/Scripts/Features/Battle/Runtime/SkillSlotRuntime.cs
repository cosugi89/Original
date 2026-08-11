using System;
using System.Collections.Generic;
using System.ComponentModel;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Runtime
{
    public enum SkillState
    {
        Unavailable,
        NotReady,
        Ready,
    }

    /// <summary>
    /// スキルパレット 1 枠ぶんの本番ランタイム状態。
    /// ひとまずデモ版の能力を引き継ぎつつ、将来の HUD / 装備連携の受け皿にする。
    /// </summary>
    [Serializable]
    /// <summary>
    /// 戦闘中スキルスロットの状態、チャージ、参照マスタを保持する。
    /// </summary>
    public class SkillSlotRuntime
    {
        private readonly int _skillId;
        private readonly BattleSkillData _skillData;
        private readonly bool _isUnlocked;

        [Description("現在保持しているチャージ量の内部値。")]
        private int _currentCharge;

        public SkillSlotRuntime(int skillId, BattleSkillData skillData = null, bool isUnlocked = true)
        {
            _skillId = Max(0, skillId);
            _skillData = skillData;
            _isUnlocked = isUnlocked;
        }

        [Description("参照元のスキル定義。SkillId が有効ならマスタから解決した内容を参照する。")]
        public BattleSkillData SkillData => _skillData;

        private bool HasAssignedSkill => _skillId > 0;
        private bool HasResolvedSkill => _skillData != null;

        [Description("UI に表示するスキル名。")]
        public string DisplayName
        {
            get
            {
                if (HasResolvedSkill)
                {
                    return _skillData.DisplayName;
                }

                if (!HasAssignedSkill)
                {
                    return IsUnlocked ? "Empty Slot" : "Locked Slot";
                }

                return "Unknown Skill";
            }
        }

        [Description("スキルの基本説明文。")]
        public string Description => HasResolvedSkill ? _skillData.Description : string.Empty;

        [Description("スキルアイコン。未設定なら既定表示を使う。")]
        public Sprite Icon => _skillData?.Icon;

        [Description("使いどころや条件を補う短い説明文。")]
        public string DescriptionSupplement => BuildDescriptionSupplement();

        [Description("参照元スキルを識別する ID。")]
        public int SkillId => HasResolvedSkill ? _skillData.SkillId : _skillId;

        [Description("参照元スキルを識別する内部キー。")]
        public string SkillKey => _skillData?.SkillKey ?? string.Empty;

        [Description("この枠が解放済みかどうか。未解放なら UNAVAILABLE 扱いになる。")]
        public bool IsUnlocked => _isUnlocked;

        [Description("この枠に実際のスキルが設定されているかどうか。未設定でも使用不可として UNAVAILABLE 扱いになる。")]
        public bool IsConfigured => HasAssignedSkill;

        [Description("READY になるために必要なチャージ量。")]
        public int RequiredCharge => HasResolvedSkill ? Max(1, _skillData.RequiredCharge) : 1;

        [Description("バトル開始時点で保持している初期チャージ量。")]
        public int StartingCharge => HasResolvedSkill ? Clamp(_skillData.StartingCharge, 0, RequiredCharge) : 0;

        [Description("ターン経過ごとに加算されるチャージ量。")]
        public int TurnChargeGain => HasResolvedSkill ? Max(0, _skillData.TurnChargeGain) : 0;

        [Description("Attack 解決時に加算されるチャージ量。")]
        public int AttackChargeGain => HasResolvedSkill ? Max(0, _skillData.AttackChargeGain) : 0;

        [Description("このスキルが持つ効果一覧。")]
        public IReadOnlyList<BattleSkillEffectEntryData> Effects => _skillData?.Effects ?? Array.Empty<BattleSkillEffectEntryData>();

        [Description("スキルの主効果カテゴリ。")]
        public BattleSkillOperationType EffectType => _skillData?.EffectType ?? BattleSkillOperationType.Damage;

        [Description("発動時に再生する演出定義。")]
        public EffectAnimationData EffectAnimation => _skillData?.EffectAnimation;

        [Description("現在保持しているチャージ量。")]
        public int CurrentCharge => _currentCharge;

        [Description("現在のチャージ状態でこのスキルを発動できるかどうか。")]
        public bool IsReady => State == SkillState.Ready;

        public SkillState State
        {
            get
            {
                if (!IsUnlocked || !HasAssignedSkill || !HasResolvedSkill)
                {
                    return SkillState.Unavailable;
                }

                return _currentCharge >= GetRequiredCharge()
                    ? SkillState.Ready
                    : SkillState.NotReady;
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

        public void GainCharge(int amount)
        {
            GainChargeInternal(amount);
        }

        public void Consume()
        {
            _currentCharge = 0;
        }

        public SkillSlotRuntime Clone()
        {
            var clone = new SkillSlotRuntime(_skillId, _skillData, _isUnlocked);
            clone.SetCurrentCharge(_currentCharge);
            return clone;
        }

        public int GetRemainingTurnsUntilReady()
        {
            if (State == SkillState.Ready)
            {
                return 0;
            }

            if (State == SkillState.Unavailable || TurnChargeGain <= 0)
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
                case SkillState.Unavailable:
                    return "UNAVAILABLE";
                case SkillState.Ready:
                    return "READY";
                default:
                    return "CHARGING";
            }
        }

        public string GetChargeLabel()
        {
            if (State == SkillState.Unavailable)
            {
                return "-/-";
            }

            return $"{_currentCharge}/{GetRequiredCharge()}";
        }

        private void GainChargeInternal(int delta)
        {
            if (State == SkillState.Unavailable || delta <= 0)
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

        private string BuildDescriptionSupplement()
        {
            if (!HasResolvedSkill)
            {
                return string.Empty;
            }

            var parts = new List<string>
            {
                $"Charge {RequiredCharge}",
            };

            if (StartingCharge > 0)
            {
                parts.Add($"Start +{StartingCharge}");
            }

            if (TurnChargeGain > 0)
            {
                parts.Add($"Turn +{TurnChargeGain}");
            }

            if (AttackChargeGain > 0)
            {
                parts.Add($"Attack +{AttackChargeGain}");
            }

            return string.Join(" / ", parts);
        }
    }
}

