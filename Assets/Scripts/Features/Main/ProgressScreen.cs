using Assets.Scripts.UI.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// Progress画面内のTraits、Insights、Remnantタブを切り替える。
    /// </summary>
    public sealed class ProgressScreen : Screen
    {
        [Header("Tab Items")]
        [SerializeField] private PartsTabItem traitsTab;
        [SerializeField] private PartsTabItem insightsTab;
        [SerializeField] private PartsTabItem remnantTab;
        [SerializeField] private TraitTabScreen traitTabScreen;
        [SerializeField] private InsightsTabScreen insightsTabScreen;
        [SerializeField] private RemnantTabScreen remnantTabScreen;

        private UnityAction _traitHandler;
        private UnityAction _insightsHandler;
        private UnityAction _remnantHandler;

        private void Awake()
        {
            _traitHandler = ShowTraits;
            _insightsHandler = ShowInsights;
            _remnantHandler = ShowRemnant;

            traitsTab.Button.onClick.AddListener(_traitHandler);
            insightsTab.Button.onClick.AddListener(_insightsHandler);
            remnantTab.Button.onClick.AddListener(_remnantHandler);
        }

        private void OnEnable()
        {
            remnantTab.gameObject.SetActive(Assets.Scripts.Systems.Save.UserDataManager.EnsureInitialized().CycleProgress.IsResurgence);
            ShowTraits();
        }

        private void OnDestroy()
        {
            traitsTab.Button.onClick.RemoveListener(_traitHandler);
            insightsTab.Button.onClick.RemoveListener(_insightsHandler);
            remnantTab.Button.onClick.RemoveListener(_remnantHandler);
        }

        private void ShowInsights()
        {
            Select(insightsTab, insightsTabScreen.gameObject);
        }

        private void ShowRemnant()
        {
            Select(remnantTab, remnantTabScreen.gameObject);
        }

        private void ShowTraits()
        {
            Select(traitsTab, traitTabScreen.gameObject);
        }

        private void Select(PartsTabItem selectedTab, GameObject selectedScreen)
        {
            insightsTabScreen.gameObject.SetActive(selectedScreen == insightsTabScreen.gameObject);
            remnantTabScreen.gameObject.SetActive(selectedScreen == remnantTabScreen.gameObject);
            traitTabScreen.gameObject.SetActive(selectedScreen == traitTabScreen.gameObject);

            traitsTab.SetNormalState();
            insightsTab.SetNormalState();
            remnantTab.SetNormalState();
            selectedTab.SetFocusedState();
        }
    }
}
