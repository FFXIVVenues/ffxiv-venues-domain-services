using System;

namespace FFXIVVenues.Veni.Utils
{
    static class MessageRepository
    {

        public static string[] StoppedMessage = new[]
        {
            "Okay! Cancelled.",
            "Oki, we'll stop. :slight_smile:",
            "Consider it dropped! :sweat_smile:",
        };

        public static string[] CreateVenueMessage = new[]
        {
            "Awesome! Great to see another venue join the community! ♥️",
            "Wooo! Congratulations on the new venue! ♥️",
            "Exciting! ♥️",
            "Great! ♥️",
            "Pog! ♥️"
        };

        public static string[] EditVenueMessage = new[]
        {
            "What would you like to change? 🥰",
            "What would you like to edit? 🙂"
        };

        public static string[] MentionOrReplyToMeMessage = new[]
        {
            "You're in a public channel, **@ Veni Ki** with your answers so she knows you're talking to her."
        };

        public static string[] AskForNameMessage = new[]
        {
            "Let's get started. What is the **name** of your venue?",
            "Great, what are we **naming** it?",
            "Great, what is the **name**? :smile:",
            "What's the **name** of your venue?",
            "Please, tell me the **name**!"
        };

        public static string[] AskForDataCenterMessage = new[]
        {
            "Which **data center** are you in?",
            "Which **data center** is your venue in?"
        };
        
        public static string[] AskForWorldMessage = new[]
        {
            "Which **world** are you in?",
            "Which **world** is your venue in?"
        };

        public static string[] AskForHousingDistrictMessage = new[]
        {
            "Which **housing district** are you in?",
            "Which **housing district** is your venue in?"
        };

        public static string[] AskForWardMessage = new[]
        {
            "Which **number ward** are you in? (type 1-30)",
            "Which **number ward** is your venue in? (type 1-30)",
            "Which **ward** are you in, cutie? (type 1-30)",
            "What's the ward, hun? (type 1-30)"
        };

        public static string[] AskForRoomMessage = new[]
        {
            "and the **room number**? (type 1-60)",
            "Which **number room** are you? (type 1-60)",
            "Which **number room** is your venue? (type 1-60)"
        };


        public static string[] AskForPlotMessage = new[]
        {
            "and the **plot number**? (type 1-60)",
            "Which **number plot** are you? (type 1-60)",
            "Which **number plot** is your venue? (type 1-60)"
        };

        public static string[] AskForSubdivisionMessage = new[]
        {
            "Are you in the **subdivision**?",
            "Is your venue in the **subdivision**?"
        };

        public static string[] AskForApartmentMessage = new[]
        {
            "and the **apartment number**?",
            "Which **number apartment** are you?",
            "Which **number apartment** is your venue?"
        };

        public static string[] AskForDescriptionMessage = new[]
        {
            "",
            "Could you write a **introduction/advertisement** of the venue? Players will see it in your card on the index. :)"
        };

        public static string[] WhatsOpenMessage = new[]
        {
            "Here's what's open right nyaaow! 😽",
            "Where should we go? 🙂",
        };

        public static string[] AskForSfwMessage = new[]
        {
            "Is it usually safe for someone looking for an **SFW** experience to walk into?",
        };

        public static string[] AskForCategories = new[]
        {
            "Oki! Select the **categories** that most suites your venue (maximum 2). 🙂",
            "Great! 🥰 Which of these **categories** apply to your venue most (maximum 2)? "
        };

        public static string[] AskForTags = new[]
        {
            "Select all the **tags** that apply to your venue. :smile:",
            "Which of these **tags** apply to the venue?"
        };

        public static string[] AskForHouseOrApartmentMessage = new[]
        {
            "Is your venue in a **house, room or apartment**?",
            "Is it a **house, room or apartment**?",
        };

        public static string[] AskForWebsiteMessage = new[]
        {
            "What is your **website address**?",
        };

        public static string[] AskForDiscordMessage = new[]
        {
            "What is your venue's permanent **discord invite** link?",
        };

        public static string[] ConfirmMessage = new[]
        {
            "Great!",
            "Alright, got it!",
            "Got it!",
            "Nice!",
            "Niicuuuu!",
            "Looks good!",
            "Thanks!",
            "Mhm, understood.",
            "Alrighty.",
        };

        public static string[] VenueOpenMessage = new[]
        {
            "Woo! The doors are open. You're glowing pink and announcements have been sent! Let's have fun today! ♥️",
            "Yay! You're glowing pink on the index. Let's have fun today! ♥️",
            "Let's do it! We... are... live!!! We're glowing pink on the index and the pings are flying! So excited. 🙂"
        };

        public static string[] VenueClosedMessage = new[]
        {
            "The doors are closed! I can't wait til next time. ♥️",
            "We're no longer pink! I'll close up. 🥰",
            "Okay! I'll lock up. 🙂"
        };

        public static string[] DontUnderstandResponses = new[]
        {
            "I don't understand. :smiling_face_with_tear:",
            "Huh? I don't get it.",
            "Hmm?",
            "Huh?",
            "Kweh!?!",
            "Kupo?"
        };

        public static string[] ShowVenueResponses = new[]
        {
            "Okay, here you go! 🥰",
            "Here you go, hun! 💓",
            "Here's what I've found! 💓"
        };

        public static string[] AskDaysOpenMessage = new[]
        {
            "What days are you open?",
            "What days is the venue open each week?"
        };

        public static string[] WelcomeMessages = new[]
        {
            "Welcome {mention}! Glad to have mew! 🥰",
            "Welcome {mention}, hope mew have a good time!",
            "Yay, {mention} joined! ♥",
            "{mention}! Hi hi! Glad to see you here. 😻",
            "Hey {mention} cutie! Good to see you. ;)",
            "Meow! {mention}! Welcome! 🥰",
            "Hey everyone! It's {mention}! ♥ Welcome welcome! 🥰"
        };

        public static string[] RolesAssigned = new[]
        {
            "I've assigned you your Venue Manager role(s). 🙂",
            "I've given you your Venue Manager role(s). 🙂",
            "Your Venue Manager role(s) are assigned. 🙂"
        };


        private static readonly Random _random = new ();
        public static string PickRandom(this string[] messageList)
        {
            if (messageList.Length == 0)
                return null;

            var index = _random.Next(messageList.Length);
            return messageList[index];
        }

    }
}
