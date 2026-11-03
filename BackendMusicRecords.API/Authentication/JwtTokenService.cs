using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MusicRecords.Api.Models;

namespace MusicRecords.Api.Authentication;

public sealed class JwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(JwtSettings settings, TimeProvider timeProvider)
    {
        settings.Validate();
        _settings = settings;
        _signingCredentials = new SigningCredentials(settings.GetSigningKey(), SecurityAlgorithms.HmacSha256);
        _timeProvider = timeProvider;
    }

    public LoginResponse CreateToken(LocalUser user)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        now = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        var expiresAtUtc = now.AddMinutes(JwtSettings.LifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            user.Username,
            user.Role);
    }
}
