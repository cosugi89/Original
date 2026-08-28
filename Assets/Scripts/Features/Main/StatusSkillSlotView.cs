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
        private const float SkillIconFramePadding = 18f;
        private const float MinimumSkillIconSize = 1f;

        [SerializeField] private int slotIndex;
        [SerializeField] private GameObject frameRoot;
        [SerializeField] private GameObject normalState;
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

        public void Bind(Action<int> onClicked)
        {
            _onClicked = onClicked;
        }

        public void Refresh(BattleSkillData skill, bool selected)
        {
            var hasSelection = skill != null && skill.SkillId > 0;

            frameRoot.SetActive(true);
            normalState.SetActive(true);
            typeIconRoot.SetActive(true);

            var icon = hasSelection && skill.Icon != null ? skill.Icon : fallbackIcon;
            skillImage.gameObject.SetActive(true);
            skillImage.sprite = icon;
            skillImage.enabled = icon != null;
            skillImage.color = Color.white;
            skillImage.preserveAspect = true;
            ApplySkillImageLayout();

            label.gameObject.SetActive(true);
            label.text = hasSelection ? BuildDisplayName(skill) : "Empty";

            focusState.SetActive(selected);
            lockState.SetActive(false);
            addOneState.SetActive(!hasSelection);
            addTwoState.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(slotIndex);
        }

        private void ApplySkillImageLayout()
        {
            var frameRect = ((RectTransform)frameRoot.transform).rect;
            var iconSize = Mathf.Max(
                MinimumSkillIconSize,
                Mathf.Min(frameRect.width, frameRect.height) - SkillIconFramePadding * 2f);
            var rectTransform = skillImage.rectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        private static string BuildDisplayName(BattleSkillData skill)
        {
            if (!string.IsNullOrWhiteSpace(skill.DisplayName))
            {
                return skill.DisplayName;
            }

            return skill.SkillId > 0 ? $"Skill {skill.SkillId}" : "Empty";
        }
    }
}
