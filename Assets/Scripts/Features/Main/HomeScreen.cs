using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// MainScene 上に配置されるホーム画面 UI のコントローラ。
    /// 現状は BattleScene への遷移ボタンのみを所管する。
    /// 将来的にショップ / メニュー等が増えた場合もこのクラスにボタンを追加していく想定。
    /// </summary>
    public class HomeScreen : Screen
    {
        [Header("Battle")]
        [SerializeField] private Button battleButton;
        [SerializeField] private bool useDebugBattleStageId = true;
        [SerializeField] private int debugBattleStageId = 0;

        private bool _isInitialized;
        private int _battleStageId = -1;

        public void Init()
        {
            if (_isInitialized)
                return;

            battleButton.onClick.RemoveListener(OnClickBattle);
            battleButton.onClick.AddListener(OnClickBattle);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            battleButton.onClick.RemoveListener(OnClickBattle);
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

            BattleSceneTransitionState.SelectedStageId = _battleStageId;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        private int GetNextStageId()
        {
            return BattleProgressService.EnsureInitialized().GetRecommendedStageId();
        }
    }
}
