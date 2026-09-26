using UnityEngine;
using UnityEngine.Events;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Features.Main;
using Assets.Scripts.UI.Components;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.Save;
using TMPro;

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

    [Header("Points")]
    [SerializeField] private TextMeshProUGUI pointBalanceText;
    [SerializeField] private TextMeshProUGUI abilityPointText;

    [Header("Main Tabs")]
    [SerializeField] private PartsTabItem statusTab;
    [SerializeField] private PartsTabItem abilityTab;
    [SerializeField] private PartsTabItem homeTab;
    [SerializeField] private PartsTabItem dungeonTab;
    [SerializeField] private PartsTabItem shopTab;

    [SerializeField] private GameObject statusTabScreen;
    [SerializeField] private GameObject abilityTabScreen;
    [SerializeField] private GameObject homeTabScreen;
    [SerializeField] private GameObject dungeonTabScreen;
    [SerializeField] private GameObject shopTabScreen;

    private AppearanceController _homeCharacterPreview;
    private GameObject _homeCharacterRoot;
    private AppearanceController _statusCharacterPreview;
    private GameObject _statusCharacterRoot;
    private UserDataManager _userDataManager;

    private UnityAction _statusTabHandler;
    private UnityAction _abilityTabHandler;
    private UnityAction _homeTabHandler;
    private UnityAction _dungeonTabHandler;
    private UnityAction _shopTabHandler;

    private void Awake()
    {
        _statusTabHandler = ShowStatus;
        _abilityTabHandler = ShowAbility;
        _homeTabHandler = ShowHome;
        _dungeonTabHandler = ShowDungeon;
        _shopTabHandler = ShowShop;

        statusTab.Button.onClick.AddListener(_statusTabHandler);
        abilityTab.Button.onClick.AddListener(_abilityTabHandler);
        homeTab.Button.onClick.AddListener(_homeTabHandler);
        dungeonTab.Button.onClick.AddListener(_dungeonTabHandler);
        shopTab.Button.onClick.AddListener(_shopTabHandler);

        ShowHome();
    }

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
        _userDataManager = UserDataManager.EnsureInitialized();
        _userDataManager.ResourceBalancesChanged += RefreshPointTexts;
        RefreshPointTexts();
    }

    /// <summary>
    /// Resource BarのPoint表示を保存済み残高で更新する。
    /// Pointを変更したControllerは、処理完了後にこのメソッドを呼んで表示を同期する。
    /// </summary>
    public void RefreshPointTexts()
    {
        var inventory = UserDataManager.EnsureInitialized().Inventory;
        var gold = inventory.GetItemCount(InventoryItemIds.Gold);
        var abilityPointBalance = inventory.GetItemCount(InventoryItemIds.AbilityPoint);
        if (pointBalanceText != null)
        {
            pointBalanceText.text = gold.ToString();
        }

        if (abilityPointText != null)
        {
            abilityPointText.text = abilityPointBalance.ToString();
        }
    }

    private void OnDestroy()
    {
        if (_userDataManager != null)
        {
            _userDataManager.ResourceBalancesChanged -= RefreshPointTexts;
        }

        statusTab.Button.onClick.RemoveListener(_statusTabHandler);
        abilityTab.Button.onClick.RemoveListener(_abilityTabHandler);
        homeTab.Button.onClick.RemoveListener(_homeTabHandler);
        dungeonTab.Button.onClick.RemoveListener(_dungeonTabHandler);
        shopTab.Button.onClick.RemoveListener(_shopTabHandler);
    }

    private void ShowStatus()
    {
        SelectTab(statusTab, statusTabScreen);
    }

    private void ShowAbility()
    {
        SelectTab(abilityTab, abilityTabScreen);
    }

    private void ShowHome()
    {
        SelectTab(homeTab, homeTabScreen);
    }

    private void ShowDungeon()
    {
        SelectTab(dungeonTab, dungeonTabScreen);
    }

    private void ShowShop()
    {
        SelectTab(shopTab, shopTabScreen);
    }

    private void SelectTab(PartsTabItem selectedTab, GameObject selectedScreen)
    {
        statusTabScreen.SetActive(selectedScreen == statusTabScreen);
        abilityTabScreen.SetActive(selectedScreen == abilityTabScreen);
        homeTabScreen.SetActive(selectedScreen == homeTabScreen);
        dungeonTabScreen.SetActive(selectedScreen == dungeonTabScreen);
        shopTabScreen.SetActive(selectedScreen == shopTabScreen);

        statusTab.SetNormalState();
        abilityTab.SetNormalState();
        homeTab.SetNormalState();
        dungeonTab.SetNormalState();
        shopTab.SetNormalState();
        selectedTab.SetFocusedState();

        SetPreviewVisibility(
            selectedScreen == homeTabScreen ? homeScreen :
            selectedScreen == statusTabScreen ? statusScreen : null);
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
