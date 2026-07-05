using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 勝敗パネルとリトライ系ボタン群の表示・イベント接続を管理する。
    /// </summary>
    public sealed class BattleResultView : MonoBehaviour
    {
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryText;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TMP_Text defeatText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private Button backToStageButton;

        public void BindActions(
            UnityAction onRetry,
            UnityAction onNextStage,
            UnityAction onBackToStage)
        {
            retryButton.onClick.AddListener(onRetry);
            nextStageButton.onClick.AddListener(onNextStage);
            backToStageButton.onClick.AddListener(onBackToStage);
        }

        public void UnbindActions(
            UnityAction onRetry,
            UnityAction onNextStage,
            UnityAction onBackToStage)
        {
            retryButton.onClick.RemoveListener(onRetry);
            nextStageButton.onClick.RemoveListener(onNextStage);
            backToStageButton.onClick.RemoveListener(onBackToStage);
        }

        public void ResetView()
        {
            victoryPanel.SetActive(false);
            victoryText.text = string.Empty;
            defeatPanel.SetActive(false);
            defeatText.text = string.Empty;
            SetButtonsVisible(false, false);
        }

        public void ShowVictory(string message)
        {
            victoryPanel.SetActive(true);
            defeatPanel.SetActive(false);
            victoryText.text = message ?? string.Empty;
            defeatText.text = string.Empty;
        }

        public void ShowDefeat(string message)
        {
            victoryPanel.SetActive(false);
            defeatPanel.SetActive(true);
            defeatText.text = message ?? string.Empty;
            victoryText.text = string.Empty;
        }

        public void SetButtonsVisible(bool isBattleEnded, bool hasNextStage)
        {
            retryButton.gameObject.SetActive(isBattleEnded);
            nextStageButton.gameObject.SetActive(isBattleEnded && hasNextStage);
            backToStageButton.gameObject.SetActive(isBattleEnded);
        }
    }
}
