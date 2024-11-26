
namespace DiscordBot.Bot.RealmBot
{
    public class IO
    {
        public static string generalDirectory;

        public static string ppeDirectory;

        public static string gameDirectory;

        public static string imagesDirectory;

        public static string emojiImageDirectory;

        public static void Iniatlize()
        {
            generalDirectory = Environment.CurrentDirectory + @"\RealmBot";
            Directory.CreateDirectory(generalDirectory);

            ppeDirectory = generalDirectory + @"\Ppe";
            Directory.CreateDirectory(ppeDirectory);

            gameDirectory = generalDirectory + @"\Game";
            Directory.CreateDirectory(gameDirectory);

            imagesDirectory = generalDirectory + @"\Images";
            Directory.CreateDirectory(imagesDirectory);

            emojiImageDirectory = generalDirectory + @"\Emotes";
            Directory.CreateDirectory(emojiImageDirectory);
        }
    }
}
