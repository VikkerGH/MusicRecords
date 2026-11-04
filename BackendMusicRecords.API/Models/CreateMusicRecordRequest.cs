using System.ComponentModel.DataAnnotations;

namespace MusicRecords.Api.Models;

public sealed class CreateMusicRecordRequest : IValidatableObject
{
    [Required(ErrorMessage = "Titel skal indeholde tekst.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kunstner skal indeholde tekst.")]
    public string Artist { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Varighed skal være positiv.")]
    public int Duration { get; set; }

    public int PublicationYear { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (PublicationYear < 1 || PublicationYear > currentYear)
        {
            yield return new ValidationResult(
                $"Udgivelsesår skal være mellem 1 og {currentYear}.",
                [nameof(PublicationYear)]);
        }
    }
}
