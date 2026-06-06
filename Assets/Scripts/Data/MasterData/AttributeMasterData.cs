using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// スキルの属性データ。属性の種別を定義するのみで種別による効果は定義しない。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Attribute Master Data", fileName = "AttributeMasterData")]
    public class AttributeMasterData : ScriptableObject
    {
        [SerializeField]
        [Description("属性ID")]
        private int attributeId = 0;

        [SerializeField]
        [Description("内部処理用キー")]
        private string attributeKey = "attribute";

        [SerializeField]
        [Description("UI表示用の属性名")]
        private string displayName = "Attribute";

        [SerializeField]
        [Description("説明")]
        private string description = string.Empty;

        [SerializeField]
        [Description("UIに表示する属性か")]
        private bool isVisibleInUI = true;

        [SerializeField]
        [Description("並び順")]
        private int sortOrder = 0;

        [SerializeField]
        [Description("アイコン")]
        private Sprite icon;

        public int AttributeId => attributeId;
        public string AttributeKey => attributeKey ?? string.Empty;
        public string DisplayName => displayName ?? "Attribute";
        public string Description => description ?? string.Empty;
        public bool IsVisibleInUI => isVisibleInUI;
        public Sprite Icon => icon;
        public int SortOrder => sortOrder;

        private void OnValidate()
        {
            attributeId = Mathf.Max(0, attributeId);
            attributeKey = (attributeKey ?? string.Empty).Trim();
            sortOrder = Mathf.Max(0, sortOrder);
        }
    }
}
