using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using UnityEngine;

namespace Assets.Scripts.Features.Title
{
    /// <summary>
    /// 現在は MainScene、将来は TitleScene に置くゲーム起動処理。
    /// 保存データとゲーム全体で共有するサービスを、画面初期化より先に準備する。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameInitializer : MonoBehaviour
    {
        [SerializeField] private UserDataManager userDataManager;

        private void Awake()
        {
            userDataManager.InitializeGameData();
        }
    }
}
