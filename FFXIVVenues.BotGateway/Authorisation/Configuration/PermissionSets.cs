using FFXIVVenues.BotGateway.Authorisation;
using System;

namespace FFXIVVenues.BotGateway.Authorisation.Configuration;

public class PermissionSet
{
    public string Name { get; set; }
    public Permission[] Permissions { get; set; } = Array.Empty<Permission>();
    public ulong[] Members { get; set; } = Array.Empty<ulong>();
}