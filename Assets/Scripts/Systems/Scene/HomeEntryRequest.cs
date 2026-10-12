using System;

namespace Assets.Scripts.Systems.Scene
{
    /// <summary>
    /// TitleScene から MainScene へ渡す、一度だけ消費される Home 開始要求。
    /// 通常の MainScene 読み込みは要求を作らず、即時表示のままにする。
    /// </summary>
    public static class HomeEntryRequest
    {
        private static Action<string> _setLoadingStatus;
        private static Action _releaseTitleBackground;
        private static Action _finishLoading;
        private static bool _isBuildRequested;

        public static void RequestBuild(
            Action<string> setLoadingStatus,
            Action releaseTitleBackground,
            Action finishLoading)
        {
            _setLoadingStatus = setLoadingStatus;
            _releaseTitleBackground = releaseTitleBackground;
            _finishLoading = finishLoading;
            _isBuildRequested = true;
        }

        public static bool TryConsumeBuild(
            out Action<string> setLoadingStatus,
            out Action releaseTitleBackground,
            out Action finishLoading)
        {
            if (!_isBuildRequested)
            {
                setLoadingStatus = null;
                releaseTitleBackground = null;
                finishLoading = null;
                return false;
            }

            setLoadingStatus = _setLoadingStatus;
            releaseTitleBackground = _releaseTitleBackground;
            finishLoading = _finishLoading;
            _setLoadingStatus = null;
            _releaseTitleBackground = null;
            _finishLoading = null;
            _isBuildRequested = false;
            return true;
        }
    }
}
