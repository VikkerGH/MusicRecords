namespace MusicRecords.Api.Models;

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, string Username, string Role);

public sealed record LoginError(string Message);
