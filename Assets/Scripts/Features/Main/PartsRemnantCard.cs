using System;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>Resurgence 中の人間ボス記録と救済再挑戦を表示する専用カード。</summary>
    public sealed class PartsRemnantCard : MonoBehaviour
    {
        [SerializeField] private Image bossImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameButton actionButton;
        [SerializeField] private RectTransform normalClearState;
        [SerializeField] private RectTransform redeemedClearState;

        public void Show(BossInsightProgress progress, Action seekRedemptionAction)
        {
            var master = progress.Master;
            bossImage.sprite = progress.Resolution == BossResolution.Redeemed
                ? master.RedeemedPortrait
                : master.DefeatedPortrait;
            bossImage.enabled = bossImage.sprite != null;
            bossImage.preserveAspect = true;
            titleText.text = master.BossName;
            descriptionText.text = master.RemnantQuote;
            normalClearState.gameObject.SetActive(progress.Resolution == BossResolution.Defeated);
            redeemedClearState.gameObject.SetActive(progress.Resolution == BossResolution.Redeemed);

            actionButton.SetVisible(progress.CanSeekRedemption);
            actionButton.ClearAction();
            if (progress.CanSeekRedemption)
            {
                actionButton.SetLabel("Seek Redemption");
                actionButton.SetAction(seekRedemptionAction);
            }
        }
    }
}
