using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Feedback Button")]
    public sealed class FeedbackButton : Button
    {
        [Header("Optional Feedback")]
        [SerializeField, Tooltip("任意。クリック時に Trigger を送る Animator。")] private Animator animator;
        [SerializeField, Tooltip("任意。クリック SE の再生に使う AudioSource。")] private AudioSource audioSource;
        [SerializeField, Tooltip("任意。クリック時に再生する SE。")] private AudioClip clickSound;
        [SerializeField, Tooltip("任意。標準Button Animation Transitionとは別に送る Trigger。")] private string clickTrigger;

        private Action _clickAction;

        public void SetAction(Action action)
        {
            _clickAction = action;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            var shouldInvoke = eventData.button == PointerEventData.InputButton.Left && IsActive() && IsInteractable();
            base.OnPointerClick(eventData);

            if (shouldInvoke)
            {
                InvokeClickFeedback();
            }
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            var shouldInvoke = IsActive() && IsInteractable();
            base.OnSubmit(eventData);

            if (shouldInvoke)
            {
                InvokeClickFeedback();
            }
        }

        private void InvokeClickFeedback()
        {
            if (animator != null && !string.IsNullOrWhiteSpace(clickTrigger))
            {
                animator.SetTrigger(clickTrigger);
            }

            if (audioSource != null && clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            _clickAction?.Invoke();
        }
    }
}
