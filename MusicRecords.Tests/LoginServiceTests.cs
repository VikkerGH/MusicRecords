using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MusicRecords.Api.Authentication;
using MusicRecords.Api.Controllers;
using MusicRecords.Api.Models;
using MusicRecords.Api.Repositories;
using MusicRecords.Api.Services;
using Xunit;

namespace MusicRecords.Tests;

public class LoginServiceTests
{
    private const string UserPassword = "Test-only-user-password!23";
    private const string AdminPassword = "Test-only-admin-password!45";

    [Theory]
    [InlineData("test-user", UserPassword, "User")]
    [InlineData("test-admin", AdminPassword, "Admin")]
    public void Login_WithCorrectCredentials_ReturnsTokenAndRole(string username, string password, string role)
    {
        var context = CreateContext();

        var response = Assert.IsType<LoginResponse>(context.Service.Login(username, password));

        Assert.Equal(username, response.Username);
        Assert.Equal(role, response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(context.Now.UtcDateTime.AddMinutes(30), response.ExpiresAtUtc);
        Assert.Equal(DateTimeKind.Utc, response.ExpiresAtUtc.Kind);
    }

    [Fact]
    public void Login_WithWrongPassword_ReturnsNull()
    {
        var context = CreateContext();

        Assert.Null(context.Service.Login("test-user", "Wrong-test-password"));
    }

    [Fact]
    public void Login_WithUnknownUsername_ReturnsNull()
    {
        var context = CreateContext();

        Assert.Null(context.Service.Login("unknown-test-user", UserPassword));
    }

    [Theory]
    [InlineData("test-user", UserPassword, "1", "User")]
    [InlineData("test-admin", AdminPassword, "2", "Admin")]
    public void IssuedJwt_ValidatesSignatureIssuerAudienceIdentityRoleAndLifetime(
        string username, string password, string id, string role)
    {
        var context = CreateContext();
        var response = Assert.IsType<LoginResponse>(context.Service.Login(username, password));
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        var principal = handler.ValidateToken(response.Token, context.Settings.CreateValidationParameters(), out var validatedToken);
        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

        Assert.True(principal.Identity!.IsAuthenticated);
        Assert.Equal(username, principal.Identity.Name);
        Assert.Equal(id, principal.FindFirst("sub")?.Value);
        Assert.Equal(role, principal.FindFirst("role")?.Value);
        Assert.True(principal.IsInRole(role));
        Assert.False(principal.IsInRole(role == "User" ? "Admin" : "User"));
        Assert.Equal(context.Settings.Issuer, jwt.Issuer);
        Assert.Equal(context.Settings.Audience, Assert.Single(jwt.Audiences));
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Header.Alg);
        Assert.Equal(context.Now.UtcDateTime, jwt.ValidFrom);
        Assert.Equal(context.Now.ToUnixTimeSeconds(), Convert.ToInt64(jwt.Payload[JwtRegisteredClaimNames.Iat]));
        Assert.Equal(response.ExpiresAtUtc, jwt.ValidTo);
        Assert.Equal(TimeSpan.FromMinutes(30), jwt.ValidTo - jwt.ValidFrom);
        Assert.Null(principal.FindFirst("password"));
        Assert.Null(principal.FindFirst("passwordHash"));
    }

