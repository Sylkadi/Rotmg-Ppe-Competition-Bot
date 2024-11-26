using DiscordBot.Bot;

namespace DiscordBot
{
    public class Program
    {
        public static List<Bot.Bot> bots = new List<Bot.Bot>()
        {
            new Bot.RealmBot.RealmBot()
        };

        public static Dictionary<string, Command> commands = Command.CreateCommandDictionary(
            new Bot.RealmBot.PromptCommands.Say(),
            new Bot.RealmBot.PromptCommands.ShadowPing(),
            new Bot.RealmBot.PromptCommands.SetBoardLocation()
        );

        static async Task Main(string[] args)
        {
            Parallel.ForEach(bots, async bot => await bot.StartBotAsync());

            Console.WriteLine("Type Q to end");
            while (true)
            {
                string userInput = Console.ReadLine();

                if (!string.IsNullOrEmpty(userInput))
                {
                    if (userInput.ToUpper() == "Q")
                    {
                        goto End;
                    }
                    await Command.ExecuteFromDictionaryAsync(commands, userInput);
                }
            }

        End:;
        }
    }
}