using System;
using Assets.Scripts.Data.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusSkillSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private GameObject frameRoot;
        [SerializeField] private GameObject normalState;
        [SerializeField] private Transform normalFrame;
        [SerializeField] private Image skillImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject focusState;
        [SerializeField] private GameObject lockState;
        [SerializeField] private GameObject addOneState;
        [SerializeField] private GameObject addTwoState;
        [SerializeField] private GameObject typeIconRoot;
        [SerializeField] private Sprite fallbackIcon;

        private Action<int> _onClicked;

        public int SlotIndex => slotIndex;

        public void BindSlot(int index, Action<int> onClicked)
        {
            slotIndex = index;
            _onClicked = onClicked;
        }

        public void Refresh(BattleSkillData skill, bool selected)
        {
            ResolveReferences();
            var hasSelection = skill != null && skill.SkillId > 0;

            SetActive(frameRoot, true);
            SetActive(normalState, true);
            SetActive(typeIconRoot, true);
            EnsureNormalFrame();

            var icon = hasSelection && skill.Icon != null ? skill.Icon : fallbackIcon;
            skillImage.gameObject.SetActive(true);
            skillImage.sprite = icon;
            skillImage.enabled = icon != null;
            skillImage.color = Color.white;
            skillImage.preserveAspect = true;

            label.gameObject.SetActive(true);
            label.text = hasSelection ? BuildDisplayName(skill) : "Empty";

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
            _onClicked?.Invoke(slotIndex);
        }

        private static string BuildDisplayName(BattleSkillData skill)
        {
            if (!string.IsNullOrWhiteSpace(skill.DisplayName))
            {
                return skill.DisplayName;
            }

            return skill.SkillId > 0 ? $"Skill {skill.SkillId}" : "Empty";
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
            skillImage ??= transform.Find("ItemFrame_01/Item")?.GetComponent<Image>();
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
            if (fallbackIcon == null && skillImage != null && skillImage.sprite != null)
            {
                fallbackIcon = skillImage.sprite;
            }
        }
    }
}
