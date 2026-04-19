using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    /* RectTransform を持たない Prefab (キャラクター / 2D・3D モデルなど) を
     * RenderTexture に焼き出して UI 上でプレビューするためのステージ。
     *
     * 仕組み:
     *   - previewRoot の配下に Prefab を Instantiate し、previewLayerName のレイヤーを再帰的に付与する。
     *   - 対になる PreviewCamera をシーンに配置し、そのレイヤーだけを映して Target RenderTexture に出力する。
     *   - 任意の RawImage.texture にその RenderTexture を挿すと、Canvas 上にプレビューが表示される。
     *
     * 使い方:
     *   - Spawn(prefab) で生成。戻り値の PreviewHandle から GameObject やコンポーネントを取り出せる。
     *   - 位置 / 回転 / スケールを調整したい場合は PreviewPlacement を渡す。
     *   - 個別破棄は Despawn(handle)、一括破棄は ClearAll()。
     *   - 1 枚の RT 内に複数 Prefab を並置する運用を想定 (例: プレイヤーと敵を同じ RT に並べる)。 */

    public class PreviewStage : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private Transform previewRoot;
        [SerializeField] private string previewLayerName = "Avatar Preview";

        private readonly HashSet<PreviewHandle> _spawned = new();

        /// <summary>
        /// 任意のプレハブを previewRoot 配下に生成する。
        /// 返却される PreviewHandle を保持すれば、後で Despawn で個別破棄できる。
        /// </summary>
        public PreviewHandle Spawn(GameObject prefab, PreviewPlacement? placement = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning("AvatarPreviewRenderer.Spawn: prefab is null.", this);
                return null;
            }
            if (previewRoot == null)
            {
                Debug.LogWarning("AvatarPreviewRenderer.Spawn: previewRoot is not assigned.", this);
                return null;
            }

            var p = placement ?? PreviewPlacement.Default;

            var instance = Instantiate(prefab, previewRoot);
            var t = instance.transform;
            t.localPosition = p.LocalPosition;
            t.localRotation = p.LocalRotation;
            t.localScale = p.LocalScale;

            SetLayerRecursively(instance, ResolvePreviewLayer());

            var handle = new PreviewHandle(Guid.NewGuid().ToString("N"), instance);
            _spawned.Add(handle);
            return handle;
        }

        /// <summary>
        /// Component 派生のプレハブを型を保ったまま生成するための糖衣。
        /// 代表例: var h = renderer.Spawn(partsManagerPrefab); var pm = h.Get&lt;PartsManager&gt;();
        /// </summary>
        public PreviewHandle Spawn<T>(T prefab, PreviewPlacement? placement = null) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogWarning("AvatarPreviewRenderer.Spawn<T>: prefab is null.", this);
                return null;
            }
            return Spawn(prefab.gameObject, placement);
        }

        /// <summary>
        /// 指定ハンドルに対応する生成物を破棄する。
        /// </summary>
        public bool Despawn(PreviewHandle handle)
        {
            if (handle == null) return false;
            if (!_spawned.Remove(handle)) return false;

            if (handle.Instance != null)
            {
                Destroy(handle.Instance);
            }
            handle.Instance = null;
            return true;
        }

        /// <summary>
        /// このレンダラが管理している生成物を全て破棄する。
        /// </summary>
        public void ClearAll()
        {
            foreach (var handle in _spawned)
            {
                if (handle.Instance != null)
                {
                    Destroy(handle.Instance);
                }
                handle.Instance = null;
            }
            _spawned.Clear();
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
