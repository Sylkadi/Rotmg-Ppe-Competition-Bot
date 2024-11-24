using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.RealmBot.PromptCommands
{
    internal class ShadowPing : Command
    {
        public ShadowPing()
        {
            Prefix = '>';
            Name = nameof(ShadowPing);
            Arguments = 4;
        }

        public override async Task ExecuteAsync(string original, string[] args)
        {
            SocketGuildChannel channel = GetChannelFromArgs(args[1], args[2]);
            if(channel != null)
            {
                await ((ISocketMessageChannel)channel).SendMessageAsync(args[3]).Result.DeleteAsync();
            }
        }
    }
}
