using Assets.Scripts.Data.MasterData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main.Dungeon
{
    public class PartsMissionRewardItem : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private Image unknownItemImage;
        [SerializeField] private Image equipmentImage;
        [SerializeField] private TextMeshProUGUI countText;

        public void SetupItem(ItemMasterData item, int quantity, bool isHidden)
        {
            itemImage.gameObject.SetActive(!isHidden);
            unknownItemImage.gameObject.SetActive(isHidden);
            equipmentImage.gameObject.SetActive(false);
            itemImage.sprite = item.Icon;
            countText.SetText($"x{quantity}");
        }

        public void SetupEquipment(EquipmentMasterData equipment, int quantity)
        {
            itemImage.gameObject.SetActive(false);
            unknownItemImage.gameObject.SetActive(false);
            equipmentImage.gameObject.SetActive(true);
            equipmentImage.sprite = equipment.Icon;
            countText.SetText($"x{quantity}");
        }

        public void SetupUnknown(int quantity)
        {
            itemImage.gameObject.SetActive(false);
            unknownItemImage.gameObject.SetActive(true);
            equipmentImage.gameObject.SetActive(false);
            countText.SetText($"x{quantity}");
        }
    }
}
