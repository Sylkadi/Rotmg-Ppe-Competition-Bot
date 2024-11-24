using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.RealmBot.Commands
{
    internal class Say : Command
    {
        public Say()
        {
            Prefix = '>';
            Name = nameof(Say);
            Arguments = 2;
        }

        public override async Task ExecuteAsync(SocketMessage source, string[] args)
        {
            await source.Channel.SendMessageAsync(args[1]);
        }
    }
}
