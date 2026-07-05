using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// プレイヤーと敵で共有する戦闘ステータス基底。
    /// </summary>
    public class BattleCombatantStats
    {
        private readonly Dictionary<BattleSkillParameterType, int> _parameterValues;
        private readonly Dictionary<string, float> _namedValues;

        public BattleCombatantStats(
            IReadOnlyDictionary<BattleSkillParameterType, int> parameterValues = null,
            IReadOnlyDictionary<string, float> namedValues = null)
        {
            _parameterValues = parameterValues != null
                ? new Dictionary<BattleSkillParameterType, int>(parameterValues)
                : new Dictionary<BattleSkillParameterType, int>();
            _namedValues = namedValues != null
                ? new Dictionary<string, float>(namedValues, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<BattleSkillParameterType, int> ParameterValues => _parameterValues;

        public IReadOnlyDictionary<string, float> NamedValues => _namedValues;

        public int GetParameterValue(BattleSkillParameterType parameterType, int fallbackValue = 0)
        {
            return _parameterValues.TryGetValue(parameterType, out var value)
                ? Math.Max(0, value)
                : Math.Max(0, fallbackValue);
        }

        public float GetNamedValue(string key, float fallbackValue = 0f)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return fallbackValue;
            }

            return _namedValues.TryGetValue(key, out var value)
                ? value
                : fallbackValue;
        }

        protected void SetParameterValue(BattleSkillParameterType parameterType, int value)
        {
            _parameterValues[parameterType] = Math.Max(0, value);
        }

        protected void SetNamedValue(string key, float value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            _namedValues[key] = value;
        }
    }
}

