using Assets.Scripts.Core;
using Assets.Scripts.UI.Dialog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LayerLab.ArtMakerUnity
{
    public class PartsSlot : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField] private UICategory uiCategory;
        [SerializeField] private Image imageIcon;
        [SerializeField] private Image imageBg;
        [SerializeField] private Image imageItem;

        public UICategory UICategory => uiCategory;

        private PartsManager _partsManager;
        private AvatarEditorPresenter _presenter;
        private Sprite[] _bgSprites;
        private bool _hasItem;

        private void OnValidate()
        {
            imageBg ??= GetComponent<Image>();
            imageIcon ??= transform.Find("Icon")?.GetComponent<Image>();
            imageItem ??= transform.Find("Item")?.GetComponent<Image>();
        }

        public void Init(PartsManager pm, Sprite[] bgSprites, AvatarEditorPresenter presenter = null)
        {
            _partsManager = pm;
            _presenter = presenter;
            _bgSprites = bgSprites;

            if (_partsManager == null) return;

            _partsManager.OnPartsChanged += OnPartsChanged;
            _partsManager.OnColorChanged += OnColorChanged;

            RefreshDisplay();
        }

        private void OnDestroy()
        {
            if (_partsManager == null) return;
            _partsManager.OnPartsChanged -= OnPartsChanged;
            _partsManager.OnColorChanged -= OnColorChanged;
        }

        private void OnPartsChanged(PartsType type, int index)
        {
            if (IsRelevantType(type))
                RefreshDisplay();
        }

        private void OnColorChanged(ColorTargetType target, Color color)
        {
            if (_presenter != null)
            {
                RefreshDisplay();
                return;
            }

            if (uiCategory == UICategory.Skin && target == ColorTargetType.Skin)
            {
                ApplyItemColor(color);
                return;
            }

            var subTypes = UICategoryConfig.GetSubTypes(uiCategory);
            if (subTypes.Length == 1 && _partsManager.CanChangeColor(subTypes[0]) &&
                _partsManager.GetColorTarget(subTypes[0]) == target)
            {
                ApplyItemColor(color);
            }
        }

        private bool IsRelevantType(PartsType type)
        {
            var subTypes = UICategoryConfig.GetSubTypes(uiCategory);
            foreach (var st in subTypes)
            {
                if (st == type) return true;
            }
            return false;
        }

        private void RefreshDisplay()
        {
            if (_presenter != null)
            {
                RefreshDisplayFromPresenter();
                return;
            }

            if (_partsManager == null) return;

            var subTypes = UICategoryConfig.GetSubTypes(uiCategory);
            bool hasItem = false;

            // Group category
            if (UICategoryConfig.IsGroup(uiCategory))
            {
                foreach (var type in subTypes)
                {
                    if (!_partsManager.IsEquipped(type)) continue;

                    int idx = _partsManager.GetActiveIndex(type);
                    Sprite thumb = _partsManager.GetThumbnail(type, idx);

                    if (imageItem != null)
                    {
                        imageItem.sprite = thumb;
                        imageItem.SetNativeSize();
                    }

                    hasItem = true;
                    break;
                }

                ApplyItemColor(Color.white);
            }
            // Single type
            else if (subTypes.Length == 1)
            {
                var partsType = subTypes[0];
                bool equipped = _partsManager.IsEquipped(partsType);

                if (equipped)
                {
                    int idx = _partsManager.GetActiveIndex(partsType);
                    Sprite thumb = _partsManager.GetThumbnail(partsType, idx);

                    if (imageItem != null)
                    {
                        imageItem.sprite = thumb;
                        imageItem.SetNativeSize();
                    }

                    hasItem = true;

                    if (_partsManager.CanChangeColor(partsType))
                    {
                        var target = _partsManager.GetColorTarget(partsType);
                        ApplyItemColor(_partsManager.GetColor(target));
                    }
                    else
                    {
                        ApplyItemColor(Color.white);
                    }
                }
            }

            _hasItem = hasItem;

            if (imageIcon != null) imageIcon.gameObject.SetActive(!hasItem);
            if (imageItem != null) imageItem.gameObject.SetActive(hasItem);

            UpdateBg();
            UpdateItemAlpha();
        }

        private void RefreshDisplayFromPresenter()
        {
            var section = _presenter?.GetSection(uiCategory);
            var selectedIcon = section?.SelectedIcon;
            _hasItem = section != null && section.HasSelection;

            if (imageItem != null)
            {
                imageItem.sprite = selectedIcon;
                if (imageItem.sprite != null)
                    imageItem.SetNativeSize();
            }

            if (section != null &&
                section.SupportsColor &&
                ColorUtility.TryParseHtmlString(section.CurrentColorHtml, out var parsedColor))
            {
                ApplyItemColor(parsedColor);
            }
            else
            {
                ApplyItemColor(Color.white);
            }

            bool hasDisplaySprite = imageItem != null && imageItem.sprite != null;
            if (imageIcon != null)
                imageIcon.gameObject.SetActive(!_hasItem || !hasDisplaySprite);
            if (imageItem != null)
                imageItem.gameObject.SetActive(_hasItem && hasDisplaySprite);

            UpdateBg();
            UpdateItemAlpha();
        }

        private void ApplyItemColor(Color color)
        {
            if (imageItem == null) return;
            color.a = imageItem.color.a;
            imageItem.color = color;
        }

        private void UpdateBg()
        {
            if (imageBg == null || _bgSprites == null || _bgSprites.Length < 2) return;

            imageBg.sprite = _hasItem ? _bgSprites[1] : _bgSprites[0];
        }

        private void UpdateItemAlpha()
        {
            if (imageItem == null) return;
            var c = imageItem.color;
            c.a = 1f;
            imageItem.color = c;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PanelPartsControl.SelectSlot(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PanelPartsControl.UnfocusSlot(this);
        }
    }
}
