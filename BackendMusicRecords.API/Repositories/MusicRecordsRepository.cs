using MusicRecords.Api.Models;

namespace MusicRecords.Api.Repositories;

public class MusicRecordsRepository
{
    private readonly object _sync = new();
    private int _nextId;
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

    public MusicRecordsRepository()
    {
        _nextId = _records.Max(record => record.Id) + 1;
    }

    public MusicRecord Add(MusicRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_sync)
        {
            var created = Copy(record);
            created.Id = _nextId;
            _nextId = checked(_nextId + 1);
            _records.Add(created);
            return Copy(created);
        }
    }

    public List<MusicRecord> GetAll(string? title = null, string? artist = null)
    {
        List<MusicRecord> records;
        lock (_sync)
        {
            records = _records.Select(Copy).ToList();
        }

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

    private static MusicRecord Copy(MusicRecord record)
    {
        return new MusicRecord
        {
            Id = record.Id,
            Title = record.Title,
            Artist = record.Artist,
            Duration = record.Duration,
            PublicationYear = record.PublicationYear
        };
    }
}
