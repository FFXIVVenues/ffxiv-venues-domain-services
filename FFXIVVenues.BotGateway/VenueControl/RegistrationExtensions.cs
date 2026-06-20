using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.Command;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.Commands;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.EditPropertyHandlers;
using FFXIVVenues.Veni.VenueControl.VenueClosing.Commands;
using FFXIVVenues.Veni.VenueControl.VenueClosing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl.VenueOpening.Command;
using FFXIVVenues.Veni.VenueControl.VenueOpening.ComponentHandlers;

namespace FFXIVVenues.Veni.VenueControl;

public static class RegistrationExtensions
{

    public static T AddVenueControlCommands<T>(this T commandBroker) where T : ICommandBroker
    {
        if (commandBroker == null)
            return default;
        
        commandBroker.Add<CreateCommand.Factory, CreateCommand.Handler>(CreateCommand.COMMAND_NAME, isMasterGuildCommand: false);
        commandBroker.Add<EditCommand.Factory, EditCommand.Handler>(EditCommand.COMMAND_NAME, isMasterGuildCommand: false);
        commandBroker.Add<CloseCommand.Factory, CloseCommand.Handler>(CloseCommand.COMMAND_NAME, isMasterGuildCommand: false);
        commandBroker.Add<OpenCommand.Factory, OpenCommand.Handler>(OpenCommand.COMMAND_NAME, isMasterGuildCommand: false);

        return commandBroker;
    }
    
    public static T AddVenueControlHandlers<T>(this T componentBroker) where T : IComponentBroker
    {
        if (componentBroker == null)
            return default;

        componentBroker.Add<DeleteHandler>(DeleteHandler.Key);
        componentBroker.Add<CloseHandler>(CloseHandler.Key);
        componentBroker.Add<OpenHandler>(OpenHandler.Key);
        
        componentBroker.Add<EditHandler>(EditHandler.Key);
        componentBroker.Add<SelectVenueToEditHandler>(SelectVenueToEditHandler.Key);
        
        componentBroker.Add<EditDescriptionHandler>(EditDescriptionHandler.Key);
        componentBroker.Add<EditDiscordHandler>(EditDiscordHandler.Key);
        componentBroker.Add<EditLocationHandler>(EditLocationHandler.Key);
        componentBroker.Add<EditScheduleHandler>(EditScheduleHandler.Key);
        componentBroker.Add<EditManagersHandler>(EditManagersHandler.Key);
        componentBroker.Add<EditNameHandler>(EditNameHandler.Key);
        componentBroker.Add<EditNsfwHandler>(EditNsfwHandler.Key);
        componentBroker.Add<EditPhotoHandler>(EditPhotoHandler.Key);
        componentBroker.Add<EditTagsHandler>(EditTagsHandler.Key);
        componentBroker.Add<EditWebsiteHandler>(EditWebsiteHandler.Key);
        
        return componentBroker;
    }
}