using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.ConversationalIntent;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ConversationalIntents;
using FFXIVVenues.Veni.VenueControl.VenueClosing.ConversationalIntent;
using FFXIVVenues.Veni.VenueControl.VenueOpening.ConversationalIntent;
using FFXIVVenues.Veni.VenueDiscovery.Intents;

namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    internal class IntentHandlerProvider : IIntentHandlerProvider
    {

        private readonly TypeMap<IIntentHandler> _intentMap;
        private readonly TypeMap<IIntentHandler> _interuptMap;

        public IntentHandlerProvider(IServiceProvider serviceProvider)
        {
            _intentMap = new TypeMap<IIntentHandler>(serviceProvider)
                .Add<CloseIntent>(IntentNames.Operation.Close)
                .Add<CreateIntent>(IntentNames.Operation.Create)
                .Add<EditIntentHandler>(IntentNames.Operation.Edit)
                .Add<OpenIntent>(IntentNames.Operation.Open)
                .Add<Show>(IntentNames.Operation.Show)
                .Add<ShowOpen>(IntentNames.Operation.ShowOpen)
                .Add<ShowForManager>(IntentNames.Operation.ShowForManager)
                .Add<Search>(IntentNames.Operation.Search)
                .Add<NoneIntent>(IntentNames.None);

            // these are currently not mapped in the CLU
            _interuptMap = new TypeMap<IIntentHandler>(serviceProvider)
                .Add<CancelIntent>(IntentNames.Interupt.Quit)
                .Add<EscalateIntent>(IntentNames.Interupt.Escalate)
                .Add<HelpIntent>(IntentNames.Interupt.Help);
        }

        public Task HandleIteruptIntent(string interupt, MessageVeniInteractionContext context) =>
            _interuptMap.Activate(interupt)?.Handle(context);

        public Task HandleIteruptIntent(string interupt, ComponentVeniInteractionContext context) =>
            _interuptMap.Activate(interupt)?.Handle(context);

        public Task HandleIteruptIntent(string interupt, SlashCommandVeniInteractionContext context) =>
            _interuptMap.Activate(interupt)?.Handle(context);

        public Task HandleIntent(string interupt, MessageVeniInteractionContext context) =>
           _intentMap.Activate(interupt)?.Handle(context) ?? new NoneIntent().Handle(context);

        public Task HandleIntent(string interupt, ComponentVeniInteractionContext context) =>
           _intentMap.Activate(interupt)?.Handle(context) ?? new NoneIntent().Handle(context);

        public Task HandleIntent(string interupt, SlashCommandVeniInteractionContext context) =>
            _intentMap.Activate(interupt)?.Handle(context) ?? new NoneIntent().Handle(context);

    }
}
