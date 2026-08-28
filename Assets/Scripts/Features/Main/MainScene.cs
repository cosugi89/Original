using UnityEngine;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Features.Main;
using Assets.Scripts.Core;

/// <summary>
/// MainScene のエントリポイント。
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
    [SerializeField] private AppearanceController sceneCharacterPreview;

    [Header("Screens")]
    [SerializeField] private HomeScreen homeScreen;
    [SerializeField] private StatusScreen statusScreen;

    private AppearanceController _homeCharacterPreview;
    private GameObject _homeCharacterRoot;
    private AppearanceController _statusCharacterPreview;
    private GameObject _statusCharacterRoot;


    private void Start()
    {
        var avatarRenderService = AppearanceAdapter.EnsureInitialized();

        _homeCharacterPreview = ResolveHomeCharacterPreview();
        ApplyAvatarTo(_homeCharacterPreview, avatarRenderService);
        _homeCharacterRoot = ResolveHomePreviewRoot();

        _statusCharacterPreview = ResolveStatusCharacterPreview();
        ApplyAvatarTo(_statusCharacterPreview, avatarRenderService);
        _statusCharacterRoot = ResolvePreviewRoot(_statusCharacterPreview);

        ExcludeAvatarPreviewLayerFromScreenCameras();
        homeScreen.Init();
        statusScreen.Init(_statusCharacterPreview, _homeCharacterPreview);
        SetPreviewVisibility(ResolveActivePreviewScreen());
    }

    public void ChangeScreen(Assets.Scripts.Features.Main.Screen screen)
    {
        SetPreviewVisibility(screen);
    }

    private AppearanceController ResolveSceneCharacterPreview()
    {
        if (sceneCharacterPreview != null)
        {
            return sceneCharacterPreview;
        }

        var partsManagers = FindObjectsByType<AppearanceController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < partsManagers.Length; i++)
        {
            var candidate = partsManagers[i];
            if (candidate != null &&
                candidate.gameObject.name == "Character" &&
                candidate.transform.parent != null &&
                candidate.transform.parent.name == "GameObject")
            {
                return candidate;
            }
        }

        return null;
    }

    private AppearanceController ResolveHomeCharacterPreview()
    {
        var existing = playerRoot != null ? playerRoot.GetComponentInChildren<AppearanceController>(true) : null;
        if (existing != null)
        {
            return existing;
        }

        var playerObject = playerRoot != null
            ? Instantiate(playerPrefab, playerRoot)
            : Instantiate(playerPrefab);
        return playerObject.GetComponent<AppearanceController>();
    }

    private AppearanceController ResolveStatusCharacterPreview()
    {
        var candidate = ResolveSceneCharacterPreview();
        if (candidate != null && ResolvePreviewRoot(candidate) != _homeCharacterRoot)
        {
            return candidate;
        }

        var statusRoot = new GameObject("StatusCharacterPreviewRoot");
        if (playerRoot != null)
        {
            statusRoot.transform.SetPositionAndRotation(playerRoot.position, playerRoot.rotation);
            statusRoot.transform.localScale = playerRoot.lossyScale;
        }

        var statusObject = Instantiate(playerPrefab, statusRoot.transform);
        MatchPreviewLocalTransform(statusObject.transform);
        var statusPreview = statusObject.GetComponent<AppearanceController>();
        statusRoot.SetActive(false);
        return statusPreview;
    }

    private void MatchPreviewLocalTransform(Transform previewTransform)
    {
        if (previewTransform == null || _homeCharacterPreview == null)
        {
            return;
        }

        previewTransform.localPosition = _homeCharacterPreview.transform.localPosition;
        previewTransform.localRotation = _homeCharacterPreview.transform.localRotation;
        previewTransform.localScale = _homeCharacterPreview.transform.localScale;
    }

    private static void ApplyAvatarTo(AppearanceController partsManager, AppearanceAdapter avatarRenderService)
    {
        if (partsManager == null)
        {
            return;
        }

        partsManager.Init();
        avatarRenderService.ApplyTo(partsManager);
    }

    private static GameObject ResolvePreviewRoot(AppearanceController partsManager)
    {
        if (partsManager == null)
        {
            return null;
        }

        if (partsManager.transform.parent != null &&
            IsPreviewRoot(partsManager.transform.parent))
        {
            return partsManager.transform.parent.gameObject;
        }

        return partsManager.gameObject;
    }

    private static bool IsPreviewRoot(Transform transform)
    {
        return transform.name == "GameObject" ||
               transform.name == "StatusCharacterPreviewRoot";
    }

    private static void ExcludeAvatarPreviewLayerFromScreenCameras()
    {
        var avatarPreviewLayer = LayerMask.NameToLayer("Avatar Preview");
        if (avatarPreviewLayer < 0)
        {
            return;
        }

        var excludedMask = ~(1 << avatarPreviewLayer);
        var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < cameras.Length; i++)
        {
            var camera = cameras[i];
            if (camera != null && camera.targetTexture == null)
            {
                camera.cullingMask &= excludedMask;
            }
        }
    }

    private void SetPreviewVisibility(Assets.Scripts.Features.Main.Screen screen)
    {
        SetActiveIfNeeded(_homeCharacterRoot, screen == homeScreen);
        SetActiveIfNeeded(_statusCharacterRoot, screen == statusScreen);
    }

    private Assets.Scripts.Features.Main.Screen ResolveActivePreviewScreen()
    {
        if (statusScreen.gameObject.activeSelf)
        {
            return statusScreen;
        }

        if (homeScreen.gameObject.activeSelf)
        {
            return homeScreen;
        }

        return null;
    }

    private GameObject ResolveHomePreviewRoot()
    {
        if (playerRoot != null)
        {
            return playerRoot.gameObject;
        }

        return _homeCharacterPreview != null ? _homeCharacterPreview.gameObject : null;
    }

    private static void SetActiveIfNeeded(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }
}
