using System;
using System.Globalization;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.Save.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// Newtonsoft.Jsonを使用した、UserDataのJSONシリアル化および逆シリアル化処理
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
        /// UserData → JSON文字列
        /// </summary>
        public static string Serialize(UserData userData)
        {
            return JsonConvert.SerializeObject(userData ?? new UserData(), Settings);
        }

        /// <summary>
        /// JSON文字列 → UserData
        /// </summary>
        public static UserData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new UserData();

            var serializer = JsonSerializer.Create(Settings);
            var token = JToken.Parse(json);
            if (token is not JObject obj)
                return token.ToObject<UserData>(serializer) ?? new UserData();

            if (obj.ContainsKey("Meta") || obj.ContainsKey("Profile"))
                return obj.ToObject<UserData>(serializer) ?? new UserData();

            var legacy = obj.ToObject<LegacyUserDataDocument>(serializer) ?? new LegacyUserDataDocument();
            return ConvertLegacy(legacy);
        }

        private static UserData ConvertLegacy(LegacyUserDataDocument legacy)
        {
            legacy ??= new LegacyUserDataDocument();
            legacy.Player ??= new LegacyPlayerDocument();

            return new UserData
            {
                Meta = new UserMetaData
                {
                    SaveVersion = legacy.SaveVersion,
                    CreatedAtUtc = legacy.CreatedAtUtc ?? "",
                    UpdatedAtUtc = legacy.UpdatedAtUtc ?? ""
                },
                Profile = new UserProfileData
                {
                    Identity = new UserIdentityData
                    {
                        PlayerId = legacy.Player.PlayerId ?? "",
                        PlayerName = legacy.Player.PlayerName ?? ""
                    },
                    Activity = new UserActivityData
                    {
                        LastPlayedAtUtc = legacy.Player.LastPlayedAtUtc ?? ""
                    },
                    Economy = new UserEconomyData
                    {
                        Gold = legacy.Player.Gold,
                        Gem = legacy.Player.Gem
                    },
                    Progression = new UserProgressionData
                    {
                        Level = legacy.Player.Level,
                        Experience = legacy.Player.Experience
                    },
                    Avatar = legacy.Player.AvatarAppearance ?? new AvatarAppearanceData(),
                    BattleProfile = new UserBattleProfileData()
                },
                Inventory = legacy.Inventory ?? new InventoryData(),
                BattleProgress = legacy.BattleProgress ?? new BattleProgressData()
            };
        }

        private sealed class LegacyUserDataDocument
        {
            public int SaveVersion { get; set; } = 1;
            public string CreatedAtUtc { get; set; } = "";
            public string UpdatedAtUtc { get; set; } = "";
            public LegacyPlayerDocument Player { get; set; } = new();
            public InventoryData Inventory { get; set; } = new();
            public BattleProgressData BattleProgress { get; set; } = new();
        }

        private sealed class LegacyPlayerDocument
        {
            public string PlayerId { get; set; } = "";
            public string PlayerName { get; set; } = "";
            public int Gold { get; set; } = 0;
            public int Gem { get; set; } = 0;
            public int Level { get; set; } = 1;
            public int Experience { get; set; } = 0;
            public string LastPlayedAtUtc { get; set; } = "";
            public AvatarAppearanceData AvatarAppearance { get; set; } = new();
        }
    }

    public sealed class EquipmentIdJsonConverter : JsonConverter<int>
    {
        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
            {
                JsonToken.Integer => NormalizeNumericId(Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture)),
                JsonToken.String => ParseEquipmentId(reader.Value as string),
                JsonToken.Null => 0,
                JsonToken.Undefined => 0,
                _ => 0,
            };
        }

        private static int NormalizeNumericId(int equipmentId)
        {
            return equipmentId > 0 ? equipmentId : 0;
        }

        private static int ParseEquipmentId(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return EquipmentIdUtility.TryConvertLegacyStringToId(text.Trim(), out var equipmentId)
                ? equipmentId
                : 0;
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
