using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordBot.Bot.RealmBot.Game
{
    public class PointList
    {
        [JsonInclude]
        public Item[] items { get; set; }

        [JsonInclude]
        public Set[] sets { get; set; }

        [JsonIgnore]
        public List<string> upperCaseItemNames { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> upperCaseToNormalDictionary { get; set; }

        [JsonIgnore]
        public Dictionary<string, Item> itemDictonary { get; private set; }

        [JsonIgnore]
        public Dictionary<string, Set> setDictionary { get; private set; }

        [JsonIgnore]
        public string filePath { get; private set; }

        [JsonIgnore]
        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

        public bool CreateFile(string fileName)
        {
            if (!string.IsNullOrEmpty(filePath)) return false;
            filePath = IO.gameDirectory + @"\" + fileName;

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return true;
            }

            return false;
        }

        public void Serialize()
        {
            if(string.IsNullOrEmpty(filePath))
            {
                Log.Warning("Serialize(), filePath is null or empty, returning");
                return;
            }

            string jsonString = JsonSerializer.Serialize(this, jsonOptions);
            File.WriteAllText(filePath, jsonString);
        }

        public void Deserialize()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Log.Warning("Deserialize(), filePath is null or empty, returning");
                return;
            }
            PointList pointListDeserialized = null;

            using(Stream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                string fileContents = "{}";
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContents = reader.ReadToEnd();
                }
                try
                {
                    pointListDeserialized = JsonSerializer.Deserialize<PointList>(fileContents, jsonOptions);
                } catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }

            if (pointListDeserialized == null)
            {
                Log.Warning("Deserialize(), pointListDeserialized is null, returning");
                return;
            }

            items = pointListDeserialized.items;
            sets = pointListDeserialized.sets;

            WeaveAndSetDictionaries();
        } 

        public void WeaveAndSetDictionaries() // Only cast after item and set arrays are set
        {
            itemDictonary = new Dictionary<string, Item>();
            upperCaseItemNames = new List<string>();
            upperCaseToNormalDictionary = new Dictionary<string, string>();
            foreach (Item item in items)
            {
                if (!itemDictonary.TryAdd(item.name, item))
                {
                    Log.Warning($"WeaveAndSetDictionaries(), item:{item.name} already exists in itemDictionary.");
                } else
                {
                    string upperCaseName = item.name.ToUpper();

                    upperCaseItemNames.Add(upperCaseName);
                    upperCaseToNormalDictionary.TryAdd(upperCaseName, item.name);
                }

                foreach(string nickName in item.nickNames)
                {
                    if(!itemDictonary.TryAdd(nickName, item))
                    {
                        Log.Warning($"WeaveAndSetDictionaries(), item nickname:{item.name} already exists in itemDictionary.");
                    } else
                    {
                        string upperCaseNickName = nickName.ToUpper();

                        upperCaseItemNames.Add(upperCaseNickName);
                        upperCaseToNormalDictionary.Add(upperCaseNickName, nickName);
                    }
                }
            }

            setDictionary = new Dictionary<string, Set>();
            foreach (Set set in sets)
            {
                if (!setDictionary.TryAdd(set.name, set))
                {
                    Log.Warning($"WeaveAndSetDictionaries(), set:{set.name} already exists in setDictionary.");
                }
            }

            foreach (Set set in sets)
            {
                set.setItems = new Item[set.itemNames.Length];

                for (int i = 0; i < set.itemNames.Length; i++)
                {
                    if (itemDictonary.TryGetValue(set.itemNames[i], out Item item))
                    {
                        set.setItems[i] = item;
                        item.relatedSet = set;
                    }
                }
            }


        }
    }
}
