using System;
using System.Collections;
using Assets.Scripts.Systems.Scene;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// TitleSceneから入った時だけ、MainScene上の実パーツを組み上げてHomeを表示する。
    /// 通常のMainScene表示では何もしない。
    /// </summary>
    public sealed class HomeEntranceDirector : MonoBehaviour
    {
        [SerializeField] private CanvasGroup homeHudCanvasGroup;
        [Tooltip("Title背景を解除する前に必ず表示する、Home専用の基底背景。")]
        [SerializeField] private GameObject homeBackground;
        [Tooltip("通常表示時だけ使う、未分割の祭壇Image。Titleからの構築時は分割パーツへ置き換える。")]
        [SerializeField] private GameObject homeStaticAltar;
        [SerializeField] private HomeBuildPart[] buildParts;
        [SerializeField] private HomeBuildPart[] foliageParts;
        [SerializeField] private HomeBuildPart[] propParts;
        [SerializeField] private float partDuration = 0.24f;
        [SerializeField] private float partStagger = 0.05f;
        [SerializeField] private float foliageDelay = 0.06f;
        [SerializeField] private float homeHudFadeDuration = 0.2f;

        private Action<string> _setLoadingStatus;
        private Action _releaseTitleBackground;
        private Action _finishLoading;
        /// <summary>
        /// TitleSceneからの入場要求がある時だけ、Homeの構築演出を始める。
        /// AngelなどHome固有の表示物はMainSceneが所有し、構築完了時だけcallbackで通知する。
        /// </summary>
        public bool StartIfRequested(Action showHomeContents)
        {
            var shouldPlay = HomeEntryRequest.TryConsumeBuild(
                out _setLoadingStatus,
                out _releaseTitleBackground,
                out _finishLoading);
            if (!shouldPlay)
            {
                return false;
            }

            homeHudCanvasGroup.alpha = 0f;
            homeHudCanvasGroup.interactable = false;
            homeHudCanvasGroup.blocksRaycasts = false;
            homeBackground.SetActive(true);
            homeStaticAltar.SetActive(false);
            _releaseTitleBackground?.Invoke();
            for (var i = 0; i < buildParts.Length; i++)
            {
                buildParts[i].Prepare();
            }

            for (var i = 0; i < foliageParts.Length; i++)
            {
                foliageParts[i].Prepare();
            }

            for (var i = 0; i < propParts.Length; i++)
            {
                propParts[i].Prepare();
            }

            StartCoroutine(PlayRoutine(showHomeContents));
            return true;
        }

        private IEnumerator PlayRoutine(Action showHomeContents)
        {
            _setLoadingStatus?.Invoke("Preparing...");

            for (var i = 0; i < buildParts.Length; i++)
            {
                StartCoroutine(buildParts[i].Build(partDuration));
                yield return new WaitForSecondsRealtime(partStagger);
            }

            yield return new WaitForSecondsRealtime(partDuration);

            yield return new WaitForSecondsRealtime(foliageDelay);
            for (var i = 0; i < foliageParts.Length; i++)
            {
                StartCoroutine(foliageParts[i].Build(partDuration));
                yield return new WaitForSecondsRealtime(partStagger);
            }

            if (foliageParts.Length > 0)
            {
                yield return new WaitForSecondsRealtime(partDuration);
            }

            for (var i = 0; i < propParts.Length; i++)
            {
                StartCoroutine(propParts[i].Build(partDuration));
                yield return new WaitForSecondsRealtime(partStagger);
            }

            if (propParts.Length > 0)
            {
                yield return new WaitForSecondsRealtime(partDuration);
                for (var i = 0; i < propParts.Length; i++)
                {
                    propParts[i].Hide();
                }
            }

            showHomeContents?.Invoke();
            _finishLoading?.Invoke();

            var elapsed = 0f;
            while (elapsed < homeHudFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                homeHudCanvasGroup.alpha = Mathf.Clamp01(elapsed / homeHudFadeDuration);
                yield return null;
            }

            homeHudCanvasGroup.alpha = 1f;
            homeHudCanvasGroup.interactable = true;
            homeHudCanvasGroup.blocksRaycasts = true;
        }
    }
}
