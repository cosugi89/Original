using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ricimi
{
    /// <summary>
    /// Fundamental button class used throughout the demo.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(AudioSource))]
    public class CleanButton : Button
    {
        #region Custom Inspector
        [SerializeField] private float fadeTime = 0.3f;
        [SerializeField] private float onHoverAlpha = 0.6f;
        [SerializeField] private float onClickAlpha = 0.4f;

        public float FadeTime => fadeTime;
        public float OnHoverAlpha => onHoverAlpha;
        public float OnClickAlpha => onClickAlpha;
        #endregion

        private CanvasGroup canvasGroup;

        protected override void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            StopAllCoroutines();
            StartCoroutine(Utils.FadeOut(canvasGroup, onHoverAlpha, fadeTime));
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            StopAllCoroutines();
            StartCoroutine(Utils.FadeIn(canvasGroup, 1.0f, fadeTime));
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            canvasGroup.alpha = onClickAlpha;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            canvasGroup.alpha = 1.0f;
        }
    }
}