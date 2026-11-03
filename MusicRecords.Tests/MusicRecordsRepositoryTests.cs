using MusicRecords.Api.Repositories;
using Xunit;

namespace MusicRecords.Tests;

public class MusicRecordsRepositoryTests
{
    [Fact]
    public void GetAll_ReturnsExpectedRecords()
    {
        var repository = new MusicRecordsRepository();

        var records = repository.GetAll();

        Assert.Collection(
            records,
            record =>
            {
                Assert.Equal(1, record.Id);
                Assert.Equal("Ta på mine frugter", record.Title);
                Assert.Equal("Appelsinsovs", record.Artist);
                Assert.Equal(3660, record.Duration);
                Assert.Equal(2025, record.PublicationYear);
            },
            record =>
            {
                Assert.Equal(2, record.Id);
                Assert.Equal("Citrusdrømme", record.Title);
                Assert.Equal("Appelsinsovs", record.Artist);
                Assert.Equal(2823, record.Duration);
                Assert.Equal(2026, record.PublicationYear);
            });
    }

    [Fact]
    public void RemovingRecordFromReturnedList_DoesNotChangeRepositoryRecords()
    {
        var repository = new MusicRecordsRepository();

        var records = repository.GetAll();
        records.RemoveAt(0);

        Assert.Equal(2, repository.GetAll().Count);
    }

    [Fact]
    public void GetAll_WithTitle_ReturnsMatchingRecords()
    {
        var repository = new MusicRecordsRepository();

        var records = repository.GetAll(title: "Citrus");

        var record = Assert.Single(records);
        Assert.Equal("Citrusdrømme", record.Title);
    }

    [Fact]
    public void GetAll_WithArtist_ReturnsMatchingRecords()
    {
        var repository = new MusicRecordsRepository();

        var records = repository.GetAll(artist: "Ukendt");

        Assert.Empty(records);
    }

    [Fact]
    public void GetAll_WithTitleAndArtist_FiltersByBoth()
    {
        var repository = new MusicRecordsRepository();

        var records = repository.GetAll(title: "Citrus", artist: "Appelsin");

        var record = Assert.Single(records);
        Assert.Equal("Citrusdrømme", record.Title);
        Assert.Equal("Appelsinsovs", record.Artist);
    }
}

