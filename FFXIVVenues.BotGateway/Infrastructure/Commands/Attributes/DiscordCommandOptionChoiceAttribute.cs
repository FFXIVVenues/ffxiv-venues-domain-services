using System;

namespace FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DiscordCommandOptionChoiceAttribute : Attribute
{
    public string OptionName { get; }
    public string ChoiceName { get; }
    public object ChoiceValue { get; }

    public DiscordCommandOptionChoiceAttribute(string optionName, string choiceName, object choiceValue)
    {
        OptionName = optionName;
        ChoiceName = choiceName;
        ChoiceValue = choiceValue;
    }
}