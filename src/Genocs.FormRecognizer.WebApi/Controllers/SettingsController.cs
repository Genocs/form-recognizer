using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace Genocs.FormRecognizer.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class SettingsController : ControllerBase
{

    private readonly IDistributedCache _distributedCache;
    private readonly IValidator<SetupSettingRequest> _setupSettingRequestValidator;

    public SettingsController(IDistributedCache distributedCache, IValidator<SetupSettingRequest> setupSettingRequestValidator)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _setupSettingRequestValidator = setupSettingRequestValidator ?? throw new ArgumentNullException(nameof(setupSettingRequestValidator));
    }

    /// <summary>
    /// It allows to setup the model classifier lookup table.
    /// </summary>
    /// <param name="request">the key value pair.</param>
    /// <returns>No Content</returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("SetupClassificationModel")]
    [HttpPost]
    public async Task<ActionResult> PostSetupClassificationModel([FromBody] SetupSettingRequest request)
    {
        var validationResult = await _setupSettingRequestValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        await _distributedCache.SetAsync(request.Key, Encoding.UTF8.GetBytes(request.Value));

        return NoContent();
    }
}
