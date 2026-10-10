using System.Collections.Generic;
using Assets.Scripts.Systems.Save;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    /// <summary>解放済み Insight の一覧を構築する画面。</summary>
    public sealed class InsightsTabScreen : Screen
    {
        [SerializeField] private PartsInsightsCard cardPrefab;
        [SerializeField] private RectTransform content;

        private readonly List<PartsInsightsCard> _cards = new();

        private void OnEnable()
        {
            RefreshCards();
        }

        private void OnDisable()
        {
            ClearCards();
        }

        private void RefreshCards()
        {
            ClearCards();

            var cycleProgress = UserDataManager.EnsureInitialized().CycleProgress;
            var progresses = cycleProgress.GetInsightProgresses();
            for (var i = 0; i < progresses.Count; i++)
            {
                var card = Instantiate(cardPrefab, content);
                card.Show(progresses[i], null);
                _cards.Add(card);
            }

            cycleProgress.MarkInsightsViewed();
        }

        private void ClearCards()
        {
            for (var i = 0; i < _cards.Count; i++)
            {
                Destroy(_cards[i].gameObject);
            }

            _cards.Clear();
        }
    }
}
