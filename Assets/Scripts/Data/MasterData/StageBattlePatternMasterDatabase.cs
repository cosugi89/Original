using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Pattern Database", fileName = "PatternDatabase")]
    public class StageBattlePatternMasterDatabase : ScriptableObject
    {
        [SerializeField]
        [Tooltip("battle pattern マスタ一覧。PatternId で検索する。")]
        private List<StageBattlePatternMasterData> patterns = new();

        public IReadOnlyList<StageBattlePatternMasterData> Patterns => patterns;

        public bool TryGetById(int patternId, out StageBattlePatternMasterData patternData)
        {
            if (patterns != null)
            {
                for (var i = 0; i < patterns.Count; i++)
                {
                    var candidate = patterns[i];
                    if (candidate != null && candidate.PatternId == patternId)
                    {
                        patternData = candidate;
                        return true;
                    }
                }
            }

            patternData = null;
            return false;
        }
    }
}
