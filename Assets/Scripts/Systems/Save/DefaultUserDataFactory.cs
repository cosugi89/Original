using System;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.Save
{
    public class DefaultUserDataFactory
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
                Profile = new UserProfileDemoData
                {
                    PlayerId = Guid.NewGuid().ToString("N"),
                    LastPlayedAtUtc = now,
                },
            };
        }

        public string CreateTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }
    }
}
