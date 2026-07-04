using System.Collections.Generic;
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
            var userData = LoadOrCreateDefault();
            Session.Initialize(userData);
        }

        public UserData LoadOrCreateDefault()
        {
            UserData userData;
            var shouldPersistImmediately = false;
            if (Repository.Exists())
            {
                userData = Repository.Load();
            }
            else
            {
                userData = DefaultFactory.Create();
                if (LegacyMigration.TryApply(userData))
                {
                    shouldPersistImmediately = true;
                }
                else
                {
                    shouldPersistImmediately = true;
                }
            }

            Normalize(userData);
            userData.Profile.LastPlayedAtUtc = DefaultFactory.CreateTimestamp();

            if (shouldPersistImmediately)
                Repository.Save(userData);

            return userData;
        }

        public void SaveSession()
        {
            Save(Session.UserData);
            Session.ClearDirty();
        }

        public void Save(UserData userData)
        {
            Normalize(userData);
            userData.Meta.UpdatedAtUtc = DefaultFactory.CreateTimestamp();
            userData.Profile.LastPlayedAtUtc = userData.Meta.UpdatedAtUtc;
            Repository.Save(userData);
        }

        public void MarkDirty()
        {
            Session.MarkDirty();
        }

        private static void Normalize(UserData userData)
        {
            if (userData == null)
                return;

            userData.Meta ??= new UserMetaData();
            userData.Profile ??= new UserProfileDemoData();
            userData.Profile.Avatar ??= new AvatarAppearanceData();
            userData.Inventory ??= new InventoryData();
            userData.BattleProgress ??= new BattleProgressData();

            userData.Profile.Avatar.Parts ??= new System.Collections.Generic.List<AvatarPartStateData>();
            userData.Profile.Avatar.Colors ??= new System.Collections.Generic.List<AvatarColorData>();
            userData.Inventory.Equipments ??= new System.Collections.Generic.List<InventoryEntryData>();
            userData.BattleProgress.Stages ??= new System.Collections.Generic.List<StageProgressData>();

            if (string.IsNullOrWhiteSpace(userData.Meta.CreatedAtUtc))
                userData.Meta.CreatedAtUtc = System.DateTime.UtcNow.ToString("O");

            if (string.IsNullOrWhiteSpace(userData.Meta.UpdatedAtUtc))
                userData.Meta.UpdatedAtUtc = userData.Meta.CreatedAtUtc;

            if (string.IsNullOrWhiteSpace(userData.Profile.PlayerId))
                userData.Profile.PlayerId = System.Guid.NewGuid().ToString("N");

            var demoProfile = new UserProfileDemoData();
            userData.Profile.MaxHp = Mathf.Max(1, userData.Profile.MaxHp);
            userData.Profile.NormalAttackDamage = Mathf.Max(1, userData.Profile.NormalAttackDamage);
            userData.Profile.DoubleAttackFollowUpDamage = Mathf.Max(1, userData.Profile.DoubleAttackFollowUpDamage);
            userData.Profile.JumpAttackDamage = Mathf.Max(1, userData.Profile.JumpAttackDamage);
            userData.Profile.SkillSlotSkillIds ??= new List<int>(demoProfile.SkillSlotSkillIds);

            if (userData.Profile.SkillSlotSkillIds.Count == 0)
            {
                userData.Profile.SkillSlotSkillIds = new List<int>(demoProfile.SkillSlotSkillIds);
            }

            for (var i = 0; i < userData.Profile.SkillSlotSkillIds.Count; i++)
            {
                userData.Profile.SkillSlotSkillIds[i] =
                    Mathf.Max(0, userData.Profile.SkillSlotSkillIds[i]);
            }
        }
    }
}
