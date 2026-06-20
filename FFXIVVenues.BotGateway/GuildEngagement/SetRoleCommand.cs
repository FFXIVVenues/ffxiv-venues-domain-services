using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.GuildEngagement;

[DiscordCommand("server managerrole set", "Set role to assign to venue managers of the specified Data Center.", GuildPermission.ManageRoles, InteractionContextType.Guild)]
[DiscordCommandOption("datacenter", "The data center to assign the given role to.", ApplicationCommandOptionType.String)]
[DiscordCommandOptionChoice("datacenter", "Crystal", "Crystal")]
[DiscordCommandOptionChoice("datacenter", "Aether", "Aether")]
[DiscordCommandOptionChoice("datacenter", "Primal", "Primal")]
[DiscordCommandOptionChoice("datacenter", "Dynamis", "Dynamis")]
[DiscordCommandOptionChoice("datacenter", "Materia", "Materia")]
[DiscordCommandOptionChoice("datacenter", "Light", "Light")]
[DiscordCommandOptionChoice("datacenter", "Chaos", "Chaos")]
[DiscordCommandOption("role", "The role to assign the user when they own a venue in the specified data center.", ApplicationCommandOptionType.Role)]
public class SetManageRoleCommand(IRepository repository) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        var guildId = slashCommand.Interaction.GuildId ?? 0;
        if (guildId == 0)
            return;

        var guildSettings = await repository.GetByIdAsync<GuildSettings>(guildId.ToString())
                            ?? new GuildSettings { GuildId = guildId };

        var dataCenter = slashCommand.GetStringArg("datacenter");
        var role = slashCommand.GetObjectArg<SocketRole>("role");

        guildSettings.DataCenterRoleMap[dataCenter] = role.Id;
        var upsertTask = repository.UpsertAsync(guildSettings);

        await slashCommand.Interaction.RespondAsync($"Great! I'll give that role to all {dataCenter} venue managers. 🥰", ephemeral: true);
        await upsertTask;
    }

}