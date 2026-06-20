namespace FFXIVVenues.Veni.AI.Clu.CluModels;

public class CluPrediction
{
    public string TopIntent { get; set; }
    public string ProjectKind { get; set; }
    public CluIntents[] Intents { get; set; }
    public CluEntities[] Entities { get; set; }
}