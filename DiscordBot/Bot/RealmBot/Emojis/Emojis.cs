using Discord;

namespace DiscordBot.Bot.RealmBot.Emojis
{
    public class Emojis
    {
        public static List<BotEmote> emoteList = new List<BotEmote>()
        {
            new BotEmote("whitebag"),
            new BotEmote("orangebag"),
            new BotEmote("redbag"),
            new BotEmote("bluebag"),
            new BotEmote("cyanbag"),
            new BotEmote("goldenbag"),
            new BotEmote("pinkbag"),
            new BotEmote("purplebag"),
            new BotEmote("eggbasket"),
            new BotEmote("brownbag"),
            new BotEmote("exaltedblueprint")
        };

        public static Dictionary<string, BotEmote> emoteDictionary = new Dictionary<string, BotEmote>();

        public static async Task InitalizeAsync() // Automaticly upload emojis to bot
        {
            IReadOnlyCollection<Emote> emotes = await RealmBot.Instance.client.GetApplicationEmotesAsync();

            foreach (BotEmote emote in emoteList)
            {
                if (!emoteDictionary.TryAdd(emote.name, emote))
                {
                    Log.Warning($"Failed to add emote {emote.name} to dictionary as it already exists.");
                }

                foreach (Emote _emote in emotes)
                {
                    if (_emote.Name == emote.name)
                    {
                        emote.emote = _emote;
                        break;
                    }
                }

                if (emote.emote == null && File.Exists(emote.imagePath))
                {
                    Log.Info($"Uploading {emote.name} to application emojis");
                    await RealmBot.Instance.client.CreateApplicationEmoteAsync(emote.name, new Image(emote.imagePath));
                }
            }
            
        }

        public static string GetEmoteString(string emoteName)
        {
            if(emoteDictionary.TryGetValue(emoteName, out BotEmote emote))
            {
                return emote.ToString();
            }
            return "null";
        }
    }

    public class BotEmote
    {
        public string name;

        public string imagePath
        {
            get
            {
                return IO.emojiImageDirectory + $@"\{name}.png";
            }
        }

        public Emote emote;

        public override string ToString() => emote.ToString();

        public BotEmote(string name)
        {
            this.name = name;
        }
    }
}
