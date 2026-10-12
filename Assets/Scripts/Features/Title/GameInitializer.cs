using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using UnityEngine;

namespace Assets.Scripts.Features.Title
{
    /// <summary>
    /// 保存データとゲーム全体で共有するサービスを初期化し、Title UIを開始する。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameInitializer : MonoBehaviour
    {
        [SerializeField] private UserDataManager userDataManager;
        [SerializeField] private TitleSceneController titleSceneController;

        private void Awake()
        {
            userDataManager.InitializeGameData();
            titleSceneController.Initialize(userDataManager);
        }
    }
}
