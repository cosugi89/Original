using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Attribute Master Database", fileName = "AttributeDatabase")]
    public class AttributeMasterDatabase : ScriptableObject
    {
        [SerializeField]
        [Tooltip("属性マスタ一覧。ID検索や一覧表示に使う。")]
        private List<AttributeMasterData> attributes = new();

        public IReadOnlyList<AttributeMasterData> Attributes => attributes ??= new List<AttributeMasterData>();

        public bool TryGetById(int attributeId, out AttributeMasterData attributeData)
        {
            if (attributes != null)
            {
                for (var i = 0; i < attributes.Count; i++)
                {
                    var candidate = attributes[i];
                    if (candidate != null && candidate.AttributeId == attributeId)
                    {
                        attributeData = candidate;
                        return true;
                    }
                }
            }

            attributeData = null;
            return false;
        }
    }
}
