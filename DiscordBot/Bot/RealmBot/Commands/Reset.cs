using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace DiscordBot.Bot.RealmBot.Commands
{
    public class Reset : Command
    {
        private static ComponentBuilder disabledButtons = new ComponentBuilder().
            WithButton("✓", "PPE_RESET_CONFIRM", ButtonStyle.Success, disabled: true).
            WithButton("X", "PPE_RESET_DECLINE", ButtonStyle.Danger, disabled: true);

        public Reset()
        {
            Prefix = '>';
            Name = nameof(Reset);
            Arguments = 2;
        }

        public override async Task ExecuteAsync(SocketMessage source, string[] args)
        {
            Ppe.Ppe target = null;

            if (!string.IsNullOrEmpty(args[1]) && source.Author.Id.ToString() == RealmBot.Instance.adminID)
            {
                if (RealmBot.Instance.competition.ppes.Exists(x => x.userID == args[1]))
                {
                    target = Ppe.Ppe.GetOrCreatePpeFromList(args[1], RealmBot.Instance.competition.ppes, RealmBot.Instance.competition.pointList);
                }
            }
            else
            {
                target = Ppe.Ppe.GetOrCreatePpeFromList(source.Author.Id.ToString(), RealmBot.Instance.competition.ppes, RealmBot.Instance.competition.pointList);
            }

            if (target == null) return;

            ComponentBuilder builder = new ComponentBuilder();

            ButtonBuilder confirm = new ButtonBuilder("✓", $"PPE_RESET_CONFIRM|{target.userID}", ButtonStyle.Success);
            ButtonBuilder decline = new ButtonBuilder("X", $"PPE_RESET_DECLINE|{target.userID}", ButtonStyle.Danger);

            builder.WithButton(confirm);
            builder.WithButton(decline);

            RestUserMessage botButtonMessage = await source.Channel.SendMessageAsync("Are you sure you want to reset your ppe?", components: builder.Build());

            lock(RealmBot.Instance.botButtonMessages)
            {
                RealmBot.Instance.botButtonMessages.Add(new RealmBot.BotButtonMessage(botButtonMessage, [confirm, decline]));
            }
        }

        public static async Task ResetPpeAsync(Ppe.Ppe ppe, SocketMessageComponent component)
        {
            Ppe.Ppe newPpe = new Ppe.Ppe();

            newPpe.userID = ppe.userID;
            newPpe.SetAsTemplate(RealmBot.Instance.competition.pointList);
            newPpe.CreateFile(ppe.userID);
            newPpe.previousPpe = ppe;

            newPpe.Serialize();
            ppe.Deserialize();

            ppe.SetNextPpes();

            await component.DeferAsync();
            await component.ModifyOriginalResponseAsync(x => x.Components = disabledButtons.Build());
            await component.Channel.SendMessageAsync($"Ppe of total points `{ppe.previousPpe.totalPoints}` has been reset.");
        }

        public static async Task DeclineResetPpeAsync(SocketMessageComponent component)
        {
            await component.DeferAsync();
            await component.ModifyOriginalResponseAsync(x => x.Components = disabledButtons.Build());
        }

    }
}
