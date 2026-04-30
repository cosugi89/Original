using System;
using System.Globalization;
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

    /// <summary>
    /// 旧セーブの文字列 stageId と新セーブの数値 stageId を両方受け入れる。
    /// 空文字や null は未設定扱いとして -1 に正規化する。
    /// </summary>
    public sealed class StageIdJsonConverter : JsonConverter<int>
    {
        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
            {
                JsonToken.Integer => Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture),
                JsonToken.String => ParseStageId(reader.Value as string),
                JsonToken.Null => -1,
                JsonToken.Undefined => -1,
                _ => -1,
            };
        }

        private static int ParseStageId(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return -1;

            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stageId)
                ? stageId
                : -1;
        }
    }
}
