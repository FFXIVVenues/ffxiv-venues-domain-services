using System;

namespace FFXIVVenues.Veni.Authorisation.Configuration;

public class PermissionSet
{
    public string Name { get; set; }
    public Permission[] Permissions { get; set; } = Array.Empty<Permission>();
    public ulong[] Members { get; set; } = Array.Empty<ulong>();
}