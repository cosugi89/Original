using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// プレイヤー側と敵側の履歴、セリフ、補足テキストを表示する。
    /// </summary>
    public sealed class BattleNarrativeView : MonoBehaviour
    {
        [SerializeField] private GameObject playerActionHistoryRoot;
        [SerializeField] private TMP_Text playerActionHistoryText;
        [SerializeField] private GameObject playerSpeechBubbleRoot;
        [SerializeField] private TMP_Text playerSpeechText;
        [SerializeField] private GameObject enemyActionHistoryRoot;
        [SerializeField] private TMP_Text enemyActionHistoryText;
        [SerializeField] private GameObject enemySpeechBubbleRoot;
        [SerializeField] private TMP_Text enemySpeechText;
        [SerializeField] private TMP_Text enemyFlavorText;

        public void ResetView()
        {
            SetActive(playerActionHistoryRoot, false);
            SetText(playerActionHistoryText, string.Empty);
            SetActive(playerSpeechBubbleRoot, false);
            SetText(playerSpeechText, string.Empty);
            SetActive(enemyActionHistoryRoot, false);
            SetText(enemyActionHistoryText, string.Empty);
            SetActive(enemySpeechBubbleRoot, false);
            SetText(enemySpeechText, string.Empty);
            SetText(enemyFlavorText, string.Empty);
        }

        public void SetPlayerNarrative(string history, string speech)
        {
            SetActive(playerActionHistoryRoot, !string.IsNullOrWhiteSpace(history));
            SetText(playerActionHistoryText, history);
            SetActive(playerSpeechBubbleRoot, !string.IsNullOrWhiteSpace(speech));
            SetText(playerSpeechText, speech);
        }

        public void SetEnemyNarrative(string history, string speech, string flavor)
        {
            SetActive(enemyActionHistoryRoot, !string.IsNullOrWhiteSpace(history));
            SetText(enemyActionHistoryText, history);
            SetActive(enemySpeechBubbleRoot, !string.IsNullOrWhiteSpace(speech));
            SetText(enemySpeechText, speech);
            SetText(enemyFlavorText, flavor);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
