using FFXIVVenues.ApiGateway.Security;

namespace FFXIVVenues.ApiGateway.Controllers.Auth.Models;

#pragma warning disable CS1591
public record WhatCanIDoDto(
    string Edit,
    string EditApproval,
    string EditManagers,
    string Delete,
    bool UnlimitedSchedules)
{
    public static WhatCanIDoDto FromCurrentUser(ICurrentUser user) =>
        new WhatCanIDoDto(user.Authorization.Edit, user.Authorization.EditApproval, user.Authorization.EditManagers,
            user.Authorization.EditManagers, user.Authorization.UnlimitedSchedules);
}