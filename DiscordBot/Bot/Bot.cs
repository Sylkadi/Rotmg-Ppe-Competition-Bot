using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Bot
{
    public class Bot
    {
        public virtual DiscordSocketClient client { get; set; }

        internal virtual string token { get; set; }

        public virtual async Task StartBotAsync()
        {
            await client.LoginAsync(Discord.TokenType.Bot, token);
            await client.StartAsync();
        }

        public virtual async void StopBotAsync() => await client.LogoutAsync();
    }
}
