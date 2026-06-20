using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVVenues.BotGateway.Utils;

public static class FfxivWorlds
{

    public const string REGION_NA = "North America"; 
    public const string REGION_OCE = "Oceania"; 
    public const string REGION_EU = "Europe"; 
    public const string REGION_JPN = "Japan"; 

    private static readonly Dictionary<string, bool> Regions = new()
    {
        { REGION_NA, true },
        { REGION_OCE, true },
        { REGION_EU, true },
        { REGION_JPN, false },
    };

    private static readonly Dictionary<string, string[]> DataCenterMap = new()
    {
        { REGION_NA, ["Aether", "Primal", "Crystal", "Dynamis"] },
        { REGION_OCE, ["Materia"] },
        { REGION_EU, ["Chaos", "Light"] },
        { REGION_JPN, ["Elemental", "Gaia", "Mana", "Meteor"] },
    };

    private static readonly Dictionary<string, string[]> WorldMap = new()
    {
        // North America            
        { "Aether", ["Adamantoise", "Cactuar", "Faerie", "Gilgamesh", "Jenova", "Midgardsormr", "Sargatanas", "Siren"] },
        { "Primal", ["Behemoth", "Excalibur", "Exodus", "Famfrit", "Hyperion", "Lamia", "Leviathan", "Ultros"] },
        { "Crystal", ["Balmung", "Brynhildr", "Coeurl", "Diabolos", "Goblin", "Malboro", "Mateus", "Zalera"] },
        { "Dynamis", ["Halicarnassus", "Maduin", "Marilith", "Seraph", "Cuchulainn", "Golem", "Kraken", "Rafflesia"] },

        // Europe
        { "Chaos", ["Cerberus", "Louisoix", "Moogle", "Omega", "Phantom", "Ragnarok", "Sagittarius", "Spriggan"] },
        { "Light", ["Alpha", "Lich", "Odin", "Phoenix", "Raiden", "Shiva", "Twintania", "Zodiark"] },
            
        // Oceanian
        { "Materia", ["Bismark", "Ravana", "Sephirot", "Sophia", "Zurvan"] },
        
        // Asia
        { "Elemental", [ "Aegis", "Atamos", "Carbuncle", "Garuda", "Gungnir", "Kujata",  "Tonberry" ]},
        { "Gaia", ["Alexander", "Bahamut", "Durandal", "Fenrir", "Ifrit", "Ridill", "Tiamat", "Ultima"] },
        { "Mana", ["Anima", "Asura", "Chocobo", "Hades", "Ixion",  "Masamune", "Pandaemonium",  "Titan"] },
        { "Meteor", ["Belias","Mandragora", "Ramuh", "Shinryu" , "Unicorn", "Valefor", "Yojimbo", "Zeromus"] }
    };

    public static string[] GetWorldsFor(params string[] dataCenters) =>
        dataCenters.SelectMany(dataCenter => WorldMap.ContainsKey(dataCenter) ? WorldMap[dataCenter] : Array.Empty<string>()).ToArray();

    public static string[] GetDataCentersFor(params string[] regions) =>
        regions.SelectMany(region => DataCenterMap.ContainsKey(region) ? DataCenterMap[region] : Array.Empty<string>()).ToArray();

    public static string[] GetSupportedRegions() =>
        Regions.Where(kv => kv.Value).Select(kv => kv.Key).ToArray();
    
    public static string GetRegionForDataCenter(string dataCenter) =>
        dataCenter == null ? null : DataCenterMap.FirstOrDefault(kv => kv.Value.Contains(dataCenter, StringComparer.InvariantCultureIgnoreCase)).Key;
    
}