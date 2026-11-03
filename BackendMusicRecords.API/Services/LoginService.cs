using Microsoft.AspNetCore.Identity;
using MusicRecords.Api.Authentication;
using MusicRecords.Api.Models;
using MusicRecords.Api.Repositories;

namespace MusicRecords.Api.Services;

public sealed class LoginService(
    UserRepository repository,
    IPasswordHasher<LocalUser> passwordHasher,
    JwtTokenService tokenService)
{
    public LoginResponse? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = repository.GetByUsername(username);
        if (user == null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return tokenService.CreateToken(user);
    }
}
