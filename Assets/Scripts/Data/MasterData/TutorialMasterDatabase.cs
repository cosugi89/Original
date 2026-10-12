using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "The Red Fever/Master Data/Tutorial Database", fileName = "TutorialMasterDatabase")]
    public sealed class TutorialMasterDatabase : ScriptableObject
    {
        [SerializeField] private List<TutorialMasterData> tutorials = new();
        public IReadOnlyList<TutorialMasterData> Tutorials => tutorials;
    }
}
