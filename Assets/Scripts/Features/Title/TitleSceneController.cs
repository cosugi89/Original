using System;
using System.Collections;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Scene;
using Assets.Scripts.Systems.Save;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Title
{
    /// <summary>
    /// TitleScene の開始入力と、MainScene 読み込み中の表示を制御する。
    /// Title背景とLoading Canvasは遷移中だけ個別にDontDestroyOnLoadで保持する。
    /// </summary>
    public sealed class TitleSceneController : MonoBehaviour
    {
        [SerializeField] private Button beginButton;
        [SerializeField] private Button resurgenceButton;
        [SerializeField] private Button connectPlayGamesButton;
        [Tooltip("PGSプラグイン導入後、手動認証を開始するコンポーネントをここへ配線する。")]
        [SerializeField] private UnityEvent connectPlayGamesRequested;
        [SerializeField] private Canvas titleBackgroundCanvas;
        [SerializeField] private CanvasGroup titleCanvasGroup;
        [SerializeField] private Canvas loadingCanvas;
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private RectTransform loadingImageTransform;
        [SerializeField] private float loadingFadeDuration = 0.2f;
        [SerializeField] private float loadingRotationInterval = 0.12f;

        private bool _isStarting;
        private bool _isInitialized;
        private float _loadingRotationElapsed;

        private void OnValidate()
        {
            loadingRotationInterval = Mathf.Max(loadingRotationInterval, 0.01f);
        }

        public void Initialize(UserDataManager userDataManager)
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
            beginButton.onClick.AddListener(BeginHome);
            resurgenceButton.onClick.AddListener(BeginHome);
            connectPlayGamesButton.onClick.AddListener(ConnectPlayGames);

            var hasTheRemnant = userDataManager.Inventory.GetItemCount(InventoryItemIds.TheRemnant) > 0;
            beginButton.gameObject.SetActive(!hasTheRemnant);
            resurgenceButton.gameObject.SetActive(hasTheRemnant);
            SetPlayGamesAuthenticated(false);
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
            {
                return;
            }

            RemoveButtonListeners();
        }

        private void RemoveButtonListeners()
        {
            beginButton.onClick.RemoveListener(BeginHome);
            resurgenceButton.onClick.RemoveListener(BeginHome);
            connectPlayGamesButton.onClick.RemoveListener(ConnectPlayGames);
        }

        private void Update()
        {
            if (!_isStarting)
            {
                return;
            }

            _loadingRotationElapsed += Time.unscaledDeltaTime;
            while (_loadingRotationElapsed >= loadingRotationInterval)
            {
                _loadingRotationElapsed -= loadingRotationInterval;
                loadingImageTransform.Rotate(0f, 0f, -72f);
            }
        }

        public void BeginHome()
        {
            if (_isStarting)
            {
                return;
            }

            _isStarting = true;
            StartCoroutine(BeginHomeRoutine());
        }

        /// <summary>
        /// PGS認証完了時に呼ぶ。認証済みなら手動接続Buttonを隠す。
        /// </summary>
        public void SetPlayGamesAuthenticated(bool isAuthenticated)
        {
            connectPlayGamesButton.gameObject.SetActive(!isAuthenticated && !_isStarting);
        }

        private void ConnectPlayGames()
        {
            if (_isStarting)
            {
                return;
            }

            connectPlayGamesRequested.Invoke();
        }

        private IEnumerator BeginHomeRoutine()
        {
            beginButton.interactable = false;
            resurgenceButton.interactable = false;
            connectPlayGamesButton.interactable = false;
            RemoveButtonListeners();
            _isInitialized = false;

            // Title UIは開始直後に役目を終える。背景は別Canvasで維持するため、
            // UIだけをまとめて消しても遷移中の背景は途切れない。
            titleCanvasGroup.gameObject.SetActive(false);

            // 通常時は3つのCanvasを同じMain Cameraで描画する。Title UIを消した後だけ、
            // 持ち越す背景とLoadingをOverlayにしてScene切替後のCameraから独立させる。
            PreparePersistentCanvas(titleBackgroundCanvas, 0);
            PreparePersistentCanvas(loadingCanvas, 1);
            loadingCanvasGroup.interactable = true;
            loadingCanvasGroup.blocksRaycasts = true;
            yield return FadeCanvasGroup(loadingCanvasGroup, 1f, loadingFadeDuration);

            // Title Scene Controller は専用Rootとして保持する。
            // Title背景とLoading Canvasも個別に保持する。
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(titleBackgroundCanvas.gameObject);
            DontDestroyOnLoad(loadingCanvasGroup.transform.root.gameObject);
            HomeEntryRequest.RequestBuild(null, ReleaseTitleBackground, FinishHomeEntryRoutine);

            var operation = SceneManager.LoadSceneAsync(SceneNames.Main, LoadSceneMode.Single);
            while (!operation.isDone)
            {
                yield return null;
            }
        }

        private void ReleaseTitleBackground()
        {
            Destroy(titleBackgroundCanvas.gameObject);
        }

        private IEnumerator FinishHomeEntryRoutine()
        {
            yield return FadeCanvasGroup(loadingCanvasGroup, 0f, loadingFadeDuration);
            _isInitialized = false;
            Destroy(loadingCanvasGroup.transform.root.gameObject);
            Destroy(gameObject);
        }

        private static void PreparePersistentCanvas(Canvas canvas, int sortingOrder)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        private static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            var startAlpha = canvasGroup.alpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }
    }
}
