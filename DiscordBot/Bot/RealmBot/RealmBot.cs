using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using DiscordBot.Bot.RealmBot.Commands;
using Discord;
using DiscordBot.Bot.RealmBot.Game;
using Discord.Rest;

namespace DiscordBot.Bot.RealmBot
{
    public class RealmBot : Bot
    {
        public static RealmBot Instance { get; private set; }

        public Dictionary<string, Command> Commands = Command.CreateCommandDictionary(
            new Find(),
            new Say(),
            new ShadowPing(),
            new Add(),
            new Reset(),
            new Total()
        );

        public Competition competition { get; private set; }

        public string adminID { get; private set; }

        public List<string> userBlackList { get; set; }

        public List<BotButtonMessage> botButtonMessages { get; set; }

        public RealmBot()
        {
            Instance = this;
            IO.Iniatlize();

            botButtonMessages = new List<BotButtonMessage>();
            userBlackList = new List<string>();

            ConfigurationBuilder builder = new ConfigurationBuilder();
            IConfiguration configuration = builder.AddUserSecrets<Program>().Build();

            token = configuration.GetSection("realmBot")["token"];
            adminID = configuration.GetSection("realmBot")["adminID"];

            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
            };

            client = new DiscordSocketClient(config);

            client.MessageReceived += MessageHandlerAsync;
            client.ButtonExecuted += ButtonHandlerAsync;
            client.Ready += ReadyAsync;
        }

        public override async Task StartBotAsync()
        {
            await base.StartBotAsync();
        }

        public async Task ExpireButtonsAsync()
        {
            List<BotButtonMessage> expiredButtons = new List<BotButtonMessage>();

            lock (botButtonMessages)
            {
                for (int i = 0; i < botButtonMessages.Count; i++)
                {
                    if (botButtonMessages[i].IsExpired())
                    {
                        expiredButtons.Add(botButtonMessages[i]);
                        botButtonMessages.RemoveAt(i);
                        i--;
                    }
                }
            }

            foreach (BotButtonMessage botButtonMessage in expiredButtons)
            {
                await botButtonMessage.DisableAllButtons();
            }
        }

        private async Task ButtonHandlerAsync(SocketMessageComponent component)
        {
            Ppe.Ppe ppe = competition.ppes.FirstOrDefault(x => x.userID == component.User.Id.ToString(), null);

            string[] args = component.Data.CustomId.Split('|');

            // arg[0] = buttonName, arg[1] = authorID, arg[2..] = whatever after
            if(ppe != null && args.Length > 1)
            {
                switch(args[0])
                {
                    case "PPE_RESET_CONFIRM" when args[1] == component.User.Id.ToString() || args[1] == adminID:
                        await Reset.ResetPpeAsync(ppe, component);
                        break;
                    case "PPE_RESET_DECLINE" when args[1] == component.User.Id.ToString() || args[1] == adminID:
                        await Reset.DeclineResetPpeAsync(component);
                        break;
                    case "ITEM_INCREMENT" when (args[1] == component.User.Id.ToString() || args[1] == adminID) && args.Length > 2:
                        await Add.ItemIncrementAsync(ppe, args[2], component);
                        break;
                    case "ITEM_DECREMENT" when (args[1] == component.User.Id.ToString() || args[1] == adminID) && args.Length > 2:
                        await Add.ItemDecrementAsync(ppe, args[2], component);
                        break;
                    case "PPE_TOTAL_PREVIOUS":
                        await Total.PreviousPpe(Int32.Parse(args[1]), component);
                        break;
                    case "PPE_TOTAL_NEXT":
                        await Total.NextPpe(Int32.Parse(args[1]), component);
                        break;
                }
            }
        }

        private async Task MessageHandlerAsync(SocketMessage message)
        {
            if (message == null || message.Author == null || message.Author.IsBot) return;

            await Command.ExecuteFromDictionaryAsync(Commands, message);
        }

        private async Task ReadyAsync()
        {
            Log.Trace($"Bot {client.CurrentUser.Username} is ready.");
            await Emojis.Emojis.InitalizeAsync();

            // Verify default point list images
            foreach(Item item in DefaultPointList.items)
            {
                if(!File.Exists(item.imagePath))
                {
                    Log.Warning($"Item: {item.name} is missing image at {item.imagePath}");
                }
            }
            competition = new Competition();
            competition.pointList = new PointList();

            if (competition.pointList.CreateFile("pointSheet.txt"))
            {
                competition.pointList.items = DefaultPointList.items;
                competition.pointList.sets = DefaultPointList.sets;

                competition.pointList.WeaveAndSetDictionaries();
                competition.pointList.Serialize();
            }
            else
            {
                competition.pointList.Deserialize();
            }

            competition.ppes = new List<Ppe.Ppe>();
            string[] ppeFiles = Directory.GetFiles(IO.ppeDirectory);

            foreach (string ppeFile in ppeFiles)
            {
                string id = Path.GetFileName(ppeFile);

                Ppe.Ppe ppe = new Ppe.Ppe();
                ppe.Iniatlize(id, competition.pointList);

                competition.ppes.Add(ppe);
            }

            await competition.DeserializeAsync();

            while (true)
            {
                try
                {
                    await Task.Delay(1000);

                    await ExpireButtonsAsync();
                    foreach (KeyValuePair<int, Total.TotalViewer> keyValuePair in Total.totalViewers)
                    {
                        await keyValuePair.Value.AttemptExpireAsync();
                    }
                    await competition.UpdateBoardAsync();
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }

        public struct BotButtonMessage
        {
            public int expireTick;

            public RestUserMessage message;

            public ButtonBuilder[] buttons;

            public BotButtonMessage(float seconds, RestUserMessage message, ButtonBuilder[] buttons)
            {
                expireTick = Environment.TickCount + Convert.ToInt32(seconds * 1000.0f);
                this.message = message;
                this.buttons = buttons;
            }

            public BotButtonMessage(RestUserMessage message, ButtonBuilder[] buttons)
            {
                expireTick = Environment.TickCount + 60000;
                this.message = message;
                this.buttons = buttons;
            }

            public bool IsExpired() => Environment.TickCount >= expireTick;

            public async Task DisableAllButtons()
            {
                ComponentBuilder builder = new ComponentBuilder();
                foreach(ButtonBuilder button in buttons)
                {
                    button.IsDisabled = true;
                    builder.WithButton(button);
                }

                await message.ModifyAsync(x =>
                {
                    x.Components = builder.Build();
                });
            }
        }
    }
}
