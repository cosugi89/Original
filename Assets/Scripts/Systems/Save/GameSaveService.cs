using System.ComponentModel;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    public class GameSaveService
    {
        public static GameSaveService Instance { get; private set; }

        [Description("現在の共有セッション。ロード結果の反映先であり保存対象でもある。")]
        public GameSession Session { get; }

        [Description("JSONファイルとの入出力を担当するリポジトリ。")]
        public IGameSaveRepository Repository { get; }

        [Description("新規セーブデータの初期値生成を担当するファクトリ。")]
        public DefaultGameSaveFactory DefaultFactory { get; }

        [Description("旧アバター専用保存から新形式への移行処理を担当する。")]
        public LegacyAvatarSaveMigration LegacyMigration { get; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInitialized();
        }

        public static GameSaveService EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            Instance = new GameSaveService(
                GameSession.Instance,
                new JsonGameSaveRepository(),
                new DefaultGameSaveFactory(),
                new LegacyAvatarSaveMigration());

            Instance.ReloadSession();
            return Instance;
        }

        public GameSaveService(
            GameSession session,
            IGameSaveRepository repository,
            DefaultGameSaveFactory defaultFactory,
            LegacyAvatarSaveMigration legacyMigration)
        {
            Session = session;
            Repository = repository;
            DefaultFactory = defaultFactory;
            LegacyMigration = legacyMigration;
        }

        public void ReloadSession()
        {
            var saveData = LoadOrCreateDefault();
            Session.Initialize(saveData);
        }

        public GameSaveData LoadOrCreateDefault()
        {
            GameSaveData saveData;
            var shouldPersistImmediately = false;
            if (Repository.Exists())
            {
                saveData = Repository.Load();
            }
            else
            {
                saveData = DefaultFactory.Create();
                if (LegacyMigration.TryApply(saveData))
                {
                    shouldPersistImmediately = true;
                }
                else
                {
                    shouldPersistImmediately = true;
                }
            }

            Normalize(saveData);
            saveData.Player.LastPlayedAtUtc = DefaultFactory.CreateTimestamp();

            if (shouldPersistImmediately)
                Repository.Save(saveData);

            return saveData;
        }

        public void SaveSession()
        {
            Save(Session.SaveData);
            Session.ClearDirty();
        }

        public void Save(GameSaveData saveData)
        {
            Normalize(saveData);
            saveData.UpdatedAtUtc = DefaultFactory.CreateTimestamp();
            saveData.Player.LastPlayedAtUtc = saveData.UpdatedAtUtc;
            Repository.Save(saveData);
        }

        public void MarkDirty()
        {
            Session.MarkDirty();
        }

        private static void Normalize(GameSaveData saveData)
        {
            if (saveData == null)
                return;

            saveData.Player ??= new PlayerData();
            saveData.Inventory ??= new InventoryData();
            saveData.BattleProgress ??= new BattleProgressData();

            saveData.Player.AvatarAppearance ??= new AvatarAppearanceData();
            saveData.Player.AvatarAppearance.Parts ??= new System.Collections.Generic.List<AvatarPartStateData>();
            saveData.Player.AvatarAppearance.Colors ??= new System.Collections.Generic.List<AvatarColorData>();
            saveData.Inventory.Equipments ??= new System.Collections.Generic.List<InventoryEntryData>();
            saveData.BattleProgress.Stages ??= new System.Collections.Generic.List<StageProgressData>();

            if (string.IsNullOrWhiteSpace(saveData.CreatedAtUtc))
                saveData.CreatedAtUtc = System.DateTime.UtcNow.ToString("O");

            if (string.IsNullOrWhiteSpace(saveData.UpdatedAtUtc))
                saveData.UpdatedAtUtc = saveData.CreatedAtUtc;

            if (string.IsNullOrWhiteSpace(saveData.Player.PlayerId))
                saveData.Player.PlayerId = System.Guid.NewGuid().ToString("N");
        }
    }
}
