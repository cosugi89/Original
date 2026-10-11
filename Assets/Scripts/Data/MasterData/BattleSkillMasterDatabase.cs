using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Battle Skill Master Database", fileName = "BattleSkillMasterDatabase")]
    public class BattleSkillMasterDatabase : ScriptableObject
    {
        [SerializeField]
        [Description("バトルスキルマスタ一覧。ID検索やロードアウト候補一覧に使う。")]
        private List<BattleSkillMasterData> skills = new();

        public List<BattleSkillMasterData> Skills => skills ??= new List<BattleSkillMasterData>();

        private Dictionary<int, BattleSkillMasterData> _byId;
        private Dictionary<string, BattleSkillMasterData> _byKey;

        private void OnEnable()
        {
            RebuildLookups();
        }

        private void OnValidate()
        {
            RebuildLookups();
        }

        public bool TryGetById(int skillId, out BattleSkillMasterData definition)
        {
            EnsureLookups();
            return _byId.TryGetValue(skillId, out definition);
        }

        public bool TryGetByKey(string skillKey, out BattleSkillMasterData definition)
        {
            EnsureLookups();

            if (string.IsNullOrWhiteSpace(skillKey))
            {
                definition = null;
                return false;
            }

            return _byKey.TryGetValue(skillKey.Trim(), out definition);
        }

        private void EnsureLookups()
        {
            if (_byId == null)
            {
                RebuildLookups();
            }
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<int, BattleSkillMasterData>();
            _byKey = new Dictionary<string, BattleSkillMasterData>(StringComparer.Ordinal);

            foreach (var skill in Skills)
            {
                if (skill == null)
                {
                    continue;
                }

                if (skill.SkillId > 0 && !_byId.TryAdd(skill.SkillId, skill))
                {
                    Debug.LogWarning($"Duplicate skillId detected in {name}: {skill.SkillId}", this);
                }

                if (!string.IsNullOrWhiteSpace(skill.SkillKey) &&
                    !_byKey.TryAdd(skill.SkillKey, skill))
                {
                    Debug.LogWarning($"Duplicate skillKey detected in {name}: {skill.SkillKey}", this);
                }
            }
        }
    }
}
