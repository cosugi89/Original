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
    public class BattleProgressService
    {
        public static BattleProgressService Instance { get; private set; }

        [Description("現在の共有セッション。ステージ進捗の読み書き先。")]
        public GameSession Session { get; }

        [Description("保存のdirty管理と永続化を担当する保存サービス。")]
        public GameSaveService SaveService { get; }

        private IReadOnlyList<BattleStageData> _allStageData;
        private Dictionary<string, BattleStageData> _byId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInitialized();
        }

        public static BattleProgressService EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            Instance = new BattleProgressService(
                GameSaveService.EnsureInitialized(),
                MasterDataResourceLoader.LoadBattleStageData());
            return Instance;
        }

        public BattleProgressService(GameSaveService saveService, IReadOnlyList<BattleStageData> battleStageData)
        {
            SaveService = saveService;
            Session = saveService.Session;
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

        public StageProgressData GetOrCreateStageProgress(string stageId)
        {
            var battleProgress = EnsureBattleProgress();
            var progress = battleProgress.Stages.FirstOrDefault(stage => stage != null && stage.StageId == (stageId ?? string.Empty));
            if (progress != null)
                return progress;

            progress = new StageProgressData
            {
                StageId = stageId ?? string.Empty,
                IsUnlocked = _byId.TryGetValue(stageId ?? string.Empty, out var data) && data.IsInitiallyUnlocked
            };
            battleProgress.Stages.Add(progress);
            SaveService.MarkDirty();
            return progress;
        }

        public bool IsUnlocked(string stageId)
        {
            return GetOrCreateStageProgress(stageId).IsUnlocked;
        }

        public bool IsCleared(string stageId)
        {
            return GetOrCreateStageProgress(stageId).IsCleared;
        }

        public void UnlockStage(string stageId)
        {
            var progress = GetOrCreateStageProgress(stageId);
            if (progress.IsUnlocked)
                return;

            progress.IsUnlocked = true;
            SaveService.MarkDirty();
        }

        public void SetLastSelectedStage(string stageId)
        {
            var battleProgress = EnsureBattleProgress();
            if (battleProgress.LastSelectedStageId == (stageId ?? string.Empty))
                return;

            battleProgress.LastSelectedStageId = stageId ?? string.Empty;
            SaveService.MarkDirty();
        }

        public void RecordStageClear(string stageId, int score = 0, string rank = "", float clearTimeSeconds = 0f)
        {
            var progress = GetOrCreateStageProgress(stageId);
            progress.IsUnlocked = true;
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
            SaveService.MarkDirty();
        }

        public IReadOnlyList<BattleStageData> GetOrderedStageDefinitions()
        {
            return _allStageData
                .Where(d => d != null)
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.StageId)
                .ToArray();
        }

        private void Initialize()
        {
            EnsureBattleProgress();
            EnsureInitialUnlockedStages();
        }

        private BattleProgressData EnsureBattleProgress()
        {
            Session.SaveData.BattleProgress ??= new BattleProgressData();
            Session.SaveData.BattleProgress.Stages ??= new List<StageProgressData>();
            return Session.SaveData.BattleProgress;
        }

        private void RebuildLookups()
        {
            _byId = new Dictionary<string, BattleStageData>();
            foreach (var data in _allStageData)
            {
                if (data == null || string.IsNullOrWhiteSpace(data.StageId))
                    continue;

                if (!_byId.TryAdd(data.StageId, data))
                {
                    Debug.LogWarning($"[BattleProgressService] Duplicate stageId: {data.StageId}");
                }
            }
        }

        private void EnsureInitialUnlockedStages()
        {
            var hasChanges = false;
            foreach (var data in _allStageData)
            {
                if (data == null || !data.IsInitiallyUnlocked || string.IsNullOrWhiteSpace(data.StageId))
                    continue;

                var progress = EnsureBattleProgress().Stages.FirstOrDefault(stage => stage != null && stage.StageId == data.StageId);
                if (progress == null)
                {
                    EnsureBattleProgress().Stages.Add(new StageProgressData
                    {
                        StageId = data.StageId,
                        IsUnlocked = true
                    });
                    hasChanges = true;
                    continue;
                }

                if (!progress.IsUnlocked)
                {
                    progress.IsUnlocked = true;
                    hasChanges = true;
                }
            }

            if (hasChanges)
                SaveService.MarkDirty();
        }

        private static bool IsBetterRank(string candidateRank, string currentRank)
        {
            if (string.IsNullOrWhiteSpace(candidateRank))
                return false;

            if (string.IsNullOrWhiteSpace(currentRank))
                return true;

            return GetRankScore(candidateRank) > GetRankScore(currentRank);
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
