using System;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>解放済み Insight を表示する専用カード。</summary>
    public sealed class PartsInsightsCard : MonoBehaviour
    {
        [SerializeField] private Image bossImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI sourceText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private GameButton actionButton;

        public void Show(BossInsightProgress progress, Action action)
        {
            var master = progress.Master;
            bossImage.sprite = progress.Resolution == BossResolution.Redeemed
                ? master.RedeemedPortrait
                : master.DefeatedPortrait;
            bossImage.enabled = bossImage.sprite != null;
            bossImage.preserveAspect = true;
            titleText.text = master.InsightName;
            sourceText.text = $"From {master.BossName}";
            descriptionText.text = master.InsightDescription;
            newBadge.SetActive(progress.IsNewInsight);

            actionButton.SetVisible(action != null);
            actionButton.ClearAction();
            if (action != null)
            {
                actionButton.SetAction(action);
            }
        }
    }
}
