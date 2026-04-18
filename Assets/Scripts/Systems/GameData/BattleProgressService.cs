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
        public BattleStageCatalog BattleStageCatalog { get; private set; }

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
                MasterDataResourceLoader.LoadBattleStageCatalog());
            Instance.Initialize();
            return Instance;
        }

        public BattleProgressService(GameSaveService saveService, BattleStageCatalog battleStageCatalog)
        {
            SaveService = saveService;
            Session = saveService.Session;
            BattleStageCatalog = battleStageCatalog;
        }

        public void RefreshCatalog(BattleStageCatalog battleStageCatalog)
        {
            BattleStageCatalog = battleStageCatalog;
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
                IsUnlocked = BattleStageCatalog != null &&
                             BattleStageCatalog.TryGetById(stageId, out var definition) &&
                             definition.IsInitiallyUnlocked
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

        public IReadOnlyList<BattleStageDefinition> GetOrderedStageDefinitions()
        {
            if (BattleStageCatalog == null)
                return new List<BattleStageDefinition>();

            return BattleStageCatalog.GetAllOrdered();
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
            if (BattleStageCatalog == null)
                return;

            var hasChanges = false;
            foreach (var definition in BattleStageCatalog.GetInitiallyUnlocked())
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.StageId))
                    continue;

                var progress = EnsureBattleProgress().Stages.FirstOrDefault(stage => stage != null && stage.StageId == definition.StageId);
                if (progress == null)
                {
                    EnsureBattleProgress().Stages.Add(new StageProgressData
                    {
                        StageId = definition.StageId,
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
