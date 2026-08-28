using System;
using System.Collections.Generic;
using Assets.Scripts.UI.Dialog;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentListView : MonoBehaviour
    {
        [SerializeField] private StatusEquipmentCategory category;
        [SerializeField] private StatusEquipmentListItemView[] itemViews;

        private readonly List<StatusEquipmentListItemView> _generatedItemViews = new();
        private bool _isInitialized;

        public StatusEquipmentCategory Category => category;

        public void Refresh(
            IReadOnlyList<AvatarPartOptionViewData> options,
            Action<AvatarPartOptionViewData> onItemClicked)
        {
            if (!_isInitialized)
            {
                var template = itemViews[0];
                var content = template.transform.parent;
                template.gameObject.SetActive(false);

                for (var i = 1; i < itemViews.Length; i++)
                {
                    itemViews[i].gameObject.SetActive(false);
                }

                for (var i = 0; i < options.Count; i++)
                {
                    _generatedItemViews.Add(Instantiate(template, content));
                }

                _isInitialized = true;
            }

            while (_generatedItemViews.Count < options.Count)
            {
                _generatedItemViews.Add(Instantiate(itemViews[0], itemViews[0].transform.parent));
            }

            for (var i = 0; i < _generatedItemViews.Count; i++)
            {
                var option = i < options.Count ? options[i] : null;
                _generatedItemViews[i].Bind(option, onItemClicked);
            }
        }
    }
}
