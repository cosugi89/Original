using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.Presentation
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
        [SerializeField] private GameObject nextHazardBoostObject;

        public void ConfigureLegacyReferences(
            TMP_Text turnText,
            TMP_Text waveText,
            TMP_Text enemyName = null,
            TMP_Text playerHp = null,
            TMP_Text enemyHp = null,
            TMP_Text confirm = null,
            GameObject nextHazardBoost = null)
        {
            turnNumberText = turnText;
            waveNumberText = waveText;
            enemyNameText = enemyName;
            playerHpText = playerHp;
            enemyHpText = enemyHp;
            confirmText = confirm;
            nextHazardBoostObject = nextHazardBoost;
        }

        public void ApplySession(BattleSessionState session)
        {
            if (session == null)
            {
                return;
            }

            if (turnNumberText != null)
            {
                turnNumberText.text = session.TurnNumber.ToString();
            }

            if (waveNumberText != null)
            {
                waveNumberText.text = session.WaveNumber.ToString();
            }

            if (enemyNameText != null)
            {
                enemyNameText.text = session.EnemyName;
            }

            if (playerHpText != null)
            {
                playerHpText.text = $"HP {session.PlayerHp}/{session.InitialPlayerHp}";
            }

            if (enemyHpText != null)
            {
                enemyHpText.text = session.EnemyHp.ToString();
            }

            if (nextHazardBoostObject != null)
            {
                nextHazardBoostObject.SetActive(session.NextTurnHazardBoosted);
            }
        }

        public void SetConfirmText(string message)
        {
            if (confirmText != null)
            {
                confirmText.text = message ?? string.Empty;
            }
        }
    }
}
