using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
