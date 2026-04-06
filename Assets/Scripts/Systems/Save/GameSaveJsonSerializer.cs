using Assets.Scripts.Systems.Save.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// Newtonsoft.Jsonを使用した、GameSaveDataのJSONシリアル化および逆シリアル化処理
    /// </summary>
    public static class GameSaveJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };

        /// <summary>
        /// GameSaveData → JSON文字列
        /// </summary>
        public static string Serialize(GameSaveData saveData)
        {
            return JsonConvert.SerializeObject(saveData ?? new GameSaveData(), Settings);
        }

        /// <summary>
        /// JSON文字列 → GameSaveData
        /// </summary>
        public static GameSaveData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new GameSaveData();

            return JsonConvert.DeserializeObject<GameSaveData>(json, Settings) ?? new GameSaveData();
        }
    }
}
