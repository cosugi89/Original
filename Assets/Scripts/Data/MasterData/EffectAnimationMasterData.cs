using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// スキル発動で再生されるエフェクトデータ
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Effect Animation Master Data", fileName = "EffectAnimationMasterData")]
    public class EffectAnimationMasterData : ScriptableObject
    {
        [SerializeField][Description("エフェクトID")]
        private int effectAnimationId = 0;

        [SerializeField][Description("内部処理用キー")]
        private string effectAnimationKey = "effect_animation";

        [SerializeField][Description("UI表示用のエフェクト名")]
        private string displayName = "Effect Animation";

        [SerializeField][Description("説明")]
        private string description = string.Empty;

        [SerializeField][Description("キャラクター本体に再生する animation 名")]
        private string characterAnimationName = string.Empty;

        [SerializeField][Description("将来の VFX 参照に使う外部キー")]
        private string visualEffectKey = string.Empty;

        [SerializeField][Description("将来の SE 参照に使う外部キー")]
        private string soundEffectKey = string.Empty;

        [SerializeField][Description("演出完了待ちの目安秒数")]
        private float waitSeconds = 0.4f;

        public int EffectAnimationId => effectAnimationId;
        public string EffectAnimationKey => effectAnimationKey ?? string.Empty;
        public string DisplayName => displayName ?? "Effect Animation";
        public string Description => description ?? string.Empty;
        public string CharacterAnimationName => characterAnimationName ?? string.Empty;
        public string VisualEffectKey => visualEffectKey ?? string.Empty;
        public string SoundEffectKey => soundEffectKey ?? string.Empty;
        public float WaitSeconds => waitSeconds;

        private void OnValidate()
        {
            effectAnimationId = Mathf.Max(0, effectAnimationId);
            effectAnimationKey = (effectAnimationKey ?? string.Empty).Trim();
            waitSeconds = Mathf.Max(0f, waitSeconds);
        }
    }
}
