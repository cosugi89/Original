using Assets.Scripts.Systems.Save.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts.Systems.Save
{
    public static class GameSaveJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };

        public static string Serialize(GameSaveData saveData)
        {
            return JsonConvert.SerializeObject(saveData ?? new GameSaveData(), Settings);
        }

        public static GameSaveData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new GameSaveData();

            return JsonConvert.DeserializeObject<GameSaveData>(json, Settings) ?? new GameSaveData();
        }
    }
}
