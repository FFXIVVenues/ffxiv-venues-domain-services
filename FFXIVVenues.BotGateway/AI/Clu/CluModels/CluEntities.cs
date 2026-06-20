namespace FFXIVVenues.Veni.AI.Clu.CluModels;

public class CluEntities
{
    public string Category { get; set; }
    public string Text { get; set; }
    public int Offset { get; set; }
    public int Length { get; set; }
    public int ConfidenceScore { get; set; }
    public CluInformation[] ExtraInformation { get; set; }
}