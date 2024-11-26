using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Text.Json;
using System.Text.Json.Serialization;
using static DiscordBot.Bot.RealmBot.Emojis.Emojis;
using DiscordBot.Bot.RealmBot.Game;
using DiscordBot.Bot.RealmBot.Ppe;

namespace DiscordBot.Bot.RealmBot
{
    public class Competition
    {
        [JsonInclude]
        public string guildID { get; set; }

        [JsonInclude]
        public string channelID { get; set; }

        [JsonInclude]
        public string messageID { get; set; }

        [JsonIgnore]
        public SocketChannel scoreboardChannel { get; set; }

        [JsonIgnore]
        public RestUserMessage scoreboardMessage { get; set; }

        [JsonIgnore]
        public List<Ppe.Ppe> ppes { get; set; }

        [JsonIgnore]
        public PointList pointList { get; set; }

        [JsonIgnore]
        private int nextUpdateTick = 0;

        [JsonIgnore]
        public string filePath
        {
            get
            {
                return IO.generalDirectory + @"\competition.txt";
            } 
        }

        [JsonIgnore]
        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

        public void Serialize()
        {
            if(!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            string jsonString = JsonSerializer.Serialize(this, jsonOptions);
            File.WriteAllText(filePath, jsonString);
        }

        public async Task DeserializeAsync()
        {
            if(!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                Serialize();
            }

            Competition competitionDeserialized = null;

            using (Stream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                string fileContents = "{}";
                using (StreamReader reader = new StreamReader(stream))
                {
                    fileContents = reader.ReadToEnd();
                    reader.Close();
                }
                competitionDeserialized = JsonSerializer.Deserialize<Competition>(fileContents, jsonOptions);

                stream.Close();
            }

            if(competitionDeserialized == null)
            {
                Log.Warning("Deserialize(), competitionDeserialized is null, returning");
                return;
            }

            guildID = competitionDeserialized.guildID;
            channelID = competitionDeserialized.channelID;
            messageID = competitionDeserialized.messageID;

            if(!string.IsNullOrEmpty(guildID) && !string.IsNullOrEmpty(channelID))
            {
                scoreboardChannel = Command.GetChannelFromArgs(guildID, channelID, 0);
                if(!string.IsNullOrEmpty(messageID))
                {
                    scoreboardMessage = (RestUserMessage)await ((ISocketMessageChannel)scoreboardChannel).GetMessageAsync(Convert.ToUInt64(messageID));
                }
            }
        }

        public async Task SetChannelAndFindMessageAsync(string guildID, string channelID)
        {
            bool found = false;
            foreach(SocketGuild guild in RealmBot.Instance.client.Guilds)
            {
                if (guild.Id.ToString() != guildID) continue;
                
                foreach(SocketGuildChannel channel in guild.Channels)
                {
                    if(channel.Id.ToString() == channelID)
                    {
                        this.guildID = guildID;
                        this.channelID = channelID;
                        scoreboardChannel = channel;

                        found = true;
                        break;
                    }
                }
                break;
            }

            if(!found)
            {
                Log.Warning($"Failed to find guild:{guildID} and channel:{channelID}");
            } else
            {
                if(!string.IsNullOrEmpty(messageID))
                {
                    scoreboardMessage = (RestUserMessage)await ((ISocketMessageChannel)scoreboardChannel).GetMessageAsync(Convert.ToUInt64(messageID));
                }
            }

            Serialize();
        }

        public async Task UpdateBoardAsync()
        {
            if (scoreboardChannel == null) return;

            if(Environment.TickCount > nextUpdateTick)
            {
                nextUpdateTick = Environment.TickCount + 10000;
            } else
            {
                return;
            }

            if (scoreboardMessage == null)
            {
                if (!string.IsNullOrEmpty(messageID))
                {
                    SocketMessage message = ((ISocketMessageChannel)scoreboardChannel).GetCachedMessage(Convert.ToUInt64(messageID));
                    Log.Info((message == null).ToString());
                    if(message == null)
                    {
                        await CreateMessageAsync();
                    } else
                    {
                        scoreboardMessage = (RestUserMessage)(IUserMessage)message;
                        await scoreboardMessage.ModifyAsync(x => x.Embed = GetScoreboardEmbed().Build());
                    }
                } else
                {
                    await CreateMessageAsync();
                }
            } else
            {
                await scoreboardMessage.ModifyAsync(x => x.Embed = GetScoreboardEmbed().Build());
            }
        }

        private async Task CreateMessageAsync()
        {
            RestUserMessage message = await ((ISocketMessageChannel)scoreboardChannel).SendMessageAsync("", embed: GetScoreboardEmbed().Build());

            messageID = message.Id.ToString();
            scoreboardMessage = message;

            Serialize();
        }

        public EmbedBuilder GetScoreboardEmbed()
        {
            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = "**Leaderboard**",
                Color = Color.Purple
            };

            lock(ppes)
            {
                foreach (Ppe.Ppe ppe in ppes)
                {
                    ppe.DetermineBestPpe();
                }
                ppes.Sort();
                ppes.Reverse();

                for(int i = 0; i < ppes.Count; i++)
                {
                    string name = ppes[i].guildUser == null ? "null" : (ppes[i].guildUser.Nickname ?? ppes[i].guildUser.Username);
                    builder.AddField(x =>
                    {
                        x.Name = $"**{i + 1}. {name}**";
                        x.Value =
                        $"**Points:** {ppes[i].bestPpe.totalPoints} " + 
                        $"{GetEmoteString("whitebag")} {ppes[i].bestPpe.bagCount.whiteBagCount} " +
                        $"{GetEmoteString("orangebag")} {ppes[i].bestPpe.bagCount.orangeBagCount} " +
                        $"{GetEmoteString("goldenbag")} {ppes[i].bagCount.goldenBagCount}" +
                        $"{GetEmoteString("redbag")} {ppes[i].bestPpe.bagCount.redBagCount} " +
                        $"{GetEmoteString("cyanbag")} {ppes[i].bestPpe.bagCount.cyanBagCount} " +
                        $"{GetEmoteString("exaltedblueprint")} {ppes[i].bestPpe.bagCount.blueBagCount}";
                        x.IsInline = false;
                    });
                }
            }


            return builder;
        }
    }
}
