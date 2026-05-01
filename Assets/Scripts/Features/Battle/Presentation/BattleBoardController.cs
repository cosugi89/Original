using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.Presentation
{
    /// <summary>
    /// 盤面状態をセル View 群へ反映し、入力イベントを集約する。
    /// </summary>
    public class BattleBoardController : MonoBehaviour
    {
        [SerializeField] private RectTransform cellRoot;
        [SerializeField] private BattleCellView cellPrefab;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] private bool reuseExistingChildren = true;

        private readonly Dictionary<BattleGridPosition, BattleCellView> _cellViews = new();
        private readonly List<BattleCellView> _activeViews = new();
        private readonly List<BattleCellView> _spawnedViews = new();
        private BattleBoardState _board;

        public event Action<BattleGridPosition, PointerEventData> CellPointerDown;
        public event Action<BattleGridPosition, PointerEventData> CellPointerEnter;
        public event Action<BattleGridPosition, PointerEventData> CellPointerUp;

        public BattleBoardState Board => _board;

        public void ConfigureExistingChildren(RectTransform root, GridLayoutGroup layoutGroup = null)
        {
            cellRoot = root;
            gridLayoutGroup = layoutGroup;
            reuseExistingChildren = true;
            cellPrefab = null;
        }

        public void RenderBoard(BattleBoardState board)
        {
            _board = board;
            ClearBoard();

            if (_board == null)
            {
                Debug.LogWarning("[BattleBoard] RenderBoard が null board で呼ばれました。");
                return;
            }

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

            if (reuseExistingChildren && cellRoot.childCount >= _board.Width * _board.Height)
            {
                Debug.Log($"[BattleBoard] 既存ノードを再利用して描画します Board={_board.Width}x{_board.Height} ChildCount={cellRoot.childCount}");
                BindExistingChildren();
                return;
            }

            if (cellPrefab == null)
            {
                Debug.LogWarning($"[BattleBoard] cellPrefab が未設定で、既存ノード数も不足しています Required={_board.Width * _board.Height} Actual={cellRoot.childCount}");
                return;
            }

            Debug.Log($"[BattleBoard] prefab 生成で描画します Board={_board.Width}x{_board.Height}");

            for (var y = 0; y < _board.Height; y++)
            {
                for (var x = 0; x < _board.Width; x++)
                {
                    var position = new BattleGridPosition(x, y);
                    if (!_board.TryGetCell(position, out var cell))
                    {
                        continue;
                    }

                    var view = Instantiate(cellPrefab, cellRoot);
                    view.Bind(cell);
                    view.PointerDownReceived += OnCellPointerDown;
                    view.PointerEnterReceived += OnCellPointerEnter;
                    view.PointerUpReceived += OnCellPointerUp;
                    _cellViews[position] = view;
                    _activeViews.Add(view);
                    _spawnedViews.Add(view);
                }
            }
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
            }

            _cellViews.Clear();

            for (var i = 0; i < _spawnedViews.Count; i++)
            {
                if (_spawnedViews[i] != null)
                {
                    Destroy(_spawnedViews[i].gameObject);
                }
            }

            _activeViews.Clear();
            _spawnedViews.Clear();
        }

        public void ClearTraceVisual()
        {
            SetTraceVisual(Array.Empty<BattleGridPosition>(), canConfirm: false);
        }

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

        private void BindExistingChildren()
        {
            var total = _board.Width * _board.Height;
            for (var i = 0; i < total; i++)
            {
                var child = cellRoot.GetChild(i) as RectTransform;
                if (child == null)
                {
                    continue;
                }

                var position = GetPositionForExistingChildIndex(i);
                if (!_board.TryGetCell(position, out var cell))
                {
                    continue;
                }

                var view = child.GetComponent<BattleCellView>();
                if (view == null)
                {
                    view = child.gameObject.AddComponent<BattleCellView>();
                }

                view.AutoBindFromHierarchy();
                view.Bind(cell);
                view.PointerDownReceived += OnCellPointerDown;
                view.PointerEnterReceived += OnCellPointerEnter;
                view.PointerUpReceived += OnCellPointerUp;
                _cellViews[position] = view;
                _activeViews.Add(view);
            }

            Debug.Log($"[BattleBoard] 既存ノードのバインド完了 ActiveViews={_activeViews.Count}");
        }

        private BattleGridPosition GetPositionForExistingChildIndex(int index)
        {
            if (gridLayoutGroup != null && gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Vertical)
            {
                var x = index / _board.Height;
                var y = index % _board.Height;
                return new BattleGridPosition(x, y);
            }

            return new BattleGridPosition(index % _board.Width, index / _board.Width);
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
