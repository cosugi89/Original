using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Stage Master Database", fileName = "StageMasterDatabase")]
    public class StageMasterDatabase : ScriptableObject
    {
        [SerializeField]
        [Tooltip("ステージマスタの一覧。先頭から順に解決される。")]
        private List<StageMasterData> stages = new();

        /// <summary>読み取り専用のステージ一覧。</summary>
        public IReadOnlyList<StageMasterData> Stages => stages;

        /// <summary>
        /// 指定した StageId に一致する StageMasterData を検索する。
        /// </summary>
        public bool TryGetById(int stageId, out StageMasterData stageData)
        {
            if (stages != null)
            {
                for (var i = 0; i < stages.Count; i++)
                {
                    var candidate = stages[i];
                    if (candidate != null && candidate.StageId == stageId)
                    {
                        stageData = candidate;
                        return true;
                    }
                }
            }

            stageData = null;
            return false;
        }
    }
}
