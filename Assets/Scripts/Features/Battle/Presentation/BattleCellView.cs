using System;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Battle.Presentation
{
    /// <summary>
    /// 単一セルの表示と pointer イベントを担当する View。
    /// </summary>
    public class BattleCellView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Color pathColor = new(1f, 0.92f, 0.45f, 1f);
        [SerializeField] private Color currentColor = new(1f, 0.7f, 0.25f, 1f);
        [SerializeField] private Color confirmableColor = new(0.45f, 0.9f, 0.55f, 1f);

        private BattleCellState _cell;
        private Outline _backgroundOutline;
        private Outline _labelOutline;
        private Vector3 _initialScale;
        private bool _hasInitialScale;

        public event Action<BattleCellView, PointerEventData> PointerDownReceived;
        public event Action<BattleCellView, PointerEventData> PointerEnterReceived;
        public event Action<BattleCellView, PointerEventData> PointerUpReceived;

        public BattleGridPosition Position => _cell != null ? _cell.Position : default;

        public BattleNodeType NodeType => _cell != null ? _cell.NodeType : BattleNodeType.Empty;

        public RectTransform RectTransform => transform as RectTransform;

        public void Bind(BattleCellState cell)
        {
            AutoBindFromHierarchy();
            EnsureRuntimeVisuals();
            _cell = cell;
            SetTraceState(isInPath: false, isCurrent: false, canConfirm: false);
        }

        public void AutoBindFromHierarchy()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
                if (backgroundImage == null)
                {
                    backgroundImage = GetComponentInChildren<Image>(includeInactive: true);
                }
            }

            if (labelText == null)
            {
                labelText = GetComponentInChildren<TMP_Text>(includeInactive: true);
            }

            if (labelText == null)
            {
                labelText = CreateFallbackLabel();
            }
        }

        public void SetTraceState(bool isInPath, bool isCurrent, bool canConfirm)
        {
            if (_cell == null)
            {
                return;
            }

            var style = GetStyle(_cell.NodeType);
            var backgroundColor = style.BaseColor;
            var outlineColor = style.AccentColor;

            if (canConfirm && isCurrent)
            {
                backgroundColor = Color.Lerp(style.BaseColor, confirmableColor, 0.72f);
                outlineColor = confirmableColor;
            }
            else if (isCurrent)
            {
                backgroundColor = Color.Lerp(style.BaseColor, currentColor, 0.62f);
                outlineColor = currentColor;
            }
            else if (isInPath)
            {
                backgroundColor = Color.Lerp(style.BaseColor, pathColor, 0.48f);
                outlineColor = pathColor;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (labelText != null)
            {
                labelText.text = style.Label;
                labelText.color = style.LabelColor;
            }

            if (_backgroundOutline != null)
            {
                _backgroundOutline.effectColor = outlineColor;
                _backgroundOutline.effectDistance = canConfirm && isCurrent
                    ? new Vector2(5f, -5f)
                    : isCurrent
                        ? new Vector2(4f, -4f)
                        : new Vector2(3f, -3f);
            }

            if (_labelOutline != null)
            {
                _labelOutline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            }

            if (!_hasInitialScale)
            {
                _initialScale = transform.localScale;
                _hasInitialScale = true;
            }

            transform.localScale = _initialScale * (canConfirm && isCurrent
                ? 1.08f
                : isCurrent
                    ? 1.05f
                    : isInPath
                        ? 1.02f
                        : 1f);
        }

        public Vector3 GetWorldCenter()
        {
            return RectTransform != null
                ? RectTransform.TransformPoint(RectTransform.rect.center)
                : transform.position;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownReceived?.Invoke(this, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnterReceived?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpReceived?.Invoke(this, eventData);
        }

        private void EnsureRuntimeVisuals()
        {
            if (labelText != null)
            {
                labelText.raycastTarget = false;
                labelText.enableAutoSizing = true;
                labelText.fontSizeMin = 18;
                labelText.fontSizeMax = 34;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.textWrappingMode = TextWrappingModes.Normal;

                if (labelText.rectTransform != null)
                {
                    labelText.rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
                    labelText.rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
                    labelText.rectTransform.offsetMin = Vector2.zero;
                    labelText.rectTransform.offsetMax = Vector2.zero;
                    labelText.rectTransform.localRotation = Quaternion.identity;
                }
            }

            if (backgroundImage != null)
            {
                backgroundImage.raycastTarget = true;
                _backgroundOutline = backgroundImage.GetComponent<Outline>();
                if (_backgroundOutline == null)
                {
                    _backgroundOutline = backgroundImage.gameObject.AddComponent<Outline>();
                }

                _backgroundOutline.useGraphicAlpha = true;
            }

            if (labelText != null)
            {
                _labelOutline = labelText.GetComponent<Outline>();
                if (_labelOutline == null)
                {
                    _labelOutline = labelText.gameObject.AddComponent<Outline>();
                }

                _labelOutline.useGraphicAlpha = true;
                _labelOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        private TMP_Text CreateFallbackLabel()
        {
            var labelObject = new GameObject("BattleNodeLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(transform, false);

            var rectTransform = labelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
            rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return labelObject.GetComponent<TextMeshProUGUI>();
        }

        private static NodeVisualStyle GetStyle(BattleNodeType nodeType)
        {
            switch (nodeType)
            {
                case BattleNodeType.Start:
                    return new NodeVisualStyle("START\n開始", new Color(0.18f, 0.64f, 0.73f, 0.98f), Color.white, new Color(0.72f, 0.96f, 1f, 1f));
                case BattleNodeType.Goal:
                    return new NodeVisualStyle("GOAL\n到達", new Color(0.14f, 0.72f, 0.38f, 0.98f), Color.white, new Color(0.73f, 1f, 0.82f, 1f));
                case BattleNodeType.Attack:
                    return new NodeVisualStyle("ATTACK\n攻撃", new Color(0.84f, 0.24f, 0.22f, 0.98f), Color.white, new Color(1f, 0.77f, 0.72f, 1f));
                case BattleNodeType.Jump:
                    return new NodeVisualStyle("JUMP\n跳躍", new Color(0.25f, 0.56f, 0.96f, 0.98f), Color.white, new Color(0.77f, 0.88f, 1f, 1f));
                case BattleNodeType.Roll:
                    return new NodeVisualStyle("ROLL\n回避", new Color(0.93f, 0.58f, 0.15f, 0.98f), Color.white, new Color(1f, 0.87f, 0.68f, 1f));
                case BattleNodeType.Dance:
                    return new NodeVisualStyle("DANCE\n舞踏", new Color(0.82f, 0.34f, 0.53f, 0.98f), Color.white, new Color(1f, 0.77f, 0.89f, 1f));
                case BattleNodeType.HazardNormal:
                    return new NodeVisualStyle("DANGER\n危険", new Color(0.25f, 0.26f, 0.3f, 0.98f), new Color(1f, 0.84f, 0.84f, 1f), new Color(1f, 0.36f, 0.36f, 1f));
                case BattleNodeType.HazardSkill:
                    return new NodeVisualStyle("SKILL\n危険", new Color(0.2f, 0.28f, 0.48f, 0.98f), new Color(0.92f, 0.98f, 1f, 1f), new Color(0.47f, 0.88f, 1f, 1f));
                default:
                    return new NodeVisualStyle(string.Empty, new Color(0.88f, 0.84f, 0.8f, 0.92f), new Color(0.33f, 0.29f, 0.26f, 1f), new Color(0.98f, 0.95f, 0.9f, 1f));
            }
        }

        private readonly struct NodeVisualStyle
        {
            public NodeVisualStyle(string label, Color baseColor, Color labelColor, Color accentColor)
            {
                Label = label;
                BaseColor = baseColor;
                LabelColor = labelColor;
                AccentColor = accentColor;
            }

            public string Label { get; }

            public Color BaseColor { get; }

            public Color LabelColor { get; }

            public Color AccentColor { get; }
        }
    }
}
