using Microsoft.AspNetCore.Identity;
using MusicRecords.Api.Authentication;
using MusicRecords.Api.Models;

namespace MusicRecords.Api.Repositories;

public sealed class UserRepository
{
    private readonly List<LocalUser> _users;

    public UserRepository(LocalUserSettings settings, IPasswordHasher<LocalUser> passwordHasher)
    {
        settings.Validate();

        _users =
        [
            CreateUser(1, settings.User, "User", passwordHasher),
            CreateUser(2, settings.Admin, "Admin", passwordHasher)
        ];
    }

    public LocalUser? GetByUsername(string username)
    {
        return _users.Find(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    private static LocalUser CreateUser(
        int id,
        LocalUserCredentials credentials,
        string role,
        IPasswordHasher<LocalUser> passwordHasher)
    {
        var user = new LocalUser(id, credentials.Username, role);
        user.PasswordHash = passwordHasher.HashPassword(user, credentials.Password);
        return user;
    }
}
