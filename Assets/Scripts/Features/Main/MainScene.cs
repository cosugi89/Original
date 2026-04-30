using UnityEngine;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Features.Main;
using Assets.Scripts.Core;

/// <summary>
/// MainScene のエントリポイント。
/// - セーブ / アバター系サービスのブートストラップ
/// - PreviewStage へのプレイヤー生成とアピアランス適用
/// - HomeScreen UI の初期化
///
/// 注意: MainScene.unity の MonoBehaviour バインディングが
/// "Assembly-CSharp::MainScene" (名前空間なし) を指しているため、
/// この型は意図的にトップレベル名前空間に置いている。
/// 既存シーンの参照を壊さないよう、名前空間を追加しないこと。
/// </summary>
public class MainScene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerRoot;

    [Header("Screens")]
    [SerializeField] private HomeScreen homeScreen;

    private PartsManager _playerInstance;


    private void Awake()
    {
        BootstrapServices();
    }

    private void Start()
    {
        var playerObject = Instantiate(playerPrefab, playerRoot);
        _playerInstance = playerObject.GetComponent<PartsManager>();
        _playerInstance.Init();

        homeScreen.Init();
    }

    public void ChangeScreen(Assets.Scripts.Features.Main.Screen screen)
    {
        // MainTabsの変更ロジックで呼ぶ
        // ScreenごとにPlayerのTransformを変える
    }

    /// <summary>
    /// 静的サービスの初期化。各 EnsureInitialized() は冪等なので
    /// ここで一度呼んでおくだけで後続のアクセスが安全になる。
    /// </summary>
    private static void BootstrapServices()
    {
        GameSaveService.EnsureInitialized();
        AvatarService.EnsureInitialized();
        InventoryService.EnsureInitialized();
        AvatarRenderService.EnsureInitialized();
    }
}
