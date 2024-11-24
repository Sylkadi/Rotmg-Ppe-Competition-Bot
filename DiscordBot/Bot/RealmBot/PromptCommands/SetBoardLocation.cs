using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.RealmBot.PromptCommands
{
    internal class SetBoardLocation : Command
    {
        public SetBoardLocation()
        {
            Prefix = '>';
            Name = nameof(SetBoardLocation);
            Arguments = 3; // args[1] = guildID, args[2] = channelID
        }

        public override async Task ExecuteAsync(string original, string[] args)
        {
            if (string.IsNullOrEmpty(args[1]) || string.IsNullOrEmpty(args[2]))
            {
                return;
            }

            bool foundGuild = false;
            bool foundChannel = false;
            foreach (SocketGuild guild in RealmBot.Instance.client.Guilds)
            {
                if(guild.Id.ToString() == args[1])
                {
                    foreach(SocketGuildChannel channel in guild.Channels)
                    {
                        if(channel.Id.ToString() == args[2])
                        {
                            await RealmBot.Instance.competition.SetChannelAndFindMessageAsync(args[1], args[2]);
                            foundChannel = true;
                            break;
                        }
                    }
                    foundGuild = true;
                    break;
                }
            }

            if(!foundGuild)
            {
                Log.Warning($"Failed to find guild from ID:{args[1]}");
            } else
            {
                if(!foundChannel)
                {
                    Log.Warning($"Failed to find channel from ID:{args[2]}");
                }
            }
        }
    }
}
