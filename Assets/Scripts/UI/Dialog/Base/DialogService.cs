using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.UI.Dialog
{
    public static class DialogService
    {
        public static async UniTask<TResult> OpenAsync<TDialog, TRequest, TResult>(
            TRequest request,
            DialogContext context)
            where TDialog : DialogBase<TResult>, IDialogRequestHandler<TRequest>
        {
            var dialog = await CreateAsync<TDialog, TResult>();
            dialog.Setup(request, context);
            return await dialog.OpenAsync();
        }

        private static UniTask<TDialog> CreateAsync<TDialog, TResult>()
            where TDialog : DialogBase<TResult>
        {
            var address = ResolveAddress<TDialog>();
            var host = DialogHost.Resolve();
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var prefab = handle.WaitForCompletion();

            if (handle.Status != AsyncOperationStatus.Succeeded || prefab == null)
            {
                Addressables.Release(handle);
                throw new InvalidOperationException($"Dialog prefab load failed: {address}");
            }

            var instance = UnityEngine.Object.Instantiate(prefab, host.DialogParent, false);
            instance.name = typeof(TDialog).Name;
            instance.AddComponent<DialogAddressableHandle>().Initialize(handle);

            var dialog = instance.GetComponent<TDialog>();
            if (dialog == null)
            {
                UnityEngine.Object.Destroy(instance);
                throw new InvalidOperationException($"Dialog prefab '{address}' does not contain {typeof(TDialog).Name}.");
            }

            if (dialog.transform is RectTransform rectTransform)
            {
                StretchToFillParent(rectTransform);
            }

            SetLayerRecursively(instance, host.gameObject.layer);
            dialog.transform.SetAsLastSibling();
            return UniTask.FromResult(dialog);
        }

        private static string ResolveAddress<TDialog>()
        {
            var attribute = (DialogAddressAttribute)Attribute.GetCustomAttribute(
                typeof(TDialog),
                typeof(DialogAddressAttribute));

            if (attribute == null || string.IsNullOrWhiteSpace(attribute.Address))
            {
                throw new InvalidOperationException($"{typeof(TDialog).Name} requires DialogAddressAttribute.");
            }

            return attribute.Address;
        }

        private static void StretchToFillParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
