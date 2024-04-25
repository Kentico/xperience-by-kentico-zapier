using Kentico.Xperience.Zapier.Auth;
using Kentico.Xperience.Zapier.Triggers.Extensions;

using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier.Triggers;

[AuthorizeZapier]
[ApiController]
public class FormSubmissionTriggerController : ControllerBase
{
    private readonly IZapierTriggerService triggerService;


    public FormSubmissionTriggerController(IZapierTriggerService triggerService) => this.triggerService = triggerService;


    [HttpPost("zapier/triggers/formsubmission")]
    public ActionResult<CreateTriggerResponse> CreateTrigger(FormSubmissionDto trigger)
    {
        int triggerId = triggerService.CreateTrigger(trigger.ObjectType, nameof(ZapierTriggerEvents.Create), trigger.ZapierUrl);
        return Ok(new CreateTriggerResponse(triggerId));
    }


    [HttpGet("zapier/triggers/formsubmission/{objectType}")]
    public async Task<ActionResult> GetFallbackDataAsync(string objectType)
    {
        var fallbackData = await triggerService.GetFallbackDataAsync(objectType, nameof(ZapierTriggerEvents.Create));
        return fallbackData != null ? Ok(fallbackData) : BadRequest();
    }
}


public record FormSubmissionDto(string ObjectType, string ZapierUrl);
