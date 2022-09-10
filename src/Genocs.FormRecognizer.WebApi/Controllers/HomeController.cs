using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.FormRecognizer.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok("Genocs - FormRecognizer WebApi");

    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPing()
        => Ok("Pong");
}
