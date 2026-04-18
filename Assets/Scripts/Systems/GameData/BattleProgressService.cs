using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        [Description("ステージIDと表示情報を解決するステージマスタ。")]
        public StageDatabase StageDatabase { get; private set; }

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
                MasterDataResourceLoader.LoadStageDatabase());
            Instance.Initialize();
            return Instance;
        }

        public BattleProgressService(GameSaveService saveService, StageDatabase stageDatabase)
        {
            SaveService = saveService;
            Session = saveService.Session;
            StageDatabase = stageDatabase;
        }

        public void RefreshDatabase(StageDatabase stageDatabase)
        {
            StageDatabase = stageDatabase;
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
            return GetOrCreateStageProgress(ToStageIdKey(stageId));
        }

        public StageProgressData GetOrCreateStageProgress(string stageId)
        {
            var stageIdKey = stageId ?? string.Empty;
            var battleProgress = EnsureBattleProgress();
            var progress = battleProgress.Stages.FirstOrDefault(stage => stage != null && stage.StageId == stageIdKey);
            if (progress != null)
                return progress;

            progress = new StageProgressData
            {
                StageId = stageIdKey,
                IsUnlocked = IsInitiallyUnlockedStage(stageIdKey)
            };
            battleProgress.Stages.Add(progress);
            SaveService.MarkDirty();
            return progress;
        }

        public bool IsUnlocked(int stageId)
        {
            return IsUnlocked(ToStageIdKey(stageId));
        }

        public bool IsUnlocked(string stageId)
        {
            return GetOrCreateStageProgress(stageId).IsUnlocked;
        }

        public bool IsCleared(int stageId)
        {
            return IsCleared(ToStageIdKey(stageId));
        }

        public bool IsCleared(string stageId)
        {
            return GetOrCreateStageProgress(stageId).IsCleared;
        }

        public void UnlockStage(int stageId)
        {
            UnlockStage(ToStageIdKey(stageId));
        }

        public void UnlockStage(string stageId)
        {
            var progress = GetOrCreateStageProgress(stageId);
            if (progress.IsUnlocked)
                return;

            progress.IsUnlocked = true;
            SaveService.MarkDirty();
        }

        public void SetLastSelectedStage(int stageId)
        {
            SetLastSelectedStage(ToStageIdKey(stageId));
        }

        public void SetLastSelectedStage(string stageId)
        {
            var stageIdKey = stageId ?? string.Empty;
            var battleProgress = EnsureBattleProgress();
            if (battleProgress.LastSelectedStageId == stageIdKey)
                return;

            battleProgress.LastSelectedStageId = stageIdKey;
            SaveService.MarkDirty();
        }

        public void RecordStageClear(int stageId, int score = 0, string rank = "", float clearTimeSeconds = 0f)
        {
            RecordStageClear(ToStageIdKey(stageId), score, rank, clearTimeSeconds);
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

        public IReadOnlyList<StageData> GetOrderedStages()
        {
            if (StageDatabase == null)
                return new List<StageData>();

            return StageDatabase.GetAllOrdered();
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

        private void EnsureInitialUnlockedStages()
        {
            if (StageDatabase == null)
                return;

            var hasChanges = false;
            foreach (var definition in StageDatabase.GetInitiallyUnlocked())
            {
                if (definition == null || definition.StageId <= 0)
                    continue;

                var definitionStageId = ToStageIdKey(definition.StageId);
                var progress = EnsureBattleProgress().Stages.FirstOrDefault(stage => stage != null && stage.StageId == definitionStageId);
                if (progress == null)
                {
                    EnsureBattleProgress().Stages.Add(new StageProgressData
                    {
                        StageId = definitionStageId,
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

        private bool IsInitiallyUnlockedStage(string stageId)
        {
            if (StageDatabase == null)
            {
                return false;
            }

            return StageDatabase
                .GetInitiallyUnlocked()
                .Any(definition => definition != null && ToStageIdKey(definition.StageId) == (stageId ?? string.Empty));
        }

        private static string ToStageIdKey(int stageId)
        {
            return Mathf.Max(0, stageId).ToString();
        }
    }
}
