using TMPro;
using Assets.Scripts.Core.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public enum SkillItemType
    {
        Weapon,
        Shield,
        Armor,
        Headgear
    }

    public sealed class PartsSkillItem : MonoBehaviour, IItemStateView
    {
        private const float SkillIconFramePadding = 18f;
        private const float MinimumSkillIconSize = 1f;

        private enum State
        {
            Normal,
            Locked,
            Disabled,
            Empty
        }

        [SerializeField] private RectTransform frameRoot;
        [SerializeField] private GameObject normalState;
        [SerializeField] private Image skillImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject focusState;
        [SerializeField] private GameObject lockState;
        [SerializeField] private GameObject disableState;
        [FormerlySerializedAs("addOneState")]
        [SerializeField] private GameObject emptyState;
        [SerializeField] private GameObject typeArea;
        [SerializeField] private GameObject attackTypeIcon;
        [SerializeField] private GameObject shieldTypeIcon;
        [SerializeField] private GameObject armorTypeIcon;
        [SerializeField] private GameObject headgearTypeIcon;
        [SerializeField] private Sprite fallbackIcon;

        private State _state;
        private bool _isSkillImageLayoutPending;

        public void ShowSkill(SkillItemType type, Sprite icon, string displayName)
        {
            ShowNormal();
            SetTypeIcon(type);
            SetSkill(icon);
            label.text = displayName ?? string.Empty;
        }

        public void ShowSkill(Sprite icon, string displayName)
        {
            ShowNormal();
            HideTypeIcons();
            SetSkill(icon);
            label.text = displayName ?? string.Empty;
        }

        public void ShowEmpty(string displayName)
        {
            ShowEmpty();
            HideTypeIcons();
            SetSkill(fallbackIcon);
            label.text = displayName ?? string.Empty;
        }

        public void ShowLocked()
        {
            SetState(State.Locked);
        }

        public void ShowDisabled()
        {
            SetState(State.Disabled);
        }

        public void ShowNormal()
        {
            SetState(State.Normal);
        }

        public void ShowEmpty()
        {
            SetState(State.Empty);
        }

        public void SetFocused(bool isFocused)
        {
            focusState.SetActive(isFocused);
        }

        private void SetState(State state)
        {
            _state = state;

            normalState.SetActive(state is State.Normal or State.Locked or State.Disabled);
            lockState.SetActive(state == State.Locked);
            disableState.SetActive(state == State.Disabled);
            emptyState.SetActive(state == State.Empty);
            focusState.SetActive(false);
            HideTypeIcons();
        }

        private void SetSkill(Sprite icon)
        {
            skillImage.sprite = icon != null ? icon : fallbackIcon;
            skillImage.enabled = skillImage.sprite != null;
            skillImage.color = Color.white;
            skillImage.preserveAspect = true;
            _isSkillImageLayoutPending = true;
            ApplySkillImageLayout();
        }

        private void SetTypeIcon(SkillItemType type)
        {
            HideTypeIcons();
            typeArea.SetActive(true);

            switch (type)
            {
                case SkillItemType.Weapon:
                    attackTypeIcon.SetActive(true);
                    return;
                case SkillItemType.Shield:
                    shieldTypeIcon.SetActive(true);
                    return;
                case SkillItemType.Armor:
                    armorTypeIcon.SetActive(true);
                    return;
                case SkillItemType.Headgear:
                    headgearTypeIcon.SetActive(true);
                    return;
            }
        }

        private void HideTypeIcons()
        {
            typeArea.SetActive(false);
            attackTypeIcon.SetActive(false);
            shieldTypeIcon.SetActive(false);
            armorTypeIcon.SetActive(false);
            headgearTypeIcon.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_isSkillImageLayoutPending)
            {
                ApplySkillImageLayout();
            }
        }

        private void ApplySkillImageLayout()
        {
            var frameRect = frameRoot.rect;
            if (frameRect.width <= SkillIconFramePadding * 2f ||
                frameRect.height <= SkillIconFramePadding * 2f)
            {
                return;
            }

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
            _isSkillImageLayoutPending = false;
        }
    }
}
