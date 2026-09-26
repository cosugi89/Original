using System.Linq;
using Assets.Scripts.Systems.Save.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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

            var root = JObject.Parse(json);
            MigrateLegacyResourceBalances(root);
            return root.ToObject<UserData>(JsonSerializer.Create(Settings)) ?? new UserData();
        }

        private static void MigrateLegacyResourceBalances(JObject root)
        {
            var inventory = root["Inventory"] as JObject ?? new JObject();
            root["Inventory"] = inventory;
            var itemStacks = inventory["ItemStacks"] as JArray ?? new JArray();
            inventory["ItemStacks"] = itemStacks;

            var profile = root["Profile"] as JObject;
            MoveLegacyBalance(profile, "Gold", Assets.Scripts.Systems.GameData.InventoryItemIds.Gold, itemStacks);
            profile?.Remove("Gem");

            var ability = root["Ability"] as JObject;
            MoveLegacyBalance(ability, "PointBalance", Assets.Scripts.Systems.GameData.InventoryItemIds.AbilityPoint, itemStacks);
        }

        private static void MoveLegacyBalance(JObject source, string propertyName, int itemId, JArray itemStacks)
        {
            var value = source?.Value<int?>(propertyName) ?? 0;
            if (value <= 0)
            {
                source?.Remove(propertyName);
                return;
            }

            var existing = itemStacks.OfType<JObject>().FirstOrDefault(stack => stack.Value<int?>("ItemId") == itemId);
            if (existing == null)
            {
                itemStacks.Add(new JObject { ["ItemId"] = itemId, ["Quantity"] = value });
            }
            else
            {
                var existingQuantity = existing.Value<int?>("Quantity") ?? 0;
                existing["Quantity"] = (int)System.Math.Min(int.MaxValue, (long)existingQuantity + value);
            }

            source.Remove(propertyName);
        }
    }
}
