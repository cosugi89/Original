using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Game Button")]
    public sealed class GameButton : Button
    {
        public enum ButtonType
        {
            Normal,
            Close
        }

        public enum FrameType
        {
            Button01,
            Button02,
            Button03
        }

        public enum ColorType
        {
            Blue,
            Brown,
            Dark,
            Gray,
            Green,
            Mint,
            OffWhite,
            Orange,
            Pink,
            Plum,
            Red,
            Yellow
        }

        public enum SizeType
        {
            Small,
            Medium,
            Large,
            IconSmall,
            IconMedium,
            Wide,
            TitleWide
        }

        public enum ContentType
        {
            None,
            Label,
            Icon,
            IconLabel,
            TitleIconLabel
        }

        public enum SoundType
        {
            None,
            Button1,
            Button2,
            Button3,
            Button4,
            Button5,
            Select,
            SelectHighTone,
            Close,
            Deep,
            Error,
            Glitch,
            Custom
        }

        [Header("Appearance")]
        [SerializeField] private ButtonType buttonType = ButtonType.Normal;
        [SerializeField] private FrameType frameType = FrameType.Button02;
        [SerializeField] private ColorType colorType = ColorType.Red;
        [SerializeField] private SizeType sizeType = SizeType.Medium;
        [SerializeField] private ContentType contentType = ContentType.Label;
        [SerializeField] private string label = "Button";
        [SerializeField] private string title;
        [SerializeField] private string value;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private bool badgeVisible;
        [SerializeField] private string badgeLabel;
        [SerializeField] private Sprite badgeSprite;

        [Header("Visual References")]
        [SerializeField] private RectTransform visualRoot;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image lightImage;
        [SerializeField] private Image highLightImage;
        [SerializeField] private Image gradientImage;
        [SerializeField] private Image shadowImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text valueText;

        [Header("State Hooks")]
        [SerializeField] private GameObject selectedRoot;
        [SerializeField] private GameObject disabledRoot;
        [SerializeField] private GameObject loadingRoot;
        [SerializeField] private GameObject badgeRoot;
        [SerializeField] private TMP_Text badgeText;
        [SerializeField] private Image badgeImage;

        [Header("Visual Assets")]
        [SerializeField, HideInInspector] private Sprite button01BgSprite;
        [SerializeField, HideInInspector] private Sprite button01InnerBorder1Sprite;
        [SerializeField, HideInInspector] private Sprite button01InnerBorder2Sprite;
        [SerializeField, HideInInspector] private Sprite button02BgSprite;
        [SerializeField, HideInInspector] private Sprite button02LightSprite;
        [SerializeField, HideInInspector] private Sprite button02HighLightSprite;
        [SerializeField, HideInInspector] private Sprite button03BgSprite;
        [SerializeField, HideInInspector] private Sprite button03LightSprite;
        [SerializeField, HideInInspector] private Sprite button03HighLightSprite;
        [SerializeField, HideInInspector] private Sprite button03GradientSprite;
        [SerializeField, HideInInspector] private Sprite button03ShadowSprite;
        [SerializeField, HideInInspector] private Sprite closeBackgroundSprite;
        [SerializeField, HideInInspector] private Sprite closeLightSprite;
        [SerializeField, HideInInspector] private Sprite closeIconSprite;

        [Header("Optional Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SoundType clickSoundType = SoundType.Button1;
        [SerializeField] private AudioClip customClickSound;
        [SerializeField] private SoundType disabledClickSoundType = SoundType.Error;
        [SerializeField] private AudioClip customDisabledClickSound;

        private Action _clickAction;
        private bool _selected;
        private bool _loading;
        private bool _interactableBeforeLoading = true;
#if UNITY_EDITOR
        private bool _applyVisualQueued;
#endif

        public bool IsSelectedState => _selected;
        public bool IsLoading => _loading;

        protected override void Awake()
        {
            base.Awake();
            _interactableBeforeLoading = interactable;
            RefreshOptionalVisuals();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshOptionalVisuals();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }

        public void SetInteractable(bool value)
        {
            if (_loading)
            {
                _interactableBeforeLoading = value;
            }
            else
            {
                interactable = value;
            }

            RefreshOptionalVisuals();
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            RefreshOptionalVisuals();
        }

        public void SetLoading(bool loading)
        {
            if (_loading == loading)
            {
                RefreshOptionalVisuals();
                return;
            }

            if (loading)
            {
                _interactableBeforeLoading = interactable;
                _loading = true;
                interactable = false;
            }
            else
            {
                _loading = false;
                interactable = _interactableBeforeLoading;
            }

            RefreshOptionalVisuals();
        }

        public void SetBadgeVisible(bool visible)
        {
            badgeVisible = visible;
            ApplyVisual();
        }

        public void SetBadgeText(string text)
        {
            badgeLabel = text ?? string.Empty;
            badgeVisible = !string.IsNullOrEmpty(badgeLabel);
            ApplyVisual();
        }

        public void SetLabel(string text)
        {
            label = text ?? string.Empty;
            ApplyVisual();
        }

        public void SetTitle(string text)
        {
            title = text ?? string.Empty;
            ApplyVisual();
        }

        public void SetValue(string text)
        {
            value = text ?? string.Empty;
            ApplyVisual();
        }

        public void SetIcon(Sprite sprite)
        {
            iconSprite = sprite;
            ApplyVisual();
        }

        public void SetAppearance(FrameType frame, ColorType color, SizeType size, ContentType content)
        {
            frameType = frame;
            colorType = color;
            sizeType = size;
            contentType = content;
            ApplyVisual();
        }

        public void SetAction(Action action)
        {
            _clickAction = action;
        }

        public void AddAction(Action action)
        {
            _clickAction += action;
        }

        public void RemoveAction(Action action)
        {
            _clickAction -= action;
        }

        public void ClearAction()
        {
            _clickAction = null;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                base.OnPointerClick(eventData);
                return;
            }

            var usable = IsActive() && IsInteractable();
            Play(usable ? clickSoundType : disabledClickSoundType, usable ? customClickSound : customDisabledClickSound);
            base.OnPointerClick(eventData);

            if (usable)
            {
                _clickAction?.Invoke();
            }
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            var usable = IsActive() && IsInteractable();
            Play(usable ? clickSoundType : disabledClickSoundType, usable ? customClickSound : customDisabledClickSound);
            base.OnSubmit(eventData);

            if (usable)
            {
                _clickAction?.Invoke();
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            RefreshOptionalVisuals();
        }

        public void ApplyVisual()
        {
            if (buttonType == ButtonType.Close)
            {
                ApplyCloseVisual();
                return;
            }

            var size = GetButtonSize(sizeType);
            ((RectTransform)transform).sizeDelta = size;
            Stretch(visualRoot, 0f, 0f, 0f, 0f);

            var colors = GetColorSet(colorType);
            ApplyFrame(frameType, colors);
            ApplyContent(size);
            ApplyBadge(size);
        }

        private void ApplyCloseVisual()
        {
            var size = new Vector2(103.2025f, 106.23987f);
            ((RectTransform)transform).sizeDelta = size;
            Stretch(visualRoot, 0f, 0f, 0f, 0f);

            SetImage(backgroundImage, closeBackgroundSprite, new Color32(90, 43, 44, 255), true, Image.Type.Sliced);
            SetImage(lightImage, closeLightSprite, new Color32(110, 41, 42, 255), true, Image.Type.Sliced);
            SetImage(highLightImage, null, Color.clear, false, Image.Type.Simple);
            SetImage(gradientImage, null, Color.clear, false, Image.Type.Sliced);
            SetImage(shadowImage, null, Color.clear, false, Image.Type.Sliced);
            Stretch(backgroundImage.rectTransform, 0f, 0f, 0f, 0f);
            Stretch(lightImage.rectTransform, 4f, 4f, 4f, 7f);

            SetOptionalActive(labelText.gameObject, false);
            SetOptionalActive(titleText.gameObject, false);
            SetOptionalActive(valueText.gameObject, false);
            SetOptionalActive(iconImage.gameObject, true);
            iconImage.sprite = closeIconSprite;
            iconImage.color = new Color32(189, 163, 137, 255);
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = false;
            iconImage.raycastTarget = false;
            Center(iconImage.rectTransform, 60.2025f, 61.23987f, 0f, 1.6000061f);

            SetOptionalActive(badgeRoot, false);
        }

        private void RefreshOptionalVisuals()
        {
            var usable = IsActive() && IsInteractable();
            SetOptionalActive(selectedRoot, usable && _selected);
            SetOptionalActive(disabledRoot, !usable);
            SetOptionalActive(loadingRoot, _loading);
        }

        private void ApplyFrame(FrameType frame, ColorSet colors)
        {
            switch (frame)
            {
                case FrameType.Button01:
                    SetImage(backgroundImage, button01BgSprite, colors.Background, true, Image.Type.Sliced);
                    SetImage(lightImage, button01InnerBorder1Sprite, colors.Light, colors.Button01InnerVisible, Image.Type.Sliced);
                    SetImage(highLightImage, button02HighLightSprite, colors.Highlight, false, Image.Type.Simple);
                    SetImage(gradientImage, button03GradientSprite, colors.Gradient, false, Image.Type.Sliced);
                    SetImage(shadowImage, button03ShadowSprite, colors.Shadow, false, Image.Type.Sliced);
                    Stretch(lightImage.rectTransform, 4f, 4f, 4f, 7f);
                    break;
                case FrameType.Button02:
                    SetImage(backgroundImage, button02BgSprite, colors.Background, true, Image.Type.Sliced);
                    SetImage(lightImage, button02LightSprite, colors.Light, true, Image.Type.Sliced);
                    SetImage(highLightImage, button02HighLightSprite, colors.Highlight, true, Image.Type.Simple);
                    SetImage(gradientImage, button03GradientSprite, colors.Gradient, false, Image.Type.Sliced);
                    SetImage(shadowImage, button03ShadowSprite, colors.Shadow, false, Image.Type.Sliced);
                    Stretch(lightImage.rectTransform, 4f, 4f, 4f, 8f);
                    SetTopLeft(highLightImage.rectTransform, 12f, 11f, 10f, 9f);
                    break;
                case FrameType.Button03:
                    SetImage(backgroundImage, button03BgSprite, colors.Background, true, Image.Type.Sliced);
                    SetImage(lightImage, button03LightSprite, colors.Light, true, Image.Type.Sliced);
                    SetImage(highLightImage, button03HighLightSprite, colors.Highlight, true, Image.Type.Simple);
                    SetImage(gradientImage, button03GradientSprite, colors.Gradient, true, Image.Type.Sliced);
                    SetImage(shadowImage, button03ShadowSprite, colors.Shadow, true, Image.Type.Sliced);
                    Stretch(lightImage.rectTransform, 7f, 7f, 5f, 13f);
                    SetTopLeft(highLightImage.rectTransform, 15f, 13f, 14f, 11f);
                    Stretch(gradientImage.rectTransform, 7f, 7f, 90f, 10f);
                    Stretch(shadowImage.rectTransform, 7f, 7f, 5f, 13f);
                    break;
            }

            Stretch(backgroundImage.rectTransform, 0f, 0f, 0f, 0f);
        }

        private void ApplyContent(Vector2 size)
        {
            var showLabel = (contentType == ContentType.Label || contentType == ContentType.IconLabel) &&
                !string.IsNullOrEmpty(label);
            var showIcon = (contentType == ContentType.Icon ||
                contentType == ContentType.IconLabel ||
                contentType == ContentType.TitleIconLabel) &&
                iconSprite != null;
            var showTitle = contentType == ContentType.TitleIconLabel && !string.IsNullOrEmpty(title);
            var showValue = contentType == ContentType.TitleIconLabel && !string.IsNullOrEmpty(value);

            SetOptionalActive(labelText.gameObject, showLabel);
            SetOptionalActive(titleText.gameObject, showTitle);
            SetOptionalActive(valueText.gameObject, showValue);
            SetOptionalActive(iconImage.gameObject, showIcon);

            labelText.text = label ?? string.Empty;
            titleText.text = title ?? string.Empty;
            valueText.text = value ?? string.Empty;
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;

            switch (contentType)
            {
                case ContentType.Icon:
                    Center(iconImage.rectTransform, Mathf.Min(size.x, size.y) * 0.5f, Mathf.Min(size.x, size.y) * 0.5f, 0f, 2f);
                    break;
                case ContentType.IconLabel:
                    if (showIcon)
                    {
                        Center(iconImage.rectTransform, size.y * 0.45f, size.y * 0.45f, -size.x * 0.22f, 2f);
                        Stretch(labelText.rectTransform, size.x * 0.35f, 18f, 18f, 18f);
                        labelText.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                    else
                    {
                        Stretch(labelText.rectTransform, GetHorizontalTextPadding(), GetHorizontalTextPadding(), 14f, 16f);
                        labelText.alignment = TextAlignmentOptions.Center;
                    }

                    labelText.fontSize = GetFontSize() * 0.75f;
                    break;
                case ContentType.TitleIconLabel:
                    Stretch(titleText.rectTransform, 24f, 24f, 12f, size.y * 0.52f);
                    titleText.fontSize = 36f;
                    titleText.alignment = TextAlignmentOptions.Center;
                    if (showIcon)
                    {
                        Center(iconImage.rectTransform, size.y * 0.34f, size.y * 0.34f, -size.x * 0.12f, -size.y * 0.15f);
                        Stretch(valueText.rectTransform, size.x * 0.52f, 28f, size.y * 0.44f, 18f);
                        valueText.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                    else
                    {
                        Stretch(valueText.rectTransform, 28f, 28f, size.y * 0.44f, 18f);
                        valueText.alignment = TextAlignmentOptions.Center;
                    }

                    valueText.fontSize = 42f;
                    break;
                default:
                    Stretch(labelText.rectTransform, GetHorizontalTextPadding(), GetHorizontalTextPadding(), 14f, 16f);
                    labelText.fontSize = GetFontSize();
                    labelText.alignment = TextAlignmentOptions.Center;
                    break;
            }
        }

        private void ApplyBadge(Vector2 size)
        {
            SetOptionalActive(badgeRoot, badgeVisible);
            badgeText.text = badgeLabel ?? string.Empty;
            SetOptionalActive(badgeText.gameObject, !string.IsNullOrEmpty(badgeLabel));
            badgeImage.sprite = badgeSprite;
            badgeImage.type = Image.Type.Simple;
            badgeImage.color = Color.white;

            var badgeSize = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.34f, 24f, 48f);
            var badgeRect = (RectTransform)badgeRoot.transform;
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);
            badgeRect.anchoredPosition = new Vector2(-badgeSize * 0.25f, -badgeSize * 0.25f);
            Stretch(badgeText.rectTransform, 2f, 2f, 2f, 2f);
            badgeText.fontSize = Mathf.Max(14f, badgeSize * 0.48f);
            badgeText.alignment = TextAlignmentOptions.Center;
        }

        private void Play(SoundType soundType, AudioClip customSound)
        {
            var clip = ResolveSound(soundType, customSound);
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private float GetHorizontalTextPadding()
        {
            switch (frameType)
            {
                case FrameType.Button01:
                    return 8f;
                case FrameType.Button03:
                    return 18f;
                default:
                    return 12f;
            }
        }

        private float GetFontSize()
        {
            switch (sizeType)
            {
                case SizeType.Small:
                    return 29f;
                case SizeType.Large:
                    return 74f;
                case SizeType.IconSmall:
                case SizeType.IconMedium:
                    return 24f;
                default:
                    return 46f;
            }
        }

        private static AudioClip ResolveSound(SoundType soundType, AudioClip customSound)
        {
            if (soundType == SoundType.Custom)
            {
                return customSound;
            }

            var path = GetSoundResourcePath(soundType);
            return string.IsNullOrEmpty(path) ? null : Resources.Load<AudioClip>(path);
        }

        private static string GetSoundResourcePath(SoundType soundType)
        {
            switch (soundType)
            {
                case SoundType.Button1:
                    return "Sounds/Button-1";
                case SoundType.Button2:
                    return "Sounds/Button-2";
                case SoundType.Button3:
                    return "Sounds/Button-3";
                case SoundType.Button4:
                    return "Sounds/Button-4";
                case SoundType.Button5:
                    return "Sounds/Button-5";
                case SoundType.Select:
                    return "Sounds/Select";
                case SoundType.SelectHighTone:
                    return "Sounds/Select (High Tone)";
                case SoundType.Close:
                    return "Sounds/Close";
                case SoundType.Deep:
                    return "Sounds/Deep";
                case SoundType.Error:
                    return "Sounds/Error";
                case SoundType.Glitch:
                    return "Sounds/Glitch";
                default:
                    return string.Empty;
            }
        }

        private static Vector2 GetButtonSize(SizeType size)
        {
            switch (size)
            {
                case SizeType.Small:
                    return new Vector2(128f, 70f);
                case SizeType.Large:
                    return new Vector2(360f, 176f);
                case SizeType.IconSmall:
                    return new Vector2(86f, 86f);
                case SizeType.IconMedium:
                    return new Vector2(104f, 106f);
                case SizeType.Wide:
                    return new Vector2(236f, 130f);
                case SizeType.TitleWide:
                    return new Vector2(355f, 140f);
                default:
                    return new Vector2(322f, 130f);
            }
        }

        private static ColorSet GetColorSet(ColorType color)
        {
            switch (color)
            {
                case ColorType.Blue:
                    return new ColorSet(0x098AD3FF, 0x29ADE6FF, 0x42D7FFFF, 0x1EA5DEFF, 0x0679BEFF, true);
                case ColorType.Brown:
                    return new ColorSet(0xA57D42FF, 0xC69A4AFF, 0xE1B76EFF, 0xBA914BFF, 0x7B5531FF, true);
                case ColorType.Dark:
                    return new ColorSet(0x2D2C2DFF, 0x393C39FF, 0x525152FF, 0x363736FF, 0x212021FF, false);
                case ColorType.Gray:
                    return new ColorSet(0x524B4AFF, 0x6B5E5AFF, 0x7B716CFF, 0x635852FF, 0x393031FF, true);
                case ColorType.Green:
                    return new ColorSet(0x6EAA3CFF, 0xA5CE31FF, 0xCCDD5AFF, 0x84B735FF, 0x529220FF, true);
                case ColorType.Mint:
                    return new ColorSet(0x06AB9AFF, 0x10D79CFF, 0x27E7ACFF, 0x0ABE94FF, 0x008A73FF, true);
                case ColorType.OffWhite:
                    return new ColorSet(0xD6D4D3FF, 0xFFFFFFFF, 0xFFFFFFFF, 0xE7E7E6FF, 0xB0A9A8FF, true);
                case ColorType.Orange:
                    return new ColorSet(0xF8653AFF, 0xF79E63FF, 0xFFB673FF, 0xFC7F42FF, 0xD65131FF, true);
                case ColorType.Pink:
                    return new ColorSet(0xBD41B5FF, 0xDE65D6FF, 0xEF71E7FF, 0xCE55C5FF, 0x942F9CFF, true);
                case ColorType.Plum:
                    return new ColorSet(0x8827F3FF, 0xB545F8FF, 0xC66DFFFF, 0xA037F9FF, 0x6A00DEFF, true);
                case ColorType.Yellow:
                    return new ColorSet(0xFB9E00FF, 0xFFCF19FF, 0xFFE928FF, 0xFFBE11FF, 0xD67D06FF, true);
                default:
                    return new ColorSet(0xCE2A42FF, 0xF74C63FF, 0xAD4D4AFF, 0x7D3233FF, 0x632022FF, true);
            }
        }

        private static void SetImage(Image image, Sprite sprite, Color color, bool active, Image.Type type)
        {
            image.gameObject.SetActive(active);
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.raycastTarget = false;
            image.preserveAspect = false;
        }

        private static void Stretch(RectTransform rectTransform, float left, float right, float top, float bottom)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private static void Center(RectTransform rectTransform, float width, float height, float x, float y)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.anchoredPosition = new Vector2(x, y);
        }

        private static void SetTopLeft(RectTransform rectTransform, float width, float height, float x, float y)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.anchoredPosition = new Vector2(x, -y);
        }

        private static void SetOptionalActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private readonly struct ColorSet
        {
            public readonly Color32 Background;
            public readonly Color32 Light;
            public readonly Color32 Highlight;
            public readonly Color32 Gradient;
            public readonly Color32 Shadow;
            public readonly bool Button01InnerVisible;

            public ColorSet(uint background, uint light, uint highlight, uint gradient, uint shadow, bool button01InnerVisible)
            {
                Background = ToColor(background);
                Light = ToColor(light);
                Highlight = ToColor(highlight);
                Gradient = ToColor(gradient);
                Shadow = ToColor(shadow);
                Button01InnerVisible = button01InnerVisible;
            }

            private static Color32 ToColor(uint value)
            {
                return new Color32(
                    (byte)((value >> 24) & 0xFF),
                    (byte)((value >> 16) & 0xFF),
                    (byte)((value >> 8) & 0xFF),
                    (byte)(value & 0xFF));
            }
        }

#if UNITY_EDITOR
        private bool HasRequiredVisualReferences()
        {
            return visualRoot != null &&
                backgroundImage != null &&
                lightImage != null &&
                highLightImage != null &&
                gradientImage != null &&
                shadowImage != null &&
                iconImage != null &&
                labelText != null &&
                titleText != null &&
                valueText != null &&
                badgeRoot != null &&
                badgeText != null &&
                badgeImage != null;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!HasRequiredVisualReferences())
            {
                return;
            }

            QueueApplyVisual();
        }

        private void QueueApplyVisual()
        {
            if (_applyVisualQueued)
            {
                return;
            }

            _applyVisualQueued = true;
            UnityEditor.EditorApplication.delayCall += ApplyVisualAfterValidate;
        }

        private void ApplyVisualAfterValidate()
        {
            _applyVisualQueued = false;
            if (this == null || !HasRequiredVisualReferences())
            {
                return;
            }

            ApplyVisual();
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
