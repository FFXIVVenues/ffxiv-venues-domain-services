using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;

internal class GetUnapprovedCommand
{
    public const string COMMAND_NAME = "getunapproved";

    internal class CommandFactory : ICommandFactory
    {
        public SlashCommandProperties GetSlashCommand()
        {
            return new SlashCommandBuilder()
                .WithName(COMMAND_NAME)
                .WithDescription("Get venues that are currently not approved and therefore hidden from the index.")
                .Build();
        }
    }

    internal class CommandHandler(
        IApiService apiService,
        IVenueRenderer venueRenderer,
        IVenueApprovalService venueApprovalService,
        IAuthorizer authorizer)
        : ICommandHandler
    {
        private List<Venue> _venues;

        public async Task HandleAsync(SlashCommandVeniInteractionContext c) 
        {
            var asker = c.Interaction.User.Id;
            var dataCenters = new List<string>();
            if (authorizer.Authorize(asker, Permission.ApproveVenue).Authorized)
            {
                dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_NA));
                dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_EU));
                dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_OCE));
                dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_JPN));
            }
            else 
            {
                if (authorizer.Authorize(asker, Permission.ApproveNaVenue).Authorized)
                    dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_NA));
                if (authorizer.Authorize(asker, Permission.ApproveEuVenue).Authorized)
                    dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_EU));
                if (authorizer.Authorize(asker, Permission.ApproveOceVenue).Authorized)
                    dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_OCE));
                if (authorizer.Authorize(asker, Permission.ApproveJpnVenue).Authorized)
                    dataCenters.AddRange(FfxivWorlds.GetDataCentersFor(FfxivWorlds.REGION_JPN));
            }
            if (!dataCenters.Any())
            {
                await c.Interaction.FollowupAsync("Sorry, you're not authorized to see unapproved venues. 😢");
                return;
            }
                
            this._venues = (await apiService.GetUnapprovedVenuesAsync())
                                            .Where(v => v.Location == null || dataCenters.Contains(v.Location.DataCenter))
                                            .Take(25)
                                            .ToList();

            if (this._venues is not { Count: > 0 })
            {
                await c.Interaction.RespondAsync("I don't have any venues needing approval. 🙂");
                return;
            }

            var selectMenuKey = c.Session.RegisterComponentHandler(this.HandleVenueSelection, ComponentPersistence.ClearRow);
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };

            foreach (var venue in this._venues)
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Name,
                    Description = venue.Location.ToString(),
                    Value = venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);

            await c.Interaction.RespondAsync("Here you go! 🥰", components: componentBuilder.Build());
        }

        private async Task HandleVenueSelection(ComponentVeniInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var asker = c.Interaction.User.Id;
            var venue = this._venues.FirstOrDefault(v => v.Id == selectedVenueId);

            var auth = authorizer.Authorize(asker, Permission.ApproveVenue, venue);
            if (!auth.Authorized)
            {
                await c.Interaction.FollowupAsync("Sorry, you're not authorized to see unapproved venues. 😢");
                return;
            }

            await c.Interaction.FollowupAsync(embed: (await venueRenderer.ValidateAndRenderAsync(venue)).Build(),
                components: new ComponentBuilder()
                    .WithButton("Approve", c.Session.RegisterComponentHandler(async cm =>
                    {
                        await venueApprovalService.ApproveVenueAsync(venue, cm.Interaction.User.Id);
                        await cm.Interaction.Channel.SendMessageAsync("Nyya! I've approved the venue! 💝");
                    }, ComponentPersistence.ClearRow), ButtonStyle.Success)
                    .WithButton("Edit", c.Session.RegisterComponentHandler(cm =>
                    {
                        c.Session.SetVenue(venue);
                        return cm.Session.MoveStateAsync<EditVenueSessionState>(cm);
                    }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                    {
                        c.Session.SetVenue(venue);
                        return cm.Session.MoveStateAsync<DeleteVenueSessionState>(cm);
                    }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                    .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm => Task.CompletedTask,
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build());
        }

    }
}