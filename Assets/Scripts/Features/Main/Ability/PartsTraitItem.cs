using System;
using Assets.Scripts.Core.Const;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    public class PartsTraitItem : MonoBehaviour
    {
        public enum VisualState
        {
            Normal,
            Focused,
            Locked,
            Disabled
        }

        public readonly struct VisualStyle
        {
            public VisualStyle(
                Sprite icon,
                int currentLevel,
                int maxLevel,
                VisualState state)
            {
                Icon = icon;
                MaxLevel = Mathf.Max(0, maxLevel);
                CurrentLevel = Mathf.Clamp(currentLevel, 0, MaxLevel);
                State = state;
            }

            public Sprite Icon { get; }
            public int CurrentLevel { get; }
            public int MaxLevel { get; }
            public VisualState State { get; }
            public bool IsMaxLevel => CurrentLevel >= MaxLevel;
            public string LevelText => IsMaxLevel ? "MAX" : $"{CurrentLevel}/{MaxLevel}";
        }

        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image iconImage;

        [Header("State Objects")]
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject focusedState;
        [SerializeField] private GameObject disabledState;

        private Action _clickAction;

        private void Awake()
        {
            button.onClick.AddListener(InvokeClickAction);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(InvokeClickAction);
        }

        public void Setup(VisualStyle style)
        {
            iconImage.sprite = style.Icon;
            iconImage.enabled = style.Icon != null;
            iconImage.preserveAspect = true;
            text.text = style.LevelText;
            SetState(style.State);
        }

        /// <summary>表示状態にかかわらずItem選択時に呼ぶ処理を設定する。</summary>
        public void SetClickAction(Action clickAction)
        {
            _clickAction = clickAction;
        }

        private void InvokeClickAction()
        {
            _clickAction?.Invoke();
        }

        private void SetState(VisualState state)
        {
            var usesDisabledVisual = state == VisualState.Locked || state == VisualState.Disabled;
            text.gameObject.SetActive(!usesDisabledVisual);
            normalState.SetActive(state == VisualState.Normal);
            focusedState.SetActive(state == VisualState.Focused);
            disabledState.SetActive(usesDisabledVisual);
            lockIcon.SetActive(state == VisualState.Locked);
            iconImage.color = usesDisabledVisual ? UIColors.TraitTree.DisableIconColor : UIColors.White;
        }
    }
}
