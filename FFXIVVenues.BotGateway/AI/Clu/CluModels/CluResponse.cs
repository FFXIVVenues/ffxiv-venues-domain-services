using System.Text.Json.Serialization;

namespace FFXIVVenues.Veni.AI.Clu.CluModels;

public class CluResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; }
    public CluResult Result { get; set; }
}