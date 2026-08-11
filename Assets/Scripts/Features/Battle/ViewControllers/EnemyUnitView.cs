using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 1 体ぶんの敵名、HP、戦型表示を管理する。
    /// </summary>
    public sealed class EnemyUnitView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TMP_Text enemyNameText;
        [SerializeField] private TMP_Text enemyHpText;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private BattleEnemyBattleTypeView battleTypeView;

        public BattleEnemyBattleTypeView BattleTypeView => battleTypeView;

        public void ApplySession(BattleSessionState session)
        {
            if (session == null)
            {
                return;
            }

            SetRootVisible(true);
            SetText(enemyNameText, session.EnemyName);
            SetText(enemyHpText, $"{session.EnemyHp}/{session.InitialEnemyHp}");
            ApplySlider(enemyHpSlider, session.EnemyHp, session.InitialEnemyHp);
        }

        public void SetBattleType(
            string battleTypeName,
            float currentValue,
            float maxValue,
            string valueText,
            Color gaugeColor,
            Sprite icon = null)
        {
            battleTypeView?.SetGauge(
                battleTypeName,
                currentValue,
                maxValue,
                valueText,
                gaugeColor,
                icon);
        }

        public void ResetView()
        {
            SetRootVisible(false);
            SetText(enemyNameText, string.Empty);
            SetText(enemyHpText, string.Empty);
            ApplySlider(enemyHpSlider, 0, 1);
        }

        private void SetRootVisible(bool visible)
        {
            if (rootObject != null)
            {
                rootObject.SetActive(visible);
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void ApplySlider(Slider slider, int value, int maxValue)
        {
            if (slider == null)
            {
                return;
            }

            var safeMax = Mathf.Max(BattleUiValues.MinimumGaugeMaxValue, maxValue);
            slider.minValue = 0f;
            slider.maxValue = safeMax;
            slider.value = Mathf.Clamp(value, 0, safeMax);
        }
    }
}