    [Fact]
    public void IssuedJwt_WithWrongSigningKey_IsRejected()
    {
        var context = CreateContext();
        var response = Assert.IsType<LoginResponse>(context.Service.Login("test-user", UserPassword));
        var validation = context.Settings.CreateValidationParameters();
        validation.IssuerSigningKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));

        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(response.Token, validation, out _));
    }

    [Fact]
    public void IssuedJwt_WithWrongIssuer_IsRejected()
    {
        var context = CreateContext();
        var response = Assert.IsType<LoginResponse>(context.Service.Login("test-user", UserPassword));
        var validation = context.Settings.CreateValidationParameters();
        validation.ValidIssuer = "Wrong-test-issuer";

        Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(response.Token, validation, out _));
    }

    [Fact]
    public void IssuedJwt_WithWrongAudience_IsRejected()
    {
        var context = CreateContext();
        var response = Assert.IsType<LoginResponse>(context.Service.Login("test-user", UserPassword));
        var validation = context.Settings.CreateValidationParameters();
        validation.ValidAudience = "Wrong-test-audience";

        Assert.Throws<SecurityTokenInvalidAudienceException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(response.Token, validation, out _));
    }

    [Fact]
    public void IssuedJwt_AfterExpiration_IsRejected()
    {
        var context = CreateContext(DateTimeOffset.UtcNow.AddMinutes(-31));
        var response = Assert.IsType<LoginResponse>(context.Service.Login("test-user", UserPassword));

        Assert.Throws<SecurityTokenExpiredException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(response.Token, context.Settings.CreateValidationParameters(), out _));
    }

    [Theory]
    [InlineData("test-user", UserPassword)]
    [InlineData("test-admin", AdminPassword)]
    public void Repository_StoresPasswordHasherHashesAndExcludesThemFromJson(string username, string password)
    {
        var context = CreateContext();
        var user = Assert.IsType<LocalUser>(context.Repository.GetByUsername(username));
        var hasher = new PasswordHasher<LocalUser>();

        Assert.NotEqual(password, user.PasswordHash);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(user, user.PasswordHash, password));
        var json = JsonSerializer.Serialize(user, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(password, json);
        Assert.DoesNotContain(user.PasswordHash, json);
    }

    [Fact]
    public void LoginResponse_ContainsOnlyThePublicResponseFields()
    {
        var context = CreateContext();
        var response = Assert.IsType<LoginResponse>(context.Service.Login("test-user", UserPassword));
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            new[] { "expiresAtUtc", "role", "token", "username" },
            document.RootElement.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.DoesNotContain(UserPassword, json);
    }

    [Fact]
    public void Controller_InvalidCredentials_ReturnSameGenericUnauthorizedMessage()
    {
        var context = CreateContext();
        var controller = new AuthController(context.Service);
        var wrongPassword = Assert.IsType<UnauthorizedObjectResult>(
            controller.Login(new LoginRequest { Username = "test-user", Password = "Wrong-test-password" }).Result);
        var unknownUser = Assert.IsType<UnauthorizedObjectResult>(
            controller.Login(new LoginRequest { Username = "unknown-test-user", Password = UserPassword }).Result);

        Assert.Equal(401, wrongPassword.StatusCode);
        Assert.Equal(401, unknownUser.StatusCode);
        Assert.Equal(Assert.IsType<LoginError>(wrongPassword.Value), Assert.IsType<LoginError>(unknownUser.Value));
        Assert.Equal("Forkert brugernavn eller adgangskode.", Assert.IsType<LoginError>(wrongPassword.Value).Message);
    }

    [Fact]
    public void Controller_ValidCredentials_ReturnsOkWithSafeResponse()
    {
        var context = CreateContext();
        var controller = new AuthController(context.Service);

        var result = Assert.IsType<OkObjectResult>(
            controller.Login(new LoginRequest { Username = "test-user", Password = UserPassword }).Result);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("User", Assert.IsType<LoginResponse>(result.Value).Role);
    }

    [Theory]
    [InlineData(null, UserPassword)]
    [InlineData("", UserPassword)]
    [InlineData("   ", UserPassword)]
    [InlineData("test-user", null)]
    [InlineData("test-user", "")]
    [InlineData("test-user", "   ")]
    [InlineData(null, null)]
    public void LoginRequest_WithMissingEmptyOrWhitespaceFields_FailsValidation(string? username, string? password)
    {
        var request = new LoginRequest { Username = username!, Password = password! };
        var results = new List<ValidationResult>();

        Assert.False(Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true));
        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64")]
    [InlineData("dGVzdA==")]
    public void JwtSettings_WithMissingOrInvalidKey_ThrowsClearConfigurationError(string? key)
    {
        var settings = new JwtSettings { Issuer = "Test-issuer", Audience = "Test-audience", SigningKey = key! };

        var error = Assert.Throws<InvalidOperationException>(settings.Validate);

        Assert.Contains("Jwt:SigningKey", error.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void JwtSettings_WithMissingIssuerOrAudience_ThrowsClearConfigurationError(bool missingIssuer)
    {
        var settings = CreateContext().Settings;
        if (missingIssuer)
        {
            settings.Issuer = "";
        }
        else
        {
            settings.Audience = "";
        }

        var error = Assert.Throws<InvalidOperationException>(settings.Validate);

        Assert.Contains(missingIssuer ? "Jwt:Issuer" : "Jwt:Audience", error.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LocalUserSettings_WithMissingPassword_ThrowsClearConfigurationError(bool userRole)
    {
        var settings = CreateUserSettings();
        if (userRole)
        {
            settings.User.Password = "";
        }
        else
        {
            settings.Admin.Password = "";
        }

        var error = Assert.Throws<InvalidOperationException>(settings.Validate);

        Assert.Contains(userRole ? "LocalUsers:User:Password" : "LocalUsers:Admin:Password", error.Message);
    }

    private static TestContext CreateContext(DateTimeOffset? issuedAt = null)
    {
        var settings = new JwtSettings
        {
            Issuer = "MusicRecords.Tests.Issuer",
            Audience = "MusicRecords.Tests.Audience",
            SigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        };
        var hasher = new PasswordHasher<LocalUser>();
        var repository = new UserRepository(CreateUserSettings(), hasher);
        var now = DateTimeOffset.FromUnixTimeSeconds((issuedAt ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds());
        var service = new LoginService(repository, hasher, new JwtTokenService(settings, new FixedTimeProvider(now)));
        return new TestContext(settings, repository, service, now);
    }

    private static LocalUserSettings CreateUserSettings()
    {
        return new LocalUserSettings
        {
            User = new LocalUserCredentials { Username = "test-user", Password = UserPassword },
            Admin = new LocalUserCredentials { Username = "test-admin", Password = AdminPassword }
        };
    }

    private sealed record TestContext(JwtSettings Settings, UserRepository Repository, LoginService Service, DateTimeOffset Now);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
