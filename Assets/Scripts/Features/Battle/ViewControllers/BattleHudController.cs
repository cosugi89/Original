using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 旧 HUD 参照を受け取りつつ、分割済み View へ表示を橋渡しする互換コントローラ。
    /// </summary>
    public class BattleHudController : MonoBehaviour
    {
        [Header("Structured Views")]
        [SerializeField] private BattleTopHudView topHudView;
        [SerializeField] private EnemyUnitView enemyUnitView;
        [SerializeField] private PlayerAreaView playerAreaView;

        [Header("Legacy HUD")]
        [SerializeField] private TMP_Text turnNumberText;
        [SerializeField] private TMP_Text waveNumberText;
        [SerializeField] private TMP_Text enemyNameText;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private TMP_Text enemyHpText;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private GameObject nextHazardBoostObject;
        [SerializeField] private bool enableLegacyConfirmText;

        public BattleTopHudView TopHudView => topHudView;

        public EnemyUnitView EnemyUnitView => enemyUnitView;

        public PlayerAreaView PlayerAreaView => playerAreaView;

        public TMP_Text LegacyConfirmText => confirmText;

        public GameObject LegacyConfirmRoot => confirmText != null ? confirmText.gameObject : null;

        public void ApplySession(BattleSessionState session)
        {
            if (session == null)
            {
                return;
            }

            topHudView?.ApplySession(session);
            enemyUnitView?.ApplySession(session);
            playerAreaView?.ApplySession(session);

            SetText(turnNumberText, $"Turn {session.TurnNumber}");
            SetText(waveNumberText, $"Wave {session.WaveNumber}");
            SetText(enemyNameText, session.EnemyName);
            SetText(playerHpText, $"HP {session.PlayerHp}/{session.InitialPlayerHp}");
            ApplySlider(playerHpSlider, session.PlayerHp, session.InitialPlayerHp);

            SetText(enemyHpText, $"{session.EnemyHp}/{session.InitialEnemyHp}");
            ApplySlider(enemyHpSlider, session.EnemyHp, session.InitialEnemyHp);

            if (nextHazardBoostObject != null)
            {
                nextHazardBoostObject.SetActive(session.NextTurnHazardBoosted);
            }
        }

        public void SetConfirmText(string message)
        {
            if (confirmText == null)
            {
                return;
            }

            if (!enableLegacyConfirmText)
            {
                return;
            }

            confirmText.text = message ?? string.Empty;
            confirmText.enabled = !string.IsNullOrWhiteSpace(message);
        }

        public void ClearConfirmText()
        {
            if (confirmText == null)
            {
                return;
            }

            confirmText.text = string.Empty;
            if (!enableLegacyConfirmText)
            {
                confirmText.enabled = false;
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
