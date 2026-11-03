using MusicRecords.Api.Models;

namespace MusicRecords.Api.Repositories;

public class MusicRecordsRepository
{
    private readonly List<MusicRecord> _records = new()
    {
        new MusicRecord
        {
            Id = 1,
            Title = "Ta på mine frugter",
            Artist = "Appelsinsovs",
            Duration = 3660,
            PublicationYear = 2025
        },
        new MusicRecord
        {
            Id = 2,
            Title = "Citrusdrømme",
            Artist = "Appelsinsovs",
            Duration = 2823,
            PublicationYear = 2026
        }
    };

    public List<MusicRecord> GetAll(string? title = null, string? artist = null)
    {
        List<MusicRecord> records = new(_records);

        if (title != null)
        {
            records = records.FindAll(record =>
                record.Title.StartsWith(title, StringComparison.OrdinalIgnoreCase));
        }

        if (artist != null)
        {
            records = records.FindAll(record =>
                record.Artist.StartsWith(artist, StringComparison.OrdinalIgnoreCase));
        }

        return records;
    }
}
