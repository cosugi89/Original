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
            Special,
            Disabled
        }

        public readonly struct VisualStyle
        {
            public VisualStyle(
                Sprite icon,
                int currentLevel,
                int maxLevel,
                VisualState state,
                bool isAlertVisible = false,
                bool isUpgradeMarkerVisible = false)
            {
                Icon = icon;
                MaxLevel = Mathf.Max(0, maxLevel);
                CurrentLevel = Mathf.Clamp(currentLevel, 0, MaxLevel);
                State = state;
                IsAlertVisible = isAlertVisible;
                IsUpgradeMarkerVisible = isUpgradeMarkerVisible;
            }

            public Sprite Icon { get; }
            public int CurrentLevel { get; }
            public int MaxLevel { get; }
            public VisualState State { get; }
            public bool IsAlertVisible { get; }
            public bool IsUpgradeMarkerVisible { get; }
            public bool IsMaxLevel => CurrentLevel >= MaxLevel;
            public string LevelText => IsMaxLevel ? "MAX" : $"{CurrentLevel}/{MaxLevel}";
        }

        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image iconImage;

        [Header("State Objects")]
        [SerializeField] private GameObject alertIcon;
        [SerializeField] private GameObject upIcon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject focusedState;
        [SerializeField] private GameObject specialState;
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
            alertIcon.SetActive(style.IsAlertVisible);
            upIcon.SetActive(style.IsUpgradeMarkerVisible);
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
            text.gameObject.SetActive(state != VisualState.Disabled);
            normalState.SetActive(state == VisualState.Normal);
            focusedState.SetActive(state == VisualState.Focused);
            specialState.SetActive(state == VisualState.Special);
            disabledState.SetActive(state == VisualState.Disabled);
            lockIcon.SetActive(state == VisualState.Disabled);
            iconImage.color = state == VisualState.Disabled ? UIColors.TraitTree.DisableIconColor : UIColors.White;
        }
    }
}
