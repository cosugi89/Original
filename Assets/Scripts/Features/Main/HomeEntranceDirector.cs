using System;
using System.Collections;
using Assets.Scripts.Systems.Scene;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// TitleSceneから入った時だけ、MainScene上の実パーツを組み上げてHomeを表示する。
    /// 通常のMainScene表示でも同じ実パーツを完成状態で表示する。
    /// </summary>
    public sealed class HomeEntranceDirector : MonoBehaviour
    {
        [SerializeField] private CanvasGroup homeHudCanvasGroup;
        [Tooltip("Home HUD Rootの外にある通貨表示と下部TabのCanvasGroup。Home HUDと同じタイミングで表示する。")]
        [SerializeField] private CanvasGroup[] additionalHudCanvasGroups;
        [Tooltip("Title背景を解除する前に必ず表示する、Home専用の基底背景。")]
        [SerializeField] private GameObject homeBackground;
        [Tooltip("位置参照として残す旧祭壇Image。表示には使わず、通常起動時も非表示にする。")]
        [SerializeField] private GameObject homeStaticAltar;
        [SerializeField] private HomeBuildPart[] buildParts;
        [SerializeField] private HomeBuildPart[] foliageParts;
        [SerializeField] private HomeBuildPart[] propParts;
        [SerializeField] private HomeBuildPart[] propFoliageParts;
        [Tooltip("演出用小道具自身のButton。構築完了後もこのButtonを操作する。")]
        [SerializeField] private Button[] propButtons;
        [FormerlySerializedAs("interactivePropObjects")]
        [Tooltip("位置参照として残す旧小道具。通常起動時も非表示のままにする。")]
        [SerializeField] private GameObject[] legacyPropObjects;
        [Min(0.01f)]
        [SerializeField] private float partDuration = 0.3f;
        [Min(0f)]
        [SerializeField] private float partStagger = 0.055f;
        [SerializeField] private float foliageDelay = 0.06f;
        [Min(0.01f)]
        [SerializeField] private float foliageDuration = 0.22f;
        [Min(0f)]
        [SerializeField] private float foliageStagger = 0.02f;
        [Min(0.01f)]
        [SerializeField] private float propDuration = 0.34f;
        [Min(0f)]
        [SerializeField] private float propStagger = 0.1f;
        [Min(0f)]
        [SerializeField] private float assemblyHoldDuration = 0.1f;
        [SerializeField] private float homeHudFadeDuration = 0.2f;

        private Action<string> _setLoadingStatus;
        private Action _releaseTitleBackground;
        private Action _finishLoading;
        /// <summary>
        /// TitleSceneからの入場要求があれば構築演出を始め、なければ同じ実パーツを即時表示する。
        /// AngelなどHome固有の表示物はMainSceneが所有し、構築完了時だけcallbackで通知する。
        /// </summary>
        public bool StartIfRequested(Action showHomeContents)
        {
            var shouldPlay = HomeEntryRequest.TryConsumeBuild(
                out _setLoadingStatus,
                out _releaseTitleBackground,
                out _finishLoading);
            homeBackground.SetActive(true);
            homeStaticAltar.SetActive(false);
            // 旧小道具の配置を確定してから非表示にする。inactiveのRectTransformも参照座標に使える。
            Canvas.ForceUpdateCanvases();
            for (var i = 0; i < legacyPropObjects.Length; i++)
            {
                legacyPropObjects[i].SetActive(false);
            }

            for (var i = 0; i < propButtons.Length; i++)
            {
                propButtons[i].interactable = false;
            }

            if (!shouldPlay)
            {
                for (var i = 0; i < buildParts.Length; i++)
                {
                    buildParts[i].ShowCompleted();
                }

                for (var i = 0; i < foliageParts.Length; i++)
                {
                    foliageParts[i].ShowCompleted(false);
                }

                for (var i = 0; i < propParts.Length; i++)
                {
                    propParts[i].ShowCompleted();
                }

                for (var i = 0; i < propFoliageParts.Length; i++)
                {
                    propFoliageParts[i].ShowCompleted(false);
                }

                SetHudVisibility(1f, true);
                for (var i = 0; i < propButtons.Length; i++)
                {
                    propButtons[i].interactable = true;
                }

                return false;
            }

            SetHudVisibility(0f, false);
            _releaseTitleBackground?.Invoke();
            for (var i = 0; i < buildParts.Length; i++)
            {
                buildParts[i].Prepare();
            }

            for (var i = 0; i < foliageParts.Length; i++)
            {
                foliageParts[i].Prepare(false);
            }

            for (var i = 0; i < propParts.Length; i++)
            {
                propParts[i].Prepare();
            }

            for (var i = 0; i < propFoliageParts.Length; i++)
            {
                propFoliageParts[i].Prepare(false);
            }

            StartCoroutine(PlayRoutine(showHomeContents));
            return true;
        }

        private IEnumerator PlayRoutine(Action showHomeContents)
        {
            _setLoadingStatus?.Invoke("Preparing...");

            Coroutine lastBuild = null;
            for (var i = 0; i < buildParts.Length; i++)
            {
                if (i > 0 && buildParts[i].BuildStage != buildParts[i - 1].BuildStage)
                {
                    yield return lastBuild;
                }

                lastBuild = StartCoroutine(buildParts[i].Build(partDuration));
                if (i + 1 < buildParts.Length && buildParts[i + 1].BuildStage == buildParts[i].BuildStage)
                {
                    yield return new WaitForSecondsRealtime(partStagger);
                }
            }

            if (buildParts.Length > 0)
            {
                yield return lastBuild;
            }

            yield return new WaitForSecondsRealtime(foliageDelay);
            for (var i = 0; i < foliageParts.Length; i++)
            {
                lastBuild = StartCoroutine(foliageParts[i].Build(foliageDuration));
                if (i + 1 < foliageParts.Length)
                {
                    yield return new WaitForSecondsRealtime(foliageStagger);
                }
            }

            if (foliageParts.Length > 0)
            {
                yield return lastBuild;
            }

            for (var i = 0; i < propParts.Length; i++)
            {
                lastBuild = StartCoroutine(propParts[i].Build(propDuration));
                if (i + 1 < propParts.Length)
                {
                    yield return new WaitForSecondsRealtime(propStagger);
                }
            }

            if (propParts.Length > 0)
            {
                yield return lastBuild;
            }

            for (var i = 0; i < propFoliageParts.Length; i++)
            {
                // 小道具自身を参照する葉も、着地済みの位置を基準に根元を固定する。
                propFoliageParts[i].Prepare(false);
                lastBuild = StartCoroutine(propFoliageParts[i].Build(foliageDuration));
                if (i + 1 < propFoliageParts.Length)
                {
                    yield return new WaitForSecondsRealtime(foliageStagger);
                }
            }

            if (propFoliageParts.Length > 0)
            {
                yield return lastBuild;
            }

            yield return new WaitForSecondsRealtime(assemblyHoldDuration);

            showHomeContents?.Invoke();
            _finishLoading?.Invoke();

            var elapsed = 0f;
            while (elapsed < homeHudFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetHudVisibility(Mathf.Clamp01(elapsed / homeHudFadeDuration), false);
                yield return null;
            }

            SetHudVisibility(1f, true);
            for (var i = 0; i < propButtons.Length; i++)
            {
                propButtons[i].interactable = true;
            }
        }

        private void SetHudVisibility(float alpha, bool interaction)
        {
            homeHudCanvasGroup.alpha = alpha;
            homeHudCanvasGroup.interactable = interaction;
            homeHudCanvasGroup.blocksRaycasts = interaction;
            for (var i = 0; i < additionalHudCanvasGroups.Length; i++)
            {
                additionalHudCanvasGroups[i].alpha = alpha;
                additionalHudCanvasGroups[i].interactable = interaction;
                additionalHudCanvasGroups[i].blocksRaycasts = interaction;
            }
        }
    }
}
