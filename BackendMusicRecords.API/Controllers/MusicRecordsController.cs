using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicRecords.Api.Models;
using MusicRecords.Api.Repositories;

namespace MusicRecords.Api.Controllers;

[ApiController]
[Route("api/records")]
public class MusicRecordsController : ControllerBase
{
    private readonly MusicRecordsRepository _repository;

    public MusicRecordsController(MusicRecordsRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<MusicRecord>),
        StatusCodes.Status200OK)]
    public ActionResult<List<MusicRecord>> GetAll(
        [FromQuery] string? title,
        [FromQuery] string? artist)
    {
        return Ok(_repository.GetAll(title, artist));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MusicRecord), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<MusicRecord> Add([FromBody] CreateMusicRecordRequest request)
    {
        var created = _repository.Add(new MusicRecord
        {
            Title = request.Title,
            Artist = request.Artist,
            Duration = request.Duration,
            PublicationYear = request.PublicationYear
        });

        return StatusCode(StatusCodes.Status201Created, created);
    }
}