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
            userData.Profile.AvatarAppearance ??= new AvatarAppearanceData();
            userData.Inventory ??= new InventoryData();
            userData.BattleProgress ??= new BattleProgressData();
            userData.Ability ??= new AbilityProgressData();

            userData.Profile.AvatarAppearance.Parts ??= new System.Collections.Generic.List<AvatarPartStateData>();
            userData.Profile.AvatarAppearance.Colors ??= new System.Collections.Generic.List<AvatarColorData>();
            userData.Inventory.Equipments ??= new System.Collections.Generic.List<InventoryEntryData>();
            userData.BattleProgress.Stages ??= new System.Collections.Generic.List<StageProgressData>();
            userData.Ability.NodeLevels ??= new System.Collections.Generic.List<AbilityTreeNodeLevelData>();
            userData.Ability.PointBalance = Mathf.Max(0, userData.Ability.PointBalance);

            if (string.IsNullOrWhiteSpace(userData.Meta.CreatedAtUtc))
                userData.Meta.CreatedAtUtc = System.DateTime.UtcNow.ToString("O");

            if (string.IsNullOrWhiteSpace(userData.Meta.UpdatedAtUtc))
                userData.Meta.UpdatedAtUtc = userData.Meta.CreatedAtUtc;

            if (string.IsNullOrWhiteSpace(userData.Profile.PlayerId))
                userData.Profile.PlayerId = System.Guid.NewGuid().ToString("N");

            userData.Profile.MaxHp = Mathf.Max(1, userData.Profile.MaxHp);
            userData.Profile.Decks ??= new List<DeckData>();
            NormalizeDecks(userData.Profile);
        }

        private static void NormalizeDecks(UserProfileData profile)
        {
            profile.CurrentDeckId = Mathf.Clamp(profile.CurrentDeckId, 1, UserProfileData.DeckCount);

            var decksById = new Dictionary<int, DeckData>();
            for (var i = 0; i < profile.Decks.Count; i++)
            {
                var deck = profile.Decks[i];
                if (deck == null || deck.DeckId < 1 || deck.DeckId > UserProfileData.DeckCount || decksById.ContainsKey(deck.DeckId))
                    continue;

                decksById.Add(deck.DeckId, deck);
            }

            profile.Decks.Clear();
            for (var deckId = 1; deckId <= UserProfileData.DeckCount; deckId++)
            {
                var deck = decksById.TryGetValue(deckId, out var existing) ? existing : new DeckData { DeckId = deckId };
                deck.RightHandEquipmentId = Mathf.Max(0, deck.RightHandEquipmentId);
                deck.LeftHandEquipmentId = Mathf.Max(0, deck.LeftHandEquipmentId);
                deck.HelmetEquipmentId = Mathf.Max(0, deck.HelmetEquipmentId);
                deck.ChestEquipmentId = Mathf.Max(0, deck.ChestEquipmentId);
                deck.SkillIds ??= new List<int>();
                NormalizeSkillIdList(deck.SkillIds);
                profile.Decks.Add(deck);
            }
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
