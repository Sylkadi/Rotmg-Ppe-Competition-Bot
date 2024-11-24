using System.Text.Json.Serialization;
using DiscordBot.Bot.RealmBot.Game;

namespace DiscordBot.Bot.RealmBot.Ppe
{
    public class ItemCount
    {
        [JsonInclude]
        public int amount { get; set; }

        [JsonInclude]
        public string name { get; set; }

        [JsonIgnore]
        public Item referenceItem { get; set; }

        public int Increment(int amount) // There is a 100% cleaner way of doing this, but this works.
        {
            int netDifference = 0;

            for (int i = 0; i < amount; i++)
            {
                if (this.amount >= referenceItem.points.Length - 1)
                {
                    int last = amount - i;

                    netDifference += referenceItem.points[^1] * last;
                    this.amount += last;
                    break;
                }

                netDifference += referenceItem.points[this.amount];
                this.amount++;
            }

            return netDifference;
        }

        public int Decrement(int amount)
        {
            amount = Math.Min(this.amount, amount);
            this.amount -= amount;

            int netDifference = -Increment(amount);

            this.amount -= amount;
            return netDifference;
        }

        public int Compute()
        {
            int amount = this.amount;
            this.amount = 0;
            return Increment(amount);
        }

    }
}
