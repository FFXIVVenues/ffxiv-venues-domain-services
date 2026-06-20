using Discord;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.VenueControl;

namespace FFXIVVenues.Veni.Utils
{
    internal static class ComponentBuilderExtensions
    {

        public static ComponentBuilder WithSkipButton<SkipTarget, ModifyTarget>(this ComponentBuilder builder, IVeniInteractionContext context) 
            where SkipTarget : ISessionState 
            where ModifyTarget : ISessionState
        {
            return builder.WithButton("►  Next/Skip", context.Session.RegisterComponentHandler(c =>
            {
                if (c.Session.InEditing())
                    return c.Session.MoveStateAsync<ModifyTarget>(c);
                return c.Session.MoveStateAsync<SkipTarget>(c);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

        public static ComponentBuilder WithBackButton(this ComponentBuilder builder, IVeniInteractionContext context, Func<Task<bool>> @override = null)
        {   
            return builder.WithButton("◄  Back", context.Session.RegisterComponentHandler(async c =>
            {
                var result = false;
                if (@override != null) result = await @override();
                else result = await c.Session.TryBackStateAsync(c);
                if (result) _ = c.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
                else await c.Interaction.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription("Cannot not go back any more!").Build());
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        }

    }
}
