using Microsoft.IdentityModel.Tokens;

namespace MusicRecords.Api.Authentication;

public sealed class JwtSettings
{
    public const int LifetimeMinutes = 30;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("Påkrævet konfiguration mangler: Jwt:Issuer.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Påkrævet konfiguration mangler: Jwt:Audience.");
        }

        GetSigningKey();
    }

    public SymmetricSecurityKey GetSigningKey()
    {
        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException("Påkrævet konfiguration mangler: Jwt:SigningKey. Brug User Secrets lokalt.");
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(SigningKey);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Jwt:SigningKey skal være en Base64-kodet tilfældig nøgle på mindst 32 bytes. Brug User Secrets lokalt.");
        }

        if (bytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey mangler eller er for kort. Brug en Base64-kodet tilfældig nøgle på mindst 32 bytes via User Secrets lokalt.");
        }

        return new SymmetricSecurityKey(bytes);
    }

    public TokenValidationParameters CreateValidationParameters()
    {
        Validate();

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSigningKey(),
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "unique_name",
            RoleClaimType = "role"
        };
    }
}
