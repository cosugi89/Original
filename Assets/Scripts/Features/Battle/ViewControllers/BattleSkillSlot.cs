using System;
using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    [Serializable]
    /// <summary>
    /// 1 つのスキルスロット表示とクリック反応、チャージ表示更新を扱う。
    /// </summary>
    public class BattleSkillSlot
    {
        [SerializeField] private Button button;
        [SerializeField] private Image image;
        [SerializeField] private Image filled;
        [SerializeField] private GameObject unavailable;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text chargeText;

        public SkillSlotRuntime Runtime { get; private set; }

        private UnityAction _clickHandler;
        private bool _defaultSpritesCached;
        private Sprite _defaultImageSprite;
        private Sprite _defaultFilledSprite;

        public void BindClickAction(UnityAction listener)
        {
            UnbindClickAction();
            if (listener == null)
            {
                return;
            }

            _clickHandler = listener;
            button.onClick.AddListener(_clickHandler);
        }

        public void UnbindClickAction()
        {
            if (button == null || _clickHandler == null)
            {
                _clickHandler = null;
                return;
            }

            button.onClick.RemoveListener(_clickHandler);
            _clickHandler = null;
        }

        public void SetRuntime(SkillSlotRuntime runtime)
        {
            Runtime = runtime;
            RefreshRuntime();
        }

        public void RefreshRuntime()
        {
            CacheDefaultSpritesIfNeeded();

            if (Runtime == null)
            {
                ApplyRuntimeIcon(null);
                SetSlotText("Empty", "EMPTY", "-/-");
                SetUnavailable();
                return;
            }

            ApplyRuntimeIcon(Runtime.Icon);
            SetSlotText(Runtime.DisplayName, BuildStateLabel(Runtime), Runtime.GetChargeLabel());

            switch (Runtime.State)
            {
                case SkillState.Unavailable:
                    SetUnavailable();
                    return;
                case SkillState.Ready:
                    SetAvailable();
                    if (filled != null)
                    {
                        filled.fillAmount = 1f;
                    }
                    return;
                default:
                    SetAvailable();
                    if (filled != null)
                    {
                        filled.fillAmount = Mathf.Clamp01((float)Runtime.CurrentCharge / Mathf.Max(BattleUiValues.MinimumGaugeMaxValue, Runtime.RequiredCharge));
                    }
                    return;
            }
        }

        public void SetSelectedVisual(bool isSelected, Color normalColor, Color selectedColor)
        {
            if (button == null)
            {
                return;
            }

            var targetColor = isSelected ? selectedColor : normalColor;

            var colors = button.colors;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.selectedColor = targetColor;
            colors.pressedColor = targetColor * BattleUiValues.PressedButtonColorMultiplier;
            button.colors = colors;

            if (button.targetGraphic != null)
            {
                button.targetGraphic.color = targetColor;
            }
        }

        private void SetUnavailable()
        {
            if (image != null)
            {
                image.enabled = false;
            }

            if (filled != null)
            {
                filled.enabled = false;
                filled.fillAmount = 0f;
            }

            if (unavailable != null)
            {
                unavailable.SetActive(true);
            }
        }

        private void SetAvailable()
        {
            if (image != null)
            {
                image.enabled = true;
            }

            if (filled != null)
            {
                filled.enabled = true;
            }

            if (unavailable != null)
            {
                unavailable.SetActive(false);
            }
        }

        private void CacheDefaultSpritesIfNeeded()
        {
            if (_defaultSpritesCached)
            {
                return;
            }

            _defaultImageSprite = image != null ? image.sprite : null;
            _defaultFilledSprite = filled != null ? filled.sprite : null;
            _defaultSpritesCached = true;
        }

        private void ApplyRuntimeIcon(Sprite icon)
        {
            if (image != null)
            {
                image.sprite = icon != null ? icon : _defaultImageSprite;
            }

            if (filled != null)
            {
                filled.sprite = icon != null ? icon : _defaultFilledSprite;
            }
        }

        private void SetSlotText(string name, string state, string charge)
        {
            SetText(nameText, name);
            SetText(stateText, state);
            SetText(chargeText, charge);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static string BuildStateLabel(SkillSlotRuntime runtime)
        {
            if (runtime == null)
            {
                return "EMPTY";
            }

            if (!runtime.IsUnlocked)
            {
                return "LOCKED";
            }

            if (!runtime.IsConfigured)
            {
                return "EMPTY";
            }

            return runtime.GetStateLabel();
        }
    }
}
