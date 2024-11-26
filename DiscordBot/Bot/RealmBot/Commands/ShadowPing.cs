using Discord.WebSocket;

namespace DiscordBot.Bot.RealmBot.Commands
{
    public class ShadowPing : Command
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
