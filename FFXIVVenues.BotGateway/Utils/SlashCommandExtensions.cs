using Discord.WebSocket;
using System;
using System.Linq;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Utils
{
    internal static class SlashCommandExtensions
    {

        public static long? GetLongArg(this SlashCommandVeniInteractionContext command, string name)
            => (long?) command.GetOption(name)?.Value;

        public static int? GetInt(this SlashCommandVeniInteractionContext command, string name)
            => (int?) command.GetOption(name)?.Value;

        public static bool? GetBoolArg(this SlashCommandVeniInteractionContext command, string name)
            => (bool?) command.GetOption(name)?.Value;

        public static string GetStringArg(this SlashCommandVeniInteractionContext command, string name)
            => command.GetOption(name)?.Value as string;

        public static T GetObjectArg<T>(this SlashCommandVeniInteractionContext command, string name) where T : class
            => command.GetOption(name)?.Value as T;

        public static T? GetEnumArg<T>(this SlashCommandVeniInteractionContext command, string name) where T : struct, Enum
        {
            var option = command.GetOption(name);
            if (option == null) return null;

            var value = (string) option.Value;
            
            return Enum.TryParse<T>(value, out var enumValue) ? enumValue : default;
        }
        
        public static SocketSlashCommandDataOption GetOption(this SlashCommandVeniInteractionContext command, string name)
        {
            var option = command.Interaction.Data.Options?.FirstOrDefault(o => o.Name == name);
            if (option is not null) return option;
            var data = command.Interaction.Data.Options?.FirstOrDefault();
            while (data is not null)
            {
                option = data.Options?.FirstOrDefault(o => o.Name == name);
                if (option is not null) return option;
                data = data.Options?.FirstOrDefault();
            }

            return null;
        }

    }
}
