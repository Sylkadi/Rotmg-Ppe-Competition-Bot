using Discord.WebSocket;

namespace DiscordBot.Bot.RealmBot.Commands
{
    public class Say : Command
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
