using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public sealed class MissionClearResult
    {
        public bool IsFirstClear { get; init; }
        public IReadOnlyList<MissionRewardData> GrantedRewards { get; init; } = Array.Empty<MissionRewardData>();
    }

    public class ProgressManager
    {
        [Description("現在の共有セッション。ステージ進捗の読み書き先。")]
        public UserDataManager Session { get; }

        [Description("保存のdirty管理と永続化を担当する保存サービス。")]
        public UserDataJsonStore UserDataStore { get; }

        private IReadOnlyList<BattleStageData> _allStageData;
        private Dictionary<int, BattleStageData> _byId;

        public ProgressManager(UserDataJsonStore userDataStore, IReadOnlyList<BattleStageData> battleStageData)
        {
            UserDataStore = userDataStore;
            Session = userDataStore.Session;
            RefreshDefinitions(battleStageData);
        }

        public void RefreshDefinitions(IReadOnlyList<BattleStageData> battleStageData)
        {
            _allStageData = battleStageData ?? Array.Empty<BattleStageData>();
            RebuildLookups();
            Initialize();
        }

        public IReadOnlyList<StageProgressData> GetAllProgress()
        {
            return EnsureBattleProgress().Stages
                .Where(stage => stage != null)
                .OrderBy(stage => stage.StageId)
                .ToArray();
        }

        public StageProgressData GetOrCreateStageProgress(int stageId)
        {
            var battleProgress = EnsureBattleProgress();
            var progress = battleProgress.Stages.FirstOrDefault(stage => stage != null && stage.StageId == stageId);
            if (progress != null)
                return progress;

            progress = new StageProgressData
            {
                StageId = stageId
            };
            battleProgress.Stages.Add(progress);
            UserDataStore.MarkDirty();
            return progress;
        }

        public bool IsUnlocked(int stageId)
        {
            if (!_byId.TryGetValue(stageId, out var definition))
                return false;

            var requiredStageIds = definition.RequiredClearedStageIds;
            if (requiredStageIds == null || requiredStageIds.Count == 0)
                return true;

            return definition.UnlockRequirementMode == StageUnlockRequirementMode.Any
                ? requiredStageIds.Any(IsCleared)
                : requiredStageIds.All(IsCleared);
        }

        public bool IsCleared(int stageId)
        {
            var progress = EnsureBattleProgress().Stages
                .FirstOrDefault(stage => stage != null && stage.StageId == stageId);
            return progress != null && progress.IsCleared;
        }

        public void SetLastSelectedStage(int stageId)
        {
            var battleProgress = EnsureBattleProgress();
            if (battleProgress.LastSelectedStageId == stageId)
                return;

            battleProgress.LastSelectedStageId = stageId;
            UserDataStore.MarkDirty();
        }

        public void SetCurrentStage(int stageId)
        {
            if (stageId < 0)
            {
                return;
            }

            SetLastSelectedStage(stageId);
            Session.SetCurrentStageId(stageId);
        }

        public MissionClearResult RecordStageClear(int stageId, int score = 0, string rank = "", float clearTimeSeconds = 0f)
        {
            var progress = GetOrCreateStageProgress(stageId);
            var isFirstClear = !progress.IsCleared;
            progress.IsCleared = true;
            progress.ClearCount += 1;
            progress.BestScore = Mathf.Max(progress.BestScore, score);

            if (IsBetterRank(rank, progress.BestRank))
            {
                progress.BestRank = rank;
            }

            if (clearTimeSeconds > 0f &&
                (progress.BestClearTimeSeconds <= 0f || clearTimeSeconds < progress.BestClearTimeSeconds))
            {
                progress.BestClearTimeSeconds = clearTimeSeconds;
            }

            progress.LastClearedAtUtc = System.DateTime.UtcNow.ToString("O");

            var grantedRewards = Array.Empty<MissionRewardData>();
            if (isFirstClear && !progress.IsClearRewardClaimed && _byId.TryGetValue(stageId, out var definition))
            {
                grantedRewards = definition.ClearRewards
                    .Where(IsValidReward)
                    .ToArray();
                foreach (var reward in grantedRewards)
                {
                    if (reward.RewardType == MissionRewardType.Item)
                    {
                        Session.Inventory.AddItem(reward.ItemId, reward.Quantity);
                    }
                    else
                    {
                        Session.Inventory.GrantEquipment(reward.EquipmentId, reward.Quantity);
                    }
                }

                progress.IsClearRewardClaimed = true;
            }

            UserDataStore.MarkDirty();
            return new MissionClearResult
            {
                IsFirstClear = isFirstClear,
                GrantedRewards = grantedRewards,
            };
        }

        public bool OverrideStageClearStateForDebug(int stageId, bool isCleared)
        {
            if (!_byId.ContainsKey(stageId))
                return false;

            var progress = EnsureBattleProgress().Stages
                .FirstOrDefault(stage => stage != null && stage.StageId == stageId);
            if (progress == null && !isCleared)
                return true;

            progress ??= GetOrCreateStageProgress(stageId);
            if (progress.IsCleared == isCleared)
                return true;

            progress.IsCleared = isCleared;
            if (isCleared)
                progress.ClearCount = Mathf.Max(1, progress.ClearCount);

            UserDataStore.MarkDirty();
            return true;
        }

        public int ClearStagesThroughForDebug(int stageId)
        {
            if (stageId <= 0)
                return 0;

            var stageIdGroup = stageId / 1000;
            var targetStageIds = GetOrderedStageDefinitions()
                .Where(definition => definition.StageId / 1000 == stageIdGroup && definition.StageId <= stageId)
                .Select(definition => definition.StageId)
                .ToArray();

            foreach (var targetStageId in targetStageIds)
            {
                OverrideStageClearStateForDebug(targetStageId, true);
            }

            return targetStageIds.Length;
        }

        public IReadOnlyList<BattleStageData> GetOrderedStageDefinitions()
        {
            return _allStageData
                .Where(d => d != null)
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.StageId)
                .ToArray();
        }

        public int GetRecommendedStageId()
        {
            var orderedDefinitions = GetOrderedStageDefinitions()
                .Where(definition => definition.StageKind != StageKind.Debug)
                .ToArray();
            if (orderedDefinitions.Length == 0)
                return -1;

            var lastSelectedStageId = EnsureBattleProgress().LastSelectedStageId;
            if (lastSelectedStageId >= 0 &&
                _byId.TryGetValue(lastSelectedStageId, out var lastSelectedDefinition) &&
                lastSelectedDefinition.StageKind != StageKind.Debug &&
                IsUnlocked(lastSelectedStageId))
            {
                return lastSelectedStageId;
            }

            var firstUnlockedUncleared = orderedDefinitions
                .FirstOrDefault(definition => IsUnlocked(definition.StageId) && !IsCleared(definition.StageId));
            if (firstUnlockedUncleared != null)
                return firstUnlockedUncleared.StageId;

            var firstUnlocked = orderedDefinitions.FirstOrDefault(definition => IsUnlocked(definition.StageId));
            if (firstUnlocked != null)
                return firstUnlocked.StageId;

            return orderedDefinitions[0].StageId;
        }

        /// <summary>
        /// 進行状況から現在選択中のステージIDを取得
        /// </summary>
        public int GetCurrentOrRecommendedStageId()
        {
            if (Session.CurrentStageId >= 0)
            {
                return Session.CurrentStageId;
            }

            return GetRecommendedStageId();
        }

        public int GetNextStageId(int stageId)
        {
            if (stageId < 0)
                return -1;

            var orderedDefinitions = GetOrderedStageDefinitions()
                .Where(definition => definition.StageKind == StageKind.Main)
                .ToArray();
            for (var i = 0; i < orderedDefinitions.Length; i++)
            {
                var definition = orderedDefinitions[i];
                if (definition == null || definition.StageId != stageId)
                    continue;

                return i + 1 < orderedDefinitions.Length
                    ? orderedDefinitions[i + 1].StageId
                    : -1;
            }

            return -1;
        }

        private void Initialize()
        {
            EnsureBattleProgress();
            PruneInvalidStageProgress();
        }

        private BattleProgressData EnsureBattleProgress()
        {
            Session.UserData.BattleProgress ??= new BattleProgressData();
            Session.UserData.BattleProgress.Stages ??= new List<StageProgressData>();
            return Session.UserData.BattleProgress;
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<int, BattleStageData>();
            foreach (var data in _allStageData)
            {
                if (data == null)
                    continue;

                if (!_byId.TryAdd(data.StageId, data))
                {
                    Debug.LogWarning($"[ProgressManager] Duplicate stageId: {data.StageId}");
                }
            }
        }

        private void PruneInvalidStageProgress()
        {
            var stages = EnsureBattleProgress().Stages;
            if (stages == null)
                return;

            var removedCount = stages.RemoveAll(stage => stage == null || !_byId.ContainsKey(stage.StageId));
            if (removedCount > 0)
            {
                UserDataStore.MarkDirty();
            }

            var battleProgress = EnsureBattleProgress();
            if (battleProgress.LastSelectedStageId >= 0 && !_byId.ContainsKey(battleProgress.LastSelectedStageId))
            {
                battleProgress.LastSelectedStageId = -1;
                UserDataStore.MarkDirty();
            }
        }

        private static bool IsBetterRank(string candidateRank, string currentRank)
        {
            if (string.IsNullOrWhiteSpace(candidateRank))
                return false;

            if (string.IsNullOrWhiteSpace(currentRank))
                return true;

            return GetRankScore(candidateRank) > GetRankScore(currentRank);
        }

        private static bool IsValidReward(MissionRewardData reward)
        {
            if (reward == null || reward.Quantity <= 0)
                return false;

            return reward.RewardType == MissionRewardType.Item
                ? reward.ItemId > 0
                : reward.EquipmentId > 0;
        }

        private static int GetRankScore(string rank)
        {
            return (rank ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "S" => 5,
                "A" => 4,
                "B" => 3,
                "C" => 2,
                "D" => 1,
                _ => 0
            };
        }
    }
}
