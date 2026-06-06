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
    public class SkillSlotRuntime
    {
        private readonly UserBattleSkillSlotData _slotData;
        private readonly BattleSkillData _skillData;

        [Description("現在保持しているチャージ量の内部値。")]
        private int _currentCharge;

        public SkillSlotRuntime(UserBattleSkillSlotData slotData, BattleSkillData skillData = null)
        {
            _slotData = slotData ?? new UserBattleSkillSlotData();
            _skillData = skillData;
        }

        [Description("参照元のセーブスロット定義。解放状態や未設定状態はこちらを使う。")]
        public UserBattleSkillSlotData SlotData => _slotData;

        [Description("参照元のスキル定義。SkillId が有効ならマスタから解決した内容を参照する。")]
        public BattleSkillData SkillData => _skillData;

        [Description("UI に表示するスキル名。")]
        public string DisplayName => _skillData?.DisplayName ?? _slotData.DisplayName ?? "Skill";

        [Description("スキルの基本説明文。")]
        public string Description => _skillData?.Description ?? _slotData.Description ?? string.Empty;

        [Description("スキルアイコン。未設定なら既定表示を使う。")]
        public Sprite Icon => _skillData?.Icon;

        [Description("使いどころや条件を補う短い説明文。")]
        public string DescriptionSupplement => string.Empty;

        [Description("参照元スキルを識別する ID。")]
        public int SkillId => _skillData != null ? _skillData.SkillId : Max(0, _slotData.SkillId);

        [Description("参照元スキルを識別する内部キー。")]
        public string SkillKey => _skillData?.SkillKey ?? string.Empty;

        [Description("この枠が解放済みかどうか。未解放なら UNAVAILABLE 扱いになる。")]
        public bool IsUnlocked => _slotData.IsUnlocked;

        [Description("この枠に実際のスキルが設定されているかどうか。未設定でも使用不可として UNAVAILABLE 扱いになる。")]
        public bool IsConfigured => _slotData.IsConfigured;

        [Description("READY になるために必要なチャージ量。")]
        public int RequiredCharge => _skillData != null
            ? Max(1, _skillData.RequiredCharge)
            : Max(1, _slotData.RequiredCharge);

        [Description("バトル開始時点で保持している初期チャージ量。")]
        public int StartingCharge => _skillData != null
            ? Clamp(_skillData.StartingCharge, 0, RequiredCharge)
            : Clamp(_slotData.StartingCharge, 0, RequiredCharge);

        [Description("ターン経過ごとに加算されるチャージ量。")]
        public int TurnChargeGain => _skillData != null
            ? Max(0, _skillData.TurnChargeGain)
            : Max(0, _slotData.TurnChargeGain);

        [Description("Attack 解決時に加算されるチャージ量。")]
        public int AttackChargeGain => Max(0, _slotData.AttackChargeGain);

        [Description("スキル発動時に与える基礎ダメージ量。")]
        public int Damage => _skillData != null
            ? Max(0, _skillData.Damage)
            : Max(0, _slotData.Damage);

        [Description("このスキルが持つ属性情報。")]
        public AttributeData Attribute => _skillData?.Attribute;

        [Description("このスキルが持つ属性一覧。")]
        public IReadOnlyList<AttributeData> Attributes => _skillData?.Attributes ?? Array.Empty<AttributeData>();

        [Description("このスキルが持つ効果一覧。")]
        public IReadOnlyList<BattleSkillEffectEntryData> Effects => _skillData?.Effects ?? Array.Empty<BattleSkillEffectEntryData>();

        [Description("スキルの主効果カテゴリ。")]
        public BattleSkillEffectType EffectType => _skillData?.EffectType ?? BattleSkillEffectType.Damage;

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
                if (!IsUnlocked || !IsConfigured)
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

        public void Consume()
        {
            _currentCharge = 0;
        }

        public SkillSlotRuntime Clone()
        {
            var clone = new SkillSlotRuntime(_slotData, _skillData);
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
