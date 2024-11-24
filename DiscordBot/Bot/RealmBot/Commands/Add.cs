using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Bot.RealmBot.Game;
using DiscordBot.Bot.RealmBot.Ppe;
using System.ComponentModel;
using System.Diagnostics;

namespace DiscordBot.Bot.RealmBot.Commands
{
    internal class Add : Command
    {
        private static ComponentBuilder disabledButtons = new ComponentBuilder().
            WithButton("+", "ITEM_INCREMENT", ButtonStyle.Success, disabled: true).
            WithButton("-", "ITEM_DECREMENT", ButtonStyle.Danger, disabled: true);

        public Add()
        {
            Prefix = '>';
            Name = nameof(Add);
            Arguments = 2;
        }

        public override async Task ExecuteAsync(SocketMessage source, string[] args)
        {
            if (string.IsNullOrEmpty(args[1])) return;
            Ppe.Ppe currentPpe = Ppe.Ppe.GetOrCreatePpeFromList(source.Author.Id.ToString(), RealmBot.Instance.competition.ppes, RealmBot.Instance.competition.pointList);

            if (currentPpe == null) return;

            string args1ToUpper = args[1].ToUpper();
            if (RealmBot.Instance.competition.pointList.upperCaseToNormalDictionary.TryGetValue(args1ToUpper, out string name) && currentPpe.itemDictionary.TryGetValue(name, out ItemCount itemCount))
            {
                await SendMessageAsync(source, itemCount);
            } else
            {
                List<NameMatchItem> nameMatchItems = new List<NameMatchItem>();

                foreach(ItemCount _itemCount in currentPpe.itemsCounts)
                {
                    nameMatchItems.Add(new NameMatchItem(_itemCount.name, Util.StringMatchDistance(args1ToUpper, _itemCount.name.ToUpper()), _itemCount));
                    foreach(string nickName in _itemCount.referenceItem.nickNames)
                    {
                        nameMatchItems.Add(new NameMatchItem(_itemCount.name, Util.StringMatchDistance(args1ToUpper, nickName.ToUpper()), _itemCount));
                    }
                }

                nameMatchItems.Sort();

                await SendMessageAsync(source, nameMatchItems[0].itemCount);
            }
        }

        public async Task SendMessageAsync(SocketMessage source, ItemCount item)
        {
            ComponentBuilder builder = new ComponentBuilder();

            ButtonBuilder increment = new ButtonBuilder("+", $"ITEM_INCREMENT|{source.Author.Id}|{item.name}", ButtonStyle.Success);
            ButtonBuilder decrement = new ButtonBuilder("-", $"ITEM_DECREMENT|{source.Author.Id}|{item.name}", ButtonStyle.Danger);

            builder.WithButton(increment);
            builder.WithButton(decrement);

            FileAttachment imageAttachment = new FileAttachment(item.referenceItem.imagePath, item.referenceItem.imageNameNoSpace + ".png");
            RestUserMessage botButtonMessage = await source.Channel.SendFileAsync(imageAttachment, $"Are you sure you want to increment/decrement `{item.name}`", components: builder.Build());

            lock(RealmBot.Instance.botButtonMessages)
            {
                RealmBot.Instance.botButtonMessages.Add(new RealmBot.BotButtonMessage(botButtonMessage, [increment, decrement]));
            }
        }

        public static async Task ItemIncrementAsync(Ppe.Ppe ppe, string itemName, SocketMessageComponent component)
        {
            ItemCount itemCount = null;
            if (!ppe.itemDictionary.TryGetValue(itemName, out itemCount)) return;

            ppe.bagCount.Add(itemCount.referenceItem.bagType, 1);
            int itemNet = itemCount.Increment(1);
            int setNet = 0;

            Setcount setCount = null;
            if(itemCount.referenceItem.relatedSet != null && ppe.setDictionary.TryGetValue(itemCount.referenceItem.relatedSet.name, out setCount))
            {
                setNet += setCount.UpdateSetCount(ppe.itemDictionary);
            }

            int oldPoints = ppe.totalPoints;
            ppe.totalPoints += itemNet + setNet;
            ppe.Serialize();

            EmbedBuilder builder = GetEmbed(component.User, itemCount, setCount, itemNet, setNet, oldPoints, ppe.totalPoints);

            await component.DeferAsync();
            await component.ModifyOriginalResponseAsync(x => x.Components = disabledButtons.Build());

            FileAttachment imageAttachment = new FileAttachment(itemCount.referenceItem.imagePath, itemCount.referenceItem.imageNameNoSpace + ".png");
            await component.Channel.SendFileAsync(imageAttachment, text: "", embed: builder.Build());
        }

