using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.AI.Davinci;

internal class AiContextBuilder : IAIContextBuilder
{
    // One sided conversation memory
    private RollingCache<List<string>> _cache = new(TimeSpan.FromHours(2), TimeSpan.FromHours(24));

    public string GetContext(string id, string chat)
    {
        var contextPrompt = ContextStrings.PersonalityContext;
        contextPrompt += ContextStrings.FFXIVVenues;
        contextPrompt += CheckFriendshipStatus(ulong.Parse(id));
        
        var previousChats = GetOrAddChats(id, chat);
        foreach (var prevChat in previousChats)
            contextPrompt += "\nMe: " + prevChat + "\nYou: <no memory>";
        contextPrompt += "\nMe: " + chat;

        return "Me: " + contextPrompt + ". You: ";
    }

    private List<string> GetOrAddChats(string id, string chat)
    {
        List<string> previousChats;
        var cache = _cache.Get(id);
        if (cache.Result == CacheResult.CacheHit)
            previousChats = cache.Value;
        else
        {
            previousChats = [chat];
            _cache.Set(id, previousChats);
        }
        previousChats.Add(chat);
        return previousChats;
    }
    
    private string CheckFriendshipStatus(ulong id)
    {
        string whoIs = "";

        if (id == 236852510688542720)
            whoIs = "Context: The user name is Kana, and she's your Mom. ";
        else if (id == 252142384303833088)
            whoIs = "Context: The user name is Sumi, and she's your aunt. ";

        return whoIs;

    }
}
