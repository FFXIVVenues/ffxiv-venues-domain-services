using Discord;

namespace FFXIVVenues.BotGateway.Utils.Broadcasting;

public record BroadcastMessage(ulong UserId, IUserMessage Message, MessageStatus Status, string Log);