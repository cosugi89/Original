using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Effect Animation Master Data", fileName = "EffectAnimationMasterData")]
    public class EffectAnimationMasterData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("演出を一意に識別するIDです。BattleSkillMasterData などから直接参照される前提ですが、将来の保存や解析でも使えるように 1 以上の一意値を入れます。")]
        [Description("演出定義を一意に識別するID。")]
        private int effectAnimationId = 0;

        [SerializeField]
        [Tooltip("Inspector や将来の演出選択UIで見せる表示名です。")]
        [Description("演出定義の表示名。")]
        private string displayName = "Effect Animation";

        [SerializeField]
        [TextArea]
        [Tooltip("この演出が何をするかの補足説明です。")]
        [Description("演出定義の説明文。")]
        private string description = string.Empty;

        [SerializeField]
        [Tooltip("キャラクター本体に再生させる animation 名です。PartsManager 側の clip 名と揃えます。")]
        [Description("キャラクター本体に再生する animation 名。")]
        private string characterAnimationName = string.Empty;

        [SerializeField]
        [Tooltip("将来の VFX / エフェクト asset 解決に使う外部キーです。未使用なら空で構いません。")]
        [Description("将来の VFX 参照に使う外部キー。")]
        private string visualEffectKey = string.Empty;

        [SerializeField]
        [Tooltip("将来の SE / ボイス参照に使う外部キーです。未使用なら空で構いません。")]
        [Description("将来の SE 参照に使う外部キー。")]
        private string soundEffectKey = string.Empty;

        [SerializeField]
        [Tooltip("演出完了待ちの基準秒数です。animation 長が取れない場合の補助にも使えます。")]
        [Description("演出完了待ちの目安秒数。")]
        private float waitSeconds = 0.4f;

        public int EffectAnimationId => effectAnimationId;
        public string DisplayName => displayName;
        public string Description => description;
        public string CharacterAnimationName => characterAnimationName;
        public string VisualEffectKey => visualEffectKey;
        public string SoundEffectKey => soundEffectKey;
        public float WaitSeconds => waitSeconds;

        private void OnValidate()
        {
            effectAnimationId = Mathf.Max(0, effectAnimationId);
            waitSeconds = Mathf.Max(0f, waitSeconds);
        }
    }
}
