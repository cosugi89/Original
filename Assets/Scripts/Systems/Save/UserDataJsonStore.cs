using System.Collections.Generic;
using System.ComponentModel;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    public class UserDataJsonStore
    {
        [Description("現在の共有セッション。ロード結果の反映先であり保存対象でもある。")]
        public UserDataManager Session { get; }

        [Description("JSONファイルとの入出力を担当するリポジトリ。")]
        public IUserDataStore Repository { get; }

        [Description("新規セーブデータの初期値生成を担当するファクトリ。")]
        public DefaultUserDataFactory DefaultFactory { get; }

        public UserDataJsonStore(
            UserDataManager session,
            IUserDataStore repository,
            DefaultUserDataFactory defaultFactory)
        {
            Session = session;
            Repository = repository;
            DefaultFactory = defaultFactory;
        }

        public void ReloadSession()
        {
            var userData = LoadOrCreateDefault();
            Session.Initialize(userData);
        }

        public UserData LoadOrCreateDefault()
        {
            UserData userData;
            if (Repository.Exists())
            {
                userData = Repository.Load();
            }
            else
            {
                userData = DefaultFactory.Create();
                Repository.Save(userData);
            }

            Normalize(userData);
            userData.Profile.LastPlayedAtUtc = DefaultFactory.CreateTimestamp();

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

            userData.Profile.MaxHp = Mathf.Max(1, userData.Profile.MaxHp);
            userData.Profile.SkillLoadout ??= new BattleSkillLoadoutData();
            userData.Profile.SkillLoadout.SkillIds ??= new List<int>();

            NormalizeSkillIdList(userData.Profile.SkillLoadout.SkillIds);
        }

        private static void NormalizeSkillIdList(List<int> skillIds)
        {
            if (skillIds == null)
            {
                return;
            }

            while (skillIds.Count < UserProfileData.BattleSkillLoadoutSlotCount)
            {
                skillIds.Add(0);
            }

            while (skillIds.Count > UserProfileData.BattleSkillLoadoutSlotCount)
            {
                skillIds.RemoveAt(skillIds.Count - 1);
            }

            for (var i = 0; i < skillIds.Count; i++)
            {
                skillIds[i] = Mathf.Max(0, skillIds[i]);
            }
        }
    }
}
