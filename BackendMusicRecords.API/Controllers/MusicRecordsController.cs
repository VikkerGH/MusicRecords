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
}