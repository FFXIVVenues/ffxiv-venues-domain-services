using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.Authorisation;

public enum Permission
{
    ApproveVenue,
        ApproveNaVenue,
        ApproveEuVenue,
        ApproveOceVenue,
        ApproveJpnVenue,
    AuditVenue,
        AuditNaVenue,
        AuditEuVenue,
        AuditOceVenue,
        AuditJpnVenue,
    ViewAuditHistory,
        ViewNaAuditHistory,
        ViewEuAuditHistory,
        ViewOceAuditHistory,
        ViewJpnAuditHistory,
    EditVenue,
        EditNaVenue,
        EditEuVenue,
        EditOceVenue,
        EditJpnVenue,
    EditPhotography,
        EditNaPhotography,
        EditEuPhotography,
        EditOcePhotography,
        EditJpnPhotography,
    EditManagers,
        EditNaManagers,
        EditEuManagers,
        EditOceManagers,
        EditJpnManagers,
    OpenVenue,
        OpenNaVenue,
        OpenEuVenue,
        OpenOceVenue,
        OpenJpnVenue,
    CloseVenue,
        CloseNaVenue,
        CloseEuVenue,
        CloseOceVenue,
        CloseJpnVenue,
    HiatusVenue,
        HiatusNaVenue,
        HiatusEuVenue,
        HiatusOceVenue,
        HiatusJpnVenue,
    DeleteVenue,
        DeleteNaVenue,
        DeleteEuVenue,
        DeleteOceVenue,
        DeleteJpnVenue,
    RespondToFlags,
        RespondToNaFlags,
        RespondToEuFlags,
        RespondToOceFlags,
        RespondToJpnFlags,
    Blacklist,
    ControlMassAudit,
    ReportMassAudit,
    SetLongSchedule,
    StreamEvents
}

public static class PermissionExtensions
{
    public static Permission? ToLocalPermission(this Permission permission, string dataCenter) =>
        permission switch
        {
            Permission.AuditVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.AuditNaVenue,  
                    FfxivWorlds.REGION_EU => Permission.AuditEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.AuditOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.AuditJpnVenue,
                    _ => null
                },
            Permission.ViewAuditHistory => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.ViewNaAuditHistory,  
                    FfxivWorlds.REGION_EU => Permission.ViewEuAuditHistory,  
                    FfxivWorlds.REGION_OCE => Permission.ViewOceAuditHistory,
                    FfxivWorlds.REGION_JPN => Permission.ViewJpnAuditHistory,
                    _ => null
                },
            Permission.ApproveVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.ApproveNaVenue,  
                    FfxivWorlds.REGION_EU => Permission.ApproveEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.ApproveOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.ApproveJpnVenue,
                    _ => null
                },
            Permission.EditManagers => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.EditNaManagers,  
                    FfxivWorlds.REGION_EU => Permission.EditEuManagers,  
                    FfxivWorlds.REGION_OCE => Permission.EditOceManagers,
                    FfxivWorlds.REGION_JPN => Permission.EditJpnManagers,
                    _ => null
                },
            Permission.EditVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.EditNaVenue,  
                    FfxivWorlds.REGION_EU => Permission.EditEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.EditOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.EditJpnVenue,
                    _ => null
                },
            Permission.OpenVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.OpenNaVenue,  
                    FfxivWorlds.REGION_EU => Permission.OpenEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.OpenOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.OpenJpnVenue,
                    _ => null
                },
            Permission.CloseVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA  => Permission.CloseNaVenue,  
                    FfxivWorlds.REGION_EU  => Permission.CloseEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.CloseOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.CloseJpnVenue,
                    _ => null
                },
            Permission.EditPhotography => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA  => Permission.EditNaPhotography,  
                    FfxivWorlds.REGION_EU  => Permission.EditEuPhotography,  
                    FfxivWorlds.REGION_OCE => Permission.EditOcePhotography,
                    FfxivWorlds.REGION_JPN => Permission.EditJpnPhotography,
                    _ => null
                },
            Permission.HiatusVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA  => Permission.HiatusNaVenue,  
                    FfxivWorlds.REGION_EU  => Permission.HiatusEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.HiatusOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.HiatusJpnVenue,
                    _ => null
                },
            Permission.DeleteVenue => 
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA  => Permission.DeleteNaVenue,  
                    FfxivWorlds.REGION_EU  => Permission.DeleteEuVenue,  
                    FfxivWorlds.REGION_OCE => Permission.DeleteOceVenue,
                    FfxivWorlds.REGION_JPN => Permission.DeleteJpnVenue,
                    _ => null
                },
            Permission.RespondToFlags =>
                FfxivWorlds.GetRegionForDataCenter(dataCenter) switch
                {
                    FfxivWorlds.REGION_NA => Permission.RespondToNaFlags,
                    FfxivWorlds.REGION_EU => Permission.RespondToEuFlags,
                    FfxivWorlds.REGION_OCE => Permission.RespondToOceFlags,
                    FfxivWorlds.REGION_JPN => Permission.RespondToJpnFlags,
                    _ => null
                },
            _ => null
        };
    
}