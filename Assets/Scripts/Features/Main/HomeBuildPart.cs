using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// Home構築演出で使う一つのUIパーツ。
    /// 最終位置を基準に、少し上から落下しながらフェードインする。
    /// </summary>
    public sealed class HomeBuildPart : MonoBehaviour
    {
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private RectTransform referenceTransform;
        [FormerlySerializedAs("stoneImage")]
        [SerializeField] private Image partImage;
        [SerializeField] private float startOffsetY = 72f;
        [SerializeField] private float startScale = 1f;
        [SerializeField] private float overshootScale = 1f;

        private Vector3 _finalLocalPosition;
        private Vector3 _finalLocalScale;
        private float _originalAlpha;

        public void Prepare()
        {
            Prepare(referenceTransform);
        }

        public void Prepare(RectTransform referenceTransform)
        {
            gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            targetTransform.SetPositionAndRotation(referenceTransform.position, referenceTransform.rotation);
            targetTransform.localScale = new Vector3(
                referenceTransform.lossyScale.x / targetTransform.parent.lossyScale.x,
                referenceTransform.lossyScale.y / targetTransform.parent.lossyScale.y,
                referenceTransform.lossyScale.z / targetTransform.parent.lossyScale.z);
            PrepareAfterReference();
        }

        private void PrepareAfterReference()
        {
            gameObject.SetActive(true);
            _finalLocalPosition = targetTransform.localPosition;
            _finalLocalScale = targetTransform.localScale;
            _originalAlpha = partImage.color.a;
            targetTransform.localPosition = _finalLocalPosition + Vector3.up * startOffsetY;
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
                var easedProgress = 1f - (1f - progress) * (1f - progress);
                targetTransform.localPosition = Vector3.Lerp(
                    _finalLocalPosition + Vector3.up * startOffsetY,
                    _finalLocalPosition,
                    easedProgress);
                var scaleProgress = progress < 0.8f ? progress / 0.8f : (progress - 0.8f) / 0.2f;
                var scaleMultiplier = progress < 0.8f
                    ? Mathf.Lerp(startScale, overshootScale, scaleProgress)
                    : Mathf.Lerp(overshootScale, 1f, scaleProgress);
                targetTransform.localScale = _finalLocalScale * scaleMultiplier;
                SetAlpha(easedProgress);
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

        private void SetAlpha(float progress)
        {
            var color = partImage.color;
            color.a = _originalAlpha * progress;
            partImage.color = color;
        }
    }
}
