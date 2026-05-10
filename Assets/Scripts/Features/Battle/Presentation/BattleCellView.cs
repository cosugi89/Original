using System;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;
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
        [SerializeField] private Image iconImage;
        [SerializeField] private Color pathColor = new(1f, 0.92f, 0.45f, 1f);
        [SerializeField] private Color currentColor = new(1f, 0.7f, 0.25f, 1f);
        [SerializeField] private Color confirmableColor = new(0.45f, 0.9f, 0.55f, 1f);

        private BattleCellState _cell;
        private BattleNodeVisualDatabase _nodeVisualDatabase;
        private Outline _backgroundOutline;
        private Vector3 _initialScale;
        private bool _hasInitialScale;

        public event Action<BattleCellView, PointerEventData> PointerDownReceived;
        public event Action<BattleCellView, PointerEventData> PointerEnterReceived;
        public event Action<BattleCellView, PointerEventData> PointerUpReceived;

        public BattleGridPosition Position => _cell != null ? _cell.Position : default;

        public BattleNodeType NodeType => _cell != null ? _cell.NodeType : BattleNodeType.Empty;

        public RectTransform RectTransform => transform as RectTransform;

        public void Bind(BattleCellState cell, BattleNodeVisualDatabase nodeVisualDatabase = null)
        {
            EnsureRuntimeVisuals();
            _cell = cell;
            _nodeVisualDatabase = nodeVisualDatabase;
            SetTraceState(isInPath: false, isCurrent: false, canConfirm: false);
        }

        private void Reset()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (iconImage == null)
            {
                var images = GetComponentsInChildren<Image>(includeInactive: true);
                for (var i = 0; i < images.Length; i++)
                {
                    if (images[i] != null && images[i] != backgroundImage)
                    {
                        iconImage = images[i];
                        break;
                    }
                }
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

            ApplyIcon(style);

            if (_backgroundOutline != null)
            {
                _backgroundOutline.effectColor = outlineColor;
                _backgroundOutline.effectDistance = canConfirm && isCurrent
                    ? new Vector2(5f, -5f)
                    : isCurrent
                        ? new Vector2(4f, -4f)
                        : new Vector2(3f, -3f);
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
            if (backgroundImage == null)
            {
                Debug.LogWarning($"[BattleCellView] Background Image が未設定です GameObject={name}");
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

            if (iconImage != null)
            {
                iconImage.raycastTarget = false;
                iconImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning($"[BattleCellView] Icon Image が未設定です GameObject={name}");
            }
        }

        private void ApplyIcon(NodeVisualStyle style)
        {
            if (iconImage == null)
            {
                return;
            }

            var showIcon = style.Icon != null;
            iconImage.sprite = style.Icon;
            iconImage.color = style.IconColor;
            iconImage.enabled = showIcon;
        }

        private NodeVisualStyle GetStyle(BattleNodeType nodeType)
        {
            var entry = _nodeVisualDatabase != null ? _nodeVisualDatabase.GetEntry(nodeType) : null;
            if (entry != null)
            {
                return new NodeVisualStyle(
                    entry.Icon,
                    entry.BaseColor,
                    entry.AccentColor,
                    entry.IconColor);
            }

            switch (nodeType)
            {
                case BattleNodeType.Start:
                    return new NodeVisualStyle(null, new Color(0.18f, 0.64f, 0.73f, 0.98f), new Color(0.72f, 0.96f, 1f, 1f), Color.white);
                case BattleNodeType.Goal:
                    return new NodeVisualStyle(null, new Color(0.14f, 0.72f, 0.38f, 0.98f), new Color(0.73f, 1f, 0.82f, 1f), Color.white);
                case BattleNodeType.Attack:
                    return new NodeVisualStyle(null, new Color(0.84f, 0.24f, 0.22f, 0.98f), new Color(1f, 0.77f, 0.72f, 1f), Color.white);
                case BattleNodeType.Jump:
                    return new NodeVisualStyle(null, new Color(0.25f, 0.56f, 0.96f, 0.98f), new Color(0.77f, 0.88f, 1f, 1f), Color.white);
                case BattleNodeType.Roll:
                    return new NodeVisualStyle(null, new Color(0.93f, 0.58f, 0.15f, 0.98f), new Color(1f, 0.87f, 0.68f, 1f), Color.white);
                case BattleNodeType.Dance:
                    return new NodeVisualStyle(null, new Color(0.82f, 0.34f, 0.53f, 0.98f), new Color(1f, 0.77f, 0.89f, 1f), Color.white);
                case BattleNodeType.HazardNormal:
                    return new NodeVisualStyle(null, new Color(0.25f, 0.26f, 0.3f, 0.98f), new Color(1f, 0.36f, 0.36f, 1f), Color.white);
                case BattleNodeType.HazardSkill:
                    return new NodeVisualStyle(null, new Color(0.2f, 0.28f, 0.48f, 0.98f), new Color(0.47f, 0.88f, 1f, 1f), Color.white);
                default:
                    return new NodeVisualStyle(null, new Color(0.88f, 0.84f, 0.8f, 0.92f), new Color(0.98f, 0.95f, 0.9f, 1f), Color.white);
            }
        }

        private readonly struct NodeVisualStyle
        {
            public NodeVisualStyle(
                Sprite icon,
                Color baseColor,
                Color accentColor,
                Color iconColor)
            {
                Icon = icon;
                BaseColor = baseColor;
                AccentColor = accentColor;
                IconColor = iconColor;
            }

            public Sprite Icon { get; }

            public Color BaseColor { get; }

            public Color AccentColor { get; }

            public Color IconColor { get; }
        }
    }
}
