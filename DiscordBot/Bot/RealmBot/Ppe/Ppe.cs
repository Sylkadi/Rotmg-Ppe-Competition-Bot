using System.Text.Json;
using System.Text.Json.Serialization;
using Discord.WebSocket;
using DiscordBot.Bot.RealmBot.Game;

namespace DiscordBot.Bot.RealmBot.Ppe
{
    public class Ppe : IComparable<Ppe>
    {
        [JsonInclude]
        public int totalPoints { get; set; }

        [JsonInclude]
        public ItemCount[] itemsCounts { get; set; }

        [JsonInclude]
        public Setcount[] setCounts { get; set; }

        [JsonInclude]
        public Bagcount bagCount { get; set; }

        [JsonInclude]
        public Ppe previousPpe { get; set; }

        [JsonIgnore]
        public Ppe nextPpe { get; set; }

        [JsonIgnore]
        public Ppe bestPpe { get; set; }

        [JsonIgnore]
        public string userID { get; set; }

        [JsonIgnore]
        public SocketGuildUser guildUser { get; set; }

        [JsonIgnore]
        public string filePath { get; private set; }

        [JsonIgnore]
        public Dictionary<string, ItemCount> itemDictionary { get; private set; }

        [JsonIgnore]
        public Dictionary<string, Setcount> setDictionary { get; private set; }

        [JsonIgnore]
        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

        public void Iniatlize(string userID, PointList fromList)
        {
            this.userID = userID;

            SetAsTemplate(fromList);
            if (CreateFile(userID))
            {
                Serialize();
            }

            Deserialize();
        }

        public void SetAsTemplate(PointList fromList)
        {
            itemsCounts = new ItemCount[fromList.items.Length];
            itemDictionary = new Dictionary<string, ItemCount>();
            for (int i = 0; i < fromList.items.Length; i++)
            {
                ItemCount itemCount = new ItemCount() { amount = 0, name = fromList.items[i].name, referenceItem = fromList.items[i] };

                itemsCounts[i] = itemCount;
                if (!itemDictionary.TryAdd(itemCount.name, itemCount))
                {
                    Log.Warning($"Item:{itemCount.name} already exists in itemDictionary");
                }
            }

            setCounts = new Setcount[fromList.sets.Length];
            setDictionary = new Dictionary<string, Setcount>();
            for (int i = 0; i < fromList.sets.Length; i++)
            {
                Setcount setCount = new Setcount() { amount = 0, name = fromList.sets[i].name, referenceSet = fromList.sets[i] };

                setCounts[i] = setCount;
                if (!setDictionary.TryAdd(setCount.name, setCount))
                {
                    Log.Warning($"Set:{setCount.name} already exists in itemDictionary");
                }
            }

            bagCount = new Bagcount();
        }

        public void RecacluateTotalPoints()
        {
            totalPoints = 0;

            bagCount = new Bagcount();

            foreach(ItemCount itemCount in itemsCounts)
            {
                totalPoints += itemCount.Compute();
                bagCount.Add(itemCount.referenceItem.bagType, itemCount.amount);
            }

            foreach(Setcount setCount in setCounts)
            {
                setCount.UpdateSetCount(itemDictionary);
                totalPoints += setCount.Compute();
            }
        }

        public bool CreateFile(string fileName)
        {
            if (!string.IsNullOrEmpty(filePath)) return false;
            filePath = IO.ppeDirectory + @"\" + fileName;

            if(!File.Exists(filePath))
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
                Log.Warning("Ppe.Deserialize(), filePath is null or empty, returning");
                return;
            }

            Ppe ppeDeserialized = null;

            using (Stream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                string fileContents = "{}";
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContents = reader.ReadToEnd();
                    reader.Close();
                }

                ppeDeserialized = JsonSerializer.Deserialize<Ppe>(fileContents, jsonOptions);

                stream.Close();
            }

            if (ppeDeserialized == null)
            {
                Log.Warning("Deserialize(), pointListDeserialized is null, returning");
                return;
            }

            foreach(ItemCount deserializedItemCount in ppeDeserialized.itemsCounts)
            {
                if(itemDictionary.TryGetValue(deserializedItemCount.name, out ItemCount itemCount))
                {
                    itemCount.amount = deserializedItemCount.amount;
                } else
                {
                    Log.Warning($"Deserialize(), Failed to find ItemCount:{deserializedItemCount.name}");
                }
            }

            foreach(Setcount deserializedSetcount in ppeDeserialized.setCounts)
            {
                if(setDictionary.TryGetValue(deserializedSetcount.name, out Setcount setCount))
                {
                    setCount.amount = deserializedSetcount.amount;
                } else
                {
                    Log.Warning($"Deserialize(), Failed to find ItemCount:{deserializedSetcount.name}");
                }
            }

            totalPoints = ppeDeserialized.totalPoints;
            bagCount = ppeDeserialized.bagCount;
            previousPpe = ppeDeserialized.previousPpe;
            foreach(SocketGuild guild in RealmBot.Instance.client.Guilds)
            {
                guildUser = guild.GetUser(Convert.ToUInt64(userID));
                if(guildUser != null)
                {
                    break;
                }
            }

            SetNextPpes();

            RecacluateTotalPoints();
            DetermineBestPpe();
        }

        public void DetermineBestPpe()
        {
            int bestPoints = totalPoints;
            Ppe bestPpe = this;

            Ppe currentPpe = previousPpe;
            while (currentPpe != null)
            {
                if (currentPpe.totalPoints > bestPoints)
                {
                    bestPoints = currentPpe.totalPoints;
                    bestPpe = currentPpe;
                }
                currentPpe = currentPpe.previousPpe;
            }

            this.bestPpe = bestPpe;
        }

        public void SetNextPpes()
        {
            Ppe currentPpe = previousPpe;
            Ppe nextPpe = this;
            while(currentPpe != null)
            {
                currentPpe.nextPpe = nextPpe;
                nextPpe = currentPpe;
                currentPpe = currentPpe.previousPpe;
            }
        }

        public static Ppe GetOrCreatePpeFromList(string id, List<Ppe> fromList, PointList pointList)
        {
            Ppe currentPpe = null;

            bool alreadyExists = false;
            foreach(Ppe ppe in fromList)
            {
                if(id == ppe.userID)
                {
                    currentPpe = ppe;
                    alreadyExists = true;
                }
            }

            if(!alreadyExists)
            {
                currentPpe = new Ppe();
                currentPpe.Iniatlize(id, pointList);
                lock(fromList)
                {
                    fromList.Add(currentPpe);
                }
            }

            return currentPpe;
        }

        public int CompareTo(Ppe? other)
        {
            return bestPpe.totalPoints.CompareTo(other.bestPpe.totalPoints);
        }
    }
}
