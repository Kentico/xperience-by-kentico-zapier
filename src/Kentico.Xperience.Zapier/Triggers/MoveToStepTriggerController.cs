using System.Data;

using CMS.ContentWorkflowEngine;
using CMS.DataEngine;

using Kentico.Xperience.Zapier.Auth;
using Kentico.Xperience.Zapier.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Zapier.Triggers;

[AuthorizeZapier]
[ApiController]
public class MoveToStepTriggerController : ControllerBase
{
    private readonly IZapierTriggerService triggerService;
    private readonly ZapierConfiguration zapierConfiguration;


    public MoveToStepTriggerController(IZapierTriggerService triggerService, IOptionsMonitor<ZapierConfiguration> zapierConfiguration)
    {
        this.triggerService = triggerService;
        this.zapierConfiguration = zapierConfiguration.CurrentValue;
    }


    [HttpPost("zapier/triggers/movetostep")]
    public ActionResult<CreateTriggerResponse> CreateTrigger(MoveToStepTriggerDto trigger)
    {
        int triggerId = triggerService.CreateTrigger(trigger.ObjectType, trigger.EventType, trigger.ZapierUrl);
        return Ok(new CreateTriggerResponse(triggerId));
    }


    [HttpGet("zapier/triggers/movetostep/{objectType}/{eventType}")]
    public async Task<ActionResult> GetFallbackDataAsync(string objectType, string eventType)
    {
        var fallbackData = await triggerService.GetFallbackDataAsync(objectType, eventType);
        return fallbackData != null ? Ok(fallbackData) : BadRequest();
    }


    [HttpGet("zapier/data/workflow-steps/{className}")]
    public ActionResult<IEnumerable<SelectOptionItem>> GetWorkflowSteps(string className)
    {
        var allowedObjects = zapierConfiguration.AllowedObjects.ToList();
        if (!allowedObjects.Contains(className, StringComparer.OrdinalIgnoreCase))
        {
            return Array.Empty<SelectOptionItem>();
        }
        var data = DataClassInfoProvider.ProviderObject.Get()
            .Source(x => x.Join<ContentWorkflowContentTypeInfo>(
                nameof(DataClassInfo.ClassID), nameof(ContentWorkflowContentTypeInfo.ContentWorkflowContentTypeContentTypeID)
            )).Source(x => x.Join<ContentWorkflowStepInfo>(
                nameof(ContentWorkflowContentTypeInfo.ContentWorkflowContentTypeContentWorkflowID), nameof(ContentWorkflowStepInfo.ContentWorkflowStepWorkflowID))
            ).Source(x => x.Join<ContentWorkflowInfo>(
                nameof(ContentWorkflowStepInfo.ContentWorkflowStepWorkflowID), nameof(ContentWorkflowInfo.ContentWorkflowID))
            )
            .WhereEquals(nameof(ContentWorkflowStepInfo.ContentWorkflowStepType), ContentWorkflowStepType.Custom)
            .WhereEquals(nameof(DataClassInfo.ClassName), className)
            .Columns(nameof(ContentWorkflowInfo.ContentWorkflowDisplayName),
            nameof(ContentWorkflowStepInfo.ContentWorkflowStepDisplayName),
            nameof(ContentWorkflowStepInfo.ContentWorkflowStepName))
            .Result;
        if (data == null || data.Tables.Count == 0)
        {
            return Array.Empty<SelectOptionItem>();
        }
        return data.Tables[0].AsEnumerable().Select(row => new SelectOptionItem
            (
                row[nameof(ContentWorkflowStepInfo.ContentWorkflowStepName)]?.ToString() ?? string.Empty,
                $"{row[nameof(ContentWorkflowStepInfo.ContentWorkflowStepDisplayName)]} ({row[nameof(ContentWorkflowInfo.ContentWorkflowDisplayName)]})"
            ));
    }


    [HttpGet($"zapier/data/types/movetostep")]
    public ActionResult<IEnumerable<SelectOptionItem>> GetContentTypesWithWorkflows() =>
        DataClassInfoProvider.ProviderObject.GetZapierTypesQuery(classType: ClassType.CONTENT_TYPE, zapierConfiguration.AllowedObjects.ToList())
            .WhereNotEquals(nameof(DataClassInfo.ClassContentTypeType), ClassContentTypeType.EMAIL)
            .Source(x => x.Join<ContentWorkflowContentTypeInfo>(
                nameof(DataClassInfo.ClassID), nameof(ContentWorkflowContentTypeInfo.ContentWorkflowContentTypeContentTypeID))
            ).Select(x => new SelectOptionItem(
                    x.ClassName,
                    $"{x.ClassDisplayName} ({x.ClassContentTypeType})"
            )).ToList();
}


public record MoveToStepTriggerDto(string ObjectType, string EventType, string ZapierUrl);