        public static async Task ItemDecrementAsync(Ppe.Ppe ppe, string itemName, SocketMessageComponent component)
        {
            ItemCount itemCount = null;
            if (!ppe.itemDictionary.TryGetValue(itemName, out itemCount)) return;

            ppe.bagCount.Add(itemCount.referenceItem.bagType, -1);
            int itemNet = itemCount.Decrement(1);
            int setNet = 0;

            Setcount setCount = null;
            if (itemCount.referenceItem.relatedSet != null && ppe.setDictionary.TryGetValue(itemCount.referenceItem.relatedSet.name, out setCount))
            {
                setNet += setCount.UpdateSetCount(ppe.itemDictionary);
            }

            int oldPoints = ppe.totalPoints;
            ppe.totalPoints += itemNet + setNet;
            ppe.Serialize();

            EmbedBuilder builder = GetEmbed(component.User, itemCount, setCount, itemNet, setNet, oldPoints, ppe.totalPoints);
            builder.Color = Color.Red;

            await component.DeferAsync();
            await component.ModifyOriginalResponseAsync(x => x.Components = disabledButtons.Build());

            FileAttachment imageAttachment = new FileAttachment(itemCount.referenceItem.imagePath, itemCount.referenceItem.imageNameNoSpace + ".png");
            await component.Channel.SendFileAsync(itemCount.referenceItem.imagePath, text: "", embed: builder.Build());
        }

        public static async Task ItemExitAsync(SocketMessageComponent component)
        {
            await component.DeferAsync();
            await component.ModifyOriginalResponseAsync(x => x.Components = disabledButtons.Build());
        }

        public static EmbedBuilder GetEmbed(SocketUser author, ItemCount item, Setcount setCount, int itemNet, int setNet, int oldTotalPoints, int newTotalPoints)
        {
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = Color.Green,
                Author = new EmbedAuthorBuilder()
                {
                    Name = author.GlobalName ?? author.Username,
                    IconUrl = author.GetAvatarUrl() ?? author.GetDefaultAvatarUrl(),
                },
                ThumbnailUrl = $"attachment://{item.referenceItem.imageNameNoSpace}.png"
            };

            if(setNet != 0)
            {
                builder.AddField(x =>
                {
                    x.Name = setCount.name;
                    x.Value = setCount.amount;
                    x.IsInline = true;
                });
                builder.AddField(x =>
                {
                    x.Name = "Net";
                    x.Value = setNet;
                    x.IsInline = true;
                });
                builder.AddField(x =>
                {
                    x.Name = "\u200b";
                    x.Value = "\u200b";
                    x.IsInline = true;
                });
            }

            builder.AddField(x =>
            {
                x.Name = item.name;
                x.Value = item.amount;
                x.IsInline = true;
            });
            builder.AddField(x =>
            {
                x.Name = "Net";
                x.Value = itemNet;
                x.IsInline = true;
            });
            builder.AddField(x =>
            {
                x.Name = "\u200b";
                x.Value = "\u200b";
                x.IsInline = true;
            });

            builder.AddField(x =>
            {
                x.Name = "Old Total";
                x.Value = oldTotalPoints;
                x.IsInline = true;
            });
            builder.AddField(x =>
            {
                x.Name = "New Total";
                x.Value = newTotalPoints;
                x.IsInline = true;
            });
            builder.AddField(x =>
            {
                x.Name = "\u200b";
                x.Value = "\u200b";
                x.IsInline = true;
            });

            return builder;
        }

        private struct NameMatchItem : IEquatable<NameMatchItem>, IComparable<NameMatchItem>
        {
            public string name;

            public float match;

            public ItemCount itemCount;

            public NameMatchItem(string name, float match, ItemCount itemCount)
            {
                this.name = name;
                this.match = match;
                this.itemCount = itemCount;
            }

            public int CompareTo(NameMatchItem other)
            {
                return match.CompareTo(other.match);
            }

            public bool Equals(NameMatchItem other)
            {
                return name == other.name;
            }
        }
    }
}
