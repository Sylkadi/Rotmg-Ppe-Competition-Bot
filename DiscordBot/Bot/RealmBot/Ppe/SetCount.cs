using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Discord;
using DiscordBot.Bot.RealmBot.Game;

namespace DiscordBot.Bot.RealmBot.Ppe
{
    public class Setcount
    {
        [JsonInclude]
        public int amount { get; set; }

        [JsonInclude]
        public string name { get; set; }

        [JsonIgnore]
        public Set referenceSet { get; set; }

        public int Increment(int amount) // There is a 100% cleaner way of doing this, but this works.
        {
            int netDifference = 0;

            for (int i = 0; i < amount; i++)
            {
                if (this.amount >= referenceSet.points.Length - 1)
                {
                    int last = amount - i;

                    netDifference += referenceSet.points[^1] * last;
                    this.amount += last;
                    break;
                }

                netDifference += referenceSet.points[this.amount];
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

        public int UpdateSetCount(Dictionary<string, ItemCount> fromDictionary)
        {
            int[] itemAmounts = new int[referenceSet.setItems.Length];
            for(int i = 0; i < referenceSet.setItems.Length; i++)
            {
                if (fromDictionary.TryGetValue(referenceSet.setItems[i].name, out ItemCount itemCount))
                {
                    itemAmounts[i] = itemCount.amount;
                }
            }
            Array.Sort(itemAmounts);

            if (itemAmounts[0] > amount)
            {
                return Increment(1);
            } else if(itemAmounts[0] < amount)
            {
                return Decrement(1);
            }

            return 0;
        }
    }
}
