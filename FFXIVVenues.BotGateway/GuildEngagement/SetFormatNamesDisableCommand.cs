using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.GuildEngagement;

[DiscordCommand("server formatnames disable", "Stop Veni formatting the display names of Venue Managers in this discord server.", GuildPermission.ManageRoles, InteractionContextType.Guild)]
public class SetFormatNamesDisableCommand(IRepository repository) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        var guildId = slashCommand.Interaction.GuildId ?? 0;
        if (guildId == 0)
            return;

        var guildSettings = await repository.GetByIdAsync<GuildSettings>(guildId.ToString())
                            ?? new GuildSettings { GuildId = guildId };

        guildSettings.FormatNames = false;
        var upsertTask = repository.UpsertAsync(guildSettings);

        await slashCommand.Interaction.RespondAsync($"Okies! I'll stop setting their names. 🙂", ephemeral: true);
        await upsertTask;
    }

}