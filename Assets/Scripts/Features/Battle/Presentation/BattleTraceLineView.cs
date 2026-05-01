using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.Presentation
{
    /// <summary>
    /// UI 上にドラッグ経路の線分を簡易描画する。
    /// segmentRoot や prefab が未設定なら no-op として振る舞う。
    /// </summary>
    public class BattleTraceLineView : MonoBehaviour
    {
        [SerializeField] private RectTransform segmentRoot;
        [SerializeField] private Image segmentPrefab;
        [SerializeField] private Camera canvasCamera;
        [SerializeField] private Color normalLineColor = new(1f, 0.82f, 0.2f, 0.95f);
        [SerializeField] private Color confirmableLineColor = new(0.45f, 0.9f, 0.55f, 0.95f);
        [SerializeField] private float lineThickness = 12f;

        private readonly List<Image> _segments = new();

        public void Clear()
        {
            for (var i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] != null)
                {
                    Destroy(_segments[i].gameObject);
                }
            }

            _segments.Clear();
        }

        public void SetLine(IReadOnlyList<Vector3> worldPoints, bool canConfirm)
        {
            Clear();

            if (segmentRoot == null || segmentPrefab == null || worldPoints == null || worldPoints.Count < 2)
            {
                return;
            }

            for (var i = 1; i < worldPoints.Count; i++)
            {
                if (!TryWorldToLocal(worldPoints[i - 1], out var from) ||
                    !TryWorldToLocal(worldPoints[i], out var to))
                {
                    continue;
                }

                var segment = Instantiate(segmentPrefab, segmentRoot);
                var rect = segment.rectTransform;
                var delta = to - from;
                var length = delta.magnitude;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = from;
                rect.sizeDelta = new Vector2(length, lineThickness);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                segment.color = canConfirm ? confirmableLineColor : normalLineColor;
                _segments.Add(segment);
            }
        }

        private bool TryWorldToLocal(Vector3 worldPoint, out Vector2 localPoint)
        {
            if (segmentRoot == null)
            {
                localPoint = default;
                return false;
            }

            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPoint);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                segmentRoot,
                screenPoint,
                canvasCamera,
                out localPoint);
        }
    }
}
