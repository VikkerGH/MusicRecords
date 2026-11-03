using System.Text.Json.Serialization;

namespace MusicRecords.Api.Models;

public sealed class LocalUser(int id, string username, string role)
{
    public int Id { get; } = id;
    public string Username { get; } = username;
    public string Role { get; } = role;

    [JsonIgnore]
    public string PasswordHash { get; internal set; } = string.Empty;
}
