using System.ComponentModel;
using Assets.Scripts.Core.Const;
using Assets.Scripts.Core.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    public class PartsTraitItem : MonoBehaviour, IItemStateView
    {
        private enum State
        {
            Normal,
            Focused,
            Special,
            Disabled
        }

        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image iconImage;

        [Header("State Objects")]
        [SerializeField] private GameObject alertIcon;
        [SerializeField] private GameObject upIcon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject focusedState;
        [SerializeField] private GameObject specialState;
        [SerializeField] private GameObject disabledState;

        private const string MaxText = "MAX";
        public void SetContent(Sprite icon, int currentLevel, int maxLevel)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
            
            text.text = currentLevel == maxLevel
                ? MaxText
                : $"{Mathf.Clamp(currentLevel, 0, Mathf.Max(0, maxLevel))}/{Mathf.Max(0, maxLevel)}";
        }

        public void SetAlertVisible(bool isVisible) => alertIcon.SetActive(isVisible);
        public void SetUpgradeMarkerVisible(bool isVisible)  => upIcon.SetActive(isVisible);
        public void ShowNormal() => SetState(State.Normal, true);
        public void ShowFocused() => SetState(State.Focused, true);
        public void ShowSpecial() => SetState(State.Special, true);
        public void ShowDisabled()  => SetState(State.Disabled, false);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ShowLocked()  => ShowDisabled(); // Interface で用意する必要があるため
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ShowEmpty() => ShowDisabled(); // Interface で用意する必要があるため

        private void SetState(State state, bool isInteractable)
        {
            text.gameObject.SetActive(state != State.Disabled);
            normalState.SetActive(state == State.Normal);
            focusedState.SetActive(state == State.Focused);
            specialState.SetActive(state == State.Special);
            disabledState.SetActive(state == State.Disabled);
            lockIcon.SetActive(state == State.Disabled);
            iconImage.color = state == State.Disabled ? UIColors.TraitTree.DisableIconColor : UIColors.White;
            button.interactable = isInteractable;
        }
    }
}
