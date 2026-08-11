using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// プレイヤー側の HP と状態表示を管理する。
    /// </summary>
    public sealed class PlayerAreaView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private Slider playerHpSlider;

        public void ApplySession(BattleSessionState session)
        {
            if (session == null)
            {
                return;
            }

            SetRootVisible(true);
            SetText(playerHpText, $"HP {session.PlayerHp}/{session.InitialPlayerHp}");
            ApplySlider(playerHpSlider, session.PlayerHp, session.InitialPlayerHp);
        }

        public void ResetView()
        {
            SetRootVisible(false);
            SetText(playerHpText, string.Empty);
            ApplySlider(playerHpSlider, 0, 1);
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
