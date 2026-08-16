using System;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private StatusEquipmentCategory category;
        [SerializeField] private GameObject frameRoot;
        [SerializeField] private GameObject normalState;
        [SerializeField] private Transform normalFrame;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject focusState;
        [SerializeField] private GameObject lockState;
        [SerializeField] private GameObject addOneState;
        [SerializeField] private GameObject addTwoState;
        [SerializeField] private GameObject typeIconRoot;
        [SerializeField] private Sprite fallbackIcon;

        private Action<StatusEquipmentCategory> _onClicked;

        public StatusEquipmentCategory Category => category;

        public void BindCategory(StatusEquipmentCategory value, Action<StatusEquipmentCategory> onClicked)
        {
            category = value;
            _onClicked = onClicked;
        }

        public void Refresh(AvatarPartSectionViewData section, bool selected)
        {
            ResolveReferences();
            var hasSelection = section != null && section.HasSelection;

            SetActive(frameRoot, true);
            SetActive(normalState, true);
            SetActive(typeIconRoot, true);
            EnsureNormalFrame();

            if (itemImage != null)
            {
                var icon = hasSelection && section.SelectedIcon != null ? section.SelectedIcon : fallbackIcon;
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = icon;
                itemImage.enabled = icon != null;
                itemImage.color = Color.white;
                itemImage.preserveAspect = true;
            }

            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.text = BuildDisplayName(section, hasSelection, category);
            }

            SetActive(focusState, selected);
            SetActive(lockState, false);
            SetActive(addOneState, !hasSelection);
            SetActive(addTwoState, false);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(category);
        }

        private static string BuildDisplayName(
            AvatarPartSectionViewData section,
            bool hasSelection,
            StatusEquipmentCategory category)
        {
            if (!hasSelection)
            {
                return BuildEmptyLabel(category);
            }

            if (!string.IsNullOrWhiteSpace(section.SelectedDisplayName))
            {
                return section.SelectedDisplayName;
            }

            return section.SelectedEquipmentId > 0 ? $"Equipment {section.SelectedEquipmentId}" : BuildEmptyLabel(category);
        }

        private static string BuildEmptyLabel(StatusEquipmentCategory category)
        {
            return category switch
            {
                StatusEquipmentCategory.Weapon => "Weapon",
                StatusEquipmentCategory.Shield => "Shield",
                StatusEquipmentCategory.Armor => "Armor",
                StatusEquipmentCategory.Headgear => "Head",
                _ => string.Empty
            };
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private void Reset()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            frameRoot ??= transform.Find("ItemFrame_01")?.gameObject;
            normalState ??= transform.Find("ItemFrame_01/NormalArea")?.gameObject;
            normalFrame ??= FindNormalFrame(normalState != null ? normalState.transform : null);
            itemImage ??= transform.Find("ItemFrame_01/Item")?.GetComponent<Image>();
            label ??= transform.Find("Text_Level")?.GetComponent<TextMeshProUGUI>();
            focusState ??= transform.Find("ItemFrame_01/Focus")?.gameObject;
            lockState ??= transform.Find("ItemFrame_01/Lock")?.gameObject;
            addOneState ??= transform.Find("ItemFrame_01/Add_1")?.gameObject;
            addTwoState ??= transform.Find("ItemFrame_01/Add_2")?.gameObject;
            typeIconRoot ??= transform.Find("BasicFrame_Diamond_H48_NoBorder_Plum")?.gameObject;
            CaptureFallbackIcon();
        }

        private void EnsureNormalFrame()
        {
            if (normalState == null)
            {
                return;
            }

            normalFrame ??= FindNormalFrame(normalState.transform);
            if (normalFrame == null)
            {
                var template = FindSiblingNormalFrameTemplate();
                if (template != null)
                {
                    normalFrame = Instantiate(template.gameObject, normalState.transform, false).transform;
                    normalFrame.gameObject.name = template.gameObject.name;
                }
            }

            if (normalFrame == null)
            {
                return;
            }

            normalFrame.gameObject.SetActive(true);
            var images = normalFrame.GetComponentsInChildren<Image>(true);
            for (var i = 0; i < images.Length; i++)
            {
                images[i].enabled = true;
            }
        }

        private Transform FindSiblingNormalFrameTemplate()
        {
            var parent = transform.parent;
            if (parent == null)
            {
                return null;
            }

            foreach (Transform sibling in parent)
            {
                if (sibling == transform)
                {
                    continue;
                }

                var siblingNormalArea = sibling.Find("ItemFrame_01/NormalArea");
                var candidate = FindNormalFrame(siblingNormalArea);
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Transform FindNormalFrame(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root)
            {
                if (child.name.StartsWith("ItemFrame_01_Normal", StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private void CaptureFallbackIcon()
        {
            if (fallbackIcon != null)
            {
                return;
            }

            fallbackIcon = transform.Find("BasicFrame_Diamond_H48_NoBorder_Plum/Icon")?.GetComponent<Image>()?.sprite;
            if (fallbackIcon == null && itemImage != null && itemImage.sprite != null)
            {
                fallbackIcon = itemImage.sprite;
            }
        }
    }
}
