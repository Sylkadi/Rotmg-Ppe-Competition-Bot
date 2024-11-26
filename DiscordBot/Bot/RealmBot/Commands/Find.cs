using Discord.WebSocket;

namespace DiscordBot.Bot.RealmBot.Commands
{
    public class Find : Command
    {
        public Find()
        {
            Prefix = '>';
            Name = nameof(Find);
            Arguments = 2;
        }

        public override async Task ExecuteAsync(SocketMessage source, string[] args)
        {
            SocketGuild guild = ((SocketGuildChannel)source.Channel).Guild;

            List<NameMatchF> nameMatch = new List<NameMatchF>();
            foreach(SocketGuildUser user in guild.Users)
            {
                if (user == null || user.Username == null) continue;

                nameMatch.Add(new NameMatchF(user.Username, Util.StringMatchPercentage(args[1], user.Username)));
            }

            if(nameMatch.Count > 0)
            {
                nameMatch.Sort();

                await source.Channel.SendMessageAsync($"{nameMatch[^1].match}, {nameMatch[^1].name}");
            }
        }

        private struct NameMatchF : IEquatable<NameMatchF>, IComparable<NameMatchF>
        {
            public string name;

            public float match;

            public NameMatchF(string name, float match)
            {
                this.name = name;
                this.match = match;
            }

            public int CompareTo(NameMatchF other)
            {
                return match.CompareTo(other.match);
            }

            public bool Equals(NameMatchF other)
            {
                return name == other.name;
            }
        }
    }
}
