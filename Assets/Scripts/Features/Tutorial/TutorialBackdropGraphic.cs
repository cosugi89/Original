using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Tutorial
{
    /// <summary>複数の矩形をくり抜く背景。入力判定はTutorialInputRaycasterが別に行う。</summary>
    public sealed class TutorialBackdropGraphic : MaskableGraphic
    {
        private readonly List<Rect> _holes = new();
        private readonly List<float> _xs = new();
        private readonly List<float> _ys = new();
        [SerializeField] private Material alphaHoleMaterial;
        private Material _alphaMaterial;

        public void SetAlphaHole(Rect rect, Texture texture, Rect uvRect, float opacity)
        {
            if (_alphaMaterial == null) _alphaMaterial = new Material(alphaHoleMaterial) { hideFlags = HideFlags.HideAndDontSave };
            _alphaMaterial.SetTexture("_MaskTex", texture);
            _alphaMaterial.SetVector("_MaskRect", new Vector4(rect.xMin, rect.yMin, Mathf.Max(rect.width, 0.001f), Mathf.Max(rect.height, 0.001f)));
            _alphaMaterial.SetVector("_MaskUV", new Vector4(uvRect.x, uvRect.y, uvRect.width, uvRect.height));
            _alphaMaterial.SetFloat("_MaskOpacity", opacity);
            material = _alphaMaterial;
            SetMaterialDirty();
        }

        public void ClearAlphaHole()
        {
            if (material == _alphaMaterial && _alphaMaterial != null) material = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_alphaMaterial != null)
            {
                if (Application.isPlaying) Destroy(_alphaMaterial);
                else DestroyImmediate(_alphaMaterial);
            }
        }
        public void SetHoles(IReadOnlyList<Rect> holes)
        {
            var changed = holes.Count != _holes.Count;
            if (!changed)
                for (var i = 0; i < holes.Count; i++) changed |= holes[i] != _holes[i];
            if (!changed) return;
            _holes.Clear();
            for (var i = 0; i < holes.Count; i++) _holes.Add(holes[i]);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var bounds = rectTransform.rect;
            _xs.Clear(); _ys.Clear();
            _xs.Add(bounds.xMin); _xs.Add(bounds.xMax);
            _ys.Add(bounds.yMin); _ys.Add(bounds.yMax);
            foreach (var hole in _holes)
            {
                _xs.Add(Mathf.Clamp(hole.xMin, bounds.xMin, bounds.xMax));
                _xs.Add(Mathf.Clamp(hole.xMax, bounds.xMin, bounds.xMax));
                _ys.Add(Mathf.Clamp(hole.yMin, bounds.yMin, bounds.yMax));
                _ys.Add(Mathf.Clamp(hole.yMax, bounds.yMin, bounds.yMax));
            }
            _xs.Sort(); _ys.Sort();
            for (var x = 0; x + 1 < _xs.Count; x++)
                for (var y = 0; y + 1 < _ys.Count; y++)
                {
                    var cell = Rect.MinMaxRect(_xs[x], _ys[y], _xs[x + 1], _ys[y + 1]);
                    if (cell.width <= 0 || cell.height <= 0 || _holes.Exists(hole => hole.Contains(cell.center))) continue;
                    var first = vh.currentVertCount;
                    vh.AddVert(new Vector3(cell.xMin, cell.yMin), color, Vector2.zero);
                    vh.AddVert(new Vector3(cell.xMin, cell.yMax), color, Vector2.zero);
                    vh.AddVert(new Vector3(cell.xMax, cell.yMax), color, Vector2.zero);
                    vh.AddVert(new Vector3(cell.xMax, cell.yMin), color, Vector2.zero);
                    vh.AddTriangle(first, first + 1, first + 2);
                    vh.AddTriangle(first + 2, first + 3, first);
                }
        }
    }
}
