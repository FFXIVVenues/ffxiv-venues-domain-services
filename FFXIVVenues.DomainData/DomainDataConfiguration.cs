namespace FFXIVVenues.DomainData;

public class DomainDataConfiguration
{
    public required string ConnectionString { get; set; }
    public string? MediaUriTemplate { get; set; }
}