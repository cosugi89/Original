using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentTabView : MonoBehaviour
    {
        [SerializeField] private StatusEquipmentCategory category;
        [SerializeField] private Button button;
        [SerializeField] private GameObject focusState;

        private UnityAction _clickHandler;

        public StatusEquipmentCategory Category => category;

        public void Bind(bool visible, Action<StatusEquipmentCategory> onClicked)
        {
            gameObject.SetActive(visible);

            if (_clickHandler != null)
            {
                button.onClick.RemoveListener(_clickHandler);
            }

            _clickHandler = () => onClicked(category);
            button.onClick.AddListener(_clickHandler);
        }

        public void SetSelected(bool selected)
        {
            focusState.SetActive(selected);
        }

        private void OnDestroy()
        {
            if (_clickHandler != null)
            {
                button.onClick.RemoveListener(_clickHandler);
            }
        }

    }
}
