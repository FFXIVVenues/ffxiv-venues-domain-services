using System;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Authorisation.Configuration;

public class AuthorisationConfiguration
{
    public ulong[] Master { get; set; } = Array.Empty<ulong>();
    public PermissionSet[] PermissionSets { get; set; } = Array.Empty<PermissionSet>();
    public Permission[] ManagerPermissions { get; set; } = Array.Empty<Permission>();
}