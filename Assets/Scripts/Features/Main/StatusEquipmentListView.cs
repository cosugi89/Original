using System;
using System.Collections.Generic;
using Assets.Scripts.UI.Dialog;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentListView : MonoBehaviour
    {
        [SerializeField] private Transform contentParent;
        [SerializeField] private StatusEquipmentListItemView itemTemplate;

        private readonly List<StatusEquipmentListItemView> _items = new();

        public void Refresh(
            IReadOnlyList<AvatarPartOptionViewData> options,
            Action<AvatarPartOptionViewData> onItemClicked)
        {
            EnsurePool(options?.Count ?? 0);

            for (var i = 0; i < _items.Count; i++)
            {
                var option = options != null && i < options.Count ? options[i] : null;
                _items[i].Bind(option, onItemClicked);
            }
        }

        private void EnsurePool(int count)
        {
            ResolveReferences(allowComponentCreation: true);
            if (contentParent == null || itemTemplate == null)
            {
                return;
            }

            CollectExistingItems();

            while (_items.Count < count)
            {
                var created = Instantiate(itemTemplate, contentParent);
                created.gameObject.name = itemTemplate.gameObject.name;
                _items.Add(created);
            }
        }

        private void CollectExistingItems()
        {
            if (_items.Count > 0 || contentParent == null || itemTemplate == null)
            {
                return;
            }

            foreach (Transform child in contentParent)
            {
                if (child == itemTemplate.transform)
                {
                    _items.Add(itemTemplate);
                    continue;
                }

                if (IsSceneSampleItem(child))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static bool IsSceneSampleItem(Transform child)
        {
            return child.GetComponent<StatusEquipmentListItemView>() != null ||
                   child.name.StartsWith("ListItem_EquipMent", StringComparison.Ordinal);
        }

        private void Reset()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences(bool allowComponentCreation = false)
        {
            if (contentParent == null)
            {
                contentParent = transform.Find("Viewport/Content");
            }

            if (itemTemplate == null && contentParent != null && contentParent.childCount > 0)
            {
                itemTemplate = contentParent.GetChild(0).GetComponent<StatusEquipmentListItemView>();
                if (itemTemplate == null && allowComponentCreation)
                {
                    itemTemplate = contentParent.GetChild(0).gameObject.AddComponent<StatusEquipmentListItemView>();
                }
            }
        }
    }
}
