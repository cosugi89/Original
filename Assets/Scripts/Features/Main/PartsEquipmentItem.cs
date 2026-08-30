using System;
using Assets.Scripts.Core.Const;
using Assets.Scripts.Core.Interfaces;
using LayerLab.ArtMakerUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class PartsEquipmentItem : MonoBehaviour, IItemStateView
    {
        private enum State
        {
            Normal,
            Locked,
            Disabled,
            Empty
        }

        [SerializeField] private GameObject normalState;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject focusState;
        [SerializeField] private GameObject checkState;
        [SerializeField] private GameObject lockState;
        [SerializeField] private GameObject disableState;
        [FormerlySerializedAs("addOneState")]
        [SerializeField] private GameObject emptyState;
        [SerializeField] private GameObject specialBorder;
        [SerializeField] private GameObject typeArea;

        [Header("Type Icons")]
        [SerializeField] private GameObject weaponIcon;
        [SerializeField] private GameObject shieldIcon;
        [SerializeField] private GameObject armorIcon;
        [SerializeField] private GameObject headgearIcon;

        [Header("Normal Frame Colors")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image innerColor1Image;
        [SerializeField] private Image glowImage;

        private State _state;

        public void ShowNormal()
        {
            SetState(State.Normal);
        }

        public void ShowDisabled()
        {
            SetState(State.Disabled);
        }

        public void ShowEmpty()
        {
            SetState(State.Empty);
        }

        public void ShowLocked()
        {
            SetState(State.Locked);
        }

        private void SetState(State state)
        {
            _state = state;

            normalState.SetActive(state is State.Normal or State.Locked or State.Disabled);
            lockState.SetActive(state == State.Locked);
            disableState.SetActive(state == State.Disabled);
            emptyState.SetActive(state == State.Empty);
            checkState.SetActive(false);
            focusState.SetActive(false);

            if (state == State.Empty)
            {
                itemImage.gameObject.SetActive(false);
                label.gameObject.SetActive(false);
                specialBorder.SetActive(false);
                typeArea.SetActive(false);
                weaponIcon.SetActive(false);
                shieldIcon.SetActive(false);
                armorIcon.SetActive(false);
                headgearIcon.SetActive(false);
            }
        }

        public void Setup(
            UICategory category,
            Sprite icon,
            string displayName,
            bool specialBorderVisible,
            UIColors.IColorSet colorSet)
        {
            if (_state != State.Normal)
            {
                return;
            }

            ApplyNormalFrameColors(colorSet);
            typeArea.SetActive(true);
            SetTypeIcon(category);

            itemImage.gameObject.SetActive(true);
            itemImage.sprite = icon;
            itemImage.enabled = icon != null;
            itemImage.color = Color.white;
            itemImage.preserveAspect = true;

            label.gameObject.SetActive(true);
            label.text = displayName;
            specialBorder.SetActive(specialBorderVisible);
        }

        public void SetFocused(bool isFocused)
        {
            focusState.SetActive(isFocused);
        }

        public void SetChecked(bool isChecked)
        {
            checkState.SetActive(isChecked);
        }

        private void ApplyNormalFrameColors(UIColors.IColorSet colorSet)
        {
            backgroundImage.color = colorSet.Background;
            innerColor1Image.color = colorSet.InnerColor1;
            glowImage.color = colorSet.Glow;
        }

        private void SetTypeIcon(UICategory category)
        {
            weaponIcon.SetActive(category == UICategory.HandRight);
            shieldIcon.SetActive(category == UICategory.HandLeft);
            armorIcon.SetActive(category == UICategory.Chest);
            headgearIcon.SetActive(category == UICategory.Helmet);

            if (category is not UICategory.HandRight and not UICategory.HandLeft and not UICategory.Chest and not UICategory.Helmet)
            {
                throw new ArgumentOutOfRangeException(nameof(category), category, "Unsupported equipment category.");
            }
        }
    }
}
