using System.Collections.Generic;
using Assets.Scripts.Core.Const;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Scene;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main.Dungeon
{
    public readonly struct RequestDialogMissionDetail
    {
        public RequestDialogMissionDetail(StageMasterData stage) => Stage = stage;
        public StageMasterData Stage { get; }
    }

    public readonly struct ResultDialogMissionDetail
    {
        private ResultDialogMissionDetail(bool started) => Started = started;
        public bool Started { get; }
        public static ResultDialogMissionDetail StartedBattle => new(true);
        public static ResultDialogMissionDetail Cancelled => new(false);
    }

    [DialogAddress("Dialog/MissionDetail")]
    public class DialogMissionDetail : DialogBase<ResultDialogMissionDetail>, IDialogRequestHandler<RequestDialogMissionDetail>
    {
        [Header("Color")]
        [SerializeField] private Image headerColor;
        [SerializeField] private Image bgColor;
        [SerializeField] private Image innerBgColor;
        [SerializeField] private Image innerBorderBgColor;
        [SerializeField] private Image middleColor;
        
        [Header("UI Parts")]
        [SerializeField] public TextMeshProUGUI titleText;
        [SerializeField] private Image enemyImage;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform listContent;
        [SerializeField] private PartsMissionRewardItem itemContentPrefab;
        [SerializeField] private GameObject enterButton;
        [SerializeField] private GameButton closeButton;
        
        [Header("Database")]
        [SerializeField] private ItemMasterDatabase itemDatabase;
        [SerializeField] private EquipmentMasterDatabase equipmentDatabase;
        
        private readonly List<PartsMissionRewardItem> _rewardItems = new();
        private StageMasterData _stage;
        private GameButton _enterGameButton;
        private bool _isStartingBattle;

        public void Setup(RequestDialogMissionDetail request, DialogContext context)
        {
            _stage = request.Stage;
            titleText.SetText(_stage.StageName);
            enemyImage.sprite = _stage.PreviewImage;
            enemyImage.enabled = _stage.PreviewImage != null;
            ApplyColors(_stage.DungeonColorTheme);
            BuildRewards();
            _isStartingBattle = false;
            _enterGameButton = enterButton.GetComponent<GameButton>();
            _enterGameButton.SetAction(OnClickEnter);
            closeButton.SetAction(OnClickClose);
            Refresh();
        }

        private void Update()
        {
            if (_isStartingBattle || !_enterGameButton.interactable)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnClickEnter();
            }
        }
        
        public void Refresh()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.horizontal = scrollRect.content.rect.width > scrollRect.viewport.rect.width;
            scrollRect.horizontalNormalizedPosition = 0f;
        }

        protected override void OnClickClose() => Close(ResultDialogMissionDetail.Cancelled);

        private void OnClickEnter()
        {
            if (_isStartingBattle)
            {
                return;
            }

            _isStartingBattle = true;
            Close(ResultDialogMissionDetail.StartedBattle);
            BattleSceneRequest.SelectedStageId = _stage.StageId;
            SceneManager.LoadScene(SceneNames.Battle);
        }

        private void BuildRewards()
        {
            for (var i = 0; i < _rewardItems.Count; i++) Destroy(_rewardItems[i].gameObject);
            _rewardItems.Clear();
            var isCleared = UserDataManager.EnsureInitialized().Progress.IsCleared(_stage.StageId);
            CreateRewardItems(isCleared, false);
            CreateRewardItems(isCleared, true);
        }

        private void CreateRewardItems(bool isCleared, bool unknownOnly)
        {
            foreach (var reward in _stage.ClearRewards)
            {
                var isHiddenItem = reward.RewardType == MissionRewardType.Item &&
                                   itemDatabase.TryGetById(reward.ItemId, out var rewardItem) &&
                                   rewardItem.IsHiddenUntilMissionClear &&
                                   !isCleared;
                if (isHiddenItem != unknownOnly) continue;

                var view = Instantiate(itemContentPrefab, listContent);
                if (reward.RewardType == MissionRewardType.Item && itemDatabase.TryGetById(reward.ItemId, out var item))
                    view.SetupItem(item, reward.Quantity, isHiddenItem);
                else if (reward.RewardType == MissionRewardType.Equipment && equipmentDatabase.TryGetById(reward.EquipmentId, out var equipment))
                    view.SetupEquipment(equipment, reward.Quantity);
                else
                    view.SetupUnknown(reward.Quantity);
                _rewardItems.Add(view);
            }
        }

        private void ApplyColors(DungeonColorTheme theme)
        {
            UIColors.IDungeonColorSet colors = theme switch
            {
                DungeonColorTheme.Blue => new UIColors.DungeonBlueColorSet(),
                DungeonColorTheme.Green => new UIColors.DungeonGreenColorSet(),
                DungeonColorTheme.Gray => new UIColors.DungeonGrayColorSet(),
                _ => new UIColors.DungeonPinkColorSet(),
            };
            headerColor.color = colors.HeaderBg;
            bgColor.color = colors.Background;
            innerBgColor.color = colors.GradientBg;
            innerBorderBgColor.color = colors.LabelBg;
            middleColor.color = colors.LabelLightBg;
        }
    }
}
