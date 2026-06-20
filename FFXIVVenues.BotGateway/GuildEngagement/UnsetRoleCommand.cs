using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.GuildEngagement;

[DiscordCommand("server managerrole unset", "Stop a role being assigned to venue managers of the specified Data Center.", GuildPermission.ManageRoles, InteractionContextType.Guild)]
[DiscordCommandOption("datacenter", "The data center to stop assigning roles to.", ApplicationCommandOptionType.String)]
[DiscordCommandOptionChoice("datacenter", "Crystal", "Crystal")]
[DiscordCommandOptionChoice("datacenter", "Aether", "Aether")]
[DiscordCommandOptionChoice("datacenter", "Primal", "Primal")]
[DiscordCommandOptionChoice("datacenter", "Dynamis", "Dynamis")]
[DiscordCommandOptionChoice("datacenter", "Materia", "Materia")]
[DiscordCommandOptionChoice("datacenter", "Light", "Light")]
[DiscordCommandOptionChoice("datacenter", "Chaos", "Chaos")]
public class UnsetManageRoleCommand(IRepository repository) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        var guildId = slashCommand.Interaction.GuildId ?? 0;
        if (guildId == 0)
            return;

        var guildSettings = await repository.GetByIdAsync<GuildSettings>(guildId.ToString())
                            ?? new GuildSettings { GuildId = guildId };

        var dataCenter = slashCommand.GetStringArg("datacenter");
        guildSettings.DataCenterRoleMap.Remove(dataCenter);
        var upsertTask = repository.UpsertAsync(guildSettings);

        await slashCommand.Interaction.RespondAsync($"Oki! I'll stop giving roles {dataCenter} venue managers. 🙂", ephemeral: true);
        await upsertTask;
    }

}