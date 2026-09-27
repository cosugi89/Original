using Assets.Scripts.Core.Const;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main.Dungeon
{
    public class PartsDungeonItem : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image headerBgImage;
        [SerializeField] private Image labelBgImage;
        [SerializeField] private Image labelBgLightImage;
        [SerializeField] private Image labelImage;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Body")]
        [SerializeField] private Image bgImage;
        [SerializeField] private Image bgGradientImage;
        [SerializeField] private Image itemImage;
        private GameButton _button;

        private void Awake()
        {
            _button = GetComponentInChildren<GameButton>(true);
        }

        public void Setup(StageMasterData stageData)
        {
            UIColors.IDungeonColorSet colorSet = stageData.DungeonColorTheme switch
            {
                DungeonColorTheme.Blue => new UIColors.DungeonBlueColorSet(),
                DungeonColorTheme.Green => new UIColors.DungeonGreenColorSet(),
                DungeonColorTheme.Gray => new UIColors.DungeonGrayColorSet(),
                _ => new UIColors.DungeonPinkColorSet()
            };

            itemImage.sprite = stageData.PreviewImage;
            itemImage.enabled = stageData.PreviewImage != null;
            itemImage.preserveAspect = true;

            labelImage.sprite = stageData.DungeonLabelImage;
            labelImage.enabled = stageData.DungeonLabelImage != null;
            labelImage.preserveAspect = true;

            titleText.text = stageData.StageName;

            bgImage.color = colorSet.Background;
            bgGradientImage.color = colorSet.GradientBg;
            headerBgImage.color = colorSet.HeaderBg;
            labelBgImage.color = colorSet.LabelBg;
            labelBgLightImage.color = colorSet.LabelLightBg;
        }

        public void SetClickAction(System.Action action)
        {
            _button.SetAction(action);
        }
    }
}
