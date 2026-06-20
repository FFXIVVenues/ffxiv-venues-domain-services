namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    static class IntentNames
    {

        public static string None = "None";

        public static class Interupt
        {
            public static string Quit = "Interupt.Quit";
            public static string Escalate = "Interupt.Escalate";
            public static string Help = "Interupt.Help";
        }

        public static class Operation
        {
            public static string Close = "Operation.Close";
            public static string Create = "Operation.Create";
            public static string Edit = "Operation.Edit";
            public static string Show = "Operation.Show";
            public static string ShowOpen = "Operation.ShowOpen";
            public static string ShowForManager = "Operation.ShowForManager";
            public static string Delete = "Operation.Delete";
            public static string Open = "Operation.Open";
            public static string Search = "Operation.Search";
        }

    }
}
