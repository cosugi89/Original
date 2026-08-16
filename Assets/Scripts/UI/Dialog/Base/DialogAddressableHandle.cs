using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.UI.Dialog
{
    internal sealed class DialogAddressableHandle : MonoBehaviour
    {
        private AsyncOperationHandle<GameObject> _handle;
        private bool _hasHandle;

        public void Initialize(AsyncOperationHandle<GameObject> handle)
        {
            _handle = handle;
            _hasHandle = true;
        }

        private void OnDestroy()
        {
            if (_hasHandle)
            {
                Addressables.Release(_handle);
            }
        }
    }
}
