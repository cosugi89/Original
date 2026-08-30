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
            Action<AvatarPartOptionViewData> onItemClicked)
        {
            if (!_isInitialized)
            {
                itemTemplate.gameObject.SetActive(false);
                _isInitialized = true;
            }

            while (_itemViews.Count < options.Count)
            {
                _itemViews.Add(Instantiate(itemTemplate, content));
            }

            for (var i = 0; i < _itemViews.Count; i++)
            {
                _itemViews[i].Bind(i < options.Count ? options[i] : null, onItemClicked);
            }
        }
    }
}
