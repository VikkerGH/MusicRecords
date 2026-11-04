using MusicRecords.Api.Models;
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

    [Fact]
    public void Add_AssignsIdAndReturnsAllFieldValues()
    {
        var repository = new MusicRecordsRepository();
        var input = CreateRecord();
        input.Id = 999;

        var created = repository.Add(input);

        Assert.Equal(3, created.Id);
        Assert.Equal("Ny plade", created.Title);
        Assert.Equal("Ny kunstner", created.Artist);
        Assert.Equal(180, created.Duration);
        Assert.Equal(DateTime.UtcNow.Year, created.PublicationYear);
        Assert.Equal(999, input.Id);
    }

    [Fact]
    public void Add_MultipleRecords_AssignsUniqueSequentialIdsWithoutChangingSeeds()
    {
        var repository = new MusicRecordsRepository();

        var first = repository.Add(CreateRecord());
        var second = repository.Add(CreateRecord());
        var records = repository.GetAll();

        Assert.Equal(3, first.Id);
        Assert.Equal(4, second.Id);
        Assert.Equal(new[] { 1, 2, 3, 4 }, records.Select(record => record.Id).Order().ToArray());
        Assert.Equal("Ta på mine frugter", records.Single(record => record.Id == 1).Title);
        Assert.Equal("Citrusdrømme", records.Single(record => record.Id == 2).Title);
    }

    [Fact]
    public void Add_RecordIsAvailableThroughGetAllAndCaseInsensitiveAndSearch()
    {
        var repository = new MusicRecordsRepository();
        var created = repository.Add(CreateRecord());

        var stored = Assert.Single(repository.GetAll(), record => record.Id == created.Id);
        var found = Assert.Single(repository.GetAll(title: "ny pl", artist: "NY KUN"));

        Assert.Equal(created.Id, stored.Id);
        Assert.Equal(created.Id, found.Id);
        Assert.Equal(created.Title, found.Title);
        Assert.Equal(created.Artist, found.Artist);
        Assert.Equal(created.Duration, found.Duration);
        Assert.Equal(created.PublicationYear, found.PublicationYear);
        Assert.Empty(repository.GetAll(title: "Ny pl", artist: "Ukendt"));
    }

    [Fact]
    public void Add_InputAndReturnedRecordCannotMutateStoredRecord()
    {
        var repository = new MusicRecordsRepository();
        var input = CreateRecord();
        var created = repository.Add(input);
        var id = created.Id;

        input.Title = "Ændret input";
        created.Id = 999;
        created.Artist = "Ændret resultat";

        var stored = Assert.Single(repository.GetAll(), record => record.Id == id);
        Assert.Equal("Ny plade", stored.Title);
        Assert.Equal("Ny kunstner", stored.Artist);
        Assert.NotSame(input, created);
        Assert.NotSame(created, stored);
    }

    [Fact]
    public void GetAll_ModifyingReturnedRecord_DoesNotChangeRepositoryRecords()
    {
        var repository = new MusicRecordsRepository();
        var snapshot = repository.GetAll();

        snapshot[0].Id = 999;
        snapshot[0].Title = "Ændret kopi";

        var stored = Assert.Single(repository.GetAll(), record => record.Id == 1);
        Assert.Equal("Ta på mine frugter", stored.Title);
        Assert.Equal(2, repository.GetAll().Count);
    }

    [Fact]
    public void GetAll_SnapshotDoesNotChangeWhenRecordIsAdded()
    {
        var repository = new MusicRecordsRepository();
        var snapshot = repository.GetAll();

        var created = repository.Add(CreateRecord());

        Assert.Equal(2, snapshot.Count);
        Assert.DoesNotContain(snapshot, record => record.Id == created.Id);
        Assert.Equal(3, repository.GetAll().Count);
    }

    [Fact]
    public async Task Add_ConcurrentReadsAndCreates_AssignsUniqueIdsAndKeepsAllRecords()
    {
        var repository = new MusicRecordsRepository();
        const int count = 100;

        var additions = Enumerable.Range(0, count).Select(index => Task.Run(() =>
        {
            var input = CreateRecord();
            input.Title = $"Samtidig plade {index}";
            var created = repository.Add(input);
            var snapshot = repository.GetAll(title: "Samtidig plade");
            Assert.Contains(snapshot, record => record.Id == created.Id);
            Assert.Equal(snapshot.Count, snapshot.Select(record => record.Id).Distinct().Count());
            return created;
        }));
        var createdRecords = await Task.WhenAll(additions);
        var records = repository.GetAll();

        Assert.Equal(count, createdRecords.Select(record => record.Id).Distinct().Count());
        Assert.Equal(count + 2, records.Count);
        Assert.Equal(Enumerable.Range(1, count + 2), records.Select(record => record.Id).Order());
        foreach (var created in createdRecords)
        {
            Assert.Contains(records, record => record.Id == created.Id && record.Title == created.Title);
        }
    }

    private static MusicRecord CreateRecord()
    {
        return new MusicRecord
        {
            Title = "Ny plade",
            Artist = "Ny kunstner",
            Duration = 180,
            PublicationYear = DateTime.UtcNow.Year
        };
    }
}

