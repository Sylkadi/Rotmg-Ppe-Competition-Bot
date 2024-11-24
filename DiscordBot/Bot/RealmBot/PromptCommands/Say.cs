using Discord.WebSocket;

namespace DiscordBot.Bot.RealmBot.PromptCommands
{
    internal class Say : Command
    {
        public Say()
        {
            Prefix = '>';
            Name = nameof(Say);
            Description = "args[1] = Guild, args[2] = Channel, args[3] = message";
            Arguments = 4;
        }

        public override async Task ExecuteAsync(string original, string[] args)
        {
            foreach (string arg in args)
            {
                if (string.IsNullOrEmpty(arg))
                {
                    Log.Info("ExecuteAsync(), argument is empty or null");
                    return;
                }
            }

            SocketGuildChannel channel = GetChannelFromArgs(args[1], args[2]);
            if(channel != null)
            {
                await ((ISocketMessageChannel)channel).SendMessageAsync(args[3]);
            }
        }
    }
}
