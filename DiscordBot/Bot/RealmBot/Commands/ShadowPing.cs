using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.RealmBot.Commands
{
    internal class ShadowPing : Command
    {
        public ShadowPing() 
        {
            Prefix = '>';
            Name = nameof(ShadowPing);
            Arguments = 2;
        }

        public override async Task ExecuteAsync(SocketMessage source, string[] args)
        {
            await source.DeleteAsync();
        }
    }
}
