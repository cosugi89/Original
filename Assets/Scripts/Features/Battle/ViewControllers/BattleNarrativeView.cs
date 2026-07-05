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
            playerActionHistoryRoot.SetActive(false);
            playerActionHistoryText.text = string.Empty;
            playerSpeechBubbleRoot.SetActive(false);
            playerSpeechText.text = string.Empty;
            enemyActionHistoryRoot.SetActive(false);
            enemyActionHistoryText.text = string.Empty;
            enemySpeechBubbleRoot.SetActive(false);
            enemySpeechText.text = string.Empty;
            enemyFlavorText.text = string.Empty;
        }

        public void SetPlayerNarrative(string history, string speech)
        {
            playerActionHistoryRoot.SetActive(!string.IsNullOrWhiteSpace(history));
            playerActionHistoryText.text = history ?? string.Empty;
            playerSpeechBubbleRoot.SetActive(!string.IsNullOrWhiteSpace(speech));
            playerSpeechText.text = speech ?? string.Empty;
        }

        public void SetEnemyNarrative(string history, string speech, string flavor)
        {
            enemyActionHistoryRoot.SetActive(!string.IsNullOrWhiteSpace(history));
            enemyActionHistoryText.text = history ?? string.Empty;
            enemySpeechBubbleRoot.SetActive(!string.IsNullOrWhiteSpace(speech));
            enemySpeechText.text = speech ?? string.Empty;
            enemyFlavorText.text = flavor ?? string.Empty;
        }
    }
}
