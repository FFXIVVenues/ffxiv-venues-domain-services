using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using ScottBrady.IdentityModel.Crypto;
using ScottBrady.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FFXIVVenues.DomainSecurity;

public class SecurityKeyLoader(SecurityOptions options)
{
    // Also available from id.ffxivvenues.com
    public const string EdDsaKeyId = "ffxivvenues-ed25519-key";
    private EdDsaSecurityKey? _edDsaSecurityKey;

    public EdDsaSecurityKey LoadEdDsaKey()
    {
        if (_edDsaSecurityKey is not null)
            return _edDsaSecurityKey;

        var privatePem = File.ReadAllText(options.SigningPrivateKeyPath);
        var publicPem = File.ReadAllText(options.SigningPublicKeyPath);
        var d = LoadKey(privatePem, true);
        var x = LoadKey(publicPem, false);
        var edDsa = EdDsa.Create(new EdDsaParameters(ExtendedSecurityAlgorithms.Curves.Ed25519) { D = d, X = x });
        return _edDsaSecurityKey = new EdDsaSecurityKey(edDsa) { KeyId = EdDsaKeyId };
    }

    private byte[] LoadKey(string pem, bool wantPrivate)
    {
        using var reader = new StringReader(pem);
        var pemObject = new PemReader(reader).ReadObject()
            ?? throw new InvalidOperationException("Signing key PEM could not be parsed.");

        var keyParameter = pemObject switch
        {
            AsymmetricCipherKeyPair pair => wantPrivate ? pair.Private : pair.Public,
            AsymmetricKeyParameter parameter => parameter,
            _ => throw new InvalidOperationException($"Unexpected PEM contents: {pemObject.GetType().Name}.")
        };

        return keyParameter switch
        {
            Ed25519PrivateKeyParameters priv when wantPrivate => priv.GetEncoded(),
            Ed25519PublicKeyParameters pub when !wantPrivate => pub.GetEncoded(),
            _ => throw new InvalidOperationException(
                $"Expected an Ed25519 {(wantPrivate ? "private" : "public")} key but got {keyParameter.GetType().Name}.")
        };
    }
}

public enum AsymmetricKeyType
{
    Public, 
    Private
}