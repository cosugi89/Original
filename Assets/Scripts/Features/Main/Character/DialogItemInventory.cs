using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Main.Character
{
    public readonly struct ItemInventoryDialogRequest
    {
    }

    public readonly struct ItemInventoryDialogResult
    {
        public static ItemInventoryDialogResult Closed => new();
    }

    [DialogAddress("Dialog/ItemInventory")]
    public sealed class DialogItemInventory :
        DialogBase<ItemInventoryDialogResult>,
        IDialogRequestHandler<ItemInventoryDialogRequest>
    {
        private const int MaximumInventoryItemSlotCount = 15;

        // アイテムを選択する前の初期状態で表示するテキスト
        private const string InitDescriptionText = "";

        [SerializeField] private GameObject scrollContent;
        [SerializeField] private PartsInventoryItem itemPrefab;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private GameButton closeButton;

        [Header("Database")]
        [SerializeField] private ItemMasterDatabase itemDatabase;

        private readonly List<PartsInventoryItem> _itemViews = new();

        public void Setup(ItemInventoryDialogRequest request, DialogContext context)
        {
            EnsurePrefabReferences();
            SetupInitVisual();
            CreateItemViews();
            closeButton.SetAction(OnClickClose);
        }

        protected override void OnClickClose()
        {
            Close(ItemInventoryDialogResult.Closed);
        }

        private void SetupInitVisual()
        {
            itemNameText.SetText(string.Empty);
            itemDescriptionText.SetText(InitDescriptionText);

            for (var i = scrollContent.transform.childCount - 1; i >= 0; i--)
            {
                var child = scrollContent.transform.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            _itemViews.Clear();
        }

        private void CreateItemViews()
        {
            var inventory = UserDataManager.EnsureInitialized().Inventory;
            var visibleItems = itemDatabase.GetImportantItems()
                .Select(definition => new
                {
                    Definition = definition,
                    Quantity = inventory.GetItemCount(definition.ItemId)
                })
                .Where(item => item.Quantity > 0 || item.Definition.IsAlwaysVisibleInInventory)
                .Take(MaximumInventoryItemSlotCount)
                .ToArray();

            foreach (var item in visibleItems)
            {
                var itemData = new Systems.Save.Models.ItemStackData
                {
                    ItemId = item.Definition.ItemId,
                    Quantity = item.Quantity
                };
                var itemView = Instantiate(itemPrefab, scrollContent.transform);
                itemView.Init(itemData, item.Definition, () => SelectItem(itemView, item.Definition));
                _itemViews.Add(itemView);
            }

            while (_itemViews.Count < MaximumInventoryItemSlotCount)
            {
                var itemView = Instantiate(itemPrefab, scrollContent.transform);
                itemView.ShowEmptyDisabled();
                _itemViews.Add(itemView);
            }
        }

        private void SelectItem(PartsInventoryItem selectedItem, ItemMasterData itemDefinition)
        {
            itemNameText.SetText(itemDefinition.DisplayName);
            itemDescriptionText.SetText(itemDefinition.Description);

            foreach (var itemView in _itemViews)
            {
                itemView.SetFocused(itemView == selectedItem);
            }
        }

        private void EnsurePrefabReferences()
        {
            if (scrollContent == null ||
                itemPrefab == null ||
                itemDatabase == null ||
                itemNameText == null ||
                itemDescriptionText == null ||
                closeButton == null)
            {
                throw new InvalidOperationException($"{nameof(DialogItemInventory)} has missing serialized prefab references.");
            }
        }
    }
}
