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
        private Sprite[] _bgSprites;
        private bool _hasItem;

        private void OnValidate()
        {
            imageBg ??= GetComponent<Image>();
            imageIcon ??= transform.Find("Icon")?.GetComponent<Image>();
            imageItem ??= transform.Find("Item")?.GetComponent<Image>();
        }

        public void Init(PartsManager pm, Sprite[] bgSprites)
        {
            _partsManager = pm;
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
