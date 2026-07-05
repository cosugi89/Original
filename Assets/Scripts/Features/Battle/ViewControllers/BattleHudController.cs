using Assets.Scripts.Features.Battle.Const;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// HUD の基本表示をまとめる軽量コントローラ。
    /// </summary>
    public class BattleHudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnNumberText;
        [SerializeField] private TMP_Text waveNumberText;
        [SerializeField] private TMP_Text enemyNameText;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private TMP_Text enemyHpText;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private GameObject nextHazardBoostObject;

        public void ApplySession(BattleSessionState session)
        {
            if (session == null)
            {
                return;
            }

            turnNumberText.text = $"Turn {session.TurnNumber}";
            waveNumberText.text = $"Wave {session.WaveNumber}";
            enemyNameText.text = session.EnemyName;
            playerHpText.text = $"HP {session.PlayerHp}/{session.InitialPlayerHp}";

            playerHpSlider.minValue = 0f;
            playerHpSlider.maxValue = Mathf.Max(BattleUiValues.MinimumGaugeMaxValue, session.InitialPlayerHp);
            playerHpSlider.value = Mathf.Clamp(session.PlayerHp, 0, session.InitialPlayerHp);

            enemyHpText.text = $"{session.EnemyHp}/{session.InitialEnemyHp}";

            enemyHpSlider.minValue = 0f;
            enemyHpSlider.maxValue = Mathf.Max(BattleUiValues.MinimumGaugeMaxValue, session.InitialEnemyHp);
            enemyHpSlider.value = Mathf.Clamp(session.EnemyHp, 0, session.InitialEnemyHp);

            nextHazardBoostObject.SetActive(session.NextTurnHazardBoosted);
        }

        public void SetConfirmText(string message)
        {
            confirmText.text = message ?? string.Empty;
        }
    }
}
