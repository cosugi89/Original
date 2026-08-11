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
            retryButton?.onClick.AddListener(onRetry);
            nextStageButton?.onClick.AddListener(onNextStage);
            backToStageButton?.onClick.AddListener(onBackToStage);
        }

        public void UnbindActions(
            UnityAction onRetry,
            UnityAction onNextStage,
            UnityAction onBackToStage)
        {
            retryButton?.onClick.RemoveListener(onRetry);
            nextStageButton?.onClick.RemoveListener(onNextStage);
            backToStageButton?.onClick.RemoveListener(onBackToStage);
        }

        public void ResetView()
        {
            SetActive(victoryPanel, false);
            SetText(victoryText, string.Empty);
            SetActive(defeatPanel, false);
            SetText(defeatText, string.Empty);
            SetButtonsVisible(false, false);
        }

        public void ShowVictory(string message)
        {
            SetActive(victoryPanel, true);
            SetActive(defeatPanel, false);
            SetText(victoryText, message);
            SetText(defeatText, string.Empty);
        }

        public void ShowDefeat(string message)
        {
            SetActive(victoryPanel, false);
            SetActive(defeatPanel, true);
            SetText(defeatText, message);
            SetText(victoryText, string.Empty);
        }

        public void SetButtonsVisible(bool isBattleEnded, bool hasNextStage)
        {
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(isBattleEnded);
            }

            if (nextStageButton != null)
            {
                nextStageButton.gameObject.SetActive(isBattleEnded && hasNextStage);
            }

            if (backToStageButton != null)
            {
                backToStageButton.gameObject.SetActive(isBattleEnded);
            }
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
