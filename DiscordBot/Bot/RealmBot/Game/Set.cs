using System.Text.Json.Serialization;

namespace DiscordBot.Bot.RealmBot.Game
{
    public class Set
    {
        public Set(string name, string[] itemNames, params int[] points)
        {
            this.name = name;
            this.points = points;
            this.itemNames = itemNames;
        }

        [JsonInclude]
        public string name { get; set; }

        [JsonInclude]
        public int[] points { get; set; }

        [JsonInclude]
        public string[] itemNames { get; set; }

        [JsonIgnore]
        public Item[] setItems { get; set; }
    }
}
