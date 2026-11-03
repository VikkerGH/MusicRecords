using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicRecords.Api.Models;
using MusicRecords.Api.Services;

namespace MusicRecords.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(LoginService loginService) : ControllerBase
{
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LoginError), StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var response = loginService.Login(request.Username, request.Password);
        if (response == null)
        {
            return Unauthorized(new LoginError("Forkert brugernavn eller adgangskode."));
        }

        return Ok(response);
    }
}
