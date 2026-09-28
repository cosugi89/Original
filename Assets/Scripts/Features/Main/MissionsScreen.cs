using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Main.Dungeon;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Dialog;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public class MissionsScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform scrollContent;
        [SerializeField] private PartsDungeonItem itemPrefab;
        [SerializeField] private StageMasterDatabase stageDatabase;

        private readonly List<PartsDungeonItem> _items = new();

        private void OnEnable()
        {
            DebugStageSettings.VisibilityChanged += RefreshItems;
            DebugStageSettings.DungeonListRefreshRequested += RefreshItems;
            RefreshItems();
        }

        private void RefreshItems()
        {
            ClearItems();

            var progress = UserDataManager.EnsureInitialized().Progress;
            var stages = stageDatabase.Stages
                .Where(stage => stage != null)
                .Where(stage => stage.StageKind != StageKind.Debug || DebugStageSettings.ShowDebugStages)
                .Where(stage => progress.IsUnlocked(stage.StageId))
                .Where(stage => !progress.IsCleared(stage.StageId))
                .OrderBy(stage => stage.SortOrder)
                .ThenBy(stage => stage.StageId);

            foreach (var stage in stages)
            {
                var item = Instantiate(itemPrefab, scrollContent);
                item.Setup(stage);
                item.SetClickAction(() => OpenMissionDetailAsync(stage).Forget());
                _items.Add(item);
            }
        }

        private void OnDisable()
        {
            DebugStageSettings.VisibilityChanged -= RefreshItems;
            DebugStageSettings.DungeonListRefreshRequested -= RefreshItems;
            ClearItems();
        }

        private void ClearItems()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                Destroy(_items[i].gameObject);
            }

            _items.Clear();
        }

        private async UniTaskVoid OpenMissionDetailAsync(StageMasterData stage)
        {
            await DialogService.OpenAsync<DialogMissionDetail, RequestDialogMissionDetail, ResultDialogMissionDetail>(
                new RequestDialogMissionDetail(stage), null);
        }
    }
}
