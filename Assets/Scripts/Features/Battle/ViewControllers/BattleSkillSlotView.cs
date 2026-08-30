using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using Assets.Scripts.Features.Main;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// Parts-SkillItem を使うバトル用スキルスロットの入力と Runtime 表示を扱う。
    /// BattleScene への配線は、対応するバトル用 Prefab の完成後に行う。
    /// </summary>
    public sealed class BattleSkillSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private PartsSkillItem skillItem;
        [SerializeField] private Image chargeFill;

        private UnityAction _clickHandler;

        public SkillSlotRuntime Runtime { get; private set; }

        public void BindClickAction(UnityAction listener)
        {
            UnbindClickAction();
            _clickHandler = listener;
            button.onClick.AddListener(_clickHandler);
        }

        public void UnbindClickAction()
        {
            if (_clickHandler == null)
            {
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
            if (Runtime == null)
            {
                skillItem.ShowEmpty("Empty");
                chargeFill.gameObject.SetActive(false);
                return;
            }

            skillItem.ShowSkill(Runtime.Icon, Runtime.DisplayName);
            var isUnavailable = Runtime.State == SkillState.Unavailable;
            if (isUnavailable)
            {
                skillItem.ShowDisabled();
                chargeFill.gameObject.SetActive(false);
                return;
            }

            chargeFill.gameObject.SetActive(true);
            chargeFill.fillAmount = Runtime.State == SkillState.Ready
                ? 1f
                : Mathf.Clamp01((float)Runtime.CurrentCharge / Mathf.Max(BattleUiValues.MinimumGaugeMaxValue, Runtime.RequiredCharge));
        }

        public void SetSelectedVisual(bool isSelected, Color normalColor, Color selectedColor)
        {
            skillItem.SetFocused(isSelected);

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
    }
}
