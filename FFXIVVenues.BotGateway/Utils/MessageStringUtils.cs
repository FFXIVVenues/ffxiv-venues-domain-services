using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FFXIVVenues.BotGateway.Utils
{
    static class MessageStringUtils
    {

        public static string StripMentions(this string message)
        {
            var regex = new Regex(@"<@!?[0-9]+>");
            return regex.Replace(message, string.Empty).Trim();
        }

        public static string StripMentions(this string message, ulong userId)
        {
            var regex = new Regex(@$"<@!?{userId}>");
            return regex.Replace(message, string.Empty).Trim();
        }

        public static List<string> AsListOfParagraphs(this string message)
        {
            return message.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }

    }
}
