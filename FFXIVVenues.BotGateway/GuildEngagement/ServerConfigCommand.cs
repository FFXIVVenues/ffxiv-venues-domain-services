using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.GuildEngagement;

[DiscordCommand("server viewconfig", "See all configuration set for this discord server.", GuildPermission.ManageRoles, InteractionContextType.Guild)]
public class ServerConfigCommand(IRepository repository) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        var guildId = slashCommand.Interaction.GuildId ?? 0;
        if (guildId == 0)
            return;

        var guildSettings = await repository.GetByIdAsync<GuildSettings>(guildId.ToString())
                            ?? new GuildSettings { GuildId = guildId };

        var text = new StringBuilder();
        text.AppendLine($"**Server Id**: {guildId}");
        text.AppendLine();

        text.AppendLine($"**Will format venue manager's names**: {(guildSettings.FormatNames ? "Yes" : "No")}");
        text.AppendLine($"**Will welcome new joiners**: {(guildSettings.WelcomeJoiners ? "Yes" : "No")}");

        text.AppendLine();
        text.AppendLine($"**Role assignment**:");
        if (!guildSettings.DataCenterRoleMap.Any())
            text.AppendLine($"Role assignment not enabled for any venue managers.");
        else foreach (var dataCenter in guildSettings.DataCenterRoleMap)
            text.AppendLine($"{dataCenter.Key} venue managers will be assigned {MentionUtils.MentionRole(dataCenter.Value)}");

        await slashCommand.Interaction.RespondAsync("Okies! Here's what I'll do for you. 🥰", embed: new EmbedBuilder().WithDescription(text.ToString()).Build(), ephemeral: true);
    }

}