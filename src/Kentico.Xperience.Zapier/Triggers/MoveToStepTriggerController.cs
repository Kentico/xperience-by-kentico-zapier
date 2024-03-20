
using CMS.DataEngine;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Zapier.Triggers;

[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class MoveToStepTriggerController : ControllerBase
{
    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly ZapierConfiguration zapierConfiguration;

    public MoveToStepTriggerController(IZapierTriggerInfoProvider zapierTriggerInfoProvider, IOptionsMonitor<ZapierConfiguration> config)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        zapierConfiguration = config.CurrentValue;
    }

    [HttpGet]
    [Route("zapier/triggers/contentobjects")]
    public ActionResult<List<KenticoInfoDto>> Objects()
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();
        var infoObjects = DataClassInfoProvider.ProviderObject.Get()
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.CONTENT_TYPE)
            .WhereNotEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.EMAIL)
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassType), nameof(DataClassInfo.ClassContentTypeType))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResult();

        return infoObjects.Select(x => new KenticoInfoDto(x.ClassName, $"{x.ClassDisplayName} ({x.ClassType}{(!string.IsNullOrEmpty(x.ClassContentTypeType) ? $" - {x.ClassContentTypeType}" : string.Empty)})"))
            .ToList();
    }

    [HttpPost]
    [Route("zapier/triggers/movetostep")]
    public ActionResult CreateTrigger(MoveToStepTriggerDto trigger)
    {
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerDisplayName = trigger.Name,
            ZapierTriggerCodeName = ZapierTriggerExtensions.GetUniqueCodename(trigger.Name),
            ZapierTriggerObjectClassType = ZapierTriggerExtensions.GetType(trigger.ObjectType),
            ZapierTriggerEnabled = true,
            ZapierTriggerEventType = trigger.EventType,
            mZapierTriggerObjectType = trigger.ObjectType,
            ZapierTriggerZapierURL = trigger.ZapierUrl
        };

        zapierTriggerInfoProvider.Set(infoObject);

        return Ok(new { TriggerId = infoObject.ZapierTriggerID });
    }


    [HttpDelete]
    [Route("zapier/triggers/movetostep/{zapId}")]
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

public record MoveToStepTriggerDto(string Name, string ObjectType, string EventType, string ZapierUrl);
