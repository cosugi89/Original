using System;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.Save
{
    public class DefaultGameSaveFactory
    {
        public UserData Create()
        {
            var now = CreateTimestamp();

            return new UserData
            {
                Meta = new UserMetaData
                {
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                Profile = new UserProfileData
                {
                    Identity = new UserIdentityData
                    {
                        PlayerId = Guid.NewGuid().ToString("N"),
                    },
                    Activity = new UserActivityData
                    {
                        LastPlayedAtUtc = now
                    },
                    BattleProfile = new UserBattleProfileData()
                },
            };
        }

        public string CreateTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }
    }
}
