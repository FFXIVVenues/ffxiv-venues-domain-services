namespace FFXIVVenues.BotGateway.Infrastructure.Security;

public class Ed25519
{
    public string PrivateKeyPath { get; set; } = "config/ed25519/private.key";
    public string PublicKeyPath { get; set; } = "config/ed25519/public.key";
}
