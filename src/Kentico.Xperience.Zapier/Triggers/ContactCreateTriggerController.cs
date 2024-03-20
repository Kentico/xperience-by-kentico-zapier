
using CMS.ContactManagement;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier.Triggers;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class ContactCreateTriggerController : ControllerBase
{
    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;

    public ContactCreateTriggerController(IZapierTriggerInfoProvider zapierTriggerInfoProvider) => this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;

    [HttpPost]
    [Route("zapier/triggers/contactcreate")]
    public ActionResult CreateTrigger(ContactTriggerDto trigger)
    {
        string objectType = ContactInfo.OBJECT_TYPE;
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerDisplayName = trigger.Name,
            ZapierTriggerCodeName = ZapierTriggerExtensions.GetUniqueCodename(trigger.Name),
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
    [Route("zapier/triggers/contactcreate/{zapId}")]
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

public record ContactTriggerDto(string Name, string ZapierUrl);
