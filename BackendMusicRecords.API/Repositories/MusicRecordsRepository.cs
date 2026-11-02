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

    public List<MusicRecord> GetAll()
    {
        return new List<MusicRecord>(_records);
    }
}
