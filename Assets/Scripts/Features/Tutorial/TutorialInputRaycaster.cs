using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Tutorial
{
    /// <summary>元のRaycast順位を保ったまま許可対象への入力だけを返す。</summary>
    public sealed class TutorialInputRaycaster : GraphicRaycaster
    {
        private readonly List<BaseRaycaster> _captured = new();
        private readonly List<RaycastResult> _hits = new();
        private readonly List<RaycastResult> _overlayHits = new();
        private readonly List<Behaviour> _disabledAncestors = new();
        private GameObject _allowed;
        private bool _allowDescendants;
        private bool _gating;
        private bool _screenTap;
        private EventSystem _eventSystem;
        private GameObject _previousSelection;
        private bool _previousNavigation;

        public void Begin(GameObject allowed, bool allowDescendants, bool screenTap = false)
        {
            End();
            _screenTap = screenTap;
            _allowed = screenTap ? null : allowed;
            _allowDescendants = allowDescendants;
            _eventSystem = EventSystem.current;
            _previousNavigation = _eventSystem.sendNavigationEvents;
            _previousSelection = _eventSystem.currentSelectedGameObject;
            _eventSystem.sendNavigationEvents = false;
            _eventSystem.SetSelectedGameObject(null);
            _gating = true;

            // Buttonの親ScrollRect等へイベントが伝播して他のUIが動くことを防ぐ。
            if (_allowed != null && !allowDescendants)
                for (var parent = allowed.transform.parent; parent != null; parent = parent.parent)
                    foreach (var behaviour in parent.GetComponents<Behaviour>())
                        if (behaviour.enabled && (behaviour is IDragHandler || behaviour is IPointerClickHandler || behaviour is IScrollHandler))
                        {
                            _disabledAncestors.Add(behaviour);
                            behaviour.enabled = false;
                        }
            CaptureRaycasters();
        }

        public void End()
        {
            if (!_gating) return;
            _gating = false;
            foreach (var raycaster in _captured)
                if (raycaster != null) raycaster.enabled = true;
            _captured.Clear();
            foreach (var behaviour in _disabledAncestors)
                if (behaviour != null) behaviour.enabled = true;
            _disabledAncestors.Clear();
            if (_eventSystem != null)
            {
                _eventSystem.sendNavigationEvents = _previousNavigation;
                _eventSystem.SetSelectedGameObject(_previousSelection != null && _previousSelection.activeInHierarchy ? _previousSelection : null);
            }
            _allowed = null;
        }

        protected override void OnDisable()
        {
            End();
            base.OnDisable();
        }

        private void CaptureRaycasters()
        {
            // チュートリアルの途中で新しいダイアログCanvasが生成された場合も対象にする。
            foreach (var raycaster in FindObjectsByType<BaseRaycaster>(FindObjectsSortMode.None))
            {
                if (raycaster == this || !raycaster.isActiveAndEnabled) continue;
                if (!_captured.Contains(raycaster)) _captured.Add(raycaster);
                raycaster.enabled = false;
            }
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!_gating) { base.Raycast(eventData, resultAppendList); return; }
            CaptureRaycasters();
            _overlayHits.Clear();
            base.Raycast(eventData, _overlayHits);
            if (_screenTap)
            {
                // 穴の中も含めて全面でTutorial自身が入力を受け、背後のUIへは渡さない。
                if (_overlayHits.Count > 0) resultAppendList.Add(_overlayHits[0]);
                return;
            }

            _hits.Clear();
            foreach (var raycaster in _captured)
                if (raycaster != null && raycaster.gameObject.activeInHierarchy)
                    raycaster.Raycast(eventData, _hits);
            _hits.Sort(CompareHits);
            if (_hits.Count > 0 && IsAllowedHit(_hits[0].gameObject))
                resultAppendList.Add(_hits[0]);
            else if (_overlayHits.Count > 0)
                resultAppendList.Add(_overlayHits[0]);
        }

        public bool IsAllowedHit(GameObject hit)
        {
            if (_allowed == null || !_allowed.activeInHierarchy || hit == null) return false;
            if (_allowDescendants) return hit.transform.IsChildOf(_allowed.transform);
            // Text/Imageの子はButton本体として許可し、入れ子の別Buttonは許可しない。
            var clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit);
            if (clickTarget != null) return clickTarget == _allowed;
            return ExecuteEvents.GetEventHandler<IPointerDownHandler>(hit) == _allowed;
        }

        // UGUI EventSystemと同じ順位。画面上で別UIに隠れている対象を透過して操作させない。
        private static int CompareHits(RaycastResult left, RaycastResult right)
        {
            if (left.module != right.module)
            {
                var lc = left.module.eventCamera;
                var rc = right.module.eventCamera;
                if (lc != null && rc != null && lc.depth != rc.depth) return rc.depth.CompareTo(lc.depth);
                if (left.module.sortOrderPriority != right.module.sortOrderPriority)
                    return right.module.sortOrderPriority.CompareTo(left.module.sortOrderPriority);
                if (left.module.renderOrderPriority != right.module.renderOrderPriority)
                    return right.module.renderOrderPriority.CompareTo(left.module.renderOrderPriority);
            }
            if (left.sortingLayer != right.sortingLayer)
                return SortingLayer.GetLayerValueFromID(right.sortingLayer).CompareTo(SortingLayer.GetLayerValueFromID(left.sortingLayer));
            if (left.sortingOrder != right.sortingOrder) return right.sortingOrder.CompareTo(left.sortingOrder);
            if (left.depth != right.depth && left.module.rootRaycaster == right.module.rootRaycaster)
                return right.depth.CompareTo(left.depth);
            if (left.distance != right.distance) return left.distance.CompareTo(right.distance);
            return left.index.CompareTo(right.index);
        }
    }
}
