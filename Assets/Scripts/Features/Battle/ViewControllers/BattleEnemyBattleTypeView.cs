using Assets.Scripts.Features.Battle.Const;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 敵の戦闘タイプ名とゲージ表示を管理する。
    /// </summary>
    public sealed class BattleEnemyBattleTypeView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TMP_Text battleTypeNameText;
        [SerializeField] private Image battleTypeIcon;
        [SerializeField] private Slider battleTypeGauge;
        [SerializeField] private Image battleTypeGaugeFillImage;
        [SerializeField] private TMP_Text battleTypeValueText;

        public Sprite CurrentIcon => battleTypeIcon.sprite;

        public void SetGauge(
            string battleTypeName,
            float currentValue,
            float maxValue,
            string valueText,
            Color gaugeColor,
            Sprite icon = null)
        {
            rootObject.SetActive(true);
            battleTypeNameText.text = battleTypeName ?? string.Empty;

            if (icon != null)
            {
                battleTypeIcon.sprite = icon;
            }

            battleTypeIcon.enabled = battleTypeIcon.sprite != null;
            battleTypeGauge.minValue = 0f;
            battleTypeGauge.maxValue = Mathf.Max(BattleUiValues.MinimumGaugeMaxValue, maxValue);
            battleTypeGauge.value = Mathf.Clamp(currentValue, 0f, battleTypeGauge.maxValue);
            battleTypeGaugeFillImage.color = gaugeColor;
            battleTypeValueText.text = valueText ?? string.Empty;
        }
    }
}
