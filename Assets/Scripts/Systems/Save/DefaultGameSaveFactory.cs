using System;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.Save
{
    public class DefaultGameSaveFactory
    {
        public GameSaveData Create()
        {
            var now = CreateTimestamp();

            return new GameSaveData
            {
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                Player = new PlayerData
                {
                    PlayerId = Guid.NewGuid().ToString("N"),
                    LastPlayedAtUtc = now
                }
            };
        }

        public string CreateTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }
    }
}
