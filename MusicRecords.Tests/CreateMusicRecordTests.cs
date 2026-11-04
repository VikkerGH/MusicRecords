using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicRecords.Api.Controllers;
using MusicRecords.Api.Models;
using MusicRecords.Api.Repositories;
using Xunit;

namespace MusicRecords.Tests;

public class CreateMusicRecordTests
{
    [Theory]
    [InlineData(null, "Kunstner", nameof(CreateMusicRecordRequest.Title))]
    [InlineData("", "Kunstner", nameof(CreateMusicRecordRequest.Title))]
    [InlineData("   ", "Kunstner", nameof(CreateMusicRecordRequest.Title))]
    [InlineData("Titel", null, nameof(CreateMusicRecordRequest.Artist))]
    [InlineData("Titel", "", nameof(CreateMusicRecordRequest.Artist))]
    [InlineData("Titel", "   ", nameof(CreateMusicRecordRequest.Artist))]
    public void Request_WithMissingEmptyOrWhitespaceText_IsInvalid(string? title, string? artist, string member)
    {
        var request = CreateRequest();
        request.Title = title!;
        request.Artist = artist!;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(member));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Request_WithNonPositiveDuration_IsInvalid(int duration)
    {
        var request = CreateRequest();
        request.Duration = duration;

        Assert.Contains(Validate(request), error => error.MemberNames.Contains(nameof(request.Duration)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Request_WithPublicationYearBelowOne_IsInvalid(int year)
    {
        var request = CreateRequest();
        request.PublicationYear = year;

        Assert.Contains(Validate(request), error => error.MemberNames.Contains(nameof(request.PublicationYear)));
    }

    [Fact]
    public void Request_WithFuturePublicationYear_IsInvalid()
    {
        var request = CreateRequest();
        request.PublicationYear = DateTime.UtcNow.Year + 1;

        Assert.Contains(Validate(request), error => error.MemberNames.Contains(nameof(request.PublicationYear)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Request_WithBoundaryPublicationYearAndPositiveDuration_IsValid(bool earliestYear)
    {
        var request = CreateRequest();
        request.PublicationYear = earliestYear ? 1 : DateTime.UtcNow.Year;
        request.Duration = earliestYear ? 1 : int.MaxValue;

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void Request_WithMissingFields_IsInvalid()
    {
        var request = Assert.IsType<CreateMusicRecordRequest>(
            JsonSerializer.Deserialize<CreateMusicRecordRequest>("{}"));

        Assert.NotEmpty(Validate(request));
    }

    [Fact]
    public void Request_HasOnlyClientWritableRecordFields()
    {
        Assert.Equal(
            new[] { "Artist", "Duration", "PublicationYear", "Title" },
            typeof(CreateMusicRecordRequest).GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Add_Returns201WithCreatedRecordAndPreservesPublicReadback()
    {
        var repository = new MusicRecordsRepository();
        var controller = new MusicRecordsController(repository)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var request = CreateRequest();

        var result = Assert.IsType<ObjectResult>(controller.Add(request).Result);
        var created = Assert.IsType<MusicRecord>(result.Value);

        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Equal(3, created.Id);
        Assert.Equal(request.Title, created.Title);
        Assert.Equal(request.Artist, created.Artist);
        Assert.Equal(request.Duration, created.Duration);
        Assert.Equal(request.PublicationYear, created.PublicationYear);
        Assert.False(controller.Response.Headers.ContainsKey("Location"));
        var getResult = Assert.IsType<OkObjectResult>(controller.GetAll(request.Title, request.Artist).Result);
        var found = Assert.Single(Assert.IsType<List<MusicRecord>>(getResult.Value));
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public void Add_ClientSuppliedIdIsIgnoredAndRepositoryAssignsId()
    {
        var repository = new MusicRecordsRepository();
        var controller = new MusicRecordsController(repository);
        var request = Assert.IsType<CreateMusicRecordRequest>(JsonSerializer.Deserialize<CreateMusicRecordRequest>(
            "{\"id\":999,\"title\":\"Klientplade\",\"artist\":\"Kunstner\",\"duration\":120,\"publicationYear\":2025}",
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var result = Assert.IsType<ObjectResult>(controller.Add(request).Result);

        Assert.Equal(3, Assert.IsType<MusicRecord>(result.Value).Id);
    }

    [Fact]
    public void Add_RequiresAdminAndDocumentsAllResponseStatusesWhileGetRemainsPublic()
    {
        var controllerType = typeof(MusicRecordsController);
        var add = controllerType.GetMethod(nameof(MusicRecordsController.Add))!;
        var getAll = controllerType.GetMethod(nameof(MusicRecordsController.GetAll))!;

        Assert.NotNull(controllerType.GetCustomAttribute<ApiControllerAttribute>());
        Assert.NotNull(add.GetCustomAttribute<HttpPostAttribute>());
        Assert.Equal("Admin", Assert.Single(add.GetCustomAttributes<AuthorizeAttribute>()).Roles);
        Assert.Null(add.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Empty(controllerType.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Empty(getAll.GetCustomAttributes<AuthorizeAttribute>());
        Assert.NotNull(getAll.GetCustomAttribute<HttpGetAttribute>());
        Assert.Equal(
            new[] { 201, 400, 401, 403 },
            add.GetCustomAttributes<ProducesResponseTypeAttribute>().Select(attribute => attribute.StatusCode).Order().ToArray());
    }

    private static CreateMusicRecordRequest CreateRequest()
    {
        return new CreateMusicRecordRequest
        {
            Title = "Ny plade",
            Artist = "Ny kunstner",
            Duration = 180,
            PublicationYear = DateTime.UtcNow.Year
        };
    }

    private static List<ValidationResult> Validate(CreateMusicRecordRequest request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);
        return errors;
    }
}
