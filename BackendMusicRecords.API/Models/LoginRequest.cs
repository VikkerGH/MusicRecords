using System.ComponentModel.DataAnnotations;

namespace MusicRecords.Api.Models;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Brugernavn skal udfyldes.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adgangskode skal udfyldes.")]
    public string Password { get; set; } = string.Empty;
}
