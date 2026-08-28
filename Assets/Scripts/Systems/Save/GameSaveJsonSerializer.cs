using Assets.Scripts.Systems.Save.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// 現行の UserData JSON 形式をシリアル化・逆シリアル化する。
    /// </summary>
    public static class GameSaveJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = { new StringEnumConverter() }
        };

        public static string Serialize(UserData userData)
        {
            return JsonConvert.SerializeObject(userData ?? new UserData(), Settings);
        }

        public static UserData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new UserData();

            return JsonConvert.DeserializeObject<UserData>(json, Settings) ?? new UserData();
        }
    }
}
