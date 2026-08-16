using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentTabView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject focusState;
        [SerializeField] private TextMeshProUGUI normalLabel;
        [SerializeField] private TextMeshProUGUI focusLabel;
        [SerializeField] private string label;

        private UnityAction _clickHandler;

        public void Bind(StatusEquipmentCategory category, bool enabled, Action<StatusEquipmentCategory> onClicked)
        {
            label = BuildLabel(category);
            SetLabel(label);
            gameObject.SetActive(enabled);

            if (button == null)
            {
                return;
            }

            if (_clickHandler != null)
            {
                button.onClick.RemoveListener(_clickHandler);
            }

            _clickHandler = () => onClicked?.Invoke(category);
            button.onClick.AddListener(_clickHandler);
        }

        public void SetSelected(bool selected)
        {
            if (focusState != null)
            {
                focusState.SetActive(selected);
            }
        }

        private void SetLabel(string value)
        {
            if (normalLabel != null)
            {
                normalLabel.text = value;
            }

            if (focusLabel != null)
            {
                focusLabel.text = value;
            }
        }

        private static string BuildLabel(StatusEquipmentCategory category)
        {
            return category switch
            {
                StatusEquipmentCategory.All => "All",
                StatusEquipmentCategory.Weapon => "Weapon",
                StatusEquipmentCategory.Shield => "Shield",
                StatusEquipmentCategory.Armor => "Armor",
                StatusEquipmentCategory.Headgear => "Head",
                StatusEquipmentCategory.Item => "Item",
                _ => category.ToString()
            };
        }

        private void OnDestroy()
        {
            if (button != null && _clickHandler != null)
            {
                button.onClick.RemoveListener(_clickHandler);
            }
        }

        private void Reset()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
            SetLabel(string.IsNullOrWhiteSpace(label) ? gameObject.name : label);
        }

        private void ResolveReferences()
        {
            button ??= GetComponentInChildren<Button>(true);
            focusState ??= transform.Find($"{gameObject.name}_Focus")?.gameObject
                           ?? transform.Find("Tab_01_Focus")?.gameObject
                           ?? transform.Find("Tab_02_Focus")?.gameObject
                           ?? transform.Find("Tab_03_Focus")?.gameObject
                           ?? transform.Find("Tab_04_Focus")?.gameObject
                           ?? transform.Find("Tab_05_Focus")?.gameObject
                           ?? transform.Find("Tab_06_Focus")?.gameObject;

            if (normalLabel == null)
            {
                var normal = transform.Find($"{gameObject.name}_Normal");
                normalLabel = normal != null
                    ? normal.GetComponentInChildren<TextMeshProUGUI>(true)
                    : null;
            }

            if (focusLabel == null && focusState != null)
            {
                focusLabel = focusState.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
    }
}
