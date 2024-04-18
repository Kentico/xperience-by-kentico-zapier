using CMS.DataEngine;

using Kentico.Integration.Zapier;
using Kentico.Xperience.Zapier.Auth;

using Microsoft.AspNetCore.Mvc;

namespace Kentico.Xperience.Zapier.Triggers;

[AuthorizeZapier]
[ApiController]
public class ZapierTriggerController : ControllerBase
{
    private readonly IInfoProvider<ZapierTriggerInfo> zapierTriggerInfoProvider;
    private readonly IZapierTriggerService zapierTriggerService;


    public ZapierTriggerController(IInfoProvider<ZapierTriggerInfo> zapierTriggerInfoProvider, IZapierTriggerService zapierTriggerService)
    {
        this.zapierTriggerInfoProvider = zapierTriggerInfoProvider;
        this.zapierTriggerService = zapierTriggerService;
    }


    [HttpDelete("zapier/triggers/{zapId}")]
    public ActionResult<DeleteTriggerResponse> DeleteTrigger(int zapId)
    {
        var zapInfoToDelete = zapierTriggerInfoProvider.Get()
            .WhereEquals(nameof(ZapierTriggerInfo.ZapierTriggerID), zapId)
            .FirstOrDefault();

        if (zapInfoToDelete != null)
        {
            zapierTriggerService.DeleteTrigger(zapInfoToDelete);
        }
        return Ok(new DeleteTriggerResponse(zapInfoToDelete != null ? "Success" : $"Info object with provided id {zapId} does not exist"));
    }
}

/// <summary>
/// Response record of the action to create the trigger
/// </summary>
/// <param name="TriggerId"></param>
public sealed record CreateTriggerResponse(int TriggerId);


/// <summary>
/// Response record of the action to delete the trigger
/// </summary>
/// <param name="Status"></param>
public sealed record DeleteTriggerResponse(string Status);


