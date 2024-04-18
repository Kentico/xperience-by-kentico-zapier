using CMS.Core;
using CMS.EventLog;

using Kentico.Xperience.Zapier.Auth;
using Kentico.Xperience.Zapier.Common;
using Kentico.Xperience.Zapier.Triggers.Extensions;

using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier.Triggers;

[AuthorizeZapier]
[ApiController]
public class EventLogTriggerController : ControllerBase
{
    private readonly IZapierTriggerService triggerService;


    public EventLogTriggerController(IZapierTriggerService triggerService) => this.triggerService = triggerService;


    [HttpPost("zapier/triggers/eventlogcreate")]
    public ActionResult<CreateTriggerResponse> CreateTrigger(EventLogTriggerDto trigger)
    {
        int triggerId = triggerService.CreateTrigger(EventLogInfo.OBJECT_TYPE, nameof(ZapierTriggerEvents.Create), trigger.ZapierUrl);
        triggerService.AssignEventSeverities(triggerId, trigger.Severity.Distinct());
        return Ok(new CreateTriggerResponse(triggerId));
    }


    [HttpGet("zapier/triggers/eventlogcreate")]
    public async Task<ActionResult> GetFallbackDataAsync()
    {
        var fallbackData = await triggerService.GetFallbackDataAsync(EventLogInfo.OBJECT_TYPE, nameof(ZapierTriggerEvents.Create));
        return fallbackData != null ? Ok(fallbackData) : BadRequest();
    }


    [HttpGet($"zapier/data/event-log-severity")]
    public ActionResult<IEnumerable<SelectOptionItem>> GetEventLogSeverities() =>
        Ok(Enum.GetValues<EventTypeEnum>().Select(e =>
        new SelectOptionItem(e.ToString().ToUpper()[0].ToString(), e.ToString())));
}


public record EventLogTriggerDto(string ZapierUrl, string[] Severity);
