using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Scene;
using Assets.Scripts.UI.Dialog;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// MainScene 上に配置されるホーム画面 UI のコントローラ。
    /// BattleScene への遷移と、Home上の補助導線を所管する。
    /// </summary>
    public class HomeScreen : Screen
    {
        [Header("Battle")]
        [SerializeField] private Button battleButton;
        [SerializeField] private bool useDebugBattleStageId = true;
        [SerializeField] private int debugBattleStageId = 0;

        [Header("Home Actions")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button achievementsButton;

        private bool _isInitialized;
        private bool _isHomeDialogOpen;
        private int _battleStageId = -1;

        public void Init()
        {
            if (_isInitialized)
                return;

            battleButton.onClick.RemoveListener(OnClickBattle);
            battleButton.onClick.AddListener(OnClickBattle);
            settingsButton.onClick.RemoveListener(OnClickSettings);
            settingsButton.onClick.AddListener(OnClickSettings);
            achievementsButton.onClick.RemoveListener(OnClickAchievements);
            achievementsButton.onClick.AddListener(OnClickAchievements);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            battleButton.onClick.RemoveListener(OnClickBattle);
            settingsButton.onClick.RemoveListener(OnClickSettings);
            achievementsButton.onClick.RemoveListener(OnClickAchievements);
        }

        private void OnClickBattle()
        {
            if (useDebugBattleStageId)
            {
                _battleStageId = debugBattleStageId;
            }
            else
            {
                _battleStageId = GetNextStageId();
            }

            if (_battleStageId < 0)
            {
                Debug.LogWarning("[HomeScreen] 遷移先の StageId を解決できなかったため、BattleScene へ遷移できません。");
                return;
            }

            BattleSceneRequest.SelectedStageId = _battleStageId;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        private int GetNextStageId()
        {
            return UserDataManager.EnsureInitialized().Progress.GetRecommendedStageId();
        }

        private void OnClickSettings() => OpenHomeDialogAsync(
            "Settings",
            "設定項目はここに表示します。").Forget();

        private void OnClickAchievements() => OpenHomeDialogAsync(
            "Achievements",
            "実績一覧は準備中です。").Forget();

        private async UniTaskVoid OpenHomeDialogAsync(string title, string description)
        {
            if (_isHomeDialogOpen)
                return;

            _isHomeDialogOpen = true;
            try
            {
                await DialogService.OpenAsync<
                    DialogCommonConfirmWithIcon,
                    RequestDialogCommonConfirmWithIcon,
                    ResultDialogCommonConfirmWithIcon>(
                    new RequestDialogCommonConfirmWithIcon(title, -1, description, null, true),
                    new DialogContext(null));
            }
            finally
            {
                _isHomeDialogOpen = false;
            }
        }
    }
}
