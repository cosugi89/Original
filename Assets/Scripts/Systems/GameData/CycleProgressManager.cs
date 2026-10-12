using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.GameData
{
    /// <summary>
    /// 周回ごとの人間ボス結末と Insight の表示状態を保存・導出する窓口。
    /// </summary>
    public sealed class CycleProgressManager
    {
        private readonly UserDataJsonStore _userDataStore;
        private readonly BossInsightDatabase _database;

        public CycleProgressManager(UserDataJsonStore userDataStore, BossInsightDatabase database)
        {
            _userDataStore = userDataStore;
            _database = database;
        }

        public bool IsResurgence => EnsureProgress().IsResurgence;
        public bool HasReachedHigherBeing => EnsureProgress().HasReachedHigherBeing;

        public IReadOnlyList<BossInsightProgress> GetInsightProgresses()
        {
            if (_database == null)
                return Array.Empty<BossInsightProgress>();

            return _database.OrderedEntries
                .Select(BuildProgress)
                .Where(progress => progress.IsInsightUnlocked)
                .ToArray();
        }

        public IReadOnlyList<BossInsightProgress> GetRemnantProgresses()
        {
            if (_database == null)
                return Array.Empty<BossInsightProgress>();

            return _database.OrderedEntries
                .Select(BuildProgress)
                .Where(progress => progress.Resolution != BossResolution.Unresolved)
                .ToArray();
        }

        public BossResolution GetBossResolution(int stageId) => FindResult(stageId)?.Resolution ?? BossResolution.Unresolved;

        public void RecordBossResolution(int stageId, BossResolution resolution)
        {
            if (stageId <= 0 || resolution == BossResolution.Unresolved)
                return;

            var result = FindResult(stageId);
            if (result == null)
            {
                result = new BossCycleResultData { StageId = stageId };
                EnsureProgress().BossResults.Add(result);
            }

            if (result.Resolution == resolution)
                return;

            result.Resolution = resolution;
            result.IsInsightViewed = false;
            Save();
            Assets.Scripts.Features.Tutorial.TutorialManager.Report(TutorialSignal.BossResultRecorded, stageId.ToString());
        }

        public void MarkInsightsViewed()
        {
            var changed = false;
            foreach (var result in EnsureProgress().BossResults)
            {
                if (result == null || result.Resolution == BossResolution.Unresolved || result.IsInsightViewed)
                    continue;

                result.IsInsightViewed = true;
                changed = true;
            }

            if (changed)
                Save();
        }

        public void SetResurgenceStarted()
        {
            var progress = EnsureProgress();
            progress.IsResurgence = true;
            progress.HasReachedHigherBeing = false;
            progress.BossResults.Clear();
            Save();
            Assets.Scripts.Features.Tutorial.TutorialManager.Report(TutorialSignal.ResurgenceStarted);
        }

        public void SetHigherBeingReached()
        {
            if (EnsureProgress().HasReachedHigherBeing)
                return;

            EnsureProgress().HasReachedHigherBeing = true;
            Save();
            Assets.Scripts.Features.Tutorial.TutorialManager.Report(TutorialSignal.HigherBeingReached);
        }

        public bool CanSeekRedemption(int stageId)
        {
            return IsResurgence &&
                   HasReachedHigherBeing &&
                   GetBossResolution(stageId) == BossResolution.Defeated;
        }

        private BossInsightProgress BuildProgress(BossInsightMasterData master)
        {
            var result = FindResult(master.StageId);
            var resolution = result?.Resolution ?? BossResolution.Unresolved;
            return new BossInsightProgress(
                master,
                resolution,
                resolution != BossResolution.Unresolved && !(result?.IsInsightViewed ?? false),
                CanSeekRedemption(master.StageId));
        }

        private BossCycleResultData FindResult(int stageId)
        {
            return EnsureProgress().BossResults.FirstOrDefault(result => result != null && result.StageId == stageId);
        }

        private CycleProgressData EnsureProgress()
        {
            _userDataStore.Session.UserData.Cycle ??= new CycleProgressData();
            _userDataStore.Session.UserData.Cycle.BossResults ??= new List<BossCycleResultData>();
            return _userDataStore.Session.UserData.Cycle;
        }

        private void Save()
        {
            _userDataStore.MarkDirty();
            _userDataStore.SaveSession();
        }
    }

    public readonly struct BossInsightProgress
    {
        public BossInsightProgress(
            BossInsightMasterData master,
            BossResolution resolution,
            bool isNewInsight,
            bool canSeekRedemption)
        {
            Master = master;
            Resolution = resolution;
            IsNewInsight = isNewInsight;
            CanSeekRedemption = canSeekRedemption;
        }

        public BossInsightMasterData Master { get; }
        public BossResolution Resolution { get; }
        public bool IsNewInsight { get; }
        public bool CanSeekRedemption { get; }
        public bool IsInsightUnlocked => Resolution != BossResolution.Unresolved;
    }
}
