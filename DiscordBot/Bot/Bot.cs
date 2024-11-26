using Discord.WebSocket;

namespace DiscordBot.Bot
{
    public class Bot
    {
        public virtual DiscordSocketClient client { get; set; }

        public virtual string token { get; set; }

        public virtual async Task StartBotAsync()
        {
            await client.LoginAsync(Discord.TokenType.Bot, token);
            await client.StartAsync();
        }

        public virtual async void StopBotAsync() => await client.LogoutAsync();
    }
}
