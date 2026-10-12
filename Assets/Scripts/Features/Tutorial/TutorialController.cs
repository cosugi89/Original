using System;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Features.Tutorial
{
    /// <summary>再利用するチュートリアルPrefabの描画と入力制限。</summary>
    public sealed class TutorialController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TutorialBackdropGraphic backdrop;
        [SerializeField] private TutorialInputRaycaster inputRaycaster;
        [SerializeField] private RectTransform tutorialHand;
        [SerializeField] private GameObject messageRoot;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField, Min(1)] private float charactersPerSecond = 40;
        [SerializeField, Min(0)] private float handAmplitude = 12;
        [SerializeField, Min(0)] private float handSpeed = 2;

        private readonly List<RectTransform> _highlights = new();
        private readonly List<Rect> _holes = new();
        private readonly Vector3[] _corners = new Vector3[4];
        private TutorialStep _step;
        private RectTransform _handTarget;
        private Button _targetButton;
        private GameObject _interactionTarget;
        private Action _advance;
        private bool _listening;
        private bool _hadInteraction;
        private RawImage _alphaHighlight;
        private bool _hadAlphaHighlight;
        private bool _screenTapConsumed;
        private int _shownFrame;
        private int _characterCount;
        private float _revealedCharacters;
        public bool IsTyping => messageText.maxVisibleCharacters < _characterCount;

        public bool HasLostTarget
        {
            get
            {
                if (_hadInteraction && (_interactionTarget == null || !_interactionTarget.activeInHierarchy)) return true;
                if (_hadAlphaHighlight && (_alphaHighlight == null || !_alphaHighlight.isActiveAndEnabled || _alphaHighlight.texture == null)) return true;
                if (_targetButton != null && (!_targetButton.enabled || !_targetButton.IsInteractable())) return true;
                foreach (var target in _highlights)
                    if (target == null || !target.gameObject.activeInHierarchy) return true;
                return _step.ShowHand && (_handTarget == null || !_handTarget.gameObject.activeInHierarchy);
            }
        }

        public void Show(TutorialStep step, IReadOnlyList<RectTransform> highlights,
            GameObject interactionTarget, RectTransform handTarget, bool resurgence, Action advance, RawImage alphaHighlight = null)
        {
            Hide();
            _step = step;
            _advance = advance;
            _alphaHighlight = alphaHighlight;
            _hadAlphaHighlight = alphaHighlight != null;
            _screenTapConsumed = false;
            _shownFrame = Time.frameCount;
            _interactionTarget = interactionTarget;
            _hadInteraction = interactionTarget != null;
            _targetButton = interactionTarget != null ? interactionTarget.GetComponent<Button>() : null;
            _handTarget = handTarget;
            _highlights.Clear();
            foreach (var target in highlights) _highlights.Add(target);
            var text = resurgence && !string.IsNullOrEmpty(step.ResurgenceMessage) ? step.ResurgenceMessage : step.Message;
            messageRoot.SetActive(!string.IsNullOrEmpty(text));
            var messageRect = (RectTransform)messageRoot.transform;
            messageRect.anchorMin = messageRect.anchorMax = new Vector2(0.5f, step.MessageAtBottom ? 0.19f : 0.5f);
            messageRect.anchoredPosition = step.MessageAtBottom ? Vector2.zero : new Vector2(0, 121.37249f);
            messageText.text = text;
            nameText.text = step.Speaker;
            nameText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(step.Speaker));
            tutorialHand.gameObject.SetActive(step.ShowHand);
            gameObject.SetActive(true);
            messageText.maxVisibleCharacters = int.MaxValue;
            messageText.ForceMeshUpdate(true);
            _characterCount = messageText.textInfo.characterCount;
            _revealedCharacters = 0;
            messageText.maxVisibleCharacters = 0;
            if (step.Advance == TutorialAdvance.TargetButton) _targetButton.onClick.AddListener(OnAdvance);
            _listening = true;
            inputRaycaster.Begin(interactionTarget, step.AllowDescendantsAndDrag, step.Advance == TutorialAdvance.ScreenTap);
            RefreshPositions();
        }

        public void Hide()
        {
            inputRaycaster.End();
            if (_listening)
            {
                if (_targetButton != null) _targetButton.onClick.RemoveListener(OnAdvance);
            }
            _listening = false;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            inputRaycaster.End();
            if (_listening)
            {
                if (_targetButton != null) _targetButton.onClick.RemoveListener(OnAdvance);
                _listening = false;
            }
        }

        private void OnAdvance() => _advance?.Invoke();
        public void OnPointerClick(PointerEventData eventData)
        {
            // 表示を生んだクリックや複数指で同じStepを二度送らない。指を離したクリックだけ扱う。
            if (_step == null || _step.Advance != TutorialAdvance.ScreenTap || _screenTapConsumed ||
                !isActiveAndEnabled || Time.frameCount <= _shownFrame ||
                eventData.button != PointerEventData.InputButton.Left || eventData.dragging) return;
            var threshold = EventSystem.current.pixelDragThreshold;
            if ((eventData.position - eventData.pressPosition).sqrMagnitude > threshold * threshold) return;
            if (IsTyping)
            {
                messageText.maxVisibleCharacters = _characterCount;
                _revealedCharacters = _characterCount;
                _shownFrame = Time.frameCount;
                return;
            }
            _screenTapConsumed = true;
            OnAdvance();
        }
        private void LateUpdate()
        {
            UpdateTypewriter(Time.unscaledDeltaTime);
            RefreshPositions();
        }

        private void UpdateTypewriter(float deltaTime)
        {
            if (!IsTyping) return;
            _revealedCharacters = Mathf.Min(_characterCount, _revealedCharacters + charactersPerSecond * deltaTime);
            messageText.maxVisibleCharacters = Mathf.FloorToInt(_revealedCharacters);
        }

        private void RefreshPositions()
        {
            if (_step == null) return;
            _holes.Clear();
            foreach (var target in _highlights)
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    var rect = ConvertRect(target, backdrop.rectTransform);
                    rect.min -= _step.HighlightPadding;
                    rect.max += _step.HighlightPadding;
                    _holes.Add(rect);
                }
            backdrop.SetHoles(_holes);
            if (_alphaHighlight != null)
            {
                var opacity = _alphaHighlight.color.a;
                foreach (var group in _alphaHighlight.GetComponentsInParent<CanvasGroup>())
                {
                    opacity *= group.alpha;
                    if (group.ignoreParentGroups) break;
                }
                backdrop.SetAlphaHole(ConvertRect(_alphaHighlight.rectTransform, backdrop.rectTransform),
                    _alphaHighlight.texture, _alphaHighlight.uvRect, opacity);
            }
            else backdrop.ClearAlphaHole();
            if (_handTarget != null && tutorialHand.gameObject.activeSelf)
            {
                var rect = ConvertRect(_handTarget, (RectTransform)tutorialHand.parent);
                tutorialHand.anchoredPosition = rect.center + _step.HandOffset +
                    Vector2.up * (Mathf.Sin(Time.unscaledTime * handSpeed * Mathf.PI * 2) * handAmplitude);
            }
        }

        private Rect ConvertRect(RectTransform source, RectTransform destination)
        {
            var sourceCanvas = source.GetComponentInParent<Canvas>().rootCanvas;
            var sourceCamera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
            var destinationCanvas = destination.GetComponentInParent<Canvas>().rootCanvas;
            var destinationCamera = destinationCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : destinationCanvas.worldCamera;
            source.GetWorldCorners(_corners);
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var corner in _corners)
            {
                var screen = RectTransformUtility.WorldToScreenPoint(sourceCamera, corner);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(destination, screen, destinationCamera, out var point);
                min = Vector2.Min(min, point); max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
