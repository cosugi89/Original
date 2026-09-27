using Assets.Scripts.UI.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// Progress画面内の既存タブを切り替える。
    /// </summary>
    public sealed class ProgressScreen : Screen
    {
        [Header("Tab Items")]
        [SerializeField] private PartsTabItem artifactTab;
        [SerializeField] private PartsTabItem skillTab;
        [SerializeField] private PartsTabItem traitTab;
        [SerializeField] private GameObject artifactScreen;
        [SerializeField] private GameObject skillScreen;
        [SerializeField] private TraitTabScreen traitTabScreen;

        private UnityAction _artifactHandler;
        private UnityAction _skillHandler;
        private UnityAction _traitHandler;

        private void Awake()
        {
            _artifactHandler = ShowArtifact;
            _skillHandler = ShowSkill;
            _traitHandler = ShowTrait;

            artifactTab.Button.onClick.AddListener(_artifactHandler);
            skillTab.Button.onClick.AddListener(_skillHandler);
            traitTab.Button.onClick.AddListener(_traitHandler);

            ShowTrait();
        }

        private void OnDestroy()
        {
            artifactTab.Button.onClick.RemoveListener(_artifactHandler);
            skillTab.Button.onClick.RemoveListener(_skillHandler);
            traitTab.Button.onClick.RemoveListener(_traitHandler);
        }

        private void ShowArtifact()
        {
            Select(artifactTab, artifactScreen);
        }

        private void ShowSkill()
        {
            Select(skillTab, skillScreen);
        }

        private void ShowTrait()
        {
            Select(traitTab, traitTabScreen.gameObject);
        }

        private void Select(PartsTabItem selectedTab, GameObject selectedScreen)
        {
            artifactScreen.SetActive(selectedScreen == artifactScreen);
            skillScreen.SetActive(selectedScreen == skillScreen);
            traitTabScreen.gameObject.SetActive(selectedScreen == traitTabScreen.gameObject);

            artifactTab.SetNormalState();
            skillTab.SetNormalState();
            traitTab.SetNormalState();
            selectedTab.SetFocusedState();
        }
    }
}
