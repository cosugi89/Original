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

        private string FractionText(string numerator, string Denominator) => $"{numerator}/{Denominator}";
        private string MaxText => "MAX";

        public void ShowNormal()
        {
            
        }

        public void ShowFocused()
        {

        }

        public void ShowSpecial()
        {

        }

        public void ShowDisabled()
        {
        }

        public void ShowEmpty() { }


        public void ShowLocked()
        {

        }

    }
}
