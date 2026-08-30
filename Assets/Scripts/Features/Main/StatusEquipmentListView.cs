using System;
using System.Collections.Generic;
using Assets.Scripts.UI.Dialog;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentListView : MonoBehaviour
    {
        [SerializeField] private StatusEquipmentCategory category;
        [SerializeField] private Transform content;
        [SerializeField] private StatusEquipmentListItemView itemTemplate;

        private readonly List<StatusEquipmentListItemView> _itemViews = new();
        private bool _isInitialized;

        public StatusEquipmentCategory Category => category;

        public void Refresh(
            IReadOnlyList<AvatarPartOptionViewData> options,
            Action<AvatarPartOptionViewData> onItemClicked,
            Action onEmptyClicked)
        {
            if (!_isInitialized)
            {
                itemTemplate.gameObject.SetActive(false);
                _isInitialized = true;
            }

            var itemCount = options.Count + (onEmptyClicked != null ? 1 : 0);
            while (_itemViews.Count < itemCount)
            {
                _itemViews.Add(Instantiate(itemTemplate, content));
            }

            for (var i = 0; i < _itemViews.Count; i++)
            {
                if (onEmptyClicked != null && i == 0)
                {
                    _itemViews[i].BindEmpty(onEmptyClicked);
                    continue;
                }

                var optionIndex = i - (onEmptyClicked != null ? 1 : 0);
                _itemViews[i].Bind(optionIndex < options.Count ? options[optionIndex] : null, onItemClicked);
            }
        }
    }
}
