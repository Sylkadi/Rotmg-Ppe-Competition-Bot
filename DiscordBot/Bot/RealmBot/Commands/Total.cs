using Discord;
using Discord.Rest;
using Discord.WebSocket;
using static DiscordBot.Bot.RealmBot.Emojis.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot.RealmBot.Commands
{
    internal class Total : Command
    {
        public static Dictionary<int, TotalViewer> totalViewers = new Dictionary<int, TotalViewer>();

        public Total()
        {
            Prefix = '>';
            Name = nameof(Total);
            Arguments = 2;
        }

        public override async Task ExecuteAsync(SocketMessage source, string[] args)
        {
            Ppe.Ppe target = null;
            SocketUser author = null;

            if (string.IsNullOrEmpty(args[1]))
            {
                target = Ppe.Ppe.GetOrCreatePpeFromList(source.Author.Id.ToString(), RealmBot.Instance.competition.ppes, RealmBot.Instance.competition.pointList);
            } else
            {
                SocketGuildUser user = GetUserFromName(((SocketGuildChannel)source.Channel).Guild, args[1]);

                author = user;
                target = Ppe.Ppe.GetOrCreatePpeFromList(user.Id.ToString(), RealmBot.Instance.competition.ppes, RealmBot.Instance.competition.pointList);
            }

            if(target == null)
            {
                return;
            }

            TotalViewer viewer = null;
            int id = Environment.TickCount;
            lock (totalViewers)
            {
                while (totalViewers.TryGetValue(id, out _))
                {
                    id++;
                }

                viewer = new TotalViewer(id, target, source.Author);
                totalViewers.Add(id, viewer);
            }
            if (viewer == null) return;

            ComponentBuilder builder = new ComponentBuilder();

            builder.WithButton("←", $"PPE_TOTAL_PREVIOUS|{id}", ButtonStyle.Secondary, disabled: target.previousPpe == null);
            builder.WithButton("→", $"PPE_TOTAL_NEXT|{id}", ButtonStyle.Secondary, disabled: target.nextPpe == null);

            RestUserMessage botMessage = await source.Channel.SendMessageAsync("", embed: GetEmbed(author ?? source.Author, target).Build(), components: builder.Build());

            viewer.restUserMessage = botMessage;
        }

        public static async Task NextPpe(int id, SocketMessageComponent component)
        {
            if(totalViewers.TryGetValue(id, out TotalViewer viewer))
            {
                await component.DeferAsync();
                await viewer.ViewNextAsync();
            }
        }

        public static async Task PreviousPpe(int id, SocketMessageComponent component)
        {
            if (totalViewers.TryGetValue(id, out TotalViewer viewer))
            {
                await component.DeferAsync();
                await viewer.ViewPreviousAsync();
            }
        }

        public static EmbedBuilder GetEmbed(SocketUser author, Ppe.Ppe ppe)
        {
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Author = new EmbedAuthorBuilder()
                {
                    Name = author.GlobalName ?? author.Username,
                    IconUrl = author.GetAvatarUrl() ?? author.GetDefaultAvatarUrl()
                }
            };

            builder.AddField(x =>
            {
                x.Name = "Total Points";
                x.Value = ppe.totalPoints;
                x.IsInline = true;
            });
            builder.AddField(x =>
            {
                x.Name = "Total bags";
                x.Value = 
                $"{GetEmoteString("whitebag")} {ppe.bagCount.whiteBagCount} " +
                $"{GetEmoteString("orangebag")} {ppe.bagCount.orangeBagCount} " +
                $"{GetEmoteString("redbag")} {ppe.bagCount.redBagCount} " +
                $"{GetEmoteString("exaltedblueprint")} {ppe.bagCount.blueBagCount}";
                x.IsInline = true;
            });

            return builder;
        }

        public class TotalViewer
        {
            private ButtonBuilder previousButton, nextButton;

            public int id { get; set; }

            public int timeoutTick { get; set; }

            public Ppe.Ppe currentPpe { get; set; }

            public RestUserMessage restUserMessage { get; set; }

            public SocketUser author { get; set; }

            public TotalViewer(int id, Ppe.Ppe currentPpe, SocketUser author)
            {
                this.id = id;
                this.currentPpe = currentPpe;
                this.author = author;
                timeoutTick = Environment.TickCount + 300000;

                previousButton = new ButtonBuilder("←", $"PPE_TOTAL_PREVIOUS|{id}", ButtonStyle.Secondary);
                nextButton = new ButtonBuilder("→", $"PPE_TOTAL_NEXT|{id}", ButtonStyle.Secondary);
            }

            public async Task ViewPreviousAsync()
            {
                if(currentPpe.previousPpe != null)
                {
                    currentPpe = currentPpe.previousPpe;
                }
                await UpdateMessageAsync();
            }

            public async Task ViewNextAsync()
            {
                if(currentPpe.nextPpe != null)
                {
                    currentPpe = currentPpe.nextPpe;
                }
                await UpdateMessageAsync();
            }

            public async Task UpdateMessageAsync()
            {
                previousButton.IsDisabled = currentPpe.previousPpe == null;
                nextButton.IsDisabled = currentPpe.nextPpe == null;

                await restUserMessage.ModifyAsync(x =>
                {
                    x.Components = new ComponentBuilder().WithButton(previousButton).WithButton(nextButton).Build();
                    x.Embed = GetEmbed(author, currentPpe).Build();
                });
            }

            public async Task AttemptExpireAsync()
            {
                if (Environment.TickCount < timeoutTick) return;

                lock (totalViewers)
                {
                    totalViewers.Remove(id);
                }

                previousButton.IsDisabled = true;
                nextButton.IsDisabled = true;

                await restUserMessage.ModifyAsync(x => x.Components = new ComponentBuilder().WithButton(previousButton).WithButton(nextButton).Build());
            }
        }
    }
}
