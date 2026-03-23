using LayerLab.ArtMakerUnity;
using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.UI.Dialog
{
    /* Canvas に Player などの RectTranceform がアタッチされていない Prefab を表示する用の Renderer です。
     * この AvatarPreviewRenderer を引数に入れて Canvas 内のコンテンツに渡し、表示したい Prefab を SpawnPreview に入れると、
     * Scene上の previewRoot で Prefab が生成され、PreviewCamera が撮影した像が RT_AvatarPreview のテクスチャに入ります。
     * そして RT_AvatarPreview を SerializeField などで取得して Canvas 内のコンテンツの任意の RawImage.texture に入れることで、
     * PlayerPrefab が表示されます。
     * 
     * MEMO: Avatar(Player)表示用に作成したので汎用的に使う場合は、命名を変えてください。 */

    public class AvatarPreviewRenderer : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private Transform previewRoot;
        [SerializeField] private string previewLayerName = "Avatar Preview";

        private PartsManager currentPreview;

        public PartsManager SpawnPreview(PartsManager prefab)
        {
            ClearPreview();

            currentPreview = Instantiate(prefab, previewRoot);
            currentPreview.transform.localPosition = Vector3.zero;
            currentPreview.transform.localRotation = Quaternion.identity;
            currentPreview.transform.localScale = Vector3.one;

            SetLayerRecursively(currentPreview.gameObject, ResolvePreviewLayer());

            return currentPreview;
        }

        public void ClearPreview()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview.gameObject);
                currentPreview = null;
            }
        }

        private void SetLayerRecursively(GameObject target, int layer)
        {
            if (target == null || layer < 0) return;

            target.layer = layer;

            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private int ResolvePreviewLayer()
        {
            int layer = LayerMask.NameToLayer(previewLayerName);
            if (layer >= 0)
                return layer;

            if (previewRoot != null)
            {
                Debug.LogWarning(
                    $"Preview layer '{previewLayerName}' was not found. Falling back to previewRoot layer '{LayerMask.LayerToName(previewRoot.gameObject.layer)}'.",
                    this);
                return previewRoot.gameObject.layer;
            }

            Debug.LogWarning($"Preview layer '{previewLayerName}' was not found.", this);
            return -1;
        }
    }
}
