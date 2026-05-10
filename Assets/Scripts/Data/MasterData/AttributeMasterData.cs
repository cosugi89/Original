using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Attribute Master Data", fileName = "AttributeMasterData")]
    public class AttributeMasterData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("属性を一意に識別するIDです。1以上の一意な値を入れます。")]
        [Description("属性定義を一意に識別するID。")]
        private int attributeId = 0;

        [SerializeField]
        [Tooltip("UIやInspectorで表示する属性名です。")]
        [Description("属性の表示名。")]
        private string displayName = "Attribute";

        [SerializeField]
        [TextArea]
        [Tooltip("属性の説明文です。")]
        [Description("属性の説明文。")]
        private string description = string.Empty;

        [SerializeField]
        [Tooltip("属性を視覚的に識別する代表色です。ダメージ装飾などで使います。")]
        [Description("属性の代表色。")]
        private Color accentColor = Color.white;

        [SerializeField]
        [Tooltip("属性アイコンです。")]
        [Description("属性のアイコン。")]
        private Sprite icon;

        [SerializeField]
        [Tooltip("属性一覧の並び順です。小さいほど前に出します。")]
        [Description("属性一覧の並び順。")]
        private int sortOrder = 0;

        public int AttributeId => attributeId;
        public string DisplayName => displayName;
        public string Description => description;
        public Color AccentColor => accentColor;
        public Sprite Icon => icon;
        public int SortOrder => sortOrder;

        private void OnValidate()
        {
            attributeId = Mathf.Max(0, attributeId);
            sortOrder = Mathf.Max(0, sortOrder);
        }
    }
}
