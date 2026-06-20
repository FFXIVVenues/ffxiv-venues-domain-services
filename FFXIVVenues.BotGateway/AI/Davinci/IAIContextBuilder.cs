namespace FFXIVVenues.BotGateway.AI.Davinci
{
    internal interface IAIContextBuilder
    {
        string GetContext(string id, string chat);
    }
}