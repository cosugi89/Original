using System;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main.Character
{
    public class PartsInventoryItem : MonoBehaviour
    {
        [SerializeField] private FeedbackButton button;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject normalState;
        [SerializeField] private GameObject disabledtate;
        [SerializeField] private GameObject focusedState;
        [SerializeField] private GameObject badgeIcon;

        private ItemStackData _itemData;
        private Action _callbackAction;

        public void Init(ItemStackData itemData, ItemMasterData itemDefinition, Action callbackAction)
        {
            _itemData = itemData;
            _callbackAction = callbackAction;

            button.SetAction(callbackAction);
            button.interactable = true;
            itemImage.enabled = true;
            itemImage.sprite = itemDefinition.Icon;
            countText.gameObject.SetActive(itemDefinition.IsQuantityVisibleInInventory);
            countText.text = itemDefinition.IsQuantityVisibleInInventory ? itemData.Quantity.ToString() : string.Empty;
            normalState.SetActive(true);
            Setup(isDisabled: false);
        }

        public void ShowEmptyDisabled()
        {
            _itemData = null;
            _callbackAction = null;

            button.SetAction(null);
            button.interactable = false;
            itemImage.sprite = null;
            itemImage.enabled = false;
            countText.gameObject.SetActive(true);
            countText.text = string.Empty;
            normalState.SetActive(true);
            Setup(isDisabled: true);
        }

        public void Setup(bool isDisabled, bool isFocused = false,  bool isActiveBadge = false)
        {
            disabledtate.SetActive(isDisabled);
            focusedState.SetActive(isFocused);
            badgeIcon.SetActive(isActiveBadge);
        }

        public void SetFocused(bool isFocused)
        {
            Setup(isDisabled: false, isFocused: isFocused);
        }
    }
}
