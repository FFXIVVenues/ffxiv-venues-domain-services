using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using ScottBrady.IdentityModel.Crypto;
using ScottBrady.IdentityModel.Tokens;
using System.Text;

namespace FFXIVVenues.DomainSecurity;

public class Signer(SecurityKeyLoader signingKeyLoader)
{
    private readonly EdDsaSecurityKey _key = signingKeyLoader.LoadEdDsaKey();

    public string Sign(string payload)
    {
        var signature = this._key.EdDsa.Sign(Encoding.UTF8.GetBytes(payload));
        return Base64UrlEncoder.Encode(signature);
    }

    public bool Verify(string payload, string signature)
    {
        var signatureBytes = Base64UrlEncoder.DecodeBytes(signature);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        return this._key.EdDsa.Verify(payloadBytes, signatureBytes);
    }

    public SigningCredentials GetSigningCredentials() =>
        new SigningCredentials(this._key, ExtendedSecurityAlgorithms.EdDsa);

}
