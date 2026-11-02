namespace MusicRecords.Api.Models;

public class MusicRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int PublicationYear { get; set; }
}
