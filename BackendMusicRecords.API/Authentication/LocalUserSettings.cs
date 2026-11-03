namespace MusicRecords.Api.Authentication;

public sealed class LocalUserSettings
{
    public LocalUserCredentials User { get; set; } = new();
    public LocalUserCredentials Admin { get; set; } = new();

    public void Validate()
    {
        ValidateUser(User, "User");
        ValidateUser(Admin, "Admin");

        if (string.Equals(User.Username, Admin.Username, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("LocalUsers:User:Username og LocalUsers:Admin:Username skal være forskellige.");
        }
    }

    private static void ValidateUser(LocalUserCredentials credentials, string role)
    {
        if (string.IsNullOrWhiteSpace(credentials.Username))
        {
            throw new InvalidOperationException($"Påkrævet konfiguration mangler: LocalUsers:{role}:Username.");
        }

        if (string.IsNullOrWhiteSpace(credentials.Password))
        {
            throw new InvalidOperationException($"Påkrævet konfiguration mangler: LocalUsers:{role}:Password. Brug User Secrets lokalt.");
        }
    }
}

public sealed class LocalUserCredentials
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
