using System;
using Discord;

namespace FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DiscordCommandOptionAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public ApplicationCommandOptionType Type { get; }
    public bool Required { get; set; }

    public DiscordCommandOptionAttribute(string name, string description, ApplicationCommandOptionType type)
    {
        Name = name;
        Description = description;
        Type = type;
    }
    
}