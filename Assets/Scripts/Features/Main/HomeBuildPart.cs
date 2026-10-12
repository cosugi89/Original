using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// Home構築演出で使う一つのUIパーツ。
    /// 参照先の座標系で完成位置を指定し、石材・小道具は上から落下、葉はその場で表示する。
    /// </summary>
    public sealed class HomeBuildPart : MonoBehaviour
    {
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private RectTransform referenceTransform;
        [FormerlySerializedAs("stoneImage")]
        [SerializeField] private Image partImage;
        [Tooltip("同じ物体としてフェードする追加Image。小道具の可視面を配線し、単一面の石材・葉では空配列にする。")]
        [SerializeField] private Image[] additionalImages = new Image[0];
        [Tooltip("参照RectTransformのローカル座標で指定する完成位置。葉は根元の座標を使う。")]
        [SerializeField] private Vector2 finalOffset;
        [Tooltip("石材の構築順。0: 土台／階段、1: 左右の石材、2: アーチ。前段の着地後に次段を開始する。")]
        [Min(0)]
        [SerializeField] private int buildStage;
        [SerializeField] private float startOffsetY = 72f;
        [SerializeField] private float startScale = 1f;
        [SerializeField] private float overshootScale = 1f;

        private Vector3 _finalLocalPosition;
        private Vector3 _finalLocalScale;
        private float _originalAlpha;
        private float[] _additionalAlphas;
        private bool _fallsFromAbove;

        public int BuildStage => buildStage;

        private void Awake()
        {
            _originalAlpha = partImage.color.a;
            _additionalAlphas = new float[additionalImages.Length];
            for (var i = 0; i < additionalImages.Length; i++)
            {
                _additionalAlphas[i] = additionalImages[i].color.a;
            }
            // Sceneの編集時は完成配置を表示し、Play開始時の完成画面先出しを防ぐ。
            SetAlpha(0f);
        }

        public void Prepare(bool fallFromAbove = true)
        {
            Prepare(referenceTransform, fallFromAbove);
        }

        public void Prepare(RectTransform positionReference, bool fallFromAbove = true)
        {
            gameObject.SetActive(true);
            targetTransform.SetPositionAndRotation(
                positionReference.TransformPoint(finalOffset), positionReference.rotation);
            targetTransform.localScale = new Vector3(
                positionReference.lossyScale.x / targetTransform.parent.lossyScale.x,
                positionReference.lossyScale.y / targetTransform.parent.lossyScale.y,
                positionReference.lossyScale.z / targetTransform.parent.lossyScale.z);
            _finalLocalPosition = targetTransform.localPosition;
            _finalLocalScale = targetTransform.localScale;
            _fallsFromAbove = fallFromAbove;
            targetTransform.localPosition = _finalLocalPosition +
                Vector3.up * (_fallsFromAbove ? startOffsetY : 0f);
            targetTransform.localScale = _finalLocalScale * startScale;
            SetAlpha(0f);
        }

        public IEnumerator Build(float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                // 前半で加速し、最後の短い区間だけ着地を収める。物理演算には依存しない。
                var fallProgress = progress < 0.82f
                    ? Mathf.Pow(progress / 0.82f, 2f) * 0.96f
                    : Mathf.Lerp(0.96f, 1f, 1f - Mathf.Pow(1f - (progress - 0.82f) / 0.18f, 2f));
                targetTransform.localPosition = Vector3.Lerp(
                    _finalLocalPosition + Vector3.up * (_fallsFromAbove ? startOffsetY : 0f),
                    _finalLocalPosition,
                    fallProgress);
                var scaleProgress = progress < 0.8f ? progress / 0.8f : (progress - 0.8f) / 0.2f;
                scaleProgress = Mathf.SmoothStep(0f, 1f, scaleProgress);
                var scaleMultiplier = progress < 0.8f
                    ? Mathf.Lerp(startScale, overshootScale, scaleProgress)
                    : Mathf.Lerp(overshootScale, 1f, scaleProgress);
                targetTransform.localScale = _finalLocalScale * scaleMultiplier;
                SetAlpha(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / (_fallsFromAbove ? 0.55f : 1f))));
                yield return null;
            }

            targetTransform.localPosition = _finalLocalPosition;
            targetTransform.localScale = _finalLocalScale;
            SetAlpha(1f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowCompleted(bool fallFromAbove = true)
        {
            Prepare(fallFromAbove);
            targetTransform.localPosition = _finalLocalPosition;
            targetTransform.localScale = _finalLocalScale;
            SetAlpha(1f);
        }

        private void SetAlpha(float progress)
        {
            var color = partImage.color;
            color.a = _originalAlpha * progress;
            partImage.color = color;
            for (var i = 0; i < additionalImages.Length; i++)
            {
                var additionalColor = additionalImages[i].color;
                additionalColor.a = _additionalAlphas[i] * progress;
                additionalImages[i].color = additionalColor;
            }
        }
    }
}
