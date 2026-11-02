using MusicRecords.Api.Models;

namespace MusicRecords.Api.Repositories;

public class MusicRecordsRepository
{
    private readonly List<MusicRecord> _records = new()
    {
        new MusicRecord
        {
            Id = 1,
            Title = "Discovery",
            Artist = "Daft Punk",
            Duration = 3660,
            PublicationYear = 2001
        },
        new MusicRecord
        {
            Id = 2,
            Title = "Abbey Road",
            Artist = "The Beatles",
            Duration = 2823,
            PublicationYear = 1969
        }
    };

    public List<MusicRecord> GetAll()
    {
        return new List<MusicRecord>(_records);
    }
}
