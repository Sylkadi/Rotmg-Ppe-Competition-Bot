using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBot.Bot.RealmBot.Game
{
    public class Item
    {
        [JsonConstructor]
        public Item(string name, string imageName, string[] nickNames, BagType bagType, params int[] points)
        {
            this.name = name;
            this.imageName = imageName;
            this.bagType = bagType;
            this.points = points;
            this.nickNames = nickNames;
        }

        public Item(string name, string[] nickNames, BagType bagType, params int[] points)
        {
            this.name = name;
            this.imageName = name;
            this.bagType = bagType;
            this.points = points;
            this.nickNames = nickNames;
        }

        [JsonInclude]
        public string name { get; set; }

        [JsonIgnore]
        private string _imageName { get; set; }

        [JsonInclude]
        public string imageName {
            get 
            {
                return _imageName;
            }
            set 
            {
                _imageName = value;
                imageNameNoSpace = _imageName.Replace(' ', '_');
            }
        }

        [JsonIgnore]
        public string imageNameNoSpace { get; private set; } // Discord dosent like spaces in attachments...

        [JsonIgnore]
        public string imagePath
        {
            get
            {
                return $@"{IO.imagesDirectory}\{imageName}.png";
            }
        }

        [JsonInclude]
        public string[] nickNames { get; set; }

        [JsonInclude]
        public int[] points { get; set; }

        [JsonInclude]
        public BagType bagType { get; set; }

        [JsonIgnore]
        public Set relatedSet { get; set; }

        public enum BagType
        {
            None, Cyan, Blue, Red, Golden, Orange, White
        }
    }
}
