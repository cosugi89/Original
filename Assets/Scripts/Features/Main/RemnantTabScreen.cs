using System.Collections.Generic;
using Assets.Scripts.Core.Const;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Features.Main
{
    /// <summary>Resurgence 中のボス結末と救済再挑戦を表示する画面。</summary>
    public sealed class RemnantTabScreen : Screen
    {
        [SerializeField] private PartsRemnantCard cardPrefab;
        [SerializeField] private RectTransform content;

        private readonly List<PartsRemnantCard> _cards = new();

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

            var progresses = UserDataManager.EnsureInitialized().CycleProgress.GetRemnantProgresses();
            for (var i = 0; i < progresses.Count; i++)
            {
                var progress = progresses[i];
                var card = Instantiate(cardPrefab, content);
                card.Show(progress, () => StartRedemptionBattle(progress.Master.StageId));
                _cards.Add(card);
            }
        }

        private void StartRedemptionBattle(int stageId)
        {
            BattleSceneRequest.SelectedStageId = stageId;
            SceneManager.LoadScene(SceneNames.Battle);
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
