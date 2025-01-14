using System.Text.Json;
using System.Text.Json.Serialization;
using Discord.WebSocket;
using DiscordBot.Bot.RealmBot.Game;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace DiscordBot.Bot.RealmBot.Ppe
{
    public class Ppe : IComparable<Ppe>
    {
        [JsonInclude]
        public int totalPoints { get; set; }

        [JsonInclude]
        public int id { get; set; }

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
        public bool itemInfographicUpdated { get; set; }

        [JsonIgnore]
        public string itemInfographicImagePath
        {
            get
            {
                if (!itemInfographicUpdated || !File.Exists(_itemitemInfographicImagePath))
                {
                    UpdateInfographic();
                }
                return _itemitemInfographicImagePath;
            }
        }

        [JsonIgnore]
        private string _itemitemInfographicImagePath
        {
            get
            {
                Ppe headPpe = this;
                while (headPpe.nextPpe != null)
                {
                    headPpe = headPpe.nextPpe;
                }

                return $@"{IO.ppeDirectory}\{id}_{headPpe.userID}_Infographic.png";
            }
        }

        [JsonIgnore]
        public string itemInfographicImageName
        {
            get
            {
                Ppe headPpe = this;
                while(headPpe.nextPpe != null)
                {
                    headPpe = headPpe.nextPpe;
                }

                return $"{id}_{headPpe.userID}";
            }
        }

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
            id = ppeDeserialized.id;

            SetNextPpes();
            SetIds();

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

        public void SetIds()
        {
            Ppe currentPpe = this;
            while (currentPpe.previousPpe != null)
            {
                currentPpe = currentPpe.previousPpe;
            }

            int id = 0;
            while(currentPpe.nextPpe != null)
            {
                currentPpe.id = id;
                id++;
                currentPpe = currentPpe.nextPpe;
            }
            currentPpe.id = id;

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

        public void UpdateInfographic()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            itemInfographicUpdated = true;

            Ppe head = this;
            while(head.nextPpe != null)
            {
                head = head.nextPpe;
            }

            int itemAmount = 0;
            int shinyAmount = 0;
            int setAmount = 0;

            List<ItemCount> itemAmounts = new List<ItemCount>();
            List<ItemCount> shinyAmounts = new List<ItemCount>();
            List<Setcount> setAmounts = new List<Setcount>();

            foreach(ItemCount itemCount in itemsCounts)
            {
                if (itemCount.amount <= 0) continue;

                if(head.itemDictionary[itemCount.name].referenceItem.isShiny)
                {
                    shinyAmounts.Add(itemCount);
                    shinyAmount += itemCount.amount;
                } else
                {
                    itemAmounts.Add(itemCount);
                    itemAmount += itemCount.amount;
                }
            }

            foreach(Setcount setCount in setCounts)
            {
                if (setCount.amount <= 0) continue;

                setAmounts.Add(setCount);
                shinyAmount += setCount.amount;
            }

            itemAmounts.Sort();
            shinyAmounts.Sort();
            setAmounts.Sort();

            int widthScale = 20;
            int imageWidth = RealmBot.infographicImageSize * widthScale;
            int imageHeight = (((itemAmount / widthScale) + 1) * RealmBot.infographicImageSize) + (((shinyAmount / widthScale) + 1) * RealmBot.infographicImageSize) + ((((setAmount * 4) / widthScale) + 1) * RealmBot.infographicImageSize);

            Bitmap bitmap = new Bitmap(imageWidth, imageHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Fill background
                g.FillRectangle(Brushes.Black, 0, 0, imageWidth, imageHeight);

                int height = 0;
                int index = 0;
                int numberOffset = (RealmBot.infographicImageSize * 3) / 4;
                void IterateIndex()
                {
                    index++;
                    if (index / widthScale == 1)
                    {
                        index = 0;
                        height++;
                    }
                }

                // Non-Shiny items
                foreach(ItemCount itemCount in itemAmounts)
                {
                    Bitmap itemBitmap = new Bitmap(head.itemDictionary[itemCount.name].referenceItem.proccessedImagePath);

                    Point imagePoint = new Point(index * RealmBot.infographicImageSize, height * RealmBot.infographicImageSize);

                    g.DrawImageUnscaled(itemBitmap, imagePoint);

                    DrawNumber(g, itemCount.amount, imagePoint.X + numberOffset, imagePoint.Y + numberOffset);
                    IterateIndex();
                }
                index = 0;
                height++;

                // Shiny items
                foreach(ItemCount itemCount in shinyAmounts)
                {
                    Bitmap itemBitmap = new Bitmap(head.itemDictionary[itemCount.name].referenceItem.proccessedImagePath);

                    Point imagePoint = new Point(index * RealmBot.infographicImageSize, height * RealmBot.infographicImageSize);

                    g.DrawImageUnscaled(itemBitmap, imagePoint);

                    DrawNumber(g, itemCount.amount, imagePoint.X + numberOffset, imagePoint.Y + numberOffset);
                    IterateIndex();
                }
                index = 0;
                height++;

                // Set items
                foreach(Setcount setCount in setAmounts)
                {
                    Point imagePoint = new Point();
                    foreach (Item item in head.setDictionary[setCount.name].referenceSet.setItems)
                    {
                        Bitmap itemBitmap = new Bitmap(item.proccessedImagePath);

                        imagePoint = new Point(index * RealmBot.infographicImageSize, height * RealmBot.infographicImageSize);
                        g.DrawImageUnscaled(itemBitmap, imagePoint);
                        IterateIndex();
                    }

                    DrawNumber(g, setCount.amount, imagePoint.X + numberOffset, imagePoint.Y + numberOffset);
                    IterateIndex();
                }
            }

            bitmap.Save(_itemitemInfographicImagePath, ImageFormat.Png);

            sw.Stop();
            Log.Info($"Image creation time: {sw.ElapsedMilliseconds}ms");
        }

        private static Font numberFont = new Font(new FontFamily("Arial"), RealmBot.infographicImageSize / 4, FontStyle.Bold, GraphicsUnit.Pixel);
        private void DrawNumber(Graphics g, int value, int atX, int atY)
        {
            int valueMagnitude = (int)Math.Log10(value <= 0 ? 1 : value);
            atX -= valueMagnitude * (int)numberFont.Size;

            g.DrawString(value.ToString(), numberFont, Brushes.White, atX, atY);
        }

        public int CompareTo(Ppe? other)
        {
            return bestPpe.totalPoints.CompareTo(other.bestPpe.totalPoints);
        }
    }
}
