using UnityEngine;
using Assets.Scripts.UI.Dialog;
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
    [Header("Preview")]
    [SerializeField] private PreviewStage previewStage;
    [SerializeField] private PartsManager playerPrefab;

    [Header("Screens")]
    [SerializeField] private HomeScreen homeScreen;

    private PreviewHandle _playerHandle;
    private PartsManager _playerInstance;

    private void Awake()
    {
        BootstrapServices();
    }

    private void Start()
    {
        SpawnPlayerPreview();
        ApplyAvatarAppearance();

        if (homeScreen != null)
        {
            homeScreen.Initialize();
        }
        else
        {
            Debug.LogWarning("MainScene: HomeScreen is not assigned.", this);
        }
    }

    private void OnDestroy()
    {
        if (previewStage != null && _playerHandle != null)
        {
            previewStage.Despawn(_playerHandle);
        }
        _playerHandle = null;
        _playerInstance = null;
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

    private void SpawnPlayerPreview()
    {
        if (previewStage == null)
        {
            Debug.LogWarning("MainScene: PreviewStage is not assigned.", this);
            return;
        }
        if (playerPrefab == null)
        {
            Debug.LogWarning("MainScene: playerPrefab is not assigned.", this);
            return;
        }

        _playerHandle = previewStage.Spawn(playerPrefab);
        _playerInstance = _playerHandle?.Get<PartsManager>();
        _playerInstance?.Init();
    }

    private void ApplyAvatarAppearance()
    {
        if (_playerInstance == null)
            return;

        var avatarRender = AvatarRenderService.EnsureInitialized();
        avatarRender.SyncSessionFromRendererIfNeeded(_playerInstance, saveAfterSync: true);
        avatarRender.ApplyTo(_playerInstance);
    }
}
