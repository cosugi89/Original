using UnityEngine;

namespace Assets.Scripts.UI
{
    public interface IDialogBackgroundHandler
    {
        Transform CachedTransform { get; }
        bool UseBackground { get; }
        bool CloseOnBackgroundClick { get; }
        void OnBackgroundClickedFromManager();
    }
}
