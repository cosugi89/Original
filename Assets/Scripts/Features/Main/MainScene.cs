using UnityEngine;
using UnityEngine.Events;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Features.Main;
using Assets.Scripts.UI.Components;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.Save;
using TMPro;
using UnityEngine.Serialization;

/// <summary>
/// MainScene のエントリポイント。
/// - PreviewStage へのプレイヤー生成とアピアランス適用
/// - HomeScreen UI と CharacterScreen UI の初期化
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

    [Header("Home Angel")]
    [SerializeField] private GameObject angelHome;

    [Header("Home Entrance")]
    [SerializeField] private HomeEntranceDirector homeEntranceDirector;

    [Header("Screens")]
    [SerializeField] private HomeScreen homeScreen;
    [FormerlySerializedAs("statusScreen")]
    [SerializeField] private CharacterScreen characterScreen;

    [Header("Points")]
    [SerializeField] private TextMeshProUGUI pointBalanceText;
    [SerializeField] private TextMeshProUGUI abilityPointText;

    [Header("Main Tabs")]
    [FormerlySerializedAs("statusTab")]
    [SerializeField] private PartsTabItem characterTab;
    [FormerlySerializedAs("abilityTab")]
    [SerializeField] private PartsTabItem progressTab;
    [SerializeField] private PartsTabItem homeTab;
    [FormerlySerializedAs("dungeonTab")]
    [SerializeField] private PartsTabItem missionsTab;
    [FormerlySerializedAs("shopTab")]
    [SerializeField] private PartsTabItem armoryTab;

    [FormerlySerializedAs("statusTabScreen")]
    [SerializeField] private GameObject characterTabScreen;
    [FormerlySerializedAs("abilityTabScreen")]
    [SerializeField] private GameObject progressTabScreen;
    [SerializeField] private GameObject homeTabScreen;
    [FormerlySerializedAs("dungeonTabScreen")]
    [SerializeField] private GameObject missionsTabScreen;
    [FormerlySerializedAs("shopTabScreen")]
    [SerializeField] private GameObject armoryTabScreen;

    private AppearanceController _homeCharacterPreview;
    private GameObject _homeCharacterRoot;
    private AppearanceController _characterPreview;
    private GameObject _characterPreviewRoot;
    private UserDataManager _userDataManager;

    private UnityAction _characterTabHandler;
    private UnityAction _progressTabHandler;
    private UnityAction _homeTabHandler;
    private UnityAction _missionsTabHandler;
    private UnityAction _armoryTabHandler;

    private void Awake()
    {
        _characterTabHandler = ShowCharacter;
        _progressTabHandler = ShowProgress;
        _homeTabHandler = ShowHome;
        _missionsTabHandler = ShowMissions;
        _armoryTabHandler = ShowArmory;

        characterTab.Button.onClick.AddListener(_characterTabHandler);
        progressTab.Button.onClick.AddListener(_progressTabHandler);
        homeTab.Button.onClick.AddListener(_homeTabHandler);
        missionsTab.Button.onClick.AddListener(_missionsTabHandler);
        armoryTab.Button.onClick.AddListener(_armoryTabHandler);

        ShowHome();
    }

    private void Start()
    {
        _userDataManager = UserDataManager.EnsureInitialized();
        InitializePreviews();
        InitializeScreens();
        StartHomeEntrance();
        _userDataManager.ResourceBalancesChanged += RefreshPointTexts;
        RefreshPointTexts();
    }

    private void InitializePreviews()
    {
        var avatarRenderService = AppearanceAdapter.EnsureInitialized();

        _homeCharacterPreview = ResolveHomeCharacterPreview();
        ApplyAvatarTo(_homeCharacterPreview, avatarRenderService);
        _homeCharacterRoot = ResolveHomePreviewRoot();

        _characterPreview = ResolveCharacterPreview();
        ApplyAvatarTo(_characterPreview, avatarRenderService);
        _characterPreviewRoot = ResolvePreviewRoot(_characterPreview);

        ExcludeAvatarPreviewLayerFromScreenCameras();
    }

    private void InitializeScreens()
    {
        homeScreen.Init();
        characterScreen.Init(_characterPreview, _homeCharacterPreview);
    }

    private void StartHomeEntrance()
    {
        SetPreviewVisibility(ResolveActivePreviewScreen(), false);
        homeEntranceDirector.StartIfRequested(ShowHomeAngel);
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

        characterTab.Button.onClick.RemoveListener(_characterTabHandler);
        progressTab.Button.onClick.RemoveListener(_progressTabHandler);
        homeTab.Button.onClick.RemoveListener(_homeTabHandler);
        missionsTab.Button.onClick.RemoveListener(_missionsTabHandler);
        armoryTab.Button.onClick.RemoveListener(_armoryTabHandler);
    }

    private void ShowCharacter()
    {
        SelectTab(characterTab, characterTabScreen);
    }

    private void ShowProgress()
    {
        SelectTab(progressTab, progressTabScreen);
    }

    private void ShowHome()
    {
        SelectTab(homeTab, homeTabScreen);
    }

    private void ShowMissions()
    {
        SelectTab(missionsTab, missionsTabScreen);
    }

    private void ShowArmory()
    {
        SelectTab(armoryTab, armoryTabScreen);
    }

    private void SelectTab(PartsTabItem selectedTab, GameObject selectedScreen)
    {
        characterTabScreen.SetActive(selectedScreen == characterTabScreen);
        progressTabScreen.SetActive(selectedScreen == progressTabScreen);
        homeTabScreen.SetActive(selectedScreen == homeTabScreen);
        missionsTabScreen.SetActive(selectedScreen == missionsTabScreen);
        armoryTabScreen.SetActive(selectedScreen == armoryTabScreen);

        characterTab.SetNormalState();
        progressTab.SetNormalState();
        homeTab.SetNormalState();
        missionsTab.SetNormalState();
        armoryTab.SetNormalState();
        selectedTab.SetFocusedState();

        SetPreviewVisibility(
            selectedScreen == homeTabScreen ? homeScreen :
            selectedScreen == characterTabScreen ? characterScreen : null);
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

    private AppearanceController ResolveCharacterPreview()
    {
        var candidate = ResolveSceneCharacterPreview();
        if (candidate != null && ResolvePreviewRoot(candidate) != _homeCharacterRoot)
        {
            return candidate;
        }

        var characterRoot = new GameObject("CharacterPreviewRoot");
        if (playerRoot != null)
        {
            characterRoot.transform.SetPositionAndRotation(playerRoot.position, playerRoot.rotation);
            characterRoot.transform.localScale = playerRoot.lossyScale;
        }

        var characterObject = Instantiate(playerPrefab, characterRoot.transform);
        MatchPreviewLocalTransform(characterObject.transform);
        var characterPreview = characterObject.GetComponent<AppearanceController>();
        characterRoot.SetActive(false);
        return characterPreview;
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
               transform.name == "CharacterPreviewRoot";
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

    private void SetPreviewVisibility(Assets.Scripts.Features.Main.Screen screen, bool showHomeAngel = true)
    {
        SetActiveIfNeeded(_homeCharacterRoot, false);
        SetActiveIfNeeded(angelHome, showHomeAngel && screen == homeScreen);
        SetActiveIfNeeded(_characterPreviewRoot, screen == characterScreen);
    }

    private void ShowHomeAngel()
    {
        SetActiveIfNeeded(angelHome, homeTabScreen.activeSelf);
    }

    private Assets.Scripts.Features.Main.Screen ResolveActivePreviewScreen()
    {
        if (characterScreen.gameObject.activeSelf)
        {
            return characterScreen;
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
