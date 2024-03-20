using CMS.DataEngine;
using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Admin;
using Kentico.Xperience.Zapier.Admin.UIPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;

namespace Kentico.Xperience.Zapier.Triggers;


[Authorize(AuthenticationSchemes = ZapierConstants.AuthenticationScheme.XbyKZapierApiKeyScheme)]
[ApiController]
public class FormSubmissionTriggerController : ControllerBase
{
    private readonly IZapierTriggerInfoProvider zapierTriggerInfoProvider;
    private readonly ZapierConfiguration zapierConfiguration;

    public FormSubmissionTriggerController(IZapierTriggerInfoProvider zapierTriggerInfoProvider, IOptionsMonitor<ZapierConfiguration> config)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        zapierConfiguration = config.CurrentValue;
    }

    [HttpGet]
    [Route("zapier/data/formsubmission/objects")]
    public ActionResult<List<KenticoInfoDto>> Objects()
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();

        var infoObjects = DataClassInfoProvider.ProviderObject.Get()
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.FORM)
            .WhereIn(nameof(DataClassInfo.ClassName), allowedObjects)
            .Columns(nameof(DataClassInfo.ClassDisplayName), nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassType), nameof(DataClassInfo.ClassContentTypeType))
            .OrderBy(nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResult();

        return infoObjects.Select(x => new KenticoInfoDto(x.ClassName, $"{x.ClassDisplayName} ({x.ClassType}{(!string.IsNullOrEmpty(x.ClassContentTypeType) ? $" - {x.ClassContentTypeType}" : string.Empty)})"))
            .ToList();
    }




    [HttpPost]
    [Route("zapier/triggers/formsubmission")]
    public ActionResult CreateTrigger(FormSubmissionDto newTrigger)
    {
        var infoObject = new ZapierTriggerInfo
        {
            ZapierTriggerDisplayName = newTrigger.Name,
            ZapierTriggerCodeName = ZapierTriggerExtensions.GetUniqueCodename(newTrigger.Name),
            ZapierTriggerObjectClassType = ZapierTriggerExtensions.GetType(newTrigger.ObjectType),
            ZapierTriggerEnabled = true,
            ZapierTriggerEventType = nameof(ZapierTriggerEvents.Create),
            mZapierTriggerObjectType = newTrigger.ObjectType,
            ZapierTriggerZapierURL = newTrigger.ZapierUrl
        };

        zapierTriggerInfoProvider.Set(infoObject);

        return Ok(new { TriggerId = infoObject.ZapierTriggerID });
    }



    [HttpDelete]
    [Route("zapier/triggers/formsubmission/{zapId}")]
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


public record FormSubmissionDto(string Name, string ObjectType, string ZapierUrl);
