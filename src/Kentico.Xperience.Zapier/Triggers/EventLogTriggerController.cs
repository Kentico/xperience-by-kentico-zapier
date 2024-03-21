

using CMS.EventLog;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier.Triggers;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class EventLogTriggerController : ControllerBase
{
    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;

    public EventLogTriggerController(IZapierTriggerInfoProvider zapierTriggerInfoProvider) => this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;


    [HttpPost]
    [Route("zapier/triggers/eventlogcreate")]
    public ActionResult CreateTrigger(EventLogTriggerDto trigger)
    {
        string objectType = EventLogInfo.OBJECT_TYPE;
        string name = ZapierTriggerExtensions.GenerateWebhookName();
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerDisplayName = name,
            ZapierTriggerCodeName = ZapierTriggerExtensions.GetUniqueCodename(name),
            ZapierTriggerObjectClassType = ZapierTriggerExtensions.GetType(objectType),
            ZapierTriggerEnabled = true,
            ZapierTriggerEventType = nameof(ZapierTriggerEvents.Create),
            mZapierTriggerObjectType = objectType,
            ZapierTriggerZapierURL = trigger.ZapierUrl
        };

        zapierTriggerInfoProvider.Set(infoObject);

        return Ok(new { TriggerId = infoObject.ZapierTriggerID });
    }


    [HttpDelete]
    [Route("zapier/triggers/eventlogcreate/{zapId}")]
    public IActionResult DeleteTrigger(string zapId)
    {
        if (!int.TryParse(zapId, out int zapierTriggerID))
        {
            BadRequest();
        }

        var ZapInfoToDelete = zapierTriggerInfoProvider.Get(zapierTriggerID);

        if (ZapInfoToDelete != null)
        {
            zapierTriggerInfoProvider.Delete(ZapInfoToDelete);
        }

        return Ok(new { Status = ZapInfoToDelete != null ? "Success" : $"Info object with provided id {zapierTriggerID} does not exist" });
    }
}


public record EventLogTriggerDto(string ZapierUrl);
