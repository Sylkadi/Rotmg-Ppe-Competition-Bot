using Discord.WebSocket;
using DiscordBot.Bot.RealmBot;
using static DiscordBot.Util;

namespace DiscordBot
{
    public class Command
    {
        public Command()
        {
            Prefix = '>';
            Name = new Random().Next(int.MaxValue).ToString();
            Description = "";
            Arguments = 0;
        }

        private char prefix;
        public char Prefix
        { 
            get { return prefix; }
            set
            {
                if(value == ' ')
                {
                    Log.Warning("Prefix cannot be ' ', setting prefix to >");
                    prefix = '>';
                } else
                {
                    prefix = value;
                }
            } 
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    Log.Warning("Command Name cannot be empty or null, setting name to a random number.");
                    name = new Random().Next(int.MaxValue).ToString();
                }
                else if (value.Contains(" "))
                {
                    Log.Warning("Command Name cannot contain \" \", setting name to a random number.");
                    name = new Random().Next(int.MaxValue).ToString();
                } else
                {
                    name = value.ToUpper();
                }
            }
        }
        public string Description { get; set; }

        public int Arguments { get; set; }

        public virtual async Task ExecuteAsync(SocketMessage source, string[] args) { }

        public static async Task ExecuteFromDictionaryAsync(Dictionary<string, Command> from, SocketMessage input)
        {
            string message = input.Content;
            if (string.IsNullOrEmpty(message)) return;

            string[] inputSplit = message.Split(' ');
            if (string.IsNullOrEmpty(inputSplit[0])) return;

            string withoutPrefix = inputSplit[0][1..];
            Command cmd = new Command();
            if(from.TryGetValue(withoutPrefix.ToUpper(), out cmd) && inputSplit[0][0] == cmd.prefix)
            {
                await cmd.ExecuteAsync(input, Util.GetArguements(message, cmd.Arguments));
                return;
            }
        }

        public virtual async Task ExecuteAsync(string original, string[] args) { }

        public static async Task ExecuteFromDictionaryAsync(Dictionary<string, Command> from, string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            string[] inputSplit = input.Split(' ');
            if (string.IsNullOrEmpty(inputSplit[0])) return;

            string withouthPrefix = inputSplit[0][1..];
            Command cmd = new Command();

            if(from.TryGetValue(withouthPrefix.ToUpper(), out cmd) && inputSplit[0][0] == cmd.prefix)
            {
                await cmd.ExecuteAsync(input, Util.GetArguements(input, cmd.Arguments));
                return;
            }
        }

        public static Dictionary<string, Command> CreateCommandDictionary(params Command[] commands)
        {
            Dictionary<string, Command> output = new Dictionary<string, Command>();

            foreach(Command command in commands)
            {
                if(!output.TryAdd(command.Name, command))
                {
                    Log.Warning($"Failed to add {command.Name} to the dictionary, as it already exits.");
                }
            }

            return output;
        }

        public static SocketGuildChannel GetChannelFromArgs(string guildIdName, string channelIdName, int idNameDistance = 1)
        {
            List<GuildMatch> nameMatches = new List<GuildMatch>();
            List<GuildMatch> idMatches = new List<GuildMatch>();
            foreach (SocketGuild guild in RealmBot.Instance.client.Guilds)
            {
                nameMatches.Add(new GuildMatch(guildIdName, StringMatchDistance(guildIdName, guild.Name), guild));
                idMatches.Add(new GuildMatch(guildIdName, StringMatchDistance(guildIdName, guild.Id.ToString()), guild));
            }
            nameMatches.Sort();
            idMatches.Sort();

            SocketGuild chosenGuild = null;

            if (nameMatches[0].match <= idNameDistance)
            {
                chosenGuild = nameMatches[0].guild;
            }
            else if (idMatches[0].match <= idNameDistance)
            {
                chosenGuild = idMatches[0].guild;
            }

            if (chosenGuild == null)
            {
                Log.Info("GetChannelFromArgs(), Failed to find guild name or id, returning null");
                return null;
            }

            List<ChannelMatch> channelNameMatches = new List<ChannelMatch>();
            List<ChannelMatch> channelIdMatches = new List<ChannelMatch>();
            foreach (SocketGuildChannel channel in chosenGuild.Channels)
            {
                channelNameMatches.Add(new ChannelMatch(channelIdName, StringMatchDistance(channelIdName, channel.Name), channel));
                channelIdMatches.Add(new ChannelMatch(channelIdName, StringMatchDistance(channelIdName, channel.Id.ToString()), channel));
            }
            channelNameMatches.Sort();
            channelIdMatches.Sort();

            SocketGuildChannel chosenChannel = null;

            if (channelNameMatches[0].match <= idNameDistance)
            {
                chosenChannel = channelNameMatches[0].channel;
            }
            else if (channelIdMatches[0].match <= idNameDistance)
            {
                chosenChannel = channelIdMatches[0].channel;
            }

            if (chosenChannel == null)
            {
                Log.Info("GetChannelFromArgs(), Failed to find channel name or id, returning null");
                return null;
            }

            return chosenChannel;
        }

        public static SocketGuildUser GetUserFromName(SocketGuild fromGuild, string name)
        {
            List<UserMatch> nameMatches = new List<UserMatch>();
            foreach(SocketGuildUser guildUser in fromGuild.Users)
            {
                if (guildUser == null) continue;

                if(guildUser.Id.ToString() == name)
                {
                    return guildUser;
                }
                if(!string.IsNullOrEmpty(guildUser.DisplayName))
                {
                    nameMatches.Add(new UserMatch(guildUser.DisplayName, StringMatchDistance(guildUser.DisplayName, name), guildUser));
                }
                if(!string.IsNullOrEmpty(guildUser.GlobalName))
                {
                    nameMatches.Add(new UserMatch(guildUser.GlobalName, StringMatchDistance(guildUser.GlobalName, name), guildUser));
                }
                if(!string.IsNullOrEmpty(guildUser.Nickname))
                {
                    nameMatches.Add(new UserMatch(guildUser.Nickname, StringMatchDistance(guildUser.Nickname, name), guildUser));
                }
                if(!string.IsNullOrEmpty(guildUser.Username))
                {
                    nameMatches.Add(new UserMatch(guildUser.Username, StringMatchDistance(guildUser.Username, name), guildUser));
                }
            }
            nameMatches.Sort();

            return nameMatches[0].user;
        }

        private struct GuildMatch : IEquatable<GuildMatch>, IComparable<GuildMatch>
        {
            public string name;

            public int match;

            public SocketGuild guild;

            public GuildMatch(string name, int match, SocketGuild guild)
            {
                this.name = name;
                this.match = match;
                this.guild = guild;
            }

            public int CompareTo(GuildMatch other)
            {
                return match.CompareTo(other.match);
            }

            public bool Equals(GuildMatch other)
            {
                return name == other.name;
            }
        }

        private struct ChannelMatch : IEquatable<ChannelMatch>, IComparable<ChannelMatch>
        {
            public string name;

            public int match;

            public SocketGuildChannel channel;

            public ChannelMatch(string name, int match, SocketGuildChannel channel)
            {
                this.name = name;
                this.match = match;
                this.channel = channel;
            }

            public int CompareTo(ChannelMatch other)
            {
                return match.CompareTo(other.match);
            }

            public bool Equals(ChannelMatch other)
            {
                return name == other.name;
            }
        }

        private struct UserMatch : IEquatable<UserMatch>, IComparable<UserMatch>
        {
            public string name;

            public int match;

            public SocketGuildUser user;

            public UserMatch(string name, int match, SocketGuildUser user)
            {
                this.name = name;
                this.match = match;
                this.user = user;
            }

            public int CompareTo(UserMatch other)
            {
                return match.CompareTo(other.match);
            }

            public bool Equals(UserMatch other)
            {
                return other.name == name;
            }
        }
    }
}
