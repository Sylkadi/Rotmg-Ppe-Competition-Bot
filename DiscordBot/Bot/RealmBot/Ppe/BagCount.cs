using DiscordBot.Bot.RealmBot.Game;

namespace DiscordBot.Bot.RealmBot.Ppe
{
    public class Bagcount
    {
        public int cyanBagCount { get; set; }

        public int blueBagCount { get; set; }

        public int redBagCount { get; set; }

        public int goldenBagCount { get; set; }

        public int orangeBagCount { get; set; }

        public int whiteBagCount { get; set; }

        public void Add(Item.BagType bagType, int value)
        {
            switch(bagType)
            {
                case Item.BagType.Cyan:
                    cyanBagCount += value;
                    break;
                case Item.BagType.Blue:
                    blueBagCount += value;
                    break;
                case Item.BagType.Red:
                    redBagCount += value;
                    break;
                case Item.BagType.Golden:
                    goldenBagCount += value;
                    break;
                case Item.BagType.Orange:
                    orangeBagCount += value;
                    break;
                case Item.BagType.White:
                    whiteBagCount += value;
                    break;
            }
        }
    }
}
