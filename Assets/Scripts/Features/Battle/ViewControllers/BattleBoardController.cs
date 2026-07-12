using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 盤面状態をセル View 群へ反映し、入力イベントを集約する。
    /// </summary>
    public class BattleBoardController : MonoBehaviour
    {
        [SerializeField] private RectTransform cellRoot;
        [SerializeField] private BattleCellView cellPrefab;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private BattleNodeVisualDatabase nodeVisualDatabase;
        [Header("Editor Generation")]
        [SerializeField, Min(1)] private int editorPreviewWidth = 5;
        [SerializeField, Min(1)] private int editorPreviewHeight = 6;
        [SerializeField] private List<BattleCellView> authoredCellViews = new();

        private readonly Dictionary<BattleGridPosition, BattleCellView> _cellViews = new();
        private readonly List<BattleCellView> _activeViews = new();
        private readonly List<BattleCellView> _spawnedViews = new();
        private BattleBoardState _board;

        public event Action<BattleGridPosition, PointerEventData> CellPointerDown;
        public event Action<BattleGridPosition, PointerEventData> CellPointerEnter;
        public event Action<BattleGridPosition, PointerEventData> CellPointerUp;

        public BattleBoardState Board => _board;

        public BattleNodeVisualDatabase NodeVisualDatabase => nodeVisualDatabase;

        public int EditorPreviewWidth => editorPreviewWidth;

        public int EditorPreviewHeight => editorPreviewHeight;

        public void ConfigureGeneratedCells(RectTransform root, GridLayoutGroup layoutGroup = null, BattleCellView prefab = null)
        {
            cellRoot = root;
            gridLayoutGroup = layoutGroup;
            if (prefab != null)
            {
                cellPrefab = prefab;
            }

            ResolveMissingSceneReferences();
            CollectAuthoredCellViewsIfNeeded();
        }

        /// <summary>
        /// 論理盤面をもとにセル prefab 群を生成し、入力可能な盤面 View を構築する。
        /// </summary>
        public void RenderBoard(BattleBoardState board)
        {
            _board = board;
            ClearBoard();

            if (_board == null)
            {
                Debug.LogWarning("[BattleBoard] RenderBoard が null board で呼ばれました。");
                return;
            }

            ResolveMissingSceneReferences();
            CollectAuthoredCellViewsIfNeeded();

            if (cellRoot == null)
            {
                Debug.LogWarning("[BattleBoard] cellRoot が未設定のため盤面を描画できません。");
                return;
            }

            if (gridLayoutGroup != null)
            {
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = _board.Width;
            }

            var total = _board.Width * _board.Height;
            if (RequiresPrefab(total) && cellPrefab == null)
            {
                Debug.LogWarning("[BattleBoard] cellPrefab が未設定です。BattleBoardController の Inspector で BattleCellView prefab を割り当ててください。");
                return;
            }

            Debug.Log($"[BattleBoard] prefab 生成で描画します Board={_board.Width}x{_board.Height}");

            for (var i = 0; i < total; i++)
            {
                var position = GetPositionForLayoutIndex(i);
                if (!_board.TryGetCell(position, out var cell))
                {
                    continue;
                }

                var view = GetOrCreateCellView(i);
                if (view == null)
                {
                    continue;
                }

                view.gameObject.SetActive(true);
                view.Bind(cell, nodeVisualDatabase);
                view.PointerDownReceived += OnCellPointerDown;
                view.PointerEnterReceived += OnCellPointerEnter;
                view.PointerUpReceived += OnCellPointerUp;
                _cellViews[position] = view;
                _activeViews.Add(view);
            }

            DisableUnusedAuthoredCellViews(total);
        }

        public void ClearBoard()
        {
            for (var i = 0; i < _activeViews.Count; i++)
            {
                var view = _activeViews[i];
                if (view == null)
                {
                    continue;
                }

                view.PointerDownReceived -= OnCellPointerDown;
                view.PointerEnterReceived -= OnCellPointerEnter;
                view.PointerUpReceived -= OnCellPointerUp;
                if (!_spawnedViews.Contains(view))
                {
                    view.gameObject.SetActive(false);
                }
            }

            _cellViews.Clear();

            for (var i = 0; i < _spawnedViews.Count; i++)
            {
                if (_spawnedViews[i] != null)
                {
                    DestroyCellView(_spawnedViews[i]);
                }
            }

            _activeViews.Clear();
            _spawnedViews.Clear();
        }

        public void ClearTraceVisual()
        {
            SetTraceVisual(Array.Empty<BattleGridPosition>(), canConfirm: false);
        }

        /// <summary>
        /// パス中セルのハイライト状態を更新する。
        /// </summary>
        public void SetTraceVisual(IReadOnlyList<BattleGridPosition> positions, bool canConfirm)
        {
            foreach (var pair in _cellViews)
            {
                pair.Value.SetTraceState(isInPath: false, isCurrent: false, canConfirm: false);
            }

            if (positions == null || positions.Count == 0)
            {
                return;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                if (!_cellViews.TryGetValue(positions[i], out var view))
                {
                    continue;
                }

                var isCurrent = i == positions.Count - 1;
                view.SetTraceState(isInPath: true, isCurrent: isCurrent, canConfirm: canConfirm);
            }
        }

        /// <summary>
        /// 論理パスを軌跡線描画用のワールド座標列へ変換する。
        /// </summary>
        public List<Vector3> BuildWorldPointPath(IReadOnlyList<BattleGridPosition> positions)
        {
            var result = new List<Vector3>();
            if (positions == null)
            {
                return result;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                if (_cellViews.TryGetValue(positions[i], out var view))
                {
                    result.Add(view.GetWorldCenter());
                }
            }

            return result;
        }

        private void OnDestroy()
        {
            ClearBoard();
        }

        private void ResolveMissingSceneReferences()
        {
            if (cellRoot == null)
            {
                cellRoot = transform as RectTransform;
            }

            if (gridLayoutGroup == null && cellRoot != null)
            {
                gridLayoutGroup = cellRoot.GetComponent<GridLayoutGroup>();
            }
        }

        private void CollectAuthoredCellViewsIfNeeded()
        {
            if (authoredCellViews.Count > 0 || cellRoot == null)
            {
                return;
            }

            for (var i = 0; i < cellRoot.childCount; i++)
            {
                var child = cellRoot.GetChild(i);
                if (child != null && child.TryGetComponent<BattleCellView>(out var view))
                {
                    authoredCellViews.Add(view);
                }
            }
        }

        private bool RequiresPrefab(int total)
        {
            for (var i = 0; i < total; i++)
            {
                if (i >= authoredCellViews.Count || authoredCellViews[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        private BattleCellView GetOrCreateCellView(int index)
        {
            if (index < authoredCellViews.Count && authoredCellViews[index] != null)
            {
                return authoredCellViews[index];
            }

            if (cellPrefab == null || cellRoot == null)
            {
                return null;
            }

            var view = Instantiate(cellPrefab, cellRoot);
            _spawnedViews.Add(view);
            return view;
        }

        private void DisableUnusedAuthoredCellViews(int usedCount)
        {
            for (var i = usedCount; i < authoredCellViews.Count; i++)
            {
                var view = authoredCellViews[i];
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private static void DestroyCellView(BattleCellView view)
        {
            if (view == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(view.gameObject);
            }
            else
            {
                DestroyImmediate(view.gameObject);
            }
        }

        /// <summary>
        /// GridLayoutGroup の並び順に従って、生成順インデックスを論理座標へ変換する。
        /// </summary>
        private BattleGridPosition GetPositionForLayoutIndex(int index)
        {
            var x = 0;
            var y = 0;

            if (gridLayoutGroup != null && gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Vertical)
            {
                x = index / _board.Height;
                y = index % _board.Height;
            }
            else
            {
                x = index % _board.Width;
                y = index / _board.Width;
            }

            if (gridLayoutGroup == null)
            {
                return new BattleGridPosition(x, y);
            }

            switch (gridLayoutGroup.startCorner)
            {
                case GridLayoutGroup.Corner.UpperRight:
                    x = (_board.Width - 1) - x;
                    break;
                case GridLayoutGroup.Corner.LowerLeft:
                    y = (_board.Height - 1) - y;
                    break;
                case GridLayoutGroup.Corner.LowerRight:
                    x = (_board.Width - 1) - x;
                    y = (_board.Height - 1) - y;
                    break;
            }

            return new BattleGridPosition(x, y);
        }

        private void OnCellPointerDown(BattleCellView view, PointerEventData eventData)
        {
            CellPointerDown?.Invoke(view.Position, eventData);
        }

        private void OnCellPointerEnter(BattleCellView view, PointerEventData eventData)
        {
            CellPointerEnter?.Invoke(view.Position, eventData);
        }

        private void OnCellPointerUp(BattleCellView view, PointerEventData eventData)
        {
            CellPointerUp?.Invoke(view.Position, eventData);
        }
    }
}
