using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Skill Master Database", fileName = "BattleSkillMasterDatabase")]
    public class BattleSkillMasterDatabase : ScriptableObject
    {
        [SerializeField]
        [Description("バトルスキルマスタ一覧。ID検索やロードアウト候補一覧に使う。")]
        private List<BattleSkillMasterData> skills = new();

        public List<BattleSkillMasterData> Skills => skills ??= new List<BattleSkillMasterData>();

        private Dictionary<int, BattleSkillMasterData> _byId;

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

            foreach (var skill in Skills)
            {
                if (skill == null || skill.SkillId <= 0)
                {
                    continue;
                }

                if (!_byId.TryAdd(skill.SkillId, skill))
                {
                    Debug.LogWarning($"Duplicate skillId detected in {name}: {skill.SkillId}", this);
                }
            }
        }
    }
}
